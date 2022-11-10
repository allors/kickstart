// <copyright file="CustomWorkEffortInventoryAssignmentCostOfGoodsSoldDerivation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;
    using Database.Derivations;
    using Resources;
    using Allors.Database.Domain.Derivations.Rules;

    public class AviationWorkEffortInventoryAssignmentCostOfGoodsSoldRule : Rule
    {
        public AviationWorkEffortInventoryAssignmentCostOfGoodsSoldRule(MetaPopulation m) : base(m, new Guid("ee67f91e-fb39-4f37-a534-7715b9e3150c")) =>
            this.Patterns = new Pattern[]
        {
            m.WorkEffortInventoryAssignment.RolePattern(v => v.InventoryItem),
            m.WorkEffortInventoryAssignment.RolePattern(v => v.Quantity),
            m.WorkEffortInventoryAssignment.RolePattern(v => v.Assignment),
            m.SupplierOffering.RolePattern(v => v.Price, v => v.Part.Part.InventoryItemsWherePart.InventoryItem.WorkEffortInventoryAssignmentsWhereInventoryItem),
            m.WorkTask.RolePattern(v => v.WorkEffortState, v => v.WorkEffortInventoryAssignmentsWhereAssignment),
            m.Part.AssociationPattern(v => v.SupplierOfferingsWherePart, v => v.InventoryItemsWherePart.InventoryItem.WorkEffortInventoryAssignmentsWhereInventoryItem),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<WorkEffortInventoryAssignment>())
            {
                if (@this.Assignment is WorkTask workTask
                   && !workTask.WorkEffortState.IsCompleted
                   && !workTask.WorkEffortState.IsFinished)
                {
                    if (@this.InventoryItem.Part.ExistSupplierOfferingsWherePart)
                    {
                        @this.UnitPurchasePrice = @this.InventoryItem.Part.SupplierOfferingsWherePart.Max(v => v.Price);
                        @this.CostOfGoodsSold = @this.Quantity * @this.UnitPurchasePrice;
                    }
                }
            }
        }
    }
}
