import { Injectable } from '@angular/core';
import {
  Action,
  ActionService,
  ActionTarget,
  CreateService,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import { M } from '@allors/default/workspace/meta';
import { Composite } from '@allors/system/workspace/meta';
import { MethodActionService } from '@allors/base/workspace/angular-material/application';

@Injectable()
export class AppActionService implements ActionService {
  actionByObjectType: Map<Composite, Action[]>;

  constructor(
    workspaceService: WorkspaceService,
    methodActionService: MethodActionService,
    createService: CreateService
  ) {
    const m = workspaceService.workspace.configuration.metaPopulation as M;

    this.actionByObjectType = new Map<Composite, Action[]>([
      // [
      //   m.SalesOrderItem,
      //   [
      //     methodActionService.create(m.SalesOrderItem.Cancel),
      //     methodActionService.create(m.SalesOrderItem.Reject),
      //     methodActionService.create(m.SalesOrderItem.Reopen),
      //   ],
      // ],
      // [
      //   m.SerialisedInventoryItem,
      //   [
      //     {
      //       name: 'changeinventory',
      //       displayName: () => 'Change Inventory',
      //       description: () => '',
      //       disabled: () => false,
      //       execute: (target: ActionTarget) => {
      //         if (!Array.isArray(target)) {
      //           createService.create({
      //             kind: 'CreateRequest',
      //             objectType: m.InventoryItemTransaction,
      //             initializer: {
      //               id: target.id,
      //               propertyType: m.InventoryItemTransaction.InventoryItem,
      //             },
      //           });
      //         }
      //       },
      //       result: null,
      //     },
      //   ],
      // ],
    ]);
  }

  action(objectType: Composite): Action[] {
    return this.actionByObjectType.get(objectType) ?? [];
  }
}
