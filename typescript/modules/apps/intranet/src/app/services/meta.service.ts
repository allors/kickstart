import {
  MetaService,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import { M } from '@allors/default/workspace/meta';
import {
  Composite,
  pluralize,
  PropertyType,
} from '@allors/system/workspace/meta';
import { Injectable } from '@angular/core';

@Injectable()
export class AppMetaService implements MetaService {
  singularNameByObject: Map<Composite | PropertyType, string>;
  pluralNameByObject: Map<Composite | PropertyType, string>;

  constructor(workspaceService: WorkspaceService) {
    const m = workspaceService.workspace.configuration.metaPopulation as M;

    this.singularNameByObject = new Map<Composite | PropertyType, string>([
      [m.CommunicationEvent.InvolvedParties, 'Communication Event'],
      [m.FixedAsset.WorkRequirementsWhereFixedAsset, 'Service Request'],
      [m.Organisation, 'Company'],
      [m.Organisation.PurchaseOrdersWhereTakenViaSupplier, 'Purchase Order'],
      [
        m.Organisation.RepeatingPurchaseInvoicesWhereSupplier,
        'Repeating PurchaseInvoice',
      ],
      [m.Part.InventoryItemsWherePart, 'Inventory'],
      [m.Part.SupplierOfferingsWherePart, 'Supplier Offering'],
      [m.Party.PartyContactMechanismsWhereParty, 'ContactMechanism Usage'],
      [m.Party.PartyRelationshipsWhereParty, 'Party Relationship'],
      [m.Party.QuotesWhereReceiver, 'Quote'],
      [m.Party.RequestsWhereOriginator, 'Request for Quote'],
      [m.Party.SalesInvoicesWhereBillToCustomer, 'Sales Invoice'],
      [m.Party.SalesOrdersWhereBillToCustomer, 'Sales Order'],
      [m.Party.SerialisedItemsWhereOwnedBy, 'Serialised Asset'],
      [m.Party.SerialisedItemsWhereRentedBy, 'Serialised Asset'],
      [m.Party.WorkEffortsWhereCustomer, 'Work Order'],
      [m.PaymentApplication.PaymentWherePaymentApplication, 'Payment'],
      [
        m.PurchaseInvoiceItem.PurchaseInvoiceWherePurchaseInvoiceItem,
        'Purchase Invoice',
      ],
      [
        m.PurchaseOrderItem.PurchaseOrderWherePurchaseOrderItem,
        'Purchase Order',
      ],
      [m.QuoteItem.QuoteWhereQuoteItem, 'Quote'],
      [m.RequestItem.RequestWhereRequestItem, 'Request for Quote'],
      [m.SalesInvoice.RepeatingSalesInvoiceWhereSource, 'Repeating Invoice'],
      [m.SalesInvoiceItem.SalesInvoiceWhereSalesInvoiceItem, 'Sales Invoice'],
      [m.SalesOrderItem.SalesOrderWhereSalesOrderItem, 'Sales Order'],
      [
        m.SerialisedItem.SerialisedInventoryItemsWhereSerialisedItem,
        'Inventory',
      ],
      [
        m.SerialisedItem.OperatingHoursTransactionsWhereSerialisedItem,
        'Operating Hours Transaction',
      ],
      [m.ShipmentItem.ShipmentWhereShipmentItem, 'Shipment'],
      [m.WorkEffort.ServiceEntriesWhereWorkEffort, 'Time Entry'],
      [
        m.InventoryItem.WorkEffortInventoryAssignmentsWhereInventoryItem,
        'Work Order',
      ],
      [
        m.WorkEffort.WorkEffortInvoiceItemAssignmentsWhereAssignment,
        'Other Item',
      ],
      [
        m.WorkEffort.WorkEffortPurchaseOrderItemAssignmentsWhereAssignment,
        'Subcontracted Work',
      ],
      [
        m.WorkEffort.WorkRequirementFulfillmentsWhereFullfillmentOf,
        'Service Request',
      ],
      [m.WorkEffort.WorkEffortAssignmentRatesWhereWorkEffort, 'Rate'],
      [
        m.WorkEffort.WorkEffortFixedAssetAssignmentsWhereAssignment,
        'Equipment',
      ],
      [m.WorkEffortInventoryAssignment.Assignment, 'Work Order'],
      [m.WorkEffortFixedAssetAssignment.Assignment, 'Work Order'],
      [m.WorkEffortInventoryAssignment.InventoryItem, 'Part used'],
      [m.WorkEffortPartyAssignment.Party, 'Mechanic'],
      [m.WorkEffortType.WorkEffortPartStandards, 'Standard Part'],
      [m.WorkRequirement, 'Service Request'],
    ]);

    this.pluralNameByObject = new Map<Composite | PropertyType, string>([
      [m.Organisation, 'Companies'],
      [m.Part.InventoryItemsWherePart, 'Inventory'],
      [m.Party.RequestsWhereOriginator, 'Requests for Quote'],
      [m.Party.VehiclesWhereOwnedBy, 'Vehicles'],
      [m.RequestItem.RequestWhereRequestItem, 'Requests for Quote'],
      [
        m.SerialisedItem.SerialisedInventoryItemsWhereSerialisedItem,
        'Inventory',
      ],
      [m.WorkEffort.ServiceEntriesWhereWorkEffort, 'Time Entries'],
      [
        m.WorkEffort.WorkEffortPurchaseOrderItemAssignmentsWhereAssignment,
        'Subcontracted Work',
      ],
      [m.WorkEffortInventoryAssignment.InventoryItem, 'Parts used'],
    ]);
  }

  singularName(metaObject: Composite | PropertyType): string {
    return this.singularNameByObject.get(metaObject) ?? metaObject.singularName;
  }

  pluralName(metaObject: Composite | PropertyType): string {
    return (
      this.pluralNameByObject.get(metaObject) ??
      pluralize(this.singularNameByObject.get(metaObject)) ??
      metaObject.pluralName
    );
  }
}
