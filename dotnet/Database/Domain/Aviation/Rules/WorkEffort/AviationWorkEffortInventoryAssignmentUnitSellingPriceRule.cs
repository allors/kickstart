// <copyright file="CustomWorkEffortInventoryAssignmentUnitSellingPriceDerivation.cs" company="Allors bvba">
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

    public class AviationWorkEffortInventoryAssignmentUnitSellingPriceRule : Rule
    {
        public AviationWorkEffortInventoryAssignmentUnitSellingPriceRule(MetaPopulation m) : base(m, new Guid("31b290a2-b536-4ff2-9517-4120ec4e902e")) =>
            this.Patterns = new Pattern[]
        {
            m.WorkEffortInventoryAssignment.RolePattern(v => v.Assignment),
            m.WorkEffortInventoryAssignment.RolePattern(v => v.Quantity),
            m.WorkEffortInventoryAssignment.RolePattern(v => v.AssignedUnitSellingPrice),
            m.WorkEffortInventoryAssignment.RolePattern(v => v.UnitPurchasePrice),
            m.WorkTask.RolePattern(v => v.MaintenanceAgreement, v => v.WorkEffortInventoryAssignmentsWhereAssignment),
            m.WorkTask.RolePattern(v => v.WorkEffortState, v => v.WorkEffortInventoryAssignmentsWhereAssignment),
            m.WorkTask.RolePattern(v => v.ScheduledStart, v => v.WorkEffortInventoryAssignmentsWhereAssignment),
            m.WorkTask.RolePattern(v => v.Customer, v => v.WorkEffortInventoryAssignmentsWhereAssignment),
            m.MaintenanceAgreement.RolePattern(v => v.WorkEffortType, v => v.WorkTasksWhereMaintenanceAgreement.WorkTask.WorkEffortInventoryAssignmentsWhereAssignment),
            m.MaintenanceAgreement.RolePattern(v => v.PartSurchargePercentage, v => v.WorkTasksWhereMaintenanceAgreement.WorkTask.WorkEffortInventoryAssignmentsWhereAssignment),
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
                        var workEffortPartsStandard = workTask.MaintenanceAgreement.WorkEffortType?.WorkEffortPartStandards?
                            .FirstOrDefault(v => v.Part.Equals(@this.InventoryItem.Part)
                                                                && v.FromDate <= workTask.ScheduledStart.Value.Date
                                                                && (!v.ExistThroughDate || v.ThroughDate >= workTask.ScheduledStart.Value.Date));

                        if (workEffortPartsStandard == null)
                        {
                            @this.UnitSellingPrice = @this.AssignedUnitSellingPrice.HasValue ?
                                @this.AssignedUnitSellingPrice.Value
                                : Math.Round(@this.UnitPurchasePrice * (1 + workTask.MaintenanceAgreement.PartSurchargePercentage / 100), 2);
                            @this.WorkEffortStandardQuantity = 0;
                        }
                        else
                        {
                            @this.WorkEffortStandardQuantity = workEffortPartsStandard.Quantity;
                            var quantity = quantityUsed - workEffortPartsStandard.Quantity;

                            if (quantity <= 0)
                            {
                                @this.UnitSellingPrice = 0;
                            }
                            else
                            {
                                @this.UnitSellingPrice = @this.AssignedUnitSellingPrice.HasValue ?
                                    @this.AssignedUnitSellingPrice.Value
                                    : Math.Round(@this.UnitPurchasePrice * (1 + workTask.MaintenanceAgreement.PartSurchargePercentage / 100), 2);
                            }
                        }
                    }
                    else if (@this.AssignedUnitSellingPrice.HasValue)
                    {
                        @this.UnitSellingPrice = @this.AssignedUnitSellingPrice.Value;
                    }
                    else if (@this.ExistAssignment && @this.Assignment.Customer is Organisation organisation && organisation.IsInternalOrganisation)
                    {
                        @this.UnitSellingPrice = Math.Round(@this.UnitPurchasePrice * (1 + transaction.GetSingleton().Settings.InternalPartSurchargePercentage / 100), 2);
                    }
                    else
                    {
                        @this.UnitSellingPrice = Math.Round(@this.UnitPurchasePrice * (1 + transaction.GetSingleton().Settings.PartSurchargePercentage / 100), 2);
                    }
                }
            }
        }
    }
}
