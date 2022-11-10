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

    public class FacilityRule : Rule
    {
        public FacilityRule(MetaPopulation m) : base(m, new Guid("de959536-0814-4053-8e60-3772c10ac73e")) =>
            this.Patterns = new Pattern[]
            {
                m.Facility.RolePattern(v => v.Owner),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<Facility>())
            {
                @this.DeriveFacility(validation);
            }
        }
    }

    public static class FacilityRuleExtensions
    {
        public static void DeriveFacility(this Facility @this, IValidation validation)
        {
            @this.Owner.DerivationTrigger = Guid.NewGuid();
        }
    }
}
