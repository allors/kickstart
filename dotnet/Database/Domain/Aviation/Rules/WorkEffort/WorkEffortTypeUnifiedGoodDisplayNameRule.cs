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
    public class WorkEffortTypeUnifiedGoodDisplayNameRule : Rule
    {
        public WorkEffortTypeUnifiedGoodDisplayNameRule(MetaPopulation m) : base(m, new Guid("523e00fe-212b-4695-b5ca-56cea42220df")) =>
            this.Patterns = new Pattern[]
            {
                m.WorkEffortType.RolePattern(v => v.UnifiedGood),
                m.UnifiedGood.RolePattern(v => v.DisplayName, v => v.WorkEffortTypesWhereUnifiedGood),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<WorkEffortType>())
            {
                @this.DeriveWorkEffortTypeUnifiedGoodDisplayName(validation);
            }
        }
    }

    public static class WorkEffortTypeUnifiedGoodDisplayNameRuleExtensions
    {
        public static void DeriveWorkEffortTypeUnifiedGoodDisplayName(this WorkEffortType @this, IValidation validation)
        {
            @this.UnifiedGoodDisplayName = @this.UnifiedGood?.DisplayName;
        }
    }
}
