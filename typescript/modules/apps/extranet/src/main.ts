import { enableProdMode } from '@angular/core';
import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';
import { JL } from 'jsnlog';

import { AppModule } from './app/app.module';
import { environment } from './environments/environment';

JL.setOptions({ defaultAjaxUrl: environment.baseUrl + 'jsnlog.logger' });

if (environment.production) {
  enableProdMode();
}

platformBrowserDynamic()
  .bootstrapModule(AppModule)
  .catch((err) => console.error(err));
