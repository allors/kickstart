import { Injectable } from '@angular/core';
import {
  Filter,
  FilterDefinition,
  FilterService,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import { M } from '@allors/default/workspace/meta';
import { Composite } from '@allors/system/workspace/meta';

@Injectable()
export class AppFilterService implements FilterService {
  filterByComposite: Map<Composite, Filter>;
  filterDefinitionByComposite: Map<Composite, FilterDefinition>;

  constructor(workspaceService: WorkspaceService) {
    this.filterByComposite = new Map();
    this.filterDefinitionByComposite = new Map();

    const m = workspaceService.workspace.configuration.metaPopulation as M;

    const define = (
      composite: Composite,
      filterDefinition: FilterDefinition
    ) => {
      this.filterDefinitionByComposite.set(composite, filterDefinition);
    };

    define(
      m.SerialisedItem,
      new FilterDefinition({
        kind: 'And',
        operands: [
          {
            kind: 'Like',
            roleType: m.SerialisedItem.ItemNumber,
            parameter: 'id',
          },
          {
            kind: 'Like',
            roleType: m.SerialisedItem.DisplayName,
            parameter: 'name',
          },
        ],
      })
    );
  }

  filter(composite: Composite): Filter {
    let filter = this.filterByComposite.get(composite);
    if (filter == null) {
      const filterDefinition = this.filterDefinitionByComposite.get(composite);
      filter = new Filter(filterDefinition);
      this.filterByComposite.set(composite, filter);
    }

    return filter;
  }
}
