import {
  Sorter,
  SorterService,
} from '@allors/base/workspace/angular-material/application';
import { WorkspaceService } from '@allors/base/workspace/angular/foundation';
import { M } from '@allors/default/workspace/meta';
import { Composite } from '@allors/system/workspace/meta';
import { Injectable } from '@angular/core';

@Injectable()
export class AppSorterService implements SorterService {
  sorterByComposite: Map<Composite, Sorter>;

  constructor(workspaceService: WorkspaceService) {
    this.sorterByComposite = new Map();

    const m = workspaceService.workspace.configuration.metaPopulation as M;

    const define = (composite: Composite, sorter: Sorter) => {
      this.sorterByComposite.set(composite, sorter);
    };

    define(
      m.EmailMessage,
      new Sorter({
        created: m.EmailMessage.DateCreated,
        sent: m.EmailMessage.DateSent,
        subject: m.EmailMessage.Subject,
      })
    );

    define(
      m.Organisation,
      new Sorter({
        name: m.Organisation.Name,
        lastModifiedDate: m.Organisation.LastModifiedDate,
      })
    );

    define(
      m.Person,
      new Sorter({
        name: [m.Person.FirstName, m.Person.LastName],
        lastModifiedDate: m.Person.LastModifiedDate,
      })
    );
  }

  sorter(composite: Composite): Sorter {
    return this.sorterByComposite.get(composite);
  }
}
