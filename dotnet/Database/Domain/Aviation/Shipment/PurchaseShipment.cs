// <copyright file="PurchaseShipment.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Linq;

    public partial class PurchaseShipment
    {
        public void AviationReceive(PurchaseShipmentReceive method)
        {
            this.ShipmentState = new ShipmentStates(this.Strategy.Transaction).Received;
            this.EstimatedArrivalDate = this.Transaction().Now().Date;

            foreach (var shipmentItem in this.ShipmentItems)
            {
                shipmentItem.ShipmentItemState = new ShipmentItemStates(this.Transaction()).Received;

                if (!shipmentItem.ExistShipmentReceiptWhereShipmentItem)
                {
                    if (!shipmentItem.ExistOrderShipmentsWhereShipmentItem)
                    {
                        new ShipmentReceiptBuilder(this.Transaction())
                            .WithQuantityAccepted(shipmentItem.Quantity)
                            .WithShipmentItem(shipmentItem)
                            .WithFacility(shipmentItem.StoredInFacility)
                            .Build();
                    }
                    else
                    {
                        foreach (var orderShipment in shipmentItem.OrderShipmentsWhereShipmentItem)
                        {
                            new ShipmentReceiptBuilder(this.Transaction())
                                .WithQuantityAccepted(orderShipment.Quantity)
                                .WithOrderItem(orderShipment.OrderItem)
                                .WithShipmentItem(shipmentItem)
                                .WithFacility(shipmentItem.StoredInFacility)
                                .Build();
                        }
                    }
                }

                if (shipmentItem.Part.InventoryItemKind.IsSerialised)
                {
                    new InventoryItemTransactionBuilder(this.Transaction())
                        .WithPart(shipmentItem.Part)
                        .WithSerialisedItem(shipmentItem.SerialisedItem)
                        .WithUnitOfMeasure(shipmentItem.Part.UnitOfMeasure)
                        .WithFacility(shipmentItem.StoredInFacility)
                        .WithReason(new InventoryTransactionReasons(this.Strategy.Transaction).IncomingShipment)
                        .WithShipmentItem(shipmentItem)
                        .WithSerialisedInventoryItemState(new SerialisedInventoryItemStates(this.Transaction()).Good)
                        .WithQuantity(1)
                        .Build();

                    shipmentItem.SerialisedItem.SerialisedItemAvailability = new SerialisedItemAvailabilities(this.Transaction()).Available;
                    shipmentItem.SerialisedItem.AvailableForSale = true;

                    if ((this.ShipToParty as InternalOrganisation)?.SerialisedItemSoldOns.Contains(new SerialisedItemSoldOns(this.Transaction()).PurchaseshipmentReceive) == true)
                    {
                        shipmentItem.SerialisedItem.OwnedBy = this.ShipToParty;
                        shipmentItem.SerialisedItem.Ownership = new Ownerships(this.Transaction()).Own;
                    }
                }
                else
                {
                    new InventoryItemTransactionBuilder(this.Transaction())
                        .WithPart(shipmentItem.Part)
                        .WithUnitOfMeasure(shipmentItem.Part.UnitOfMeasure)
                        .WithFacility(shipmentItem.StoredInFacility)
                        .WithReason(new InventoryTransactionReasons(this.Strategy.Transaction).IncomingShipment)
                        .WithNonSerialisedInventoryItemState(new NonSerialisedInventoryItemStates(this.Transaction()).Good)
                        .WithShipmentItem(shipmentItem)
                        .WithQuantity(shipmentItem.Quantity)
                        .WithCost(shipmentItem.UnitPurchasePrice)
                        .Build();

                    foreach (var orderShipment in shipmentItem.OrderShipmentsWhereShipmentItem)
                    {
                        var purchaseOrderItem = (PurchaseOrderItem)orderShipment.OrderItem;
                        if (purchaseOrderItem.ExistWorkTask 
                            && (purchaseOrderItem.WorkTask.WorkEffortState.IsCreated || purchaseOrderItem.WorkTask.WorkEffortState.IsInProgress))
                        {
                            var inventoryAssignment = purchaseOrderItem.WorkTask.WorkEffortInventoryAssignmentsWhereAssignment.FirstOrDefault(v => v.InventoryItem.Part.Equals(shipmentItem.Part));
                            var inventoryItem = shipmentItem.Part.InventoryItemsWherePart.FirstOrDefault(v => v.Facility.Equals(shipmentItem.StoredInFacility));

                            if (inventoryItem != null)
                            {
                                if (inventoryAssignment == null)
                                {
                                    inventoryAssignment = new WorkEffortInventoryAssignmentBuilder(this.Strategy.Transaction)
                                                                .WithAssignment(purchaseOrderItem.WorkTask)
                                                                .WithInventoryItem(inventoryItem)
                                                                .WithQuantity(purchaseOrderItem.QuantityOrdered)
                                                                .Build();
                                }
                                else
                                {
                                    inventoryAssignment.Quantity += purchaseOrderItem.QuantityOrdered;
                                }
                            }
                        }
                    }
                }
            }

            method.StopPropagation = true;
        }
    }
}
