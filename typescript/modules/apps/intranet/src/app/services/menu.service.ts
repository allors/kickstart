import {
  MenuItem,
  MenuService,
} from '@allors/base/workspace/angular/application';
import { WorkspaceService } from '@allors/base/workspace/angular/foundation';
import { M } from '@allors/default/workspace/meta';
import { Injectable } from '@angular/core';

@Injectable()
export class AppMenuService implements MenuService {
  private _fullMenu: MenuItem[];

  constructor(workspaceService: WorkspaceService) {
    const m = workspaceService.workspace.configuration.metaPopulation as M;
    this._fullMenu = [
      { title: 'Home', icon: 'home', link: '/' },
      {
        title: 'Contacts',
        icon: 'group',
        children: [
          { objectType: m.Person },
          { objectType: m.Organisation },
          { objectType: m.CommunicationEvent },
        ],
      },
      {
        title: 'Products',
        icon: 'label',
        children: [
          { objectType: m.SerialisedItem, title: 'Serialised Assets' },
          { objectType: m.UnifiedGood, title: 'Products' },
          { objectType: m.NonUnifiedPart, title: 'Spare Parts' },
          { objectType: m.Vehicle },
          { objectType: m.ProductCategory, title: 'Product Categories' },
          { objectType: m.PartCategory, title: 'Part Categories' },
          {
            objectType: m.SerialisedItemCharacteristic,
            title: 'Characteristics',
          },
          { objectType: m.Brand },
          { objectType: m.Model },
          { objectType: m.ProductType },
          { objectType: m.Facility },
        ],
      },
      {
        title: 'Sales',
        icon: 'credit_card',
        children: [
          { objectType: m.RequestForQuote },
          { objectType: m.ProductQuote },
          { objectType: m.SalesOrder },
          { objectType: m.SalesInvoice },
        ],
      },
      {
        title: 'Purchasing',
        icon: 'local_shipping',
        children: [
          { objectType: m.PurchaseOrder },
          { objectType: m.PurchaseInvoice },
        ],
      },
      {
        title: 'Shipments',
        icon: 'local_shipping',
        children: [{ objectType: m.Shipment }, { objectType: m.Carrier }],
      },
      {
        title: 'WorkEfforts',
        icon: 'schedule',
        children: [
          { objectType: m.WorkRequirement, title: 'Service Requests' },
          { objectType: m.WorkEffort, title: 'Work Orders' },
          { objectType: m.WorkEffortType, title: 'Work Order Types' },
          { objectType: m.MaintenanceAgreement, title: 'Service Agreements' },
          { link: '/workflow/timesheet', title: 'Timesheets' },
        ],
      },
      {
        title: 'HR',
        icon: 'group',
        children: [
          { objectType: m.PositionType },
          { objectType: m.PositionTypeRate },
        ],
      },
      {
        title: 'Accounting',
        icon: 'money',
        children: [{ objectType: m.ExchangeRate }],
      },
      {
        title: 'Admin',
        icon: 'admin_panel_settings',
        children: [
          { objectType: m.Settings, title: 'Settings' },
          { objectType: m.EmailMessage },
        ],
      },
    ];
  }

  menu(): MenuItem[] {
    return this._fullMenu;
  }
}
