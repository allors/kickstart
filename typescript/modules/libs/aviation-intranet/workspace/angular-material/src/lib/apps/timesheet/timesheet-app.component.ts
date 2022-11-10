import { Component, Self, OnInit, OnDestroy } from '@angular/core';
import {
  CalendarEvent,
  CalendarEventTimesChangedEvent,
  CalendarView,
} from 'angular-calendar';
import { Title } from '@angular/platform-browser';
import { Subscription, combineLatest } from 'rxjs';
import { switchMap } from 'rxjs/operators';

import { colors } from './colors';
import { MetaType } from './scheduler/scheduler-calendar-utils';
import {
  ContextService,
  EditDialogService,
  ErrorService,
  MetaService,
  RefreshService,
} from '@allors/base/workspace/angular/foundation';
import { InternalOrganisationId } from '@allors/apps-intranet/workspace/angular-material';
import { M } from '@allors/default/workspace/meta';
import { TimeEntry } from '@allors/default/workspace/domain';
import { WorkEffortStateInProgressId } from '../constants';

const DAY_START = 6;
const DAY_STOP = 19;

@Component({
  templateUrl: 'timesheet-app.component.html',
  providers: [ContextService],
})
export class TimesheetAppComponent implements OnInit, OnDestroy {
  title = 'Timesheet';

  viewDate = new Date();
  calendarView = CalendarView.Day;

  officeHours = true;

  dayStartHour = DAY_START;
  dayEndHour = DAY_STOP;
  hourSegments = 4;

  events: CalendarEvent<MetaType>[] = [];

  readonly m: M;

  private subscription: Subscription;

  constructor(
    @Self() public allors: ContextService,
    public metaService: MetaService,
    public refreshService: RefreshService,
    private errorService: ErrorService,
    private editService: EditDialogService,
    private internalOrganisationId: InternalOrganisationId,
    titleService: Title
  ) {
    this.m = allors.metaPopulation as M;
    titleService.setTitle(this.title);
  }

  ngOnInit(): void {
    const { m } = this;
    const { pullBuilder: p } = m;

    this.subscription = combineLatest(
      this.refreshService.refresh$,
      this.internalOrganisationId.observable$
    )
      .pipe(
        switchMap(([, internalOrginsationId]) => {
          const pulls = [
            p.TimeEntry({
              name: 'timesheet',
              predicate: {
                kind: 'ContainedIn',
                propertyType: m.TimeEntry.WorkEffort,
                extent: {
                  kind: 'Filter',
                  objectType: m.WorkEffort,
                  predicate: {
                    kind: 'Equals',
                    propertyType: m.WorkEffort.ExecutedBy,
                    objectId: internalOrginsationId,
                  },
                },
              },
              include: {
                Worker: {
                  EmploymentsWhereEmployee: {},
                },
                WorkEffort: {
                  WorkEffortState: {},
                },
              },
            }),
          ];

          return this.allors.context.pull(pulls);
        })
      )
      .subscribe((loaded) => {
        this.allors.context.reset();

        const timeEntries = loaded.collection<TimeEntry>('timesheet') ?? [];

        this.events = timeEntries.map((timeEntry) => {
          const workEffort = timeEntry.WorkEffort;
          const workEffortState = workEffort.WorkEffortState;

          const inProgress =
            workEffortState?.UniqueId == WorkEffortStateInProgressId;

          const editable = inProgress;

          const title = timeEntry.WorkEffort?.WorkEffortNumber;
          const start = new Date(timeEntry.FromDate);
          const end = timeEntry.ThroughDate
            ? new Date(timeEntry.ThroughDate)
            : null;

          const draggable = editable;

          const resizable = {
            beforeStart: editable,
            afterEnd: !!end,
          };

          const meta: MetaType = {
            timeEntry,
          };

          const color = inProgress
            ? end
              ? colors['yellow']
              : colors['red']
            : colors['blue'];

          return { title, start, end, draggable, resizable, meta, color };
        });
      });
  }

  ngOnDestroy(): void {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }

  officeHoursChanged(event) {
    this.dayStartHour = this.officeHours ? DAY_START : 0;
    this.dayEndHour = this.officeHours ? DAY_STOP : 24;
  }

  eventClicked({ event }: { event: CalendarEvent }): void {
    if (event.meta) {
      const timeEntry = event.meta.timeEntry as TimeEntry;

      this.editService
        .edit({
          kind: 'EditRequest',
          objectId: timeEntry.id,
          objectType: timeEntry.strategy.cls,
        })
        .subscribe(() => this.refreshService.refresh());
    }
  }

  eventTimesChanged({
    event: {
      meta: { timeEntry },
    },
    newStart,
    newEnd,
  }: CalendarEventTimesChangedEvent<MetaType>) {
    timeEntry.FromDate = newStart;
    timeEntry.ThroughDate = timeEntry.ThroughDate ? newEnd : null;
    this.save();
  }

  private save() {
    this.allors.context.push().subscribe(() => {
      this.refreshService.refresh();
    }, this.errorService.errorHandler);
  }
}
