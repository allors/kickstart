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

    public class SerialisedItemSuppliedByCountryCodeRule : Rule
    {
        public SerialisedItemSuppliedByCountryCodeRule(MetaPopulation m) : base(m, new Guid("c245302c-8afb-420b-931f-5bd351b91684")) =>
            this.Patterns = new Pattern[]
            {
                m.SerialisedItem.RolePattern(v => v.SuppliedBy),
                m.Organisation.RolePattern(v => v.GeneralCorrespondence, v => v.SerialisedItemsWhereSuppliedBy. SerialisedItem),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SerialisedItem>())
            {
                @this.DeriveSerialisedItemSuppliedByCountryCode(validation);
            }
        }
    }

    public static class SerialisedItemSuppliedByCountryCodeRuleExtensions
    {
        public static void DeriveSerialisedItemSuppliedByCountryCode(this SerialisedItem @this, IValidation validation)
        {
            var address = @this.SuppliedBy?.GeneralCorrespondence as PostalAddress;
            @this.SuppliedByCountryCode = address?.Country?.IsoCode;
        }
    }
}
