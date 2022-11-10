import { Component, Self } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import {
  Organisation,
  Person,
  SupplierOffering,
  Vehicle,
} from '@allors/default/workspace/domain';
import {
  RefreshService,
  SharedPullService,
  UserId,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import {
  NavigationService,
  PanelService,
  ScopedService,
  AllorsOverviewPageComponent,
} from '@allors/base/workspace/angular/application';
import { IPullResult, Path, Pull } from '@allors/system/workspace/domain';
import { AllorsMaterialPanelService } from '@allors/base/workspace/angular-material/application';
import { M } from '@allors/default/workspace/meta';
import { PropertyType } from '@allors/system/workspace/meta';

@Component({
  templateUrl: './organisation-overview-page.component.html',
  providers: [
    ScopedService,
    {
      provide: PanelService,
      useClass: AllorsMaterialPanelService,
    },
  ],
})
export class OrganisationOverviewPageComponent extends AllorsOverviewPageComponent {
  m: M;
  object: Organisation;
  supplierOfferings: SupplierOffering[];
  vehicles: Vehicle[];

  contactMechanismTarget: Path;
  serialisedItemTarget: PropertyType[];

  hasSupplierOfferings: () => boolean;
  hasVehicles: () => boolean;

  constructor(
    @Self() scopedService: ScopedService,
    @Self() panelService: PanelService,
    public navigation: NavigationService,
    sharedPullService: SharedPullService,
    refreshService: RefreshService,
    route: ActivatedRoute,
    workspaceService: WorkspaceService
  ) {
    super(
      scopedService,
      panelService,
      sharedPullService,
      refreshService,
      route,
      workspaceService
    );
    this.m = workspaceService.workspace.configuration.metaPopulation as M;
    const { m } = this;
    const { pathBuilder: p } = this.m;

    this.contactMechanismTarget = p.Party({
      CurrentPartyContactMechanisms: { ContactMechanism: {} },
    });

    this.serialisedItemTarget = [
      m.Party.SerialisedItemsWhereOwnedBy,
      m.Party.SerialisedItemsWhereRentedBy,
    ];
    this.hasSupplierOfferings = () => this.supplierOfferings?.length > 0;
    this.hasVehicles = () => this.vehicles?.length > 0;
  }

  onPreSharedPull(pulls: Pull[], prefix?: string) {
    const {
      m: { pullBuilder: p },
    } = this;

    pulls.push(
      p.Organisation({
        name: prefix,
        objectId: this.scoped.id,
        include: {
          SupplierOfferingsWhereSupplier: {},
          VehiclesWhereOwnedBy: {},
        },
      })
    );
  }

  onPostSharedPull(pullResult: IPullResult, prefix?: string) {
    this.object = pullResult.object(prefix);
    this.supplierOfferings = this.object.SupplierOfferingsWhereSupplier;
    this.vehicles = this.object.VehiclesWhereOwnedBy;
  }
}
