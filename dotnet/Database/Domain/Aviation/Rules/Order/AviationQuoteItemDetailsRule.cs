// <copyright file="CustomQuoteItemDetailsDerivation.cs" company="Allors bvba">
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
    public class AviationQuoteItemDetailsRule : Rule
    {
        public AviationQuoteItemDetailsRule(MetaPopulation m) : base(m, new Guid("06714004-c671-4357-987c-1de04b72f9c1")) =>
            this.Patterns = new Pattern[]
            {
                m.QuoteItem.RolePattern(v => v.SerialisedItem),
                m.QuoteItem.RolePattern(v => v.Product),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var transaction = cycle.Transaction;

            foreach (var @this in matches.Cast<QuoteItem>())
            {
                if (!@this.ExistDetails
                    || (@this.ExistCurrentVersion
                        && @this.CurrentVersion.ExistSerialisedItem
                        && @this.SerialisedItem != @this.CurrentVersion.SerialisedItem)
                    || (@this.ExistCurrentVersion
                        && @this.CurrentVersion.ExistProduct
                        && @this.Product != @this.CurrentVersion.Product))
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

                        if (@this.SerialisedItem.ExistManufacturingYear)
                        {
                            builder.Append($", YOM: {@this.SerialisedItem.ManufacturingYear}");
                        }

                        var details = builder.ToString();

                        if (details.StartsWith(","))
                        {
                            details = details.Substring(2);
                        }

                        @this.Details = details;

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

                        var details = builder.ToString();

                        if (details.StartsWith(","))
                        {
                            details = details.Substring(2);
                        }

                        @this.Details = details;
                    }
                }
            }
        }
    }
}
