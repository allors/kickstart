import { Composite } from '@allors/system/workspace/meta';
import {
  FormService,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';

import { Injectable } from '@angular/core';
import {
  OperatingHoursTransactionFormComponent,
  SerialisedItemEditFormComponent,
  UserFormComponent,
  WorkRequirementFormComponent,
} from '@allors/aviation-extranet/workspace/angular-material';
import { M } from '@allors/default/workspace/meta';

@Injectable()
export class AppFormService implements FormService {
  createFormByObjectType: Map<Composite, unknown>;
  editFormByObjectType: Map<Composite, unknown>;
  formByObjectType: Map<Composite, unknown>;

  constructor(workspaceService: WorkspaceService) {
    const m = workspaceService.workspace.configuration.metaPopulation as M;

    this.createFormByObjectType = new Map<Composite, unknown>([]);

    this.editFormByObjectType = new Map<Composite, unknown>([
      [m.SerialisedItem, SerialisedItemEditFormComponent],
      [m.Person, UserFormComponent],
    ]);

    this.formByObjectType = new Map<Composite, unknown>([
      [m.OperatingHoursTransaction, OperatingHoursTransactionFormComponent],
      [m.WorkRequirement, WorkRequirementFormComponent],
    ]);
  }

  createForm(objectType: Composite) {
    return (
      this.createFormByObjectType.get(objectType) ??
      this.formByObjectType.get(objectType)
    );
  }

  editForm(objectType: Composite) {
    return (
      this.editFormByObjectType.get(objectType) ??
      this.formByObjectType.get(objectType)
    );
  }
}

export const createComponents: any[] = [];

export const editComponents: any[] = [SerialisedItemEditFormComponent];

export const components: any[] = [
  OperatingHoursTransactionFormComponent,
  UserFormComponent,
  WorkRequirementFormComponent,
];
