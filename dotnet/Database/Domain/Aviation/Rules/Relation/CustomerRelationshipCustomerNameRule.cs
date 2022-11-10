// <copyright file="CustomerRelationshipCustomerNameDerivation.cs" company="Allors bvba">
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
    public class CustomerRelationshipCustomerNameRule : Rule
    {
        public CustomerRelationshipCustomerNameRule(MetaPopulation m) : base(m, new Guid("8c29cc86-36c4-4621-9850-a16bd49cc4db")) =>
            this.Patterns = new Pattern[]
            {
                m.CustomerRelationship.RolePattern(v => v.Customer),
                m.Party.RolePattern(v => v.DisplayName, v => v.CustomerRelationshipsWhereCustomer),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<CustomerRelationship>())
            {
                @this.CustomerName = @this.Customer.DisplayName;
            }
        }
    }
}
