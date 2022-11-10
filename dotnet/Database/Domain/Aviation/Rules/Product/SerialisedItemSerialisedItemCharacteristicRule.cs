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

    public class SerialisedItemSerialisedItemCharacteristicRule : Rule
    {
        public SerialisedItemSerialisedItemCharacteristicRule(MetaPopulation m) : base(m, new Guid("304d280f-ecae-4453-8bce-0a251e4e27e3")) =>
            this.Patterns = new Pattern[]
            {
                m.SerialisedItem.RolePattern(v => v.SerialisedItemCharacteristics),
                m.SerialisedItemCharacteristic.RolePattern(v => v.Value, v => v.SerialisedItemWhereSerialisedItemCharacteristic.SerialisedItem),
                m.SerialisedItemCharacteristic.RolePattern(v => v.Value, v => v.PartWhereSerialisedItemCharacteristic.Part.SerialisedItems.SerialisedItem),
                m.Part.RolePattern(v => v.SerialisedItemCharacteristics, v => v.SerialisedItems.SerialisedItem),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SerialisedItem>())
            {
                @this.DeriveSerialisedItemSerialisedItemCharacteristic(validation);
            }
        }
    }

    public static class SerialisedItemSerialisedItemCharacteristicRuleExtensions
    {
        public static void DeriveSerialisedItemSerialisedItemCharacteristic(this SerialisedItem @this, IValidation validation)
        {
            @this.ChassisNumber = @this.SerialisedItemCharacteristics?.FirstOrDefault(v => v.SerialisedItemCharacteristicType.IsChassisNumber)?.Value;
            @this.EngineBrand = @this.SerialisedItemCharacteristics?.FirstOrDefault(v => v.SerialisedItemCharacteristicType.IsEngineBrand)?.Value;
            @this.EngineModel = @this.SerialisedItemCharacteristics?.FirstOrDefault(v => v.SerialisedItemCharacteristicType.IsEngineModel)?.Value;
            @this.EngineSerialNumber = @this.SerialisedItemCharacteristics?.FirstOrDefault(v => v.SerialisedItemCharacteristicType.IsEngineSerialNumber)?.Value;
            @this.OperatingHours = @this.SerialisedItemCharacteristics?.FirstOrDefault(v => v.SerialisedItemCharacteristicType.IsOperatingHours)?.Value;
            @this.Length = @this.SerialisedItemCharacteristics?.FirstOrDefault(v => v.SerialisedItemCharacteristicType.IsLength)?.Value;
            @this.Width = @this.SerialisedItemCharacteristics?.FirstOrDefault(v => v.SerialisedItemCharacteristicType.IsWidth)?.Value;
            @this.Height = @this.SerialisedItemCharacteristics?.FirstOrDefault(v => v.SerialisedItemCharacteristicType.IsHeight)?.Value;
            @this.Weight = @this.SerialisedItemCharacteristics?.FirstOrDefault(v => v.SerialisedItemCharacteristicType.IsWeight)?.Value;
        }
    }
}
