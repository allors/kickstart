import { Observable } from 'rxjs';
import { map, tap, filter } from 'rxjs/operators';
import { Injectable } from '@angular/core';
import {
  AllorsBarcodeService,
  AuthenticationService,
} from '@allors/base/workspace/angular/foundation';
import { Command } from './command';

@Injectable()
export class AllorsCommandService {
  command$: Observable<Command>;

  constructor(
    barcodeService: AllorsBarcodeService,
    authenticationService: AuthenticationService
  ) {
    this.command$ = barcodeService.scan$.pipe(
      map((v) => {
        if (v.startsWith('*')) {
          const [name, ...args] = v.split('|');
          return { name: name.substring(1), args };
        } else {
          return { name: 'barcode', args: [v] };
        }
      }),
      tap((command) => {
        if (command.name === 'logout') {
          authenticationService.logout();
        }
      }),
      filter((command) => {
        return !(command.name === 'logout');
      })
    );
  }
}
