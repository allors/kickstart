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
    public class LocalAdministratorsGlobalMemberRule : Rule
    {
        public LocalAdministratorsGlobalMemberRule(MetaPopulation m) : base(m, new Guid("ea47c995-5e5b-4424-92bd-b515c90e72f9")) =>
            this.Patterns = new Pattern[]
            {
                m.Person.AssociationPattern(v => v.InternalOrganisationsWhereLocalAdministrator),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<Person>())
            {
                @this.DeriveLocalAdministratorsGlobalMember(validation);
            }
        }
    }

    public static class LocalAdministratorsGlobalRuleExtensions
    {
        public static void DeriveLocalAdministratorsGlobalMember(this Person @this, IValidation validation)
        {
            if (@this.ExistInternalOrganisationsWhereLocalAdministrator)
            {
                new UserGroups(@this.Strategy.Transaction).LocalAdministratorsGlobal.AddMember(@this);
            }
            else
            {
                new UserGroups(@this.Strategy.Transaction).LocalAdministratorsGlobal.RemoveMember(@this);
            }
        }
    }
}
