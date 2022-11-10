// <copyright file="WorkEffortTotalRevenueCustomDerivation.cs" company="Allors bvba">
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
    public class AviationWorkEffortTotalRevenueRule : Rule
    {
        public AviationWorkEffortTotalRevenueRule(MetaPopulation m) : base(m, new Guid("f93faf40-78da-4e98-9b32-13e186e88417")) =>
            this.Patterns = new Pattern[]
            {
                m.WorkTask.RolePattern(v => v.MaintenanceAgreement),
                m.TimeEntry.RolePattern(v => v.WorkEffort, v => v.WorkEffort, m.WorkTask),
                m.TimeEntry.RolePattern(v => v.BillingAmount, v => v.WorkEffort, m.WorkTask),
                m.TimeEntry.RolePattern(v => v.IsBillable, v => v.WorkEffort, m.WorkTask),
                m.TimeEntry.RolePattern(v => v.AmountOfTime, v => v.WorkEffort, m.WorkTask),
                m.TimeEntry.RolePattern(v => v.BillableAmountOfTime, v => v.WorkEffort, m.WorkTask),
                m.WorkEffortType.RolePattern(v => v.StandardWorkHours, v => v.MaintenanceAgreementsWhereWorkEffortType.MaintenanceAgreement.WorkTasksWhereMaintenanceAgreement),
                m.MaintenanceAgreement.RolePattern(v => v.WorkEffortType, v => v.WorkTasksWhereMaintenanceAgreement),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<WorkTask>())
            {
                if (@this.ExistMaintenanceAgreement && @this.MaintenanceAgreement.ExistWorkEffortType)
                {
                    var frequencies = new TimeFrequencies(@this.Strategy.Transaction);

                    var totalBillableAmountOfTimeInMinutes = @this.BillableTimeEntries().Sum(v => v.BillableAmountOfTimeInMinutes);
                    var totalBillableAmountOfTimeInHours = Math.Round((decimal)frequencies.Minute.ConvertToFrequency(totalBillableAmountOfTimeInMinutes, frequencies.Hour), 2);
                    @this.BillableAmountOfTimeInHours = (totalBillableAmountOfTimeInHours - @this.MaintenanceAgreement.WorkEffortType.StandardWorkHours) < 0 ?
                           0M : totalBillableAmountOfTimeInHours - @this.MaintenanceAgreement.WorkEffortType.StandardWorkHours;

                    @this.TotalLabourRevenue = Math.Round((decimal)(@this.MaintenanceAgreement.HourlyRate * @this.BillableAmountOfTimeInHours), 2);
                }
                else
                {
                    @this.BillableAmountOfTimeInHours = 0;
                    @this.TotalLabourRevenue = Math.Round(@this.BillableTimeEntries().Sum(v => v.BillingAmount), 2);
                }
            }
        }
    }
}
