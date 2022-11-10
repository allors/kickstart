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
    using Resources;

    public class AviationQuoteItemRule : Rule
    {
        public AviationQuoteItemRule(MetaPopulation m) : base(m, new Guid("5d735d4d-0ad4-41b5-86c1-14a277a5fb79")) =>
            this.Patterns = new Pattern[]
            {
                m.QuoteItem.RolePattern(v => v.InvoiceItemType),
                m.QuoteItem.RolePattern(v => v.Product),
                m.QuoteItem.RolePattern(v => v.ProductFeature),
                m.QuoteItem.RolePattern(v => v.SparePartDescription),
                m.QuoteItem.RolePattern(v => v.Deliverable),
                m.QuoteItem.RolePattern(v => v.WorkEffort),
                m.QuoteItem.RolePattern(v => v.SerialisedItem),
                m.QuoteItem.RolePattern(v => v.Quantity),
                m.QuoteItem.RolePattern(v => v.RequestItem),
                m.QuoteItem.RolePattern(v => v.UnitOfMeasure),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var transaction = cycle.Transaction;

            foreach (var @this in matches.Cast<QuoteItem>())
            {
                var quote = @this.QuoteWhereQuoteItem;

                if (@this.ExistInvoiceItemType
                    && (@this.InvoiceItemType.IsProductFeatureItem 
                        || @this.InvoiceItemType.IsProductItem))
                {
                    validation.AssertAtLeastOne(@this, this.M.QuoteItem.Product, this.M.QuoteItem.ProductFeature, this.M.QuoteItem.SerialisedItem, this.M.QuoteItem.Deliverable, this.M.QuoteItem.WorkEffort);
                    validation.AssertExistsAtMostOne(@this, this.M.QuoteItem.Product, this.M.QuoteItem.ProductFeature, this.M.QuoteItem.Deliverable, this.M.QuoteItem.WorkEffort);
                    validation.AssertExistsAtMostOne(@this, this.M.QuoteItem.SerialisedItem, this.M.QuoteItem.ProductFeature, this.M.QuoteItem.Deliverable, this.M.QuoteItem.WorkEffort);
                }

                if (@this.ExistInvoiceItemType && @this.InvoiceItemType.IsPartItem)
                {
                    validation.AssertAtLeastOne(@this, this.M.QuoteItem.Product, this.M.QuoteItem.SparePartDescription);
                    validation.AssertExistsAtMostOne(@this, this.M.QuoteItem.Product, this.M.QuoteItem.SparePartDescription);
                }

                if (@this.ExistSerialisedItem && @this.Quantity != 1)
                {
                    validation.AddError(@this, @this.Meta.Quantity, ErrorMessages.SerializedItemQuantity);
                }

                if (@this.ExistRequestItem)
                {
                    @this.RequiredByDate = @this.RequestItem.RequiredByDate;
                }

                if (!@this.ExistUnitOfMeasure)
                {
                    @this.UnitOfMeasure = new UnitsOfMeasure(@this.Strategy.Transaction).Piece;
                }

                if (@this.QuoteWhereQuoteItem is ProductQuote productQuote
                    && @this.ExistProduct
                    && !productQuote.ProductQuoteItemsByProduct.Any(v => v.Product.Equals(@this.Product)))
                {
                    productQuote.AddProductQuoteItemsByProduct(new ProductQuoteItemByProductBuilder(transaction).WithProduct(@this.Product).Build());
                }
            }
        }
    }
}
