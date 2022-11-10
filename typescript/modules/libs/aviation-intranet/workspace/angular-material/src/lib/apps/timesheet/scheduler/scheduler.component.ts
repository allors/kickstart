import {
  ChangeDetectorRef,
  Component,
  ElementRef,
  Inject,
  LOCALE_ID,
} from '@angular/core';
import {
  CalendarWeekViewComponent,
  DateAdapter,
  getWeekViewPeriod,
} from 'angular-calendar';
import { CalendarEvent } from 'calendar-utils';

import {
  SchedulerCalendarUtils,
  SchedulerView,
} from './scheduler-calendar-utils';

@Component({
  selector: 'scheduler',
  templateUrl: 'scheduler.component.html',
  providers: [SchedulerCalendarUtils],
})
export class SchedulerComponent extends CalendarWeekViewComponent {
  override daysInWeek = 1;

  override view: SchedulerView;

  constructor(
    cdr: ChangeDetectorRef,
    utils: SchedulerCalendarUtils,
    @Inject(LOCALE_ID) locale: string,
    dateAdapter: DateAdapter,
    element: ElementRef<HTMLElement>
  ) {
    super(cdr, utils, locale, dateAdapter, element);
  }

  override getDayColumnWidth(eventRowContainer: HTMLElement): number {
    return Math.floor(eventRowContainer.offsetWidth / this.view.people.length);
  }

  override getWeekView(events: CalendarEvent[]) {
    return this.utils.getWeekView({
      events,
      viewDate: this.viewDate,
      weekStartsOn: this.weekStartsOn,
      excluded: this.excludeDays,
      precision: this.precision,
      absolutePositionedEvents: true,
      hourSegments: this.hourSegments,
      dayStart: {
        hour: this.dayStartHour,
        minute: this.dayStartMinute,
      },
      dayEnd: {
        hour: this.dayEndHour,
        minute: this.dayEndMinute,
      },
      segmentHeight: this.hourSegmentHeight,
      weekendDays: this.weekendDays,
      ...getWeekViewPeriod(
        this.dateAdapter,
        this.viewDate,
        this.weekStartsOn,
        this.excludeDays,
        this.daysInWeek
      ),
    });
  }
}
