import { Person, TimeEntry } from '@allors/default/workspace/domain';
import { Injectable } from '@angular/core';
import { CalendarUtils, CalendarEvent } from 'angular-calendar';
import { WeekView, GetWeekViewArgs } from 'calendar-utils';
import { isAfter } from 'date-fns';

export interface MetaType {
  timeEntry: TimeEntry;
}

export interface SchedulerView extends WeekView {
  people: Person[];
}

interface SchedulerViewArgs extends GetWeekViewArgs {
  events?: CalendarEvent<MetaType>[];
}

@Injectable()
export class SchedulerCalendarUtils extends CalendarUtils {
  override getWeekView(args: SchedulerViewArgs): SchedulerView {
    const { period } = super.getWeekView(args);

    const people = [
      ...new Set(
        args.events
          .map((v) => v.meta?.timeEntry?.Worker)
          .filter(
            (v) =>
              v.EmploymentsWhereEmployee.length > 0 &&
              (!v.EmploymentsWhereEmployee[0].ThroughDate ||
                isAfter(
                  new Date(v.EmploymentsWhereEmployee[0].ThroughDate),
                  period.start
                ))
          )
      ),
    ];

    const view: SchedulerView = {
      period,
      allDayEventRows: [],
      hourColumns: [],
      people,
    };

    view.people.forEach((user, columnIndex) => {
      const events = args.events.filter(
        (event) => event.meta?.timeEntry?.Worker?.id === user.id
      );
      const columnView = super.getWeekView({
        ...args,
        events,
      });
      view.hourColumns.push(columnView.hourColumns[0]);
    });

    return view;
  }
}
