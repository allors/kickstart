// <copyright file="SerialisedItemCustomDerivation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using Allors.Database.Meta;
using Allors.Database.Derivations;
using Resources;
using Allors.Database.Domain.Derivations.Rules;

namespace Allors.Database.Domain
{
    public class AviationSerialisedItemRule : Rule
    {
        public AviationSerialisedItemRule(MetaPopulation m) : base(m, new Guid("200930d7-8e8a-49c2-b255-8c1fd59e462f")) =>
            this.Patterns = new Pattern[]
            {
                m.SerialisedItem.RolePattern(v => v.SerialisedItemAvailability),
                m.SerialisedItem.RolePattern(v => v.Ownership),
                m.SerialisedInventoryItem.RolePattern(v => v.Quantity, v => v.SerialisedItem),
                m.Facility.RolePattern(v => v.Name, v => v.InventoryItemsWhereFacility.InventoryItem.AsSerialisedInventoryItem.SerialisedItem),
                m.UnifiedGood.RolePattern(v => v.IataGseCode, v => v.SerialisedItems),
                m.IataGseCode.RolePattern(v => v.Code, v => v.UnifiedGoodsWhereIataGseCode.UnifiedGood.SerialisedItems),
                m.SalesInvoiceItem.RolePattern(v => v.TotalExVat, v => v.SerialisedItem),
                m.SerialisedItem.AssociationPattern(v => v.SerialisedInventoryItemsWhereSerialisedItem),
                m.SerialisedItem.AssociationPattern(v => v.SalesInvoiceItemsWhereSerialisedItem),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SerialisedItem>())
            {
                @this.Location = @this.ExistSerialisedInventoryItemsWhereSerialisedItem ? @this.SerialisedInventoryItemsWhereSerialisedItem.FirstOrDefault(v => v.QuantityOnHand == 1)?.Facility?.Name : string.Empty;
                @this.IataCode = (@this.PartWhereSerialisedItem as UnifiedGood)?.IataGseCode?.Code;

                var invoiceItem = @this.SalesInvoiceItemsWhereSerialisedItem.OrderByDescending(v => v.SalesInvoiceWhereSalesInvoiceItem.InvoiceDate).FirstOrDefault();
                if (invoiceItem != null && @this.ExistSerialisedItemAvailability && @this.SerialisedItemAvailability.IsSold && @this.Ownership.IsThirdParty)
                {
                    @this.SellingPrice = invoiceItem.TotalExVat;
                }
            }
        }
    }
}
