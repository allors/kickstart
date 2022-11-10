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
    using Derivations;
    using Meta;
    using Derivations.Rules;
    using Resources;

    public class AviationCustomSerialisedItemRule : Rule
    {
        public AviationCustomSerialisedItemRule(MetaPopulation m) : base(m, new Guid("fd361bf3-42cc-4efe-bc8c-b9b6e89cb840")) =>
            this.Patterns = new Pattern[]
            {
                m.SerialisedItem.RolePattern(v => v.AcquiredDate),
                m.SerialisedItem.RolePattern(v => v.AcquisitionYear),
                m.SerialisedItem.RolePattern(v => v.SerialNumber),
                m.SerialisedItem.RolePattern(v => v.PurchaseOrder),
                m.SerialisedItem.RolePattern(v => v.SerialisedItemAvailability),
                m.Part.RolePattern(v => v.SerialisedItemCharacteristics, v => v.SerialisedItems),
                m.Part.RolePattern(v => v.ProductType, v => v.SerialisedItems),
                m.ProductType.RolePattern(v => v.SerialisedItemCharacteristicTypes, v => v.PartsWhereProductType.Part.SerialisedItems),
                m.SerialisedItem.AssociationPattern(v => v.PartWhereSerialisedItem),
                m.Part.AssociationPattern(v => v.SupplierOfferingsWherePart, v => v.SerialisedItems),
                m.SerialisedItemCharacteristic.RolePattern(v => v.Value, v => v.PartWhereSerialisedItemCharacteristic.Part.SerialisedItems.SerialisedItem),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SerialisedItem>())
            {
                validation.AssertExistsAtMostOne(@this, @this.Meta.AcquiredDate, @this.Meta.AcquisitionYear);

                var doubles = @this.PartWhereSerialisedItem?.SerialisedItems.Where(v =>
                    !string.IsNullOrEmpty(v.SerialNumber)
                    && v.SerialNumber != "."
                    && v.SerialNumber.Equals(@this.SerialNumber)).ToArray();
                if (doubles?.Length > 1)
                {
                    validation.AddError(@this, @this.Meta.SerialNumber, ErrorMessages.SameSerialNumber);
                }

                var characteristicsToDelete = @this.SerialisedItemCharacteristics.ToList();
                var part = @this.PartWhereSerialisedItem;

                if (@this.ExistPartWhereSerialisedItem && part.ExistProductType)
                {
                    foreach (var characteristicType in part.ProductType.SerialisedItemCharacteristicTypes)
                    {
                        var characteristic = @this.SerialisedItemCharacteristics.FirstOrDefault(v2 => Equals(v2.SerialisedItemCharacteristicType, characteristicType));
                        var fromPart = part.SerialisedItemCharacteristics.FirstOrDefault(v => Equals(characteristicType, v.SerialisedItemCharacteristicType));

                        // Operating hours are calculated using operatinghourstransactions
                        if (characteristic == null)
                        {
                            var newCharacteristic = new SerialisedItemCharacteristicBuilder(@this.Strategy.Transaction)
                                .WithSerialisedItemCharacteristicType(characteristicType).Build();
                            @this.AddSerialisedItemCharacteristic(newCharacteristic);

                            if (fromPart != null && !characteristicType.IsOperatingHours)
                            {
                                newCharacteristic.Value = fromPart.Value;
                            }
                        }
                        else
                        {
                            if (fromPart != null && !characteristicType.IsOperatingHours)
                            {
                                characteristic.Value = fromPart.Value;
                            }

                            characteristicsToDelete.Remove(characteristic);
                        }
                    }
                }

                foreach (var characteristic in characteristicsToDelete)
                {
                    @this.RemoveSerialisedItemCharacteristic(characteristic);
                }
            }
        }
    }
}
