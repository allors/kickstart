import { Component, OnDestroy, OnInit, Self } from '@angular/core';
import { Title } from '@angular/platform-browser';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap } from 'rxjs/operators';

import {
  Action,
  ContextService,
  Filter,
  FilterService,
  MediaService,
  RefreshService,
  Table,
  TableRow,
} from '@allors/base/workspace/angular/foundation';
import { Settings } from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  EditActionService,
  OverviewActionService,
  SorterService,
} from '@allors/base/workspace/angular-material/application';
import { NavigationService } from '@allors/base/workspace/angular/application';
import { FetcherService } from '@allors/apps-intranet/workspace/angular-material';

interface Row extends TableRow {
  object: Settings;
  name: string;
}

@Component({
  templateUrl: './settings-list-page.component.html',
  providers: [ContextService],
})
export class SettingsListPageComponent implements OnInit, OnDestroy {
  public title = 'Settings';

  table: Table<Row>;

  edit: Action;

  private subscription: Subscription;

  filter: Filter;
  m: M;

  constructor(
    @Self() public allors: ContextService,
    public refreshService: RefreshService,
    public overviewService: OverviewActionService,
    public editRoleService: EditActionService,
    public navigation: NavigationService,
    public mediaService: MediaService,
    private fetcher: FetcherService,
    public filterService: FilterService,
    public sorterService: SorterService,

    titleService: Title
  ) {
    this.allors.context.name = this.constructor.name;
    titleService.setTitle(this.title);

    this.m = this.allors.context.configuration.metaPopulation as M;

    this.edit = editRoleService.edit();
    this.edit.result.subscribe(() => {
      this.table.selection.clear();
    });

    this.table = new Table({
      selection: true,
      columns: [{ name: 'name' }],
      actions: [this.edit],
      defaultAction: this.edit,
      pageSize: 50,
    });
  }

  ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;

    this.subscription = combineLatest([
      this.refreshService.refresh$,
      this.table.sort$,
      this.table.pager$,
    ])
      .pipe(
        switchMap(() => {
          const pulls = [pull.Settings({})];

          return this.allors.context.pull(pulls);
        })
      )
      .subscribe((loaded) => {
        this.allors.context.reset();
        const objects = loaded.collection<Settings>(m.Settings);

        this.table.data = objects?.map((v) => {
          return {
            object: v,
            name: `Settings`,
          } as Row;
        });
      });
  }

  public ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }
}
