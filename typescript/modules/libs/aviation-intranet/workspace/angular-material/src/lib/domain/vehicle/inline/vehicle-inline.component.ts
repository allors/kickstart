import {
  Component,
  Output,
  EventEmitter,
  OnInit,
  OnDestroy,
} from '@angular/core';

import { M } from '@allors/default/workspace/meta';
import { Vehicle } from '@allors/default/workspace/domain';
import { ContextService } from '@allors/base/workspace/angular/foundation';

@Component({
  selector: 'vehicle-inline',
  templateUrl: './vehicle-inline.component.html',
})
export class VehicleInlineComponent implements OnInit, OnDestroy {
  @Output()
  public saved: EventEmitter<Vehicle> = new EventEmitter<Vehicle>();

  @Output()
  public cancelled: EventEmitter<any> = new EventEmitter();

  public m: M;
  vehicle: Vehicle;

  constructor(private allors: ContextService) {
    this.m = this.allors.context.configuration.metaPopulation as M;
  }

  public ngOnInit(): void {
    const m = this.m;
    const { pullBuilder: pull } = m;

    const pulls = [];

    this.allors.context.pull(pulls).subscribe((loaded) => {
      this.vehicle = this.allors.context.create<Vehicle>(m.Vehicle);
    });
  }

  public ngOnDestroy(): void {
    if (this.vehicle) {
      this.vehicle.strategy.delete();
    }
  }

  public cancel(): void {
    this.cancelled.emit();
  }

  public save(): void {
    this.saved.emit(this.vehicle);
    this.vehicle = undefined;
  }
}
