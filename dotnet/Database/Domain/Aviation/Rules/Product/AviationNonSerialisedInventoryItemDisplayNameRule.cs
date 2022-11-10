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

    public class AviationNonSerialisedInventoryItemDisplayNameRule : Rule
    {
        public AviationNonSerialisedInventoryItemDisplayNameRule(MetaPopulation m) : base(m, new Guid("68d96df9-0f1e-47b1-b82e-7767c4f4a7bf")) =>
            this.Patterns = new Pattern[]
            {
                m.NonSerialisedInventoryItem.RolePattern(v => v.Part),
                m.NonSerialisedInventoryItem.RolePattern(v => v.Facility),
                m.NonSerialisedInventoryItem.RolePattern(v => v.QuantityOnHand),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<NonSerialisedInventoryItem>())
            {
                @this.DeriveAviationNonSerialisedInventoryItemDisplayName(validation);
            }
        }
    }

    public static class AviationNonSerialisedInventoryItemDisplayNameRuleExtensions
    {
        public static void DeriveAviationNonSerialisedInventoryItemDisplayName(this NonSerialisedInventoryItem @this, IValidation validation) => @this.DisplayName = $"{@this.Part?.Name} at {@this.Facility?.Name} with state {@this.NonSerialisedInventoryItemState?.Name}";
    }
}
