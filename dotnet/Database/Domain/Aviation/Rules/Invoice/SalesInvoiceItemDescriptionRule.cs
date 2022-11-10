// <copyright file="SalesInvoiceItemDescriptionDerivation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using Allors.Database.Meta;
using Allors.Database.Derivations;
using System.Text;
using Allors.Database.Domain.Derivations.Rules;

namespace Allors.Database.Domain
{
    public class SalesInvoiceItemDescriptionRule : Rule
    {
        public SalesInvoiceItemDescriptionRule(MetaPopulation m) : base(m, new Guid("48b743be-d5a2-4d76-bf34-42d8b20165da")) =>
            this.Patterns = new Pattern[]
            {
                m.SalesInvoiceItem.RolePattern(v => v.SerialisedItem),
                m.SalesInvoiceItem.RolePattern(v => v.Product),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;
            var changeSet = cycle.ChangeSet;

            foreach (var @this in matches.Cast<SalesInvoiceItem>())
            {
                if (changeSet.Created.Contains(@this) && !@this.ExistDescription)
                {
                    if (@this.ExistSerialisedItem)
                    {
                        var builder = new StringBuilder();
                        var part = @this.SerialisedItem.PartWhereSerialisedItem;

                        if (part != null && part.ExistManufacturedBy)
                        {
                            builder.Append($", Manufacturer: {part.ManufacturedBy.DisplayName}");
                        }

                        if (part != null && part.ExistBrand)
                        {
                            builder.Append($", Brand: {part.Brand.Name}");
                        }

                        if (part != null && part.ExistModel)
                        {
                            builder.Append($", Model: {part.Model.Name}");
                        }

                        if (part != null && part.ExistHsCode)
                        {
                            builder.Append($", HS code: {part.HsCode}");
                        }

                        builder.Append($", SN: {@this.SerialisedItem.SerialNumber}");

                        if (@this.SerialisedItem.ExistManufacturingYear)
                        {
                            builder.Append($", YOM: {@this.SerialisedItem.ManufacturingYear}");
                        }

                        foreach (SerialisedItemCharacteristic characteristic in @this.SerialisedItem.SerialisedItemCharacteristics)
                        {
                            if (characteristic.ExistValue)
                            {
                                var characteristicType = characteristic.SerialisedItemCharacteristicType;
                                if (characteristicType.ExistUnitOfMeasure)
                                {
                                    var uom = characteristicType.UnitOfMeasure.ExistAbbreviation
                                                    ? characteristicType.UnitOfMeasure.Abbreviation
                                                    : characteristicType.UnitOfMeasure.Name;
                                    builder.Append(
                                        $", {characteristicType.Name}: {characteristic.Value} {uom}");
                                }
                                else
                                {
                                    builder.Append($", {characteristicType.Name}: {characteristic.Value}");
                                }
                            }
                        }

                        var details = builder.ToString();

                        if (details.StartsWith(","))
                        {
                            details = details.Substring(2);
                        }

                        @this.Description = details;

                    }
                    else if (@this.ExistProduct && @this.Product is UnifiedGood unifiedGood)
                    {
                        var builder = new StringBuilder();

                        if (unifiedGood != null && unifiedGood.ExistManufacturedBy)
                        {
                            builder.Append($", Manufacturer: {unifiedGood.ManufacturedBy.DisplayName}");
                        }

                        if (unifiedGood != null && unifiedGood.ExistBrand)
                        {
                            builder.Append($", Brand: {unifiedGood.Brand.Name}");
                        }

                        if (unifiedGood != null && unifiedGood.ExistModel)
                        {
                            builder.Append($", Model: {unifiedGood.Model.Name}");
                        }

                        foreach (SerialisedItemCharacteristic characteristic in unifiedGood.SerialisedItemCharacteristics)
                        {
                            if (characteristic.ExistValue)
                            {
                                var characteristicType = characteristic.SerialisedItemCharacteristicType;
                                if (characteristicType.ExistUnitOfMeasure)
                                {
                                    var uom = characteristicType.UnitOfMeasure.ExistAbbreviation
                                                    ? characteristicType.UnitOfMeasure.Abbreviation
                                                    : characteristicType.UnitOfMeasure.Name;
                                    builder.Append($", {characteristicType.Name}: {characteristic.Value} {uom}");
                                }
                                else
                                {
                                    builder.Append($", {characteristicType.Name}: {characteristic.Value}");
                                }
                            }
                        }

                        var details = builder.ToString();

                        if (details.StartsWith(","))
                        {
                            details = details.Substring(2);
                        }

                        @this.Description = details;
                    }
                }
            }
        }
    }
}
