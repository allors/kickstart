// <copyright file="OrganisationCalculationRule.cs" company="Allors bvba">
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
    public class PersonIsUserRule : Rule
    {
        public PersonIsUserRule(MetaPopulation m) : base(m, new Guid("c519d963-f7ec-45e8-8ffc-aef94c8323ab")) =>
            this.Patterns = new Pattern[]
            {
                m.User.RolePattern(v => v.UserName),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<User>())
            {
                @this.IsUser = @this.ExistUserName;
            }
        }
    }
}
