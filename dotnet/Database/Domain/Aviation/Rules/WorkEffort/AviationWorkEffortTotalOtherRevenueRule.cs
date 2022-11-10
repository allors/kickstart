// <copyright file="WorkEffortTotalOtherRevenueRule.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Allors.Database.Meta;
    using Allors.Database.Derivations;
    using Derivations.Rules;

    public class AviationWorkEffortTotalOtherRevenueRule : Rule
    {
        public AviationWorkEffortTotalOtherRevenueRule(MetaPopulation m) : base(m, new Guid("17258391-99ba-497f-90a5-075ad72e2fa6")) =>
            this.Patterns = new Pattern[]
            {
                m.WorkEffortInvoiceItemAssignment.RolePattern(v => v.Assignment, v => v.Assignment),
                m.WorkEffortInvoiceItem.RolePattern(v => v.Amount, v => v.WorkEffortInvoiceItemAssignmentWhereWorkEffortInvoiceItem.WorkEffortInvoiceItemAssignment.Assignment),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<WorkEffort>())
            {
                @this.DeriveAviationWorkEffortTotalOtherRevenue(validation);
            }
        }
    }

    public static class AviationWorkEffortTotalOtherRevenueRuleExtensions
    {
        public static void DeriveAviationWorkEffortTotalOtherRevenue(this WorkEffort @this, IValidation validation) => @this.TotalOtherRevenue = Rounder.RoundDecimal(@this.WorkEffortInvoiceItemAssignmentsWhereAssignment
                .Where(v => v.ExistWorkEffortInvoiceItem
                            && !v.WorkEffortInvoiceItem.InvoiceItemType.IsSundries
                            && v.WorkEffortInvoiceItem.Amount.HasValue)
                .Sum(v => v.WorkEffortInvoiceItem.Amount.Value), 2);
    }
}
