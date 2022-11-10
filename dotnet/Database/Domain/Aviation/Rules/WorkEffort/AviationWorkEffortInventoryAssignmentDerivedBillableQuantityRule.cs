// <copyright file="CustomWorkEffortInventoryAssignmentDerivedBillableQuantityDerivation.cs" company="Allors bvba">
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

    public class AviationWorkEffortInventoryAssignmentDerivedBillableQuantityRule : Rule
    {
        public AviationWorkEffortInventoryAssignmentDerivedBillableQuantityRule(MetaPopulation m) : base(m, new Guid("752755b1-1291-47e6-9972-2347c7f3f005")) =>
            this.Patterns = new Pattern[]
        {
            m.WorkEffortInventoryAssignment.RolePattern(v => v.Assignment),
            m.WorkEffortInventoryAssignment.RolePattern(v => v.Quantity),
            m.WorkEffortInventoryAssignment.RolePattern(v => v.AssignedBillableQuantity),
            m.WorkTask.RolePattern(v => v.MaintenanceAgreement, v => v.WorkEffortInventoryAssignmentsWhereAssignment),
            m.WorkTask.RolePattern(v => v.WorkEffortState, v => v.WorkEffortInventoryAssignmentsWhereAssignment),
            m.WorkTask.RolePattern(v => v.ScheduledStart, v => v.WorkEffortInventoryAssignmentsWhereAssignment),
            m.MaintenanceAgreement.RolePattern(v => v.WorkEffortType, v => v.WorkTasksWhereMaintenanceAgreement.WorkTask.WorkEffortInventoryAssignmentsWhereAssignment),
            m.WorkEffortType.RolePattern(v => v.WorkEffortPartStandards, v => v.MaintenanceAgreementsWhereWorkEffortType.MaintenanceAgreement.WorkTasksWhereMaintenanceAgreement.WorkTask.WorkEffortInventoryAssignmentsWhereAssignment),
            m.WorkEffortPartStandard.RolePattern(v => v.FromDate, v => v.WorkEffortTypeWhereWorkEffortPartStandard.WorkEffortType.MaintenanceAgreementsWhereWorkEffortType.MaintenanceAgreement.WorkTasksWhereMaintenanceAgreement.WorkTask.WorkEffortInventoryAssignmentsWhereAssignment),
            m.WorkEffortPartStandard.RolePattern(v => v.ThroughDate, v => v.WorkEffortTypeWhereWorkEffortPartStandard.WorkEffortType.MaintenanceAgreementsWhereWorkEffortType.MaintenanceAgreement.WorkTasksWhereMaintenanceAgreement.WorkTask.WorkEffortInventoryAssignmentsWhereAssignment),
            m.WorkEffortPartStandard.RolePattern(v => v.Quantity, v => v.WorkEffortTypeWhereWorkEffortPartStandard.WorkEffortType.MaintenanceAgreementsWhereWorkEffortType.MaintenanceAgreement.WorkTasksWhereMaintenanceAgreement.WorkTask.WorkEffortInventoryAssignmentsWhereAssignment),
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
                    if (workTask.ExistMaintenanceAgreement && workTask.ExistScheduledStart)
                    {
                        var quantityUsed = @this.AssignedBillableQuantity ?? @this.Quantity;
                        var workEffortPartsStandard = workTask.MaintenanceAgreement.WorkEffortType?.WorkEffortPartStandards
                            .FirstOrDefault(v => v.Part.Equals(@this.InventoryItem.Part)
                                                                && v.FromDate <= workTask.ScheduledStart.Value.Date
                                                                && (!v.ExistThroughDate || v.ThroughDate >= workTask.ScheduledStart.Value.Date));

                        if (workEffortPartsStandard == null)
                        {
                            @this.WorkEffortStandardQuantity = 0;
                            @this.ExtraQuantity = quantityUsed;
                            @this.DerivedBillableQuantity = quantityUsed;
                        }
                        else
                        {
                            @this.WorkEffortStandardQuantity = workEffortPartsStandard.Quantity;
                            var quantity = quantityUsed - workEffortPartsStandard.Quantity;

                            if (quantity <= 0)
                            {
                                @this.ExtraQuantity = 0;
                                @this.DerivedBillableQuantity = 0;
                            }
                            else
                            {
                                @this.ExtraQuantity = quantity;
                                @this.DerivedBillableQuantity = quantity;
                            }
                        }
                    }
                    else
                    {
                        @this.DerivedBillableQuantity = @this.AssignedBillableQuantity ?? @this.Quantity;
                    }
                }
            }
        }
    }
}
