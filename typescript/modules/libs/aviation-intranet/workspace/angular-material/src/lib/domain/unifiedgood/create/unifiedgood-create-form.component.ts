import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult } from '@allors/system/workspace/domain';
import {
  Brand,
  InventoryItemKind,
  Model,
  ProductIdentificationType,
  ProductNumber,
  ProductType,
  Settings,
  UnifiedGood,
  VatRate,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';
import { FetcherService } from '@allors/apps-intranet/workspace/angular-material';

@Component({
  templateUrl: './unifiedgood-create-form.component.html',
  providers: [ContextService],
})
export class UnifiedGoodCreateFormComponent extends AllorsFormComponent<UnifiedGood> {
  readonly m: M;

  productTypes: ProductType[];
  inventoryItemKinds: InventoryItemKind[];
  vatRates: VatRate[];
  goodIdentificationTypes: ProductIdentificationType[];
  productNumber: ProductNumber;
  settings: Settings;
  goodNumberType: ProductIdentificationType;
  brands: Brand[];
  selectedBrand: Brand;
  models: Model[];
  selectedModel: Model;
  addBrand = false;
  addModel = false;

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm,
    private fetcher: FetcherService
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;
  }

  onPrePull(pulls: Pull[]): void {
    const { m } = this;
    const { pullBuilder: p } = m;

    pulls.push(
      this.fetcher.Settings,
      p.InventoryItemKind({}),
      p.ProductType({ sorting: [{ roleType: m.ProductType.Name }] }),
      p.VatRate({}),
      p.ProductIdentificationType({}),
      p.Brand({
        include: {
          Models: {},
        },
        sorting: [{ roleType: m.Brand.Name }],
      })
    );

    this.onPrePullInitialize(pulls);
  }

  onPostPull(pullResult: IPullResult) {
    this.object = this.context.create(this.createRequest.objectType);

    this.onPostPullInitialize(pullResult);
    this.inventoryItemKinds = pullResult.collection<InventoryItemKind>(
      this.m.InventoryItemKind
    );
    this.productTypes = pullResult.collection<ProductType>(this.m.ProductType);
    this.vatRates = pullResult.collection<VatRate>(this.m.VatRate);
    this.brands = pullResult.collection<Brand>(this.m.Brand);
    this.goodIdentificationTypes =
      pullResult.collection<ProductIdentificationType>(
        this.m.ProductIdentificationType
      );
    this.settings = this.fetcher.getSettings(pullResult);

    this.goodNumberType = this.goodIdentificationTypes?.find(
      (v) => v.UniqueId === 'b640630d-a556-4526-a2e5-60a84ab0db3f'
    );

    this.object.Name = '<Automatically set on saving>';

    if (!this.settings.UseProductNumberCounter) {
      this.productNumber = this.allors.context.create<ProductNumber>(
        this.m.ProductNumber
      );
      this.productNumber.ProductIdentificationType = this.goodNumberType;

      this.object.addProductIdentification(this.productNumber);
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
      this.object.Brand = brand;

      this.models = this.selectedBrand.Models.sort((a, b) =>
        a.Name > b.Name ? 1 : b.Name > a.Name ? -1 : 0
      );
    });
  }

  public modelSelected(model: Model): void {
    this.object.Model = model;
  }
}
