// <copyright file="WorkRequirementFleetCodeRule.cs" company="Allors bvba">
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
    public class WorkRequirementFleetCodeRule : Rule
    {
        public WorkRequirementFleetCodeRule(MetaPopulation m) : base(m, new Guid("62843f27-3f32-4c42-a6e9-9df5bc42f1eb")) =>
            this.Patterns = new Pattern[]
            {
                m.WorkRequirement.RolePattern(v => v.FixedAsset),
                m.SerialisedItem.RolePattern(v => v.CustomerReferenceNumber, v => v.WorkRequirementsWhereFixedAsset),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<WorkRequirement>())
            {
                @this.FleetCode = ((SerialisedItem)@this.FixedAsset)?.CustomerReferenceNumber;
            }
        }
    }
}
