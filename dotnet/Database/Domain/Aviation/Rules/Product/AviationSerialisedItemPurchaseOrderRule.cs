// <copyright file="CustomSerialisedItemPurchaseOrderDerivation.cs" company="Allors bvba">
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

    public class AviationSerialisedItemPurchaseOrderRule : Rule
    {
        public AviationSerialisedItemPurchaseOrderRule(MetaPopulation m) : base(m, new Guid("9df0d1ea-6b76-44f0-bce2-404977921ddd")) =>
            this.Patterns = new Pattern[]
            {
                m.PurchaseOrder.RolePattern(v => v.ValidOrderItems, v => v.PurchaseOrderItems.PurchaseOrderItem.SerialisedItem),
                m.PurchaseOrder.RolePattern(v => v.PurchaseOrderState, v => v.PurchaseOrderItems.PurchaseOrderItem.SerialisedItem),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SerialisedItem>())
            {
                @this.PurchaseOrder = @this.PurchaseOrderItemsWhereSerialisedItem
                    .Where(v => v.ExistOrderWhereValidOrderItem
                                && ((PurchaseOrder)v.OrderWhereValidOrderItem).TakenViaSupplier is Organisation organisation
                                && !organisation.IsInternalOrganisation
                                && (((PurchaseOrder)v.OrderWhereValidOrderItem).PurchaseOrderState.Equals(new PurchaseOrderStates(transaction).Sent)
                                    || ((PurchaseOrder)v.OrderWhereValidOrderItem).PurchaseOrderState.Equals(new PurchaseOrderStates(transaction).Completed)
                                    || ((PurchaseOrder)v.OrderWhereValidOrderItem).PurchaseOrderState.Equals(new PurchaseOrderStates(transaction).Finished))
                                && (v.InvoiceItemType.Equals(new InvoiceItemTypes(transaction).PartItem)
                                    || v.InvoiceItemType.Equals(new InvoiceItemTypes(transaction).ProductItem)
                                    || v.InvoiceItemType.Equals(new InvoiceItemTypes(transaction).GseUnmotorized)))?
                    .OrderBy(v => v.PurchaseOrderWherePurchaseOrderItem.OrderDate)
                    .LastOrDefault()?
                   .PurchaseOrderWherePurchaseOrderItem;
            }
        }
    }
}
