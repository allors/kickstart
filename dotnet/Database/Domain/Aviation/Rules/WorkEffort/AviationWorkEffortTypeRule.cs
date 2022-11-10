// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database.Derivations;
    using Meta;
    using Derivations.Rules;

    public class AviationWorkEffortTypeRule : Rule
    {
        public AviationWorkEffortTypeRule(MetaPopulation m) : base(m, new Guid("810b2f1e-f2d3-4bec-a760-377ebf30642a")) =>
            this.Patterns = new Pattern[]
        {
            m.WorkEffortType.RolePattern(v => v.UnifiedGood),
            m.WorkEffortType.RolePattern(v => v.ProductCategory),
            m.WorkEffortType.RolePattern(v => v.WorkEffortPartStandards),
            m.WorkEffortPartStandard.RolePattern(v => v.FromDate, v => v.WorkEffortTypeWhereWorkEffortPartStandard),
            m.WorkEffortPartStandard.RolePattern(v => v.ThroughDate, v => v.WorkEffortTypeWhereWorkEffortPartStandard),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<WorkEffortType>())
            {
                @this.CurrentWorkEffortPartStandards = @this.WorkEffortPartStandards
                    .Where(v => v.FromDate <= @this.Transaction().Now() && (!v.ExistThroughDate || v.ThroughDate >= @this.Transaction().Now()))
                    .ToArray();

                @this.InactiveWorkEffortPartStandards = @this.WorkEffortPartStandards
                    .Except(@this.CurrentWorkEffortPartStandards)
                    .ToArray();

                validation.AssertAtLeastOne(@this, @this.M.WorkEffortType.UnifiedGood, @this.M.WorkEffortType.ProductCategory);
                validation.AssertExistsAtMostOne(@this, @this.M.WorkEffortType.UnifiedGood, @this.M.WorkEffortType.ProductCategory);
            }
        }
    }
}
