import { Injectable } from '@angular/core';
import { SingletonId } from '@allors/base/workspace/angular/foundation';
import { WorkspaceService } from '@allors/base/workspace/angular/foundation';
import { M, PullBuilder } from '@allors/default/workspace/meta';
import { IPullResult, Pull } from '@allors/system/workspace/domain';
import { Locale, Settings } from '@allors/default/workspace/domain';

const x = {};

@Injectable({
  providedIn: 'root',
})
export class FetcherService {
  m: M;
  pull: PullBuilder;

  constructor(
    private singletonId: SingletonId,
    private workspaceService: WorkspaceService
  ) {
    this.m = workspaceService.workspace.configuration.metaPopulation as M;
    this.pull = this.m.pullBuilder;
  }

  public get locales(): Pull {
    return this.pull.Singleton({
      name: 'FetcherAdditionalLocales',
      objectId: this.singletonId.value,
      select: {
        AdditionalLocales: {
          include: {
            Language: x,
            Country: x,
          },
        },
      },
    });
  }

  getAdditionalLocales(loaded: IPullResult) {
    return loaded.collection<Locale>('FetcherAdditionalLocales');
  }

  public get Settings(): Pull {
    return this.pull.Singleton({
      name: 'FetcherSettings',
      objectId: this.singletonId.value,
      select: {
        Settings: {
          include: {
            PreferredCurrency: x,
            DefaultFacility: x,
          },
        },
      },
    });
  }

  getSettings(loaded: IPullResult) {
    return loaded.object<Settings>('FetcherSettings');
  }
}
