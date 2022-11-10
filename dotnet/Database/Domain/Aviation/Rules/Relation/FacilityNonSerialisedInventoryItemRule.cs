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

    public class FacilityNonSerialisedInventoryItemRule : Rule
    {
        public FacilityNonSerialisedInventoryItemRule(MetaPopulation m) : base(m, new Guid("b37e21b6-ac3a-4bce-9612-20ec9f34b394")) =>
            this.Patterns = new Pattern[]
            {
                m.Facility.RolePattern(v => v.FacilityType),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<Facility>())
            {
                @this.DeriveFacilityNonSerialisedInventoryItem(validation);
            }
        }
    }

    public static class FacilityNonSerialisedInventoryItemRuleExtensions
    {
        public static void DeriveFacilityNonSerialisedInventoryItem(this Facility @this, IValidation validation)
        {
            if (@this.FacilityType.IsWorkshop)
            {
                foreach (NonSerialisedInventoryItem inventoryItem in @this.Strategy.Transaction.Extent<NonSerialisedInventoryItem>().ToArray())
                {
                    if (@this.ExistWorkshopWarehouse && @this.WorkshopWarehouse.Equals(inventoryItem.Facility))
                    {
                        inventoryItem.DeriveNonSerialisedInventoryItemSecurityToken(validation);
                    }
                }
            }
        }
    }
}
