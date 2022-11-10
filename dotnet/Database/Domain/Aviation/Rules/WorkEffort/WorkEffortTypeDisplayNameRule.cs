// <copyright file="WorkEffortTypeDeniedPermissionDerivation.cs" company="Allors bvba">
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
    public class WorkEffortTypeDisplayNameRule : Rule
    {
        public WorkEffortTypeDisplayNameRule(MetaPopulation m) : base(m, new Guid("45c75e21-2592-485b-b16b-be9ec4256a7e")) =>
            this.Patterns = new Pattern[]
            {
                m.WorkEffortType.RolePattern(v => v.FromDate),
                m.WorkEffortType.RolePattern(v => v.ThroughDate),
                m.WorkEffortType.RolePattern(v => v.UnifiedGoodDisplayName),
                m.WorkEffortType.RolePattern(v => v.ProductCategoryDisplayName),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<WorkEffortType>())
            {
                @this.DeriveWorkEffortTypeDisplayName(validation);
            }
        }
    }

    public static class WorkEffortTypeDisplayNameRuleExtensions
    {
        public static void DeriveWorkEffortTypeDisplayName(this WorkEffortType @this, IValidation validation)
        {
            if (@this.ExistUnifiedGoodDisplayName)
            {
                @this.DisplayName = @this.ThroughDate.HasValue
                    ? $"{@this.Name} for {@this.UnifiedGoodDisplayName} valid from {@this.FromDate.Date::yyyy-MM-dd} through {@this.ThroughDate.Value.Date::yyyy-MM-dd}"
                    : $"{@this.Name} for {@this.UnifiedGoodDisplayName} valid from {@this.FromDate.Date::yyyy-MM-dd}";
            }

            if (@this.ExistProductCategoryDisplayName)
            {
                @this.DisplayName = @this.ThroughDate.HasValue
                    ? $"{@this.Name} for {@this.ProductCategoryDisplayName} valid from {@this.FromDate.Date::yyyy-MM-dd} through {@this.ThroughDate.Value.Date::yyyy-MM-dd}"
                    : $"{@this.Name} for {@this.ProductCategoryDisplayName} valid from {@this.FromDate.Date::yyyy-MM-dd}";
            }
        }
    }
}
