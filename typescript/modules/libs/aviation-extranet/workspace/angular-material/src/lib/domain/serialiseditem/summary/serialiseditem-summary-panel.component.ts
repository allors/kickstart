import { Component } from '@angular/core';

import {
  MediaService,
  RefreshService,
  SharedPullService,
} from '@allors/base/workspace/angular/foundation';

import { WorkspaceService } from '@allors/base/workspace/angular/foundation';
import {
  AllorsViewSummaryPanelComponent,
  NavigationService,
  PanelService,
  ScopedService,
} from '@allors/base/workspace/angular/application';
import { IPullResult, Pull } from '@allors/system/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import { Part, SerialisedItem } from '@allors/default/workspace/domain';

@Component({
  // tslint:disable-next-line:component-selector
  selector: 'serialiseditem-summary-panel',
  templateUrl: './serialiseditem-summary-panel.component.html',
})
export class SerialisedItemSummaryPanelComponent extends AllorsViewSummaryPanelComponent {
  m: M;

  serialisedItem: SerialisedItem;
  part: Part;

  constructor(
    scopedService: ScopedService,
    panelService: PanelService,
    refreshService: RefreshService,
    sharedPullService: SharedPullService,
    workspaceService: WorkspaceService,
    public navigation: NavigationService,
    private mediaService: MediaService
  ) {
    super(scopedService, panelService, sharedPullService, refreshService);
    this.m = workspaceService.workspace.configuration.metaPopulation as M;
  }

  onPreSharedPull(pulls: Pull[], prefix?: string) {
    const { m } = this;
    const { pullBuilder: p } = m;

    const id = this.scoped.id;

    pulls.push(
      p.SerialisedItem({
        name: prefix,
        objectId: id,
        include: {
          PrimaryPhoto: {},
        },
      })
    );
  }

  onPostSharedPull(loaded: IPullResult, prefix?: string) {
    this.serialisedItem = loaded.object<SerialisedItem>(prefix);
    this.part = loaded.object<Part>(`${prefix}_part`);
  }

  get src(): string {
    const media = this.serialisedItem.PrimaryPhoto;
    if (media) {
      if (media.InDataUri) {
        return media.InDataUri;
      } else if (media.UniqueId) {
        return this.mediaService.url(media);
      }
    }

    return undefined;
  }
}
