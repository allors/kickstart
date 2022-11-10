// <copyright file="CustomerShipment.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Linq;

namespace Allors.Database.Domain
{
    public partial class CustomerShipment
    {
        public void AviationPick(CustomerShipmentPick method)
        {
            this.AviationCreatePickList();

            foreach (ShipmentItem shipmentItem in this.ShipmentItems)
            {
                shipmentItem.ShipmentItemState = new ShipmentItemStates(this.strategy.Transaction).Picking;
            }

            this.ShipmentState = new ShipmentStates(this.Strategy.Transaction).Picking;

            method.StopPropagation = true;
        }

        // Differs from base in that inventory items are not filtered on Facility.Owner
        private void AviationCreatePickList()
        {
            if (this.ExistShipmentItems && this.ExistShipToParty)
            {
                var pickList = new PickListBuilder(this.Strategy.Transaction).WithShipToParty(this.ShipToParty).WithStore(this.Store).Build();

                foreach (var shipmentItem in this.ShipmentItems
                    .Where(v => v.ShipmentItemState.Equals(new ShipmentItemStates(this.strategy.Transaction).Created)
                                || v.ShipmentItemState.Equals(new ShipmentItemStates(this.strategy.Transaction).Picking)))
                {
                    var quantityIssued = 0M;
                    foreach (ItemIssuance itemIssuance in shipmentItem.ItemIssuancesWhereShipmentItem)
                    {
                        quantityIssued += itemIssuance.Quantity;
                    }

                    var quantityToIssue = shipmentItem.Quantity - quantityIssued;
                    if (quantityToIssue == 0)
                    {
                        return;
                    }

                    var unifiedGood = shipmentItem.Good as UnifiedGood;
                    var nonUnifiedGood = shipmentItem.Good as NonUnifiedGood;
                    var nonUnifiedPart = shipmentItem.Good as NonUnifiedPart;
                    var serialized = unifiedGood?.InventoryItemKind.Equals(new InventoryItemKinds(this.strategy.Transaction).Serialised);
                    var part = unifiedGood ?? nonUnifiedGood?.Part ?? nonUnifiedPart;

                    var inventoryItems = part.InventoryItemsWherePart;
                    SerialisedInventoryItem issuedFromSerializedInventoryItem = null;

                    foreach (InventoryItem inventoryItem in shipmentItem.ReservedFromInventoryItems)
                    {
                        // shipment item originates from sales order. Sales order item has only 1 ReservedFromInventoryItem.
                        // Foreach loop wil execute once.
                        var pickListItem = new PickListItemBuilder(this.Strategy.Transaction)
                            .WithInventoryItem(inventoryItem)
                            .WithQuantity(quantityToIssue)
                            .Build();

                        new ItemIssuanceBuilder(this.Strategy.Transaction)
                            .WithInventoryItem(pickListItem.InventoryItem)
                            .WithShipmentItem(shipmentItem)
                            .WithQuantity(pickListItem.Quantity)
                            .WithPickListItem(pickListItem)
                            .Build();

                        pickList.AddPickListItem(pickListItem);

                        if (serialized.HasValue && serialized.Value)
                        {
                            issuedFromSerializedInventoryItem = (SerialisedInventoryItem)inventoryItem;
                        }
                    }

                    // shipment item is not linked to sales order item
                    if (!shipmentItem.ExistReservedFromInventoryItems)
                    {
                        var quantityLeftToIssue = quantityToIssue;
                        foreach (InventoryItem inventoryItem in inventoryItems)
                        {
                            if (serialized.HasValue && serialized.Value && quantityLeftToIssue > 0)
                            {
                                var serializedInventoryItem = (SerialisedInventoryItem)inventoryItem;
                                if (serializedInventoryItem.AvailableToPromise == 1)
                                {
                                    var pickListItem = new PickListItemBuilder(this.Strategy.Transaction)
                                        .WithInventoryItem(inventoryItem)
                                        .WithQuantity(quantityLeftToIssue)
                                        .Build();

                                    new ItemIssuanceBuilder(this.Strategy.Transaction)
                                        .WithInventoryItem(inventoryItem)
                                        .WithShipmentItem(shipmentItem)
                                        .WithQuantity(pickListItem.Quantity)
                                        .WithPickListItem(pickListItem)
                                        .Build();

                                    pickList.AddPickListItem(pickListItem);
                                    quantityLeftToIssue = 0;
                                    issuedFromSerializedInventoryItem = serializedInventoryItem;
                                }
                            }
                            else if (quantityLeftToIssue > 0)
                            {
                                var nonSerializedInventoryItem = (NonSerialisedInventoryItem)inventoryItem;
                                var quantity = quantityLeftToIssue > nonSerializedInventoryItem.AvailableToPromise
                                    ? nonSerializedInventoryItem.AvailableToPromise
                                    : quantityLeftToIssue;

                                if (quantity > 0)
                                {
                                    var pickListItem = new PickListItemBuilder(this.Strategy.Transaction)
                                        .WithInventoryItem(inventoryItem)
                                        .WithQuantity(quantity)
                                        .Build();

                                    new ItemIssuanceBuilder(this.Strategy.Transaction)
                                        .WithInventoryItem(inventoryItem)
                                        .WithShipmentItem(shipmentItem)
                                        .WithQuantity(pickListItem.Quantity)
                                        .WithPickListItem(pickListItem)
                                        .Build();

                                    pickList.AddPickListItem(pickListItem);
                                    quantityLeftToIssue -= pickListItem.Quantity;
                                }
                            }
                        }
                    }
                }
            }
        }
    }
}
