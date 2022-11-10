// <copyright file="PersonLocalCreatorsRule.cs" company="Allors bvba">
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
    public class CreatorsMemberRule : Rule
    {
        public CreatorsMemberRule(MetaPopulation m) : base(m, new Guid("e45893d4-9129-4121-9345-db4fbf42f1fb")) =>
            this.Patterns = new Pattern[]
            {
                m.Person.AssociationPattern(v => v.InternalOrganisationsWhereLocalEmployee),
                m.Person.AssociationPattern(v => v.UserGroupsWhereMember),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<Person>())
            {
                @this.DeriveCreatorsMember(validation);
            }
        }
    }

    public static class CreatorsMemberRuleExtensions
    {
        public static void DeriveCreatorsMember(this Person @this, IValidation validation)
        {
            var otherUsergroups = @this.UserGroupsWhereMember
                .Where(v => v.IsSelectable == true)
                .Except(new List<UserGroup>() { new UserGroups(@this.Strategy.Transaction).Creators, 
                                                new UserGroups(@this.Strategy.Transaction).Employees })
                .ToArray();

            if (@this.ExistInternalOrganisationsWhereLocalEmployee || otherUsergroups.Count() > 0)
            {
                new UserGroups(@this.Strategy.Transaction).Creators.AddMember(@this);
            }
            else
            {
                new UserGroups(@this.Strategy.Transaction).Creators.RemoveMember(@this);
            }
        }
    }
}
