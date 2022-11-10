import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult } from '@allors/system/workspace/domain';
import {
  Brand,
  Enumeration,
  Facility,
  InventoryItemTransaction,
  InventoryTransactionReason,
  Locale,
  Model,
  Ownership,
  Person,
  ProductCategory,
  SerialisedInventoryItem,
  SerialisedItem,
  SerialisedItemAvailability,
  SerialisedItemCharacteristicType,
  SerialisedItemState,
  UnifiedGood,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
  SearchFactory,
  UserId,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

import {
  FetcherService,
  Filters,
} from '@allors/apps-intranet/workspace/angular-material';

@Component({
  // tslint:disable-next-line:component-selector
  selector: 'serialiseditem-edit-form',
  templateUrl: './serialiseditem-edit-form.component.html',
  providers: [ContextService],
})
export class SerialisedItemEditFormComponent extends AllorsFormComponent<SerialisedItem> {
  readonly m: M;

  locales: Locale[];
  serialisedItemStates: Enumeration[];
  ownerships: Enumeration[];
  part: UnifiedGood;
  currentFacility: Facility;
  addChassisBrand = false;
  addChassisModel = false;
  chassisBrands: Brand[];
  selectedChassisBrand: Brand;
  chassisModels: Model[];
  selectedChassisModel: Model;
  brands: Brand[];
  selectedBrand: Brand;
  isAdministrator: boolean;
  selectedCategory: ProductCategory;
  serialisedItemAvailabilities: Enumeration[];
  user: Person;
  serialisedInventoryItems: SerialisedInventoryItem[];
  inventoryItem: SerialisedInventoryItem;
  physicalCount: InventoryTransactionReason;
  internalOrganisationsFilter: SearchFactory;
  partiesFilter: SearchFactory;
  unifiedGoodsFilter: SearchFactory;
  suppliersFilter: SearchFactory;
  operatingHours: SerialisedItemCharacteristicType;
  partWhereSerialisedItem: string;

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm,
    private fetcher: FetcherService,
    private userId: UserId
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;
    const { treeBuilder } = this.m;

    this.internalOrganisationsFilter = Filters.internalOrganisationsFilter(
      this.m
    );
    this.partiesFilter = Filters.partiesFilter(this.m);
    this.unifiedGoodsFilter = Filters.unifiedGoodsFilter(this.m, treeBuilder);
    this.suppliersFilter = Filters.allSuppliersFilter(this.m);

    this.partWhereSerialisedItem = 'partWhereSerialisedItem';
  }

  onPrePull(pulls: Pull[]): void {
    const { m } = this;
    const { pullBuilder: p } = m;

    pulls.push(
      p.SerialisedItem({
        name: '_object',
        objectId: this.editRequest.objectId,
        include: {
          SerialisedItemState: {},
          ChassisBrand: {},
          ChassisModel: {},
          SerialisedItemCharacteristics: {
            SerialisedItemCharacteristicType: {
              UnitOfMeasure: {},
            },
          },
          LocalisedDescriptions: {
            Locale: {},
          },
          LocalisedComments: {
            Locale: {},
          },
          LocalisedKeywords: {
            Locale: {},
          },
          Ownership: {},
          Buyer: {},
          Seller: {},
          OwnedBy: {},
          RentedBy: {},
          PrimaryPhoto: {},
          SecondaryPhotos: {},
          AdditionalPhotos: {},
          PrivatePhotos: {},
          PublicElectronicDocuments: {},
          PrivateElectronicDocuments: {},
          PublicLocalisedElectronicDocuments: {},
          PrivateLocalisedElectronicDocuments: {},
          PurchaseInvoice: {},
          PurchaseOrder: {},
          SuppliedBy: {},
          AssignedSuppliedBy: {},
          SerialisedItemAvailability: {},
          PartWhereSerialisedItem: {},
        },
      }),
      this.fetcher.locales,
      p.SerialisedItem({
        name: this.partWhereSerialisedItem,
        objectId: this.editRequest.objectId,
        select: {
          PartWhereSerialisedItem: {
            include: {
              SerialisedItems: {},
              UnifiedGood_IataGseCode: {},
              UnifiedGood_ProductCategoriesWhereProduct: {
                PrimaryParent: {
                  PrimaryParent: {},
                },
              },
            },
          },
        },
      }),
      p.SerialisedItem({
        objectId: this.editRequest.objectId,
        select: {
          SerialisedInventoryItemsWhereSerialisedItem: {
            include: {
              InventoryItemTransactionsWhereInventoryItem: {
                Part: {
                  PartWeightedAverage: {},
                },
              },
              SerialisedItem: {},
              Facility: {},
              UnitOfMeasure: {},
              Lot: {},
            },
          },
        },
      }),
      p.SerialisedItemState({
        predicate: {
          kind: 'Equals',
          propertyType: m.SerialisedItemState.IsActive,
          value: true,
        },
        sorting: [{ roleType: m.SerialisedItemState.Name }],
      }),
      p.SerialisedItemAvailability({
        predicate: {
          kind: 'Equals',
          propertyType: m.SerialisedItemAvailability.IsActive,
          value: true,
        },
        sorting: [{ roleType: m.SerialisedItemAvailability.Name }],
      }),
      p.Ownership({
        predicate: {
          kind: 'Equals',
          propertyType: m.Ownership.IsActive,
          value: true,
        },
        sorting: [{ roleType: m.Ownership.Name }],
      }),
      p.Brand({
        include: {
          Models: {},
        },
        sorting: [{ roleType: m.Brand.Name }],
      }),
      p.Person({
        objectId: this.userId.value,
        include: { UserGroupsWhereMember: {} },
      }),
      p.InventoryTransactionReason({}),
      p.SerialisedItemCharacteristicType({})
    );

    this.onPrePullInitialize(pulls);
  }

  onPostPull(pullResult: IPullResult) {
    this.object = pullResult.object('_object');

    this.onPostPullInitialize(pullResult);

    this.brands = pullResult.collection<Brand>(this.m.Brand);
    this.locales = this.fetcher.getAdditionalLocales(pullResult);
    this.serialisedItemStates = pullResult.collection<SerialisedItemState>(
      this.m.SerialisedItemState
    );
    this.serialisedItemAvailabilities =
      pullResult.collection<SerialisedItemAvailability>(
        this.m.SerialisedItemAvailability
      );
    this.ownerships = pullResult.collection<Ownership>(this.m.Ownership);
    this.part = pullResult.object<UnifiedGood>(this.partWhereSerialisedItem);
    this.selectedCategory = this.part.ProductCategoriesWhereProduct
      ? this.part.ProductCategoriesWhereProduct[0]
      : null;

    this.user = pullResult.object<Person>(this.m.Person);

    this.isAdministrator =
      this.user.UserGroupsWhereMember.findIndex(
        (v) => v.UniqueId === 'cdc04209-683b-429c-bed2-440851f430df'
      ) > -1;

    const transactionReasons =
      pullResult.collection<InventoryTransactionReason>(
        this.m.InventoryTransactionReason
      );
    this.physicalCount = transactionReasons?.find(
      (v) => v.UniqueId === '971d0321-a86d-450c-adaa-18b3c2114714'
    );

    const characteristicTypes =
      pullResult.collection<SerialisedItemCharacteristicType>(
        this.m.SerialisedItemCharacteristicType
      );
    this.operatingHours = characteristicTypes?.find(
      (v) => v.UniqueId === 'ac38d868-c541-49d5-9bcc-dca118aa707d'
    );

    this.serialisedInventoryItems =
      pullResult.collection<SerialisedInventoryItem>(
        this.m.SerialisedItem.SerialisedInventoryItemsWhereSerialisedItem
      );
    this.inventoryItem = this.serialisedInventoryItems?.find(
      (v) => v.Quantity === 1
    );
    if (this.inventoryItem) {
      this.currentFacility = this.inventoryItem.Facility;
    }

    this.selectedChassisBrand = this.object.ChassisBrand;
    this.selectedChassisModel = this.object.ChassisModel;

    if (this.selectedChassisBrand) {
      this.chassisBrandSelected(this.selectedChassisBrand);
    }
  }

  public chassisBrandAdded(brand: any): void {
    this.brands.push(brand);
    this.selectedChassisBrand = brand;
    this.chassisModels = [];
    this.selectedChassisModel = undefined;
    this.object.ChassisBrand = brand;
  }

  public chassisModelAdded(model: any): void {
    this.selectedChassisBrand.addModel(model);
    this.chassisModels = this.selectedChassisBrand.Models.sort((a, b) =>
      a.Name > b.Name ? 1 : b.Name > a.Name ? -1 : 0
    );
    this.selectedChassisModel = model;
    this.chassisModelSelected(this.selectedChassisModel);
  }

  public partChanged(part: any) {
    if (part != null) {
      const previousPart = this.part;

      part.DerivationTrigger = null;

      if (this.inventoryItem) {
        const inventoryItemTransaction =
          this.allors.context.create<InventoryItemTransaction>(
            this.m.InventoryItemTransaction
          );
        inventoryItemTransaction.TransactionDate = new Date();
        inventoryItemTransaction.Part = previousPart;
        inventoryItemTransaction.Cost =
          previousPart.PartWeightedAverage?.AverageCost;
        inventoryItemTransaction.Facility = this.inventoryItem.Facility;
        inventoryItemTransaction.UnitOfMeasure =
          this.inventoryItem.UnitOfMeasure;
        inventoryItemTransaction.Lot = this.inventoryItem.Lot;
        inventoryItemTransaction.SerialisedItem = this.object;
        inventoryItemTransaction.SerialisedInventoryItemState =
          this.inventoryItem.SerialisedInventoryItemState;
        inventoryItemTransaction.Quantity = '-1';
        inventoryItemTransaction.Reason = this.physicalCount;

        const inventoryItemTransaction2 =
          this.allors.context.create<InventoryItemTransaction>(
            this.m.InventoryItemTransaction
          );
        inventoryItemTransaction2.TransactionDate = new Date();
        inventoryItemTransaction2.Part = part;
        inventoryItemTransaction2.Cost = part.PartWeightedAverage?.AverageCost;
        inventoryItemTransaction2.Facility = this.inventoryItem.Facility;
        inventoryItemTransaction2.UnitOfMeasure =
          this.inventoryItem.UnitOfMeasure;
        inventoryItemTransaction2.Lot = this.inventoryItem.Lot;
        inventoryItemTransaction2.SerialisedItem = this.object;
        inventoryItemTransaction2.SerialisedInventoryItemState =
          this.inventoryItem.SerialisedInventoryItemState;
        inventoryItemTransaction2.Quantity = '1';
        inventoryItemTransaction2.Reason = this.physicalCount;
      }
    }
  }

  public chassisBrandSelected(brand: any): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    const pulls = [
      pull.Brand({
        object: brand,
        include: {
          Models: x,
        },
      }),
    ];

    this.allors.context.pull(pulls).subscribe(() => {
      this.object.ChassisBrand = brand;

      this.chassisModels = this.selectedChassisBrand.Models.sort((a, b) =>
        a.Name > b.Name ? 1 : b.Name > a.Name ? -1 : 0
      );
    });
  }

  public chassisModelSelected(model: Model): void {
    this.object.ChassisModel = model;
  }
}
