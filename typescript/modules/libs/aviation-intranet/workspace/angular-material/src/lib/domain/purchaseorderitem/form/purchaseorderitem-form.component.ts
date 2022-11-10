import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult, And } from '@allors/system/workspace/domain';
import {
  Facility,
  InternalOrganisation,
  InventoryItem,
  InvoiceItemType,
  IrpfRegime,
  NonSerialisedInventoryItem,
  NonUnifiedPart,
  Part,
  Person,
  Product,
  PurchaseOrder,
  PurchaseOrderItem,
  SerialisedInventoryItem,
  SerialisedItem,
  Settings,
  SupplierOffering,
  UnifiedGood,
  VatRegime,
  WorkEffortState,
  WorkTask,
} from '@allors/default/workspace/domain';
import { M, TreeBuilder } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
  SearchFactory,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

import { isAfter, isBefore } from 'date-fns';
import {
  FetcherService,
  Filters,
  InternalOrganisationId,
} from '@allors/apps-intranet/workspace/angular-material';

@Component({
  templateUrl: './purchaseorderitem-form.component.html',
  providers: [ContextService],
})
export class PurchaseOrderItemFormComponent extends AllorsFormComponent<PurchaseOrderItem> {
  readonly m: M;
  readonly treeBuilder: TreeBuilder;

  order: PurchaseOrder;
  inventoryItems: InventoryItem[];
  vatRegimes: VatRegime[];
  irpfRegimes: IrpfRegime[];
  serialisedInventoryItem: SerialisedInventoryItem;
  nonSerialisedInventoryItem: NonSerialisedInventoryItem;
  invoiceItemTypes: InvoiceItemType[];
  partItemType: InvoiceItemType;
  productItemType: InvoiceItemType;
  transportItemType: InvoiceItemType;
  repairAndMaintenanceItemType: InvoiceItemType;
  otherItemType: InvoiceItemType;
  gseUnMotorisedType: InvoiceItemType;
  part: Part;
  serialisedItems: SerialisedItem[];
  serialisedItem: SerialisedItem;
  serialised: boolean;
  nonUnifiedPart: boolean;
  unifiedGood: boolean;
  addFacility = false;
  supplierOffering: SupplierOffering;
  ownWarehouses: Facility[];
  facilities: Facility[];
  workOrders: WorkTask[];
  partsFilter: SearchFactory;
  unifiedGoodsFilter: SearchFactory;
  showIrpf: boolean;
  vatRegimeInitialRole: VatRegime;
  irpfRegimeInitialRole: IrpfRegime;
  internalOrganisation: InternalOrganisation;
  settings: Settings;
  addPart = false;
  newPart = false;
  workEffortCreated: WorkEffortState;
  workEffortInProgress: WorkEffortState;
  deliveryDateIniatiolRole: Date;

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm,
    private fetcher: FetcherService,
    private internalOrganisationId: InternalOrganisationId
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;
    const { treeBuilder } = this.m;
    this.treeBuilder = treeBuilder;

    this.unifiedGoodsFilter = Filters.unifiedGoodsFilter(this.m, treeBuilder);
  }

  onPrePull(pulls: Pull[]): void {
    const { m } = this;
    const { pullBuilder: p } = m;

    pulls.push(
      this.fetcher.internalOrganisation,
      this.fetcher.Settings,
      this.fetcher.ownWarehousesAndStorageLocations,
      p.InvoiceItemType({
        predicate: {
          kind: 'Equals',
          propertyType: m.InvoiceItemType.IsActive,
          value: true,
        },
        sorting: [{ roleType: m.InvoiceItemType.Name }],
      }),
      p.IrpfRegime({
        sorting: [{ roleType: m.IrpfRegime.Name }],
      }),
      p.Facility({
        name: 'AllFacilities',
        sorting: [{ roleType: m.Facility.Name }],
      }),
      p.WorkEffortState({})
    );

    if (this.editRequest) {
      pulls.push(
        p.PurchaseOrderItem({
          name: '_object',
          objectId: this.editRequest.objectId,
          include: {
            InvoiceItemType: {},
            PurchaseOrderItemState: {},
            PurchaseOrderItemShipmentState: {},
            PurchaseOrderItemPaymentState: {},
            Part: {},
            SerialisedItem: {},
            StoredInFacility: {},
            WorkTask: {},
            DerivedVatRegime: {
              VatRates: {},
            },
            DerivedIrpfRegime: {
              IrpfRates: {},
            },
          },
        }),
        p.PurchaseOrderItem({
          objectId: this.editRequest.objectId,
          select: {
            PurchaseOrderWherePurchaseOrderItem: {
              include: {
                TakenViaSupplier: {},
                DerivedVatRegime: {
                  VatRates: {},
                },
                DerivedIrpfRegime: {
                  IrpfRates: {},
                },
              },
            },
          },
        })
      );
    }

    const initializer = this.createRequest?.initializer;
    if (initializer) {
      pulls.push(
        p.PurchaseOrder({
          objectId: initializer.id,
          include: {
            PurchaseOrderItems: {},
            DerivedVatRegime: {},
            DerivedIrpfRegime: {},
            TakenViaSupplier: {},
          },
        })
      );
    }
  }

  onPostPull(pullResult: IPullResult) {
    this.object = this.editRequest
      ? pullResult.object('_object')
      : this.context.create(this.createRequest.objectType);

    this.internalOrganisation =
      this.fetcher.getInternalOrganisation(pullResult);
    this.settings = this.fetcher.getSettings(pullResult);
    this.showIrpf = this.internalOrganisation.Country.IsoCode === 'ES';
    this.vatRegimes = this.internalOrganisation.Country.DerivedVatRegimes;
    this.irpfRegimes = pullResult.collection<IrpfRegime>(this.m.IrpfRegime);
    this.ownWarehouses =
      this.fetcher.getOwnWarehousesAndStorageLocations(pullResult);
    this.facilities = pullResult.collection<Facility>('AllFacilities');
    this.workOrders = pullResult.collection<WorkTask>(this.m.WorkTask);

    this.invoiceItemTypes = pullResult.collection<InvoiceItemType>(
      this.m.InvoiceItemType
    );
    this.partItemType = this.invoiceItemTypes?.find(
      (v: InvoiceItemType) =>
        v.UniqueId === 'ff2b943d-57c9-4311-9c56-9ff37959653b'
    );
    this.productItemType = this.invoiceItemTypes?.find(
      (v: InvoiceItemType) =>
        v.UniqueId === '0d07f778-2735-44cb-8354-fb887ada42ad'
    );
    this.transportItemType = this.invoiceItemTypes?.find(
      (v: InvoiceItemType) =>
        v.UniqueId === '96c1c0ff-b0f1-480f-91a7-4658bebe6674'
    );
    this.repairAndMaintenanceItemType = this.invoiceItemTypes?.find(
      (v: InvoiceItemType) =>
        v.UniqueId === 'f2d9770b-f933-48b0-a495-df80cb702fce'
    );
    this.otherItemType = this.invoiceItemTypes?.find(
      (v: InvoiceItemType) =>
        v.UniqueId === '8ab1f56a-b07e-4552-83a7-ca2da2043740'
    );
    this.gseUnMotorisedType = this.invoiceItemTypes?.find(
      (v: InvoiceItemType) =>
        v.UniqueId === 'c9362657-b081-4030-ac94-9622a2bbde08'
    );

    const workEffortStates = pullResult.collection<WorkEffortState>(
      this.m.WorkEffortState
    );
    this.workEffortCreated = workEffortStates?.find(
      (v: WorkEffortState) =>
        v.UniqueId === 'c082cd60-5c5f-4948-bdb1-06bd9c385751'
    );
    this.workEffortInProgress = workEffortStates?.find(
      (v: WorkEffortState) =>
        v.UniqueId === '7a83df7b-9918-4b10-8f99-48896f9db105'
    );

    if (this.createRequest) {
      this.order = pullResult.object<PurchaseOrder>(this.m.PurchaseOrder);
      this.order.addPurchaseOrderItem(this.object);
      this.vatRegimeInitialRole = this.order.DerivedVatRegime;
      this.irpfRegimeInitialRole = this.order.DerivedIrpfRegime;
      this.deliveryDateIniatiolRole = this.order.DeliveryDate;
      this.object.StoredInFacility = this.order.StoredInFacility;
    } else {
      this.order = this.object.PurchaseOrderWherePurchaseOrderItem;

      if (this.object.Part) {
        this.unifiedGood = this.object.Part.strategy.cls === this.m.UnifiedGood;
        this.nonUnifiedPart =
          this.object.Part.strategy.cls === this.m.NonUnifiedPart;
        this.updateFromPart(this.object.Part);
        this.refreshWorkOrders();
      }
    }

    this.partsFilter = new SearchFactory({
      objectType: this.m.NonUnifiedPart,
      roleTypes: [
        this.m.NonUnifiedPart.Name,
        this.m.NonUnifiedPart.SearchString,
      ],
      include: this.treeBuilder.NonUnifiedPart({ InventoryItemKind: {} }),
      post: (predicate: And) => {
        predicate.operands.push({
          kind: 'ContainedIn',
          propertyType: this.m.NonUnifiedPart.SupplierOfferingsWherePart,
          extent: {
            kind: 'Filter',
            objectType: this.m.SupplierOffering,
            predicate: {
              kind: 'And',
              operands: [
                {
                  kind: 'Equals',
                  propertyType: this.m.SupplierOffering.Supplier,
                  object: this.order.TakenViaSupplier,
                },
                {
                  kind: 'LessThan',
                  roleType: this.m.SupplierOffering.FromDate,
                  value: this.order.OrderDate,
                },
                {
                  kind: 'Or',
                  operands: [
                    {
                      kind: 'Not',
                      operand: {
                        kind: 'Exists',
                        propertyType: this.m.SupplierOffering.ThroughDate,
                      },
                    },
                    {
                      kind: 'GreaterThan',
                      roleType: this.m.SupplierOffering.ThroughDate,
                      value: this.order.OrderDate,
                    },
                  ],
                },
              ],
            },
          },
        });
      },
    });
  }

  public invoiceItemTypeSelected(): void {
    if (this.object.InvoiceItemType === this.partItemType) {
      this.refreshWorkOrders();
    }
  }

  public refreshWorkOrders(): void {
    const m = this.m;
    const { pullBuilder: p } = m;

    const pulls = [
      p.WorkTask({
        predicate: {
          kind: 'And',
          operands: [
            {
              kind: 'Equals',
              propertyType: m.WorkTask.TakenBy,
              objectId: this.internalOrganisationId.value,
            },
            {
              kind: 'Or',
              operands: [
                {
                  kind: 'Equals',
                  propertyType: m.WorkTask.WorkEffortState,
                  object: this.workEffortCreated,
                },
                {
                  kind: 'Equals',
                  propertyType: m.WorkTask.WorkEffortState,
                  object: this.workEffortInProgress,
                },
              ],
            },
          ],
        },
        sorting: [{ roleType: m.WorkTask.SortableWorkEffortNumber }],
      }),
    ];

    this.allors.context.pull(pulls).subscribe((pullResult) => {
      this.workOrders = pullResult.collection<WorkTask>(m.WorkTask);
      if (this.editRequest && this.object.WorkTask)
        this.workOrders.push(this.object.WorkTask);
    });
  }

  public goodSelected(unifiedGood: any): void {
    if (unifiedGood) {
      this.part = unifiedGood;

      this.serialised =
        this.part.InventoryItemKind.UniqueId ===
        '2596e2dd-3f5d-4588-a4a2-167d6fbe3fae';

      if (this.serialised) {
        this.object.QuantityOrdered = '1';
      }

      this.refreshSerialisedItems(unifiedGood);
    }
  }

  public serialisedItemSelected(serialisedItem: any): void {
    if (serialisedItem) {
      this.serialisedItem = this.part.SerialisedItems?.find(
        (v) => v === serialisedItem
      );
      this.object.QuantityOrdered = '1';
    }
  }

  public partSelected(part: any): void {
    if (part) {
      this.unifiedGood = this.object.Part.strategy.cls === this.m.UnifiedGood;
      this.nonUnifiedPart =
        this.object.Part.strategy.cls === this.m.NonUnifiedPart;

      if (this.unifiedGood) {
        const unifiedGood = this.object.Part as UnifiedGood;
        this.serialised =
          unifiedGood.InventoryItemKind.UniqueId ===
          '2596e2dd-3f5d-4588-a4a2-167d6fbe3fae';
      }

      if (this.nonUnifiedPart) {
        const nonUnifiedPart = this.object.Part as NonUnifiedPart;
        this.serialised =
          nonUnifiedPart.InventoryItemKind.UniqueId ===
          '2596e2dd-3f5d-4588-a4a2-167d6fbe3fae';
      }

      if (this.serialised) {
        this.object.QuantityOrdered = '1';
      }

      this.updateFromPart(part);
    }
  }

  public facilityAdded(facility: any): void {
    this.facilities.push(facility);
    this.object.StoredInFacility = facility;
  }

  public partAdded(part: NonUnifiedPart): void {
    part.DefaultFacility = this.object.StoredInFacility;
    this.newPart = true;
    this.object.Part = part;
    this.supplierOffering = this.allors.context.create<SupplierOffering>(
      this.m.SupplierOffering
    );
    this.supplierOffering.Supplier = this.order.TakenViaSupplier;
    this.supplierOffering.Part = part;
    this.supplierOffering.UnitOfMeasure = part.UnitOfMeasure;
    this.supplierOffering.Currency = this.settings.PreferredCurrency;
  }

  public override save(): void {
    this.onSave();
    super.save();
  }

  private refreshSerialisedItems(product: Product): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    const pulls = [
      pull.NonUnifiedGood({
        objectId: product.id,
        select: {
          Part: {
            include: {
              SerialisedItems: x,
              InventoryItemKind: x,
            },
          },
        },
      }),
      pull.UnifiedGood({
        objectId: product.id,
        include: {
          InventoryItemKind: x,
          SerialisedItems: x,
        },
      }),
    ];

    this.allors.context.pull(pulls).subscribe(() => {
      this.serialisedItems = this.part.SerialisedItems;
      this.serialised =
        this.part.InventoryItemKind.UniqueId ===
        '2596e2dd-3f5d-4588-a4a2-167d6fbe3fae';
    });
  }

  private updateFromPart(part: Part) {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    const pulls = [
      pull.Part({
        object: part,
        select: {
          SerialisedItems: {
            include: {
              OwnedBy: x,
            },
          },
        },
      }),
      pull.Part({
        object: part,
        select: {
          SupplierOfferingsWherePart: {
            include: {
              Supplier: x,
            },
          },
        },
      }),
      pull.Part({
        object: part,
        include: {
          InventoryItemKind: x,
          DefaultFacility: x,
        },
      }),
    ];

    this.allors.context;
    this.allors.context.pull(pulls).subscribe((pullResult) => {
      this.part = (pullResult.object<UnifiedGood>(m.UnifiedGood) ||
        pullResult.object<Part>(m.Part)) as Part;
      this.serialised =
        part.InventoryItemKind.UniqueId ===
        '2596e2dd-3f5d-4588-a4a2-167d6fbe3fae';

      const supplierOfferings = pullResult.collection<SupplierOffering>(
        m.Part.SupplierOfferingsWherePart
      );
      this.supplierOffering = supplierOfferings?.find(
        (v) =>
          isBefore(new Date(v.FromDate), new Date()) &&
          (!v.ThroughDate || isAfter(new Date(v.ThroughDate), new Date())) &&
          v.Supplier === this.order.TakenViaSupplier
      );

      this.serialisedItems = pullResult.collection<SerialisedItem>(
        m.Part.SerialisedItems
      );

      if (this.object.SerialisedItem) {
        this.serialisedItems.push(this.object.SerialisedItem);
      }
    });
  }

  private onSave() {
    if (
      this.object.InvoiceItemType !== this.partItemType &&
      this.object.InvoiceItemType !== this.partItemType
    ) {
      this.object.QuantityOrdered = '1';
    }
  }
}
