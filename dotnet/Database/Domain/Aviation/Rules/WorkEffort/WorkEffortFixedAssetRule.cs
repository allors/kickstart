// <copyright file="WorkRequirementDeniedPermissionRule.cs" company="Allors bvba">
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

    public class WorkEffortFixedAssetRule : Rule
    {
        public WorkEffortFixedAssetRule(MetaPopulation m) : base(m, new Guid("cd900681-0665-4d94-b0d1-a5202c556239")) =>
            this.Patterns = new Pattern[]
        {
            m.WorkEffortFixedAssetAssignment.RolePattern(v => v.FixedAsset),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<WorkEffortFixedAssetAssignment>())
            {
                var validation = cycle.Validation;
                @this.DeriveWorkEffortFixedAsset(validation);
            }
        }
    }

    public static class WorkEffortFixedAssetRuleExtensions
    {
        public static void DeriveWorkEffortFixedAsset(this WorkEffortFixedAssetAssignment @this, IValidation validation)
        {
            var serialisedItem = @this.FixedAsset as SerialisedItem;
            if (serialisedItem != null)
            {
                serialisedItem.DerivationTrigger = Guid.NewGuid();
            }
        }
    }
}
