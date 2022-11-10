// <copyright file="WorkEffortGrandTotalDerivation.cs" company="Allors bvba">
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
    public class AviationWorkEffortGrandTotalRule : Rule
    {
        public AviationWorkEffortGrandTotalRule(MetaPopulation m) : base(m, new Guid("dd37afcf-6c80-4a46-bc59-3b361955769b")) =>
            this.Patterns = new Pattern[]
            {
                m.WorkTask.RolePattern(v => v.TotalLabourRevenue),
                m.WorkTask.RolePattern(v => v.TotalMaterialRevenue),
                m.WorkTask.RolePattern(v => v.TotalSubContractedRevenue),
                m.WorkTask.RolePattern(v => v.TotalOtherRevenue),
                m.WorkTask.RolePattern(v => v.MaintenanceAgreement),
                m.MaintenanceAgreement.RolePattern(v => v.FixedPrice, v => v.WorkTasksWhereMaintenanceAgreement),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<WorkTask>())
            {
                @this.DeriveAviationWorkEffortGrandTotal(validation);
            }
        }
    }

    public static class AviationWorkEffortGrandTotalRuleExtensions
    {
        public static void DeriveAviationWorkEffortGrandTotal(this WorkTask @this, IValidation validation)
        {
            var fixedPrice = @this.ExistMaintenanceAgreement ? @this.MaintenanceAgreement.FixedPrice : 0;

            @this.GrandTotal = fixedPrice > 0
                ? Rounder.RoundDecimal(fixedPrice, 2)
                : Rounder.RoundDecimal(@this.TotalLabourRevenue + @this.TotalMaterialRevenue + @this.TotalSubContractedRevenue + @this.TotalOtherRevenue, 2);
        }
    }
}
