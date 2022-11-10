import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult } from '@allors/system/workspace/domain';
import {
  Party,
  PartyRate,
  Person,
  RateType,
  TimeEntry,
  TimeFrequency,
  TimeSheet,
  WorkEffort,
  WorkEffortAssignmentRate,
  WorkEffortPartyAssignment,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

@Component({
  templateUrl: './timeentry-form.component.html',
  providers: [ContextService],
})
export class TimeEntryFormComponent extends AllorsFormComponent<TimeEntry> {
  readonly m: M;

  frequencies: TimeFrequency[];
  rateTypes: RateType[];
  workers: Party[];
  workEffortAssignmentRates: WorkEffortAssignmentRate[];
  workEfforts: WorkEffort[];

  timeSheet: TimeSheet;
  worker: Person;
  workEffort: WorkEffort;
  workEffortRate: WorkEffortAssignmentRate;
  partyRate: PartyRate;
  derivedBillingRate: number;
  customerRate: PartyRate;

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;
  }

  onPrePull(pulls: Pull[]): void {
    const { m } = this;
    const { pullBuilder: p } = m;

    pulls.push(
      p.RateType({ sorting: [{ roleType: this.m.RateType.Name }] }),
      p.TimeFrequency({
        sorting: [{ roleType: this.m.TimeFrequency.Name }],
      })
    );

    if (this.editRequest) {
      pulls.push(
        p.TimeEntry({
          name: '_object',
          objectId: this.editRequest.objectId,
          include: {
            WorkEffort: {},
            TimeFrequency: {},
            BillingFrequency: {},
          },
        }),
        p.TimeEntry({
          name: 'PartyAssignment',
          objectId: this.editRequest.objectId,
          select: {
            WorkEffort: {
              WorkEffortPartyAssignmentsWhereAssignment: {
                include: {
                  Party: {},
                },
              },
            },
          },
        })
      );
    }

    const initializer = this.createRequest?.initializer;
    if (initializer) {
      pulls.push(
        p.WorkEffort({
          objectId: initializer.id,
        }),
        p.WorkEffort({
          name: 'PartyAssignment',
          objectId: initializer.id,
          select: {
            WorkEffortPartyAssignmentsWhereAssignment: {
              include: {
                Party: {},
              },
            },
          },
        })
      );
    }
  }

  onPostPull(pullResult: IPullResult) {
    this.object = this.editRequest
      ? pullResult.object('_object')
      : this.context.create(this.createRequest.objectType);

    this.rateTypes = pullResult.collection<RateType>(this.m.RateType);
    this.frequencies = pullResult.collection<TimeFrequency>(
      this.m.TimeFrequency
    );
    const hour = this.frequencies?.find(
      (v) => v.UniqueId === 'db14e5d5-5eaf-4ec8-b149-c558a28d99f5'
    );

    if (this.editRequest) {
      this.worker = this.object.Worker;
      this.workEffort = this.object.WorkEffort;

      const workEffortPartyAssignments =
        pullResult.collection<WorkEffortPartyAssignment>('PartyAssignment');
      this.workers = Array.from(
        new Set(workEffortPartyAssignments?.map((v) => v.Party)).values()
      );
    } else {
      this.object.IsBillable = true;
      this.object.BillingFrequency = hour;
      this.object.TimeFrequency = hour;
      this.object.FromDate = new Date();

      this.workEffort = pullResult.object<WorkEffort>(this.m.WorkEffort);
      this.object.WorkEffort = this.workEffort;

      const workEffortPartyAssignments =
        pullResult.collection<WorkEffortPartyAssignment>('PartyAssignment');
      this.workers = Array.from(
        new Set(workEffortPartyAssignments?.map((v) => v.Party)).values()
      );
    }

    if (this.worker) {
      this.workerSelected(this.worker);
    }
  }

  public findBillingRate(): void {
    if (this.worker) {
      this.workerSelected(this.worker);
    }
  }

  public workerSelected(party: Party): void {
    const m = this.m;
    const { pullBuilder: pull } = m;
    const x = {};

    const pulls = [
      pull.Party({
        name: 'TimeSheetWhereWorker',
        objectId: party.id,
        select: {
          Person_TimeSheetWhereWorker: {
            include: {
              TimeEntries: x,
            },
          },
        },
      }),
    ];

    this.allors.context.pull(pulls).subscribe((pullResult) => {
      this.timeSheet = pullResult.object<TimeSheet>('TimeSheetWhereWorker');
    });
  }

  public override save(): void {
    this.onSave();
    super.save();
  }

  private onSave() {
    if (!this.object.TimeSheetWhereTimeEntry) {
      this.timeSheet.addTimeEntry(this.object);
    }

    if (!this.object.WorkEffort) {
      this.object.WorkEffort = this.workEffort;
    }
  }
}
