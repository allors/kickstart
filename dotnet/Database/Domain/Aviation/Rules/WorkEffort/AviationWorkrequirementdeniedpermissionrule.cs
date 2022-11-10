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

    public class AviationWorkRequirementDeniedPermissionRule : Rule
    {
        public AviationWorkRequirementDeniedPermissionRule(MetaPopulation m) : base(m, new Guid("e1990336-3d5b-437b-b110-896aad4ed60b")) =>
            this.Patterns = new Pattern[]
        {
            m.WorkRequirement.RolePattern(v => v.TransitionalRevocations),
        };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<WorkRequirement>())
            {
                @this.Revocations = @this.TransitionalRevocations;
            }
        }
    }
}
