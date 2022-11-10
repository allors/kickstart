// <copyright file="WorkEffortTotalMaterialRevenueDerivation.cs" company="Allors bvba">
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

    public class AviationWorkEffortTotalMaterialRevenueRule : Rule
    {
        public AviationWorkEffortTotalMaterialRevenueRule(MetaPopulation m) : base(m, new Guid("441ca408-5af2-429d-96ff-29e83d2b3519")) =>
            this.Patterns = new Pattern[]
            {
                m.WorkEffortInventoryAssignment.RolePattern(v => v.Assignment, v => v.Assignment),
                m.WorkEffortInventoryAssignment.RolePattern(v => v.AssignedBillableQuantity, v => v.Assignment),
                m.WorkEffortInventoryAssignment.RolePattern(v => v.Quantity, v => v.Assignment),
                m.WorkEffortInventoryAssignment.RolePattern(v => v.UnitSellingPrice, v => v.Assignment),
                m.WorkEffortInvoiceItemAssignment.RolePattern(v => v.Assignment, v => v.Assignment),
                m.WorkEffortInvoiceItem.RolePattern(v => v.Amount, v => v.WorkEffortInvoiceItemAssignmentWhereWorkEffortInvoiceItem.WorkEffortInvoiceItemAssignment.Assignment),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<WorkEffort>())
            {
                @this.DeriveAviationWorkEffortTotalMaterialRevenueRule(validation);
            }
        }
    }

    public static class AviationWorkEffortTotalMaterialRevenueRuleExtensions
    {
        public static void DeriveAviationWorkEffortTotalMaterialRevenueRule(this WorkEffort @this, IValidation validation)
        {
            var partsRevenue = Rounder.RoundDecimal(@this.WorkEffortInventoryAssignmentsWhereAssignment
                .Where(v => v.DerivedBillableQuantity > 0)
                .Sum(v => v.DerivedBillableQuantity * v.UnitSellingPrice), 2);

            var sundriesRevenue = Rounder.RoundDecimal(@this.WorkEffortInvoiceItemAssignmentsWhereAssignment
                .Where(v => v.ExistWorkEffortInvoiceItem
                            && v.WorkEffortInvoiceItem.InvoiceItemType.IsSundries
                            && v.WorkEffortInvoiceItem.Amount.HasValue)
                .Sum(v => v.WorkEffortInvoiceItem.Amount.Value), 2);

            @this.TotalMaterialRevenue = partsRevenue + sundriesRevenue;
        }
    }
}
