import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult } from '@allors/system/workspace/domain';
import {
  ProductCategory,
  SerialisedItem,
  UnifiedGood,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
  UserId,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';
import { FetcherService } from '../../../services/fetcher/fetcher-service';

@Component({
  // tslint:disable-next-line:component-selector
  selector: 'serialiseditem-edit-form',
  templateUrl: './serialiseditem-edit-form.component.html',
  providers: [ContextService],
})
export class SerialisedItemEditFormComponent extends AllorsFormComponent<SerialisedItem> {
  readonly m: M;

  part: UnifiedGood;
  selectedCategory: ProductCategory;

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm,
    private fetcher: FetcherService,
    private userId: UserId
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;
  }

  onPrePull(pulls: Pull[]): void {
    const { m } = this;
    const { pullBuilder: p } = m;

    pulls.push(
      p.SerialisedItem({
        name: '_object',
        objectId: this.editRequest.objectId,
        include: {
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
          PrimaryPhoto: {},
          SecondaryPhotos: {},
          AdditionalPhotos: {},
          PublicElectronicDocuments: {},
          PublicLocalisedElectronicDocuments: {},
          PartWhereSerialisedItem: {},
        },
      }),
      p.SerialisedItem({
        objectId: this.editRequest.objectId,
        include: {
          PartWhereSerialisedItem: {
            UnifiedGood_IataGseCode: {},
            UnifiedGood_ProductCategoriesWhereProduct: {
              PrimaryParent: {
                PrimaryParent: {},
              },
            },
          },
        },
      })
    );

    this.onPrePullInitialize(pulls);
  }

  onPostPull(pullResult: IPullResult) {
    this.object = pullResult.object('_object');

    this.onPostPullInitialize(pullResult);

    this.part = this.object.PartWhereSerialisedItem as UnifiedGood;
    this.selectedCategory = this.part.ProductCategoriesWhereProduct
      ? this.part.ProductCategoriesWhereProduct[0]
      : null;
  }
}
