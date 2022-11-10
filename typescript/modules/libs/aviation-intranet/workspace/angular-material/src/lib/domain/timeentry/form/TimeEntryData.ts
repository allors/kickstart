import { ObjectData } from '@allors/workspace/angular/base';

export interface TimeEntryData extends ObjectData {
  workerId?: string;
  fromDate?: Date;
}
