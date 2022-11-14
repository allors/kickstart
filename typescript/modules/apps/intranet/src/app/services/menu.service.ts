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
        children: [{ objectType: m.Person }, { objectType: m.Organisation }],
      },
      {
        title: 'Admin',
        icon: 'admin_panel_settings',
        children: [{ objectType: m.EmailMessage }],
      },
    ];
  }

  menu(): MenuItem[] {
    return this._fullMenu;
  }
}
