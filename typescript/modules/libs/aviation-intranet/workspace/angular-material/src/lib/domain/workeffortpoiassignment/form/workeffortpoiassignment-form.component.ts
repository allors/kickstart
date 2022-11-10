import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult } from '@allors/system/workspace/domain';
import {
  PurchaseOrder,
  PurchaseOrderItem,
  WorkEffort,
  WorkEffortPurchaseOrderItemAssignment,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';
import { InternalOrganisationId } from '@allors/apps-intranet/workspace/angular-material';

@Component({
  templateUrl: './workeffortpoiassignment-form.component.html',
  providers: [ContextService],
})
export class WorkEffortPurchaseOrderItemAssignmentFormComponent extends AllorsFormComponent<WorkEffortPurchaseOrderItemAssignment> {
  readonly m: M;
  workEffort: WorkEffort;
  selectedPurchaseOrder: PurchaseOrder;
  purchaseOrders: PurchaseOrder[];
  purchaseOrderItems: PurchaseOrderItem[];

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm,
    private internalOrganisationId: InternalOrganisationId
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;
  }

  onPrePull(pulls: Pull[]): void {
    const { m } = this;
    const { pullBuilder: p } = m;

    pulls.push(
      p.PurchaseOrderItem({
        name: 'purchaseOrder',
        predicate: {
          kind: 'And',
          operands: [
            {
              kind: 'Not',
              operand: {
                kind: 'Exists',
                propertyType:
                  m.PurchaseOrderItem
                    .WorkEffortPurchaseOrderItemAssignmentsWherePurchaseOrderItem,
              },
            },
            {
              kind: 'Not',
              operand: {
                kind: 'Exists',
                propertyType: m.PurchaseOrderItem.Part,
              },
            },
            {
              kind: 'ContainedIn',
              propertyType: m.PurchaseOrderItem.InvoiceItemType,
              extent: {
                kind: 'Filter',
                objectType: m.InvoiceItemType,
                predicate: {
                  kind: 'Equals',
                  propertyType: m.InvoiceItemType.UniqueId,
                  value: 'f2d9770b-f933-48b0-a495-df80cb702fce',
                },
              },
            },
            {
              kind: 'ContainedIn',
              propertyType:
                m.PurchaseOrderItem.PurchaseOrderWherePurchaseOrderItem,
              extent: {
                kind: 'Filter',
                objectType: m.PurchaseOrder,
                predicate: {
                  kind: 'Equals',
                  propertyType: m.PurchaseOrder.OrderedBy,
                  value: this.internalOrganisationId.value,
                },
              },
            },
          ],
        },
        select: {
          PurchaseOrderWherePurchaseOrderItem: {
            include: {
              PurchaseOrderItems: {
                InvoiceItemType: {},
              },
              TakenViaSupplier: {},
              WorkEffortPurchaseOrderItemAssignmentsWherePurchaseOrder: {},
            },
          },
        },
      })
    );

    if (this.editRequest) {
      pulls.push(
        p.WorkEffortPurchaseOrderItemAssignment({
          name: '_object',
          objectId: this.editRequest.objectId,
          include: {
            Assignment: {},
            PurchaseOrder: {
              PurchaseOrderItems: {},
            },
            PurchaseOrderItem: {},
          },
        })
      );
    }

    const initializer = this.createRequest?.initializer;
    if (initializer) {
      pulls.push(
        p.WorkEffort({
          objectId: initializer.id,
        })
      );
    }
  }

  onPostPull(pullResult: IPullResult) {
    this.object = this.editRequest
      ? pullResult.object('_object')
      : this.context.create(this.createRequest.objectType);

    this.purchaseOrders = pullResult.collection<PurchaseOrder>('purchaseOrder');

    if (this.createRequest) {
      this.workEffort = pullResult.object<WorkEffort>(this.m.WorkEffort);
      this.object.Assignment = this.workEffort;
      this.object.Quantity = 1;
    } else {
      this.selectedPurchaseOrder = this.object.PurchaseOrder;
      this.workEffort = this.object.Assignment;

      this.purchaseOrders.push(this.selectedPurchaseOrder);
      this.purchaseOrderSelected(this.selectedPurchaseOrder);
    }
  }

  public purchaseOrderSelected(purchaseOrder: PurchaseOrder): void {
    this.purchaseOrderItems = purchaseOrder.PurchaseOrderItems?.filter(
      (v) =>
        v.InvoiceItemType.UniqueId == 'f2d9770b-f933-48b0-a495-df80cb702fce'
    );
  }
}
