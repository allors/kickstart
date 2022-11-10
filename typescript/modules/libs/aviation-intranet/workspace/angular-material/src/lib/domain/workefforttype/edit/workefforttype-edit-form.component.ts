import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult } from '@allors/system/workspace/domain';
import {
  ProductCategory,
  WorkEffortType,
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
  selector: 'workefforttype-edit-form',
  templateUrl: './workefforttype-edit-form.component.html',
  providers: [ContextService],
})
export class WorkEffortTypeEditFormComponent extends AllorsFormComponent<WorkEffortType> {
  readonly m: M;
  serialisedgoodsFilter: SearchFactory;
  categories: ProductCategory[];

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm,
    private fetcher: FetcherService
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;

    this.serialisedgoodsFilter = Filters.serialisedgoodsFilter(this.m);
  }

  onPrePull(pulls: Pull[]): void {
    const { m } = this;
    const { pullBuilder: p } = m;

    pulls.push(this.fetcher.locales);

    if (this.editRequest) {
      pulls.push(
        p.WorkEffortType({
          name: '_object',
          objectId: this.editRequest.objectId,
          include: {
            UnifiedGood: {},
          },
        }),
        p.ProductCategory({
          sorting: [{ roleType: m.ProductCategory.DisplayName }],
        })
      );
    }

    this.onPrePullInitialize(pulls);
  }

  onPostPull(pullResult: IPullResult) {
    this.object = pullResult.object('_object');
    this.categories = pullResult.collection<ProductCategory>(
      this.m.ProductCategory
    );

    this.onPostPullInitialize(pullResult);
  }
}
