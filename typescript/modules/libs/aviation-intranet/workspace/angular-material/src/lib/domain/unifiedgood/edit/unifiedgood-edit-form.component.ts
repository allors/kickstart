import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult } from '@allors/system/workspace/domain';
import {
  Brand,
  Facility,
  InventoryItemKind,
  Locale,
  Model,
  Organisation,
  PriceComponent,
  ProductCategory,
  ProductIdentificationType,
  ProductNumber,
  ProductType,
  Scope,
  Settings,
  UnifiedGood,
  UnitOfMeasure,
  VatRate,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
  SearchFactory,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';
import {
  FetcherService,
  Filters,
} from '@allors/apps-intranet/workspace/angular-material';

@Component({
  // tslint:disable-next-line:component-selector
  selector: 'unifiedgood-edit-form',
  templateUrl: './unifiedgood-edit-form.component.html',
  providers: [ContextService],
})
export class UnifiedGoodEditFormComponent extends AllorsFormComponent<UnifiedGood> {
  readonly m: M;

  facility: Facility;
  facilities: Facility[];
  locales: Locale[];
  inventoryItemKinds: InventoryItemKind[];
  productTypes: ProductType[];
  categories: ProductCategory[];
  vatRates: VatRate[];
  brands: Brand[];
  selectedBrand: Brand;
  models: Model[];
  selectedModel: Model;
  organisations: Organisation[];
  addBrand = false;
  addModel = false;
  goodIdentificationTypes: ProductIdentificationType[];
  productNumber: ProductNumber;
  originalCategory: ProductCategory;
  selectedCategory: ProductCategory;
  unitsOfMeasure: UnitOfMeasure[];
  currentSellingPrice: PriceComponent;
  internalOrganisation: Organisation;
  settings: Settings;
  public scopes: Scope[];

  manufacturersFilter: SearchFactory;

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm,
    private fetcher: FetcherService
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;

    this.manufacturersFilter = Filters.manufacturersFilter(this.m);
  }

  onPrePull(pulls: Pull[]): void {
    const { m } = this;
    const { pullBuilder: p } = m;

    pulls.push(
      this.fetcher.locales,
      this.fetcher.Settings,
      p.UnifiedGood({
        name: '_object',
        objectId: this.editRequest.objectId,
        include: {
          IataGseCode: {},
          PrimaryPhoto: {},
          Photos: {},
          PublicElectronicDocuments: {},
          PrivateElectronicDocuments: {},
          PublicLocalisedElectronicDocuments: {},
          PrivateLocalisedElectronicDocuments: {},
          ManufacturedBy: {},
          SuppliedBy: {},
          DefaultFacility: {},
          SerialisedItemCharacteristics: {
            LocalisedValues: {},
            SerialisedItemCharacteristicType: {
              UnitOfMeasure: {},
              LocalisedNames: {},
            },
          },
          ProductIdentifications: {
            ProductIdentificationType: {},
          },
          Brand: {
            Models: {},
          },
          Model: {},
          LocalisedNames: {
            Locale: {},
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
        },
      }),
      p.UnitOfMeasure({}),
      p.InventoryItemKind({}),
      p.ProductIdentificationType({}),
      p.Facility({}),
      p.VatRate({}),
      p.Scope({}),
      p.ProductIdentificationType({}),
      p.ProductType({ sorting: [{ roleType: m.ProductType.Name }] }),
      p.ProductCategory({
        sorting: [{ roleType: m.ProductCategory.Name }],
      }),
      p.Brand({
        include: {
          Models: {},
        },
        sorting: [{ roleType: m.Brand.Name }],
      }),
      p.UnifiedGood({
        name: 'OriginalCategories',
        objectId: this.editRequest.objectId,
        select: { ProductCategoriesWhereProduct: {} },
      })
    );

    this.onPrePullInitialize(pulls);
  }

  onPostPull(pullResult: IPullResult) {
    this.object = pullResult.object('_object');

    this.onPostPullInitialize(pullResult);

    this.originalCategory =
      pullResult.collection<ProductCategory>('OriginalCategories')?.[0];
    this.selectedCategory = this.originalCategory;

    this.inventoryItemKinds = pullResult.collection<InventoryItemKind>(
      this.m.InventoryItemKind
    );
    this.productTypes = pullResult.collection<ProductType>(this.m.ProductType);
    this.scopes = pullResult.collection<Scope>(this.m.Scope);
    this.brands = pullResult.collection<Brand>(this.m.Brand);
    this.locales = this.fetcher.getAdditionalLocales(pullResult);
    this.facilities = pullResult.collection<Facility>(this.m.Facility);
    this.unitsOfMeasure = pullResult.collection<UnitOfMeasure>(
      this.m.UnitOfMeasure
    );
    this.settings = this.fetcher.getSettings(pullResult);
    this.vatRates = pullResult.collection<VatRate>(this.m.VatRate);
    this.goodIdentificationTypes =
      pullResult.collection<ProductIdentificationType>(
        this.m.ProductIdentificationType
      );
    this.categories = pullResult.collection<ProductCategory>(
      this.m.ProductCategory
    );
    this.categories.sort((a, b) =>
      a.DisplayName > b.DisplayName ? 1 : b.DisplayName > a.DisplayName ? -1 : 0
    );

    const goodNumberType = this.goodIdentificationTypes?.find(
      (v) => v.UniqueId === 'b640630d-a556-4526-a2e5-60a84ab0db3f'
    );

    this.productNumber = this.object.ProductIdentifications?.find(
      (v) => v.ProductIdentificationType === goodNumberType
    );

    this.selectedBrand = this.object.Brand;
    this.selectedModel = this.object.Model;

    if (this.selectedBrand) {
      this.brandSelected(this.selectedBrand);
    }
  }

  public brandAdded(brand: Brand): void {
    this.brands.push(brand);
    this.selectedBrand = brand;
    this.models = [];
    this.selectedModel = undefined;
    this.object.Brand = this.selectedBrand;
  }

  public modelAdded(model: Model): void {
    this.selectedBrand.addModel(model);
    this.models = this.selectedBrand.Models.sort((a, b) =>
      a.Name > b.Name ? 1 : b.Name > a.Name ? -1 : 0
    );
    this.selectedModel = model;
    this.object.Model = this.selectedModel;
  }

  public brandSelected(brand: Brand): void {
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
      this.object.Brand = this.selectedBrand;
      this.models = this.selectedBrand.Models.sort((a, b) =>
        a.Name > b.Name ? 1 : b.Name > a.Name ? -1 : 0
      );
    });
  }

  public ModelChanged(): void {
    this.object.Model = this.selectedModel;
  }

  public categorySelected(category: ProductCategory): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    const pulls = [
      pull.ProductCategory({
        object: category,
        include: {
          Products: x,
        },
      }),
      pull.ProductCategory({
        name: 'originalCategory',
        object: this.originalCategory,
        include: {
          Products: x,
        },
      }),
    ];

    this.allors.context.pull(pulls).subscribe((loaded) => {
      if (this.selectedCategory !== this.originalCategory) {
        this.selectedCategory.addProduct(this.object);

        if (this.originalCategory) {
          this.originalCategory.removeProduct(this.object);
        }
      }
    });
  }
}
