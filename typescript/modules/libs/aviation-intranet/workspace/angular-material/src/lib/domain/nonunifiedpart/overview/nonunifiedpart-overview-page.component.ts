import { Component, Self } from '@angular/core';
import { ActivatedRoute } from '@angular/router';
import {
  RefreshService,
  SharedPullService,
  WorkspaceService,
} from '@allors/base/workspace/angular/foundation';
import {
  NavigationService,
  PanelService,
  ScopedService,
  AllorsOverviewPageComponent,
} from '@allors/base/workspace/angular/application';
import { AllorsMaterialPanelService } from '@allors/base/workspace/angular-material/application';
import { M } from '@allors/default/workspace/meta';
import { Pull, IPullResult, Path } from '@allors/system/workspace/domain';
import { NonUnifiedPart, Part } from '@allors/default/workspace/domain';

@Component({
  templateUrl: './nonunifiedpart-overview-page.component.html',
  providers: [
    ScopedService,
    {
      provide: PanelService,
      useClass: AllorsMaterialPanelService,
    },
  ],
})
export class NonUnifiedPartOverviewPageComponent extends AllorsOverviewPageComponent {
  title = 'Part';

  object: Part;
  m: M;
  nonSerialisedInventoryItemTarget: Path;
  serialisedInventoryItemTarget: Path;
  workOrderTarget: Path;
  purchaseOrderTarget: Path;
  purchaseInvoiceTarget: Path;
  salesOrderTarget: Path;
  salesInvoiceTarget: Path;
  quoteTarget: Path;
  shipmentTarget: Path;

  serialised: () => boolean;
  nonSerialised: () => boolean;

  part: NonUnifiedPart;

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

    this.nonSerialisedInventoryItemTarget = p.NonUnifiedPart({
      InventoryItemsWherePart: {},
      ofType: m.NonSerialisedInventoryItem,
    });

    this.serialisedInventoryItemTarget = p.NonUnifiedPart({
      InventoryItemsWherePart: {},
      ofType: m.SerialisedInventoryItem,
    });

    this.workOrderTarget = p.Part({
      InventoryItemsWherePart: {
        WorkEffortInventoryAssignmentsWhereInventoryItem: {
          Assignment: {},
        },
      },
    });

    this.purchaseOrderTarget = p.UnifiedProduct({
      Part_PurchaseOrderItemsWherePart: {
        PurchaseOrderWherePurchaseOrderItem: {},
      },
    });

    this.purchaseInvoiceTarget = p.UnifiedProduct({
      Part_PurchaseInvoiceItemsWherePart: {
        PurchaseInvoiceWherePurchaseInvoiceItem: {},
      },
    });

    this.salesOrderTarget = p.UnifiedProduct({
      SalesOrderItemsWhereProduct: {
        SalesOrderWhereSalesOrderItem: {},
      },
    });

    this.salesInvoiceTarget = p.UnifiedProduct({
      SalesInvoiceItemsWhereProduct: {
        SalesInvoiceWhereSalesInvoiceItem: {},
      },
    });

    this.quoteTarget = p.UnifiedProduct({
      QuoteItemsWhereProduct: {
        QuoteWhereQuoteItem: {},
      },
    });

    this.shipmentTarget = p.UnifiedProduct({
      Part_ShipmentItemsWherePart: {
        ShipmentWhereShipmentItem: {},
      },
    });

    this.serialised = () =>
      this.part.InventoryItemKind.UniqueId ===
      '2596e2dd-3f5d-4588-a4a2-167d6fbe3fae';

    this.nonSerialised = () => !this.serialised();
  }

  onPreSharedPull(pulls: Pull[], prefix?: string) {
    const {
      m: { pullBuilder: p },
    } = this;

    pulls.push(
      p.NonUnifiedPart({
        name: prefix,
        objectId: this.scoped.id,
        include: {
          InventoryItemKind: {},
        },
      })
    );
  }

  onPostSharedPull(pullResult: IPullResult, prefix?: string) {
    this.part = pullResult.object<NonUnifiedPart>(prefix);
  }
}
