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
import { Filters } from '@allors/apps-intranet/workspace/angular-material';

@Component({
  templateUrl: './workefforttype-create-form.component.html',
  providers: [ContextService],
})
export class WorkEffortTypeCreateFormComponent extends AllorsFormComponent<WorkEffortType> {
  public m: M;
  serialisedgoodsFilter: SearchFactory;
  categories: ProductCategory[];

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;

    this.serialisedgoodsFilter = Filters.serialisedgoodsFilter(this.m);
  }

  onPrePull(pulls: Pull[]): void {
    const { m } = this;
    const { pullBuilder: p } = m;
    pulls.push(
      p.ProductCategory({
        sorting: [{ roleType: m.ProductCategory.DisplayName }],
      })
    );

    this.onPrePullInitialize(pulls);
  }

  onPostPull(pullResult: IPullResult) {
    this.object = this.context.create(this.createRequest.objectType);
    this.categories = pullResult.collection<ProductCategory>(
      this.m.ProductCategory
    );

    this.onPostPullInitialize(pullResult);
  }
}
