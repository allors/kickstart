// <copyright file="CustomWorkEffortPurchaseOrderItemAssignmentUnitSellingPriceDerivation.cs" company="Allors bvba">
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
    public class AviationWorkEffortPurchaseOrderItemAssignmentUnitSellingPriceRule : Rule
    {
        public AviationWorkEffortPurchaseOrderItemAssignmentUnitSellingPriceRule(MetaPopulation m) : base(m, new Guid("19a657ec-19ce-4564-9189-aa0596a2b646")) =>
            this.Patterns = new Pattern[]
            {
                m.WorkEffortPurchaseOrderItemAssignment.RolePattern(v => v.Assignment),
                m.WorkEffortPurchaseOrderItemAssignment.RolePattern(v => v.AssignedUnitSellingPrice),
                m.WorkEffortPurchaseOrderItemAssignment.RolePattern(v => v.UnitPurchasePrice),
                m.WorkEffort.RolePattern(v => v.Customer, v => v.WorkEffortPurchaseOrderItemAssignmentsWhereAssignment),
                m.WorkEffort.RolePattern(v => v.ExecutedBy, v => v.WorkEffortPurchaseOrderItemAssignmentsWhereAssignment),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<WorkEffortPurchaseOrderItemAssignment>())
            {
                @this.DeriveAviationWorkEffortPurchaseOrderItemAssignmentUnitSellingPriceRule(validation);
            }
        }
    }

    public static class AviationWorkEffortPurchaseOrderItemAssignmentUnitSellingPriceRuleExtensions
    {
        public static void DeriveAviationWorkEffortPurchaseOrderItemAssignmentUnitSellingPriceRule(this WorkEffortPurchaseOrderItemAssignment @this, IValidation validation)
        {
            var transaction = @this.Strategy.Transaction;

            if (@this.AssignedUnitSellingPrice.HasValue)
            {
                @this.UnitSellingPrice = @this.AssignedUnitSellingPrice.Value;
            }
            else if (@this.Assignment?.Customer is Organisation organisation && organisation.IsInternalOrganisation)
            {
                if (@this.Assignment.Customer == @this.Assignment.ExecutedBy)
                {
                    @this.UnitSellingPrice = @this.UnitPurchasePrice;
                }
                else
                {
                    @this.UnitSellingPrice = Math.Round(@this.UnitPurchasePrice * (1 + transaction.GetSingleton().Settings.InternalSubletSurchargePercentage / 100), 2);
                }
            }
            else
            {
                @this.UnitSellingPrice = Math.Round(@this.UnitPurchasePrice * (1 + transaction.GetSingleton().Settings.SubletSurchargePercentage / 100), 2);
            }
        }
    }
}
