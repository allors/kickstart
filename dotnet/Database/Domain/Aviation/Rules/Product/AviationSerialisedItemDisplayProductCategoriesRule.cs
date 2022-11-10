// <copyright file="CustomSerialisedItemDisplayProductCategoriesDerivation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;
    using Database.Derivations;
    using Allors.Database.Domain.Derivations.Rules;

    public class AviationSerialisedItemDisplayProductCategoriesRule : Rule
    {
        public AviationSerialisedItemDisplayProductCategoriesRule(MetaPopulation m) : base(m, new Guid("44f31fdc-efb9-4418-9ec1-bf76129ee863")) =>
            this.Patterns = new Pattern[]
            {
                m.Product.AssociationPattern(v => v.ProductCategoriesWhereAllProduct, v => v.AsUnifiedGood.SerialisedItems, m.Part),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SerialisedItem>())
            {
                if (@this.ExistPartWhereSerialisedItem && @this.PartWhereSerialisedItem.GetType().Name == typeof(UnifiedGood).Name)
                {
                    var unifiedGood = @this.PartWhereSerialisedItem as UnifiedGood;
                    var categories = string.Join(", ", unifiedGood.ProductCategoriesWhereProduct.Select(v => v.DisplayName));

                    if (!string.IsNullOrEmpty(categories))
                    {
                        @this.ProductCategoriesDisplayName = categories.Substring(categories.IndexOf('/') + 1);
                    }
                }
            }
        }
    }
}
