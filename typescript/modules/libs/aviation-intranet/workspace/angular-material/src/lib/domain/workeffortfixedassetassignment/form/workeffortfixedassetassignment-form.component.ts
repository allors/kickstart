import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult, IObject } from '@allors/system/workspace/domain';
import {
  AssetAssignmentStatus,
  Enumeration,
  FixedAsset,
  OperatingHoursTransaction,
  Organisation,
  Party,
  SerialisedItem,
  Vehicle,
  WorkEffort,
  WorkEffortFixedAssetAssignment,
  WorkTask,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
  SearchFactory,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';
import { Filters } from '@allors/apps-intranet/workspace/angular-material';

@Component({
  templateUrl: './workeffortfixedassetassignment-form.component.html',
  providers: [ContextService],
})
export class WorkEffortFixedAssetAssignmentFormComponent extends AllorsFormComponent<WorkEffortFixedAssetAssignment> {
  readonly m: M;
  workTask: WorkTask;
  assignment: WorkEffort;
  serialisedItem: SerialisedItem;
  assetAssignmentStatuses: Enumeration[];
  transaction: OperatingHoursTransaction;
  title: string;
  externalCustomer: boolean;
  fixedAssets: FixedAsset[];
  serialisedItemsFilter: SearchFactory;
  isSerialisedItem: boolean;
  isVehicle: boolean;
  vehicle: Vehicle;
  addVehicle = false;

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;

    this.serialisedItemsFilter = Filters.serialisedItemsFilter(this.m);
  }

  onPrePull(pulls: Pull[]): void {
    const { m } = this;
    const { pullBuilder: p } = m;

    pulls.push(
      p.AssetAssignmentStatus({
        predicate: {
          kind: 'Equals',
          propertyType: m.AssetAssignmentStatus.IsActive,
          value: true,
        },
        sorting: [{ roleType: m.AssetAssignmentStatus.Name }],
      })
    );

    if (this.editRequest) {
      pulls.push(
        p.WorkEffortFixedAssetAssignment({
          name: '_object',
          objectId: this.editRequest.objectId,
          include: {
            Assignment: {},
            FixedAsset: {},
            AssetAssignmentStatus: {},
          },
        })
      );
    }

    const initializer = this.createRequest?.initializer;
    if (initializer) {
      pulls.push(
        p.WorkTask({
          objectId: initializer.id,
          include: {
            Customer: {},
            MaintenanceAgreement: {},
          },
        })
      );
    }
  }

  onPostPull(pullResult: IPullResult) {
    this.object = this.editRequest
      ? pullResult.object('_object')
      : this.context.create(this.createRequest.objectType);

    this.assetAssignmentStatuses = pullResult.collection<AssetAssignmentStatus>(
      this.m.AssetAssignmentStatus
    );

    this.workTask = pullResult.object<WorkTask>(this.m.WorkTask);

    this.serialisedItem = this.object?.FixedAsset as SerialisedItem;

    if (this.serialisedItem === null) {
      const b2bCustomer = this.workTask.Customer as Organisation;
      this.externalCustomer =
        b2bCustomer === null || !b2bCustomer.IsInternalOrganisation;

      if (this.externalCustomer) {
        this.updateFixedAssets(this.workTask.Customer);
      }
    }

    if (this.createRequest) {
      if (this.serialisedItem !== undefined) {
        this.object.FixedAsset = this.serialisedItem;
      }

      if (
        this.workTask !== undefined &&
        this.workTask.strategy.cls === this.m.WorkTask
      ) {
        this.assignment = this.workTask as WorkEffort;
        this.object.Assignment = this.assignment;
      }
    } else {
      this.fixedAssetSelected(this.object.FixedAsset);
    }
  }
  public override save(): void {
    this.onSave();
    super.save();
  }

  private onSave() {
    if (this.object.OperatingHours) {
      if (this.transaction === undefined) {
        this.transaction =
          this.allors.context.create<OperatingHoursTransaction>(
            this.m.OperatingHoursTransaction
          );
        const serialisedItem = this.object.FixedAsset as SerialisedItem;
        this.transaction.SerialisedItem = serialisedItem;
        this.transaction.RecordingDate = new Date();
        this.transaction.Value = this.object.OperatingHours;

        const previousTransaction =
          serialisedItem.SyncedOperatingHoursTransactions.sort((a, b) =>
            a.CreationDate < b.CreationDate
              ? 1
              : b.CreationDate < a.CreationDate
              ? -1
              : 0
          )[0];
        this.transaction.PreviousTransaction = previousTransaction;
      } else {
        this.transaction.Value = this.object.OperatingHours;
      }
    }
  }

  public fixedAssetSelected(fixedAsset: IObject): void {
    this.isSerialisedItem = fixedAsset.strategy.cls === this.m.SerialisedItem;
    this.isVehicle = fixedAsset.strategy.cls === this.m.Vehicle;
    this.vehicle = fixedAsset as Vehicle;
  }

  private updateFixedAssets(customer: Party) {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    const pullName1 = 'serialisedItems-1';
    const pullName2 = 'serialisedItems-2';
    const pullName3 = 'vehicles';

    const pulls = [
      pull.Party({
        object: customer,
        results: [
          {
            name: pullName1,
            select: {
              SerialisedItemsWhereRentedBy: {
                include: {
                  PartWhereSerialisedItem: {
                    ProductCategoriesWhereProduct: {
                      PrimaryParent: {},
                    },
                  },
                },
              },
            },
          },
          {
            name: pullName2,
            select: {
              SerialisedItemsWhereOwnedBy: {
                include: {
                  PartWhereSerialisedItem: {
                    ProductCategoriesWhereProduct: {
                      PrimaryParent: {},
                    },
                  },
                },
              },
            },
          },
          {
            name: pullName3,
            select: {
              VehiclesWhereOwnedBy: x,
            },
          },
        ],
      }),
    ];

    this.allors.context.pull(pulls).subscribe((pullResult) => {
      const serialisedItems1 =
        pullResult.collection<FixedAsset>(pullName1) ?? [];
      const serialisedItems2 =
        pullResult.collection<FixedAsset>(pullName2) ?? [];
      let serialisedItems = serialisedItems1.concat(serialisedItems2);
      const vehicles = pullResult.collection<FixedAsset>(pullName3) ?? [];

      if (this.workTask.MaintenanceAgreement) {
        const filter1 = serialisedItems?.filter((v: SerialisedItem) => {
          return (
            this.workTask.MaintenanceAgreement.WorkEffortType.UnifiedGood
              ?.id === v.PartWhereSerialisedItem.id
          );
        });

        const filter2 = serialisedItems?.filter((v: SerialisedItem) => {
          return (
            this.workTask.MaintenanceAgreement.WorkEffortType.ProductCategory
              ?.id ===
            v.PartWhereSerialisedItem?.ProductCategoriesWhereProduct[0]?.id
          );
        });

        const filter3 = serialisedItems?.filter((v: SerialisedItem) => {
          return (
            this.workTask.MaintenanceAgreement.WorkEffortType.ProductCategory
              ?.id ===
            v.PartWhereSerialisedItem?.ProductCategoriesWhereProduct[0]
              ?.PrimaryParent?.id
          );
        });

        serialisedItems = filter1.concat(filter2).concat(filter3);
      }

      this.fixedAssets = serialisedItems
        .concat(vehicles)
        .sort((a, b) =>
          a.DisplayName > b.DisplayName
            ? 1
            : b.DisplayName > a.DisplayName
            ? -1
            : 0
        );
    });
  }

  public vehicleAdded(vehicle: Vehicle): void {
    this.fixedAssets.push(vehicle);
    this.object.FixedAsset = vehicle;
    vehicle.OwnedBy = this.workTask.Customer;
  }
}
