// <copyright file="SalesInvoiceItemAssignedUnitPriceRule.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using Allors.Database.Meta;
using Allors.Database.Derivations;
using Allors.Database.Domain.Derivations.Rules;

namespace Allors.Database.Domain
{
    public class WorkEffortInvoiceItemAmountRule : Rule
    {
        public WorkEffortInvoiceItemAmountRule(MetaPopulation m) : base(m, new Guid("3820316d-e1f6-4038-a4bc-c10ecc1d83c5")) =>
            this.Patterns = new Pattern[]
            {
                m.WorkTask.RolePattern(v => v.TotalLabourRevenue, v => v.WorkEffortInvoiceItemAssignmentsWhereAssignment.WorkEffortInvoiceItemAssignment.WorkEffortInvoiceItem),
                m.WorkEffortInvoiceItem.RolePattern(v => v.AssignedAmount),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<WorkEffortInvoiceItem>())
            {
                @this.DeriveWorkEffortInvoiceItemAmount(validation);
            }
        }
    }

    public static class WorkEffortInvoiceItemAmountRuleExtensions
    {
        public static void DeriveWorkEffortInvoiceItemAmount(this WorkEffortInvoiceItem @this, IValidation validation)
        {
            var workTask = (WorkTask)@this.WorkEffortInvoiceItemAssignmentWhereWorkEffortInvoiceItem.Assignment;

            if (workTask != null)
            {
                var customer = (Organisation)workTask.Customer;
                var billableAmountOfTime = workTask.BillableTimeEntries().Sum(v => v.BillableAmountOfTimeInMinutes) / 60;
                var settings = @this.Transaction().GetSingleton().Settings;

                if (@this.InvoiceItemType.IsCleaning && billableAmountOfTime > 0 && customer != null && !customer.ExcludeCleaning)
                {
                    CalculateAmount(@this, workTask.CleaningExpression, billableAmountOfTime, customer.CleaningMinimum, customer.CleaningMaximum, validation);
                }
                else if (@this.InvoiceItemType.IsSundries && billableAmountOfTime > 0 && customer != null && !customer.ExcludeSundries)
                {
                    CalculateAmount(@this, workTask.SundriesExpression, billableAmountOfTime, customer.SundriesMinimum, customer.SundriesMaximum, validation);
                }
                else
                {
                    @this.Amount = @this.AssignedAmount.HasValue ? @this.AssignedAmount.Value : 0M;
                }
            }
        }

        private static void CalculateAmount(WorkEffortInvoiceItem @this, Expression expression, decimal billableAmountOfTime, decimal? minimum, decimal? maximum, IValidation validation)
        {
            expression.AddParameter("1", billableAmountOfTime);

            try
            {
                var calculatedAmount = (decimal)expression.Evaluate();
                if (minimum.HasValue && minimum.Value > calculatedAmount)
                {
                    calculatedAmount = minimum.Value;
                }

                if (maximum.HasValue && maximum.Value < calculatedAmount)
                {
                    calculatedAmount = maximum.Value;
                }

                @this.Amount = @this.ExistAssignedAmount ?
                        @this.AssignedAmount.Value :
                        calculatedAmount;

            }
            catch (DivideByZeroException)
            {
                // Division by zero means no calculation
                @this.RemoveAmount();
            }
            catch (Exception e)
            {
                // other errors will get reported
                validation.AddError(@this, @this.Meta.Amount, e.ToString());
            }
        }
    }
}
