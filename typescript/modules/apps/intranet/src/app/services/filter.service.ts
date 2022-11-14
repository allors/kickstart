import { Injectable } from '@angular/core';
import {
  Filter,
  FilterDefinition,
  FilterService,
  SearchFactory,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import { M } from '@allors/default/workspace/meta';
import { Composite } from '@allors/system/workspace/meta';
import {
  Country,
  Currency,
  Person,
  User,
} from '@allors/default/workspace/domain';

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

    const countrySearch = new SearchFactory({
      objectType: m.Country,
      roleTypes: [m.Country.Name],
    });

    const userSearch = new SearchFactory({
      objectType: m.User,
      roleTypes: [m.User.UserEmail],
    });

    define(
      m.EmailMessage,
      new FilterDefinition(
        {
          kind: 'And',
          operands: [
            {
              kind: 'Like',
              roleType: m.EmailMessage.Subject,
              parameter: 'subject',
            },
            {
              kind: 'Equals',
              propertyType: m.EmailMessage.Sender,
              parameter: 'Sender',
            },
            {
              kind: 'Contains',
              propertyType: m.EmailMessage.Recipients,
              parameter: 'Recipient',
            },
          ],
        },
        {
          Sender: {
            search: () => userSearch,
            display: (v: User) => v && v.UserEmail,
          },
          Recipient: {
            search: () => userSearch,
            display: (v: User) => v && v.UserEmail,
          },
        }
      )
    );

    define(
      m.Organisation,
      new FilterDefinition({
        kind: 'And',
        operands: [
          { kind: 'Like', roleType: m.Organisation.Name, parameter: 'name' },
        ],
      })
    );

    define(
      m.Person,
      new FilterDefinition(
        {
          kind: 'And',
          operands: [
            {
              kind: 'Like',
              roleType: m.Person.FirstName,
              parameter: 'firstName',
            },
            {
              kind: 'Like',
              roleType: m.Person.LastName,
              parameter: 'lastName',
            },
          ],
        },
        {
          country: {
            search: () => countrySearch,
            display: (v: Country) => v && v.Name,
          },
        }
      )
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
