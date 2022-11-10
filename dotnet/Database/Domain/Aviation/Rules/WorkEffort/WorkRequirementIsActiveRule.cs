// <copyright file="Domain.cs" company="Allors bvba">
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
    public class WorkRequirementIsActiveRule : Rule
    {
        public WorkRequirementIsActiveRule(MetaPopulation m) : base(m, new Guid("08b1b4a4-9734-4d08-b0af-6242f08caa73")) =>
            this.Patterns = new Pattern[]
            {
                m.WorkRequirement.RolePattern(v => v.RequirementState),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<WorkRequirement>())
            {
                @this.DeriveWorkRequirementIsActive(validation);
            }
        }
    }

    public static class WorkRequirementIsActiveRuleExtensions
    {
        public static void DeriveWorkRequirementIsActive(this WorkRequirement @this, IValidation validation)
        {
            @this.IsActive = @this.RequirementState.IsCreated || @this.RequirementState.IsInProgress;
        }
    }
}
