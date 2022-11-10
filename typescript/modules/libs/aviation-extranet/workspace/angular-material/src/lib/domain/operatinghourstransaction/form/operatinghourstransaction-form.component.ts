import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult } from '@allors/system/workspace/domain';
import {
  OperatingHoursTransaction,
  SerialisedItem,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';
import { FetcherService } from '../../../services/fetcher/fetcher-service';

@Component({
  templateUrl: './operatinghourstransaction-form.component.html',
  providers: [ContextService],
})
export class OperatingHoursTransactionFormComponent extends AllorsFormComponent<OperatingHoursTransaction> {
  readonly m: M;

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

    pulls.push(this.fetcher.locales);

    if (this.editRequest) {
      pulls.push(
        p.OperatingHoursTransaction({
          name: '_object',
          objectId: this.editRequest.objectId,
          include: {
            PreviousTransaction: {},
          },
        })
      );
    }

    const initializer = this.createRequest?.initializer;
    if (initializer) {
      pulls.push(
        p.SerialisedItem({
          objectId: initializer.id,
          include: {
            OperatingHoursTransactionsWhereSerialisedItem: {},
          },
        })
      );
    }
  }

  onPostPull(pullResult: IPullResult) {
    this.object = this.editRequest
      ? pullResult.object('_object')
      : this.context.create(this.createRequest.objectType);

    if (this.createRequest) {
      const serialisedItem = pullResult.object<SerialisedItem>(
        this.m.SerialisedItem
      );
      this.object.SerialisedItem = serialisedItem;

      const previousTransaction =
        serialisedItem.SyncedOperatingHoursTransactions.sort((a, b) =>
          a.CreationDate < b.CreationDate
            ? 1
            : b.CreationDate < a.CreationDate
            ? -1
            : 0
        )[0];
      this.object.PreviousTransaction = previousTransaction;
    }
  }
}
