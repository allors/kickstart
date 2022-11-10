// <copyright file="UnifiedGoodAssumedMonthlyOperatingHoursDerivation.cs" company="Allors bvba">
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
    public class UnifiedGoodDerivedAssumedMonthlyOperatingHoursRule : Rule
    {
        public UnifiedGoodDerivedAssumedMonthlyOperatingHoursRule(MetaPopulation m) : base(m, new Guid("158126b6-2ce5-4ce2-acc7-3d2d13ddeb21")) =>
            this.Patterns = new Pattern[]
            {
                m.UnifiedGood.RolePattern(v => v.AssignedAssumedMonthlyOperatingHours),
                m.ProductType.RolePattern(v => v.AssumedMonthlyOperatingHours, v => v.PartsWhereProductType),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<UnifiedGood>())
            {
                @this.DerivedAssumedMonthlyOperatingHours = @this.AssignedAssumedMonthlyOperatingHours ?? @this.ProductType?.AssumedMonthlyOperatingHours ;
            }
        }
    }
}
