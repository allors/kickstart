// <copyright file="TreeCache.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Configuration
{
    using System.Collections.Generic;
    using Domain;
    using Meta;

    public class WorkspaceMask : IWorkspaceMask
    {
        private readonly Dictionary<IClass, IRoleType> masks;

        public WorkspaceMask(MetaPopulation m) =>
            this.masks = new Dictionary<IClass, IRoleType>
            {
                { m.CustomerRelationship, m.CustomerRelationship.FromDate },
                { m.CustomerReturn, m.CustomerReturn.ShipmentNumber},
                { m.CustomerShipment, m.CustomerShipment.ShipmentNumber},
                { m.DropShipment, m.Transfer.ShipmentNumber},
                { m.Facility, m.Facility.Name},
                { m.InventoryItemTransaction, m.InventoryItemTransaction.TransactionDate},
                { m.NonSerialisedInventoryItem, m.NonSerialisedInventoryItem.QuantityOnHand},
                { m.NonUnifiedPart, m.NonUnifiedPart.Name},
                { m.OperatingHoursTransaction, m.OperatingHoursTransaction.Value },
                { m.Organisation, m.Organisation.Name },
                { m.Person, m.Person.LastName },
                { m.ProductQuote, m.ProductQuote.QuoteNumber },
                { m.PurchaseInvoice, m.PurchaseInvoice.Description },
                { m.PurchaseInvoiceItem, m.PurchaseInvoiceItem.Description },
                { m.PurchaseOrder, m.PurchaseOrder.Description },
                { m.PurchaseOrderItem, m.PurchaseOrderItem.Description },
                { m.PurchaseReturn, m.PurchaseReturn.ShipmentNumber},
                { m.PurchaseShipment, m.PurchaseShipment.ShipmentNumber},
                { m.QuoteItem, m.QuoteItem.Comment },
                { m.RequestForInformation, m.RequestForInformation.RequestNumber },
                { m.RequestForProposal, m.RequestForProposal.RequestNumber },
                { m.RequestForQuote, m.RequestForQuote.RequestNumber },
                { m.RequestItem, m.RequestItem.Description },
                { m.SalesInvoice, m.SalesInvoice.Description },
                { m.SalesInvoiceItem, m.SalesInvoiceItem.Description },
                { m.SalesOrder, m.SalesOrder.Description },
                { m.SalesOrderItem, m.SalesOrderItem.Description },
                { m.SerialisedInventoryItem, m.SerialisedInventoryItem.DisplayName},
                { m.SerialisedItem, m.SerialisedItem.DisplayName },
                { m.SubContractorRelationship, m.SubContractorRelationship.FromDate },
                { m.SupplierOffering, m.SupplierOffering.Supplier },
                { m.SupplierRelationship, m.SupplierRelationship.FromDate },
                { m.TimeSheet, m.TimeSheet.Worker},
                { m.TimeEntry, m.TimeEntry.WorkEffort},
                { m.Transfer, m.Transfer.ShipmentNumber},
                { m.WorkEffortFixedAssetAssignment, m.WorkEffortFixedAssetAssignment.FixedAsset},
                { m.WorkEffortInventoryAssignment, m.WorkEffortInventoryAssignment.Quantity},
                { m.WorkRequirement, m.WorkRequirement.RequirementNumber},
                { m.WorkRequirementFulfillment, m.WorkRequirementFulfillment.FixedAsset},
                { m.WorkTask, m.WorkTask.WorkEffortNumber},
            };

        public IDictionary<IClass, IRoleType> GetMasks(string workspaceName) => this.masks;
    }
}
