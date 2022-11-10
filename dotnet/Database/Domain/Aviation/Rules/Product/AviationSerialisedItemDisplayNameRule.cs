// <copyright file="CustomSerialisedItemNameRule.cs" company="Allors bvba">
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
    public class AviationSerialisedItemDisplayNameRule : Rule
    {
        public AviationSerialisedItemDisplayNameRule(MetaPopulation m) : base(m, new Guid("6928fec8-5598-48fc-9f0f-2a9c1b4b4900")) =>
            this.Patterns = new Pattern[]
            {
                m.SerialisedItem.RolePattern(v => v.ItemNumber),
                m.SerialisedItem.AssociationPattern(v => v.PartWhereSerialisedItem),
                m.Part.RolePattern(v => v.Name, v => v.SerialisedItems),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SerialisedItem>())
            {
                @this.DeriveAviationSerialisedDisplayName(validation);
            }
        }
    }

    public static class AviationSerialisedItemDisplayNameRuleExtensions
    {
        public static void DeriveAviationSerialisedDisplayName(this SerialisedItem @this, IValidation validation)
        {
            @this.DisplayName = $"{@this.ItemNumber} {@this.PartWhereSerialisedItem?.Name} SN: {@this.SerialNumber}";
        }
    }
}
