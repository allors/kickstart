// <copyright file="WorkEffortTotalCostDerivation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using Allors.Database.Meta;
using Allors.Database.Derivations;
using Allors.Database.Domain.Derivations.Rules;
using Allors.Database.Domain.Derivations.Rules;

namespace Allors.Database.Domain
{
    public class WorkEffortTotalCostRule : Rule
    {
        public WorkEffortTotalCostRule(MetaPopulation m) : base(m, new Guid("d045607f-fd57-4b2d-a4cd-d1eca970ce6d")) =>
            this.Patterns = new Pattern[]
            {
                m.TimeEntry.RolePattern(v => v.WorkEffort, v => v.WorkEffort),
                m.TimeEntry.RolePattern(v => v.Cost, v => v.WorkEffort),
                m.WorkEffortInventoryAssignment.RolePattern(v => v.CostOfGoodsSold, v => v.Assignment),
                m.WorkEffortPurchaseOrderItemAssignment.RolePattern(v => v.Quantity, v => v.Assignment),
                m.WorkEffortPurchaseOrderItemAssignment.RolePattern(v => v.UnitPurchasePrice, v => v.Assignment),
                m.WorkTask.AssociationPattern(v => v.WorkEffortInventoryAssignmentsWhereAssignment),
                m.WorkTask.AssociationPattern(v => v.WorkEffortPurchaseOrderItemAssignmentsWhereAssignment),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<WorkEffort>())
            {
                @this.TotalLabourCost = Math.Round(@this.ServiceEntriesWhereWorkEffort.Sum(v => ((TimeEntry)v).Cost), 2);
                @this.TotalMaterialCost = Math.Round(@this.WorkEffortInventoryAssignmentsWhereAssignment.Sum(v => v.CostOfGoodsSold), 2);
                @this.TotalSubContractedCost = Math.Round(@this.WorkEffortPurchaseOrderItemAssignmentsWhereAssignment.Sum(v => v.Quantity * v.UnitPurchasePrice), 2);
                @this.TotalCost = Math.Round(@this.TotalLabourCost + @this.TotalMaterialCost + @this.TotalSubContractedCost, 2);
            }
        }
    }
}
