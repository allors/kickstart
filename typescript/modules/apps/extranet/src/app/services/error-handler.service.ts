import { ErrorHandler, Injectable, Injector } from '@angular/core';
import { Router } from '@angular/router';
import { JL } from 'jsnlog';

@Injectable()
export class ErrorHandlerService implements ErrorHandler {
  constructor(private injector: Injector) {}

  handleError(error: any) {
    // if (error instanceof SyntaxError) {
    //   return;
    // }

    // const message: string = error && error.message;
    // if (message.startsWith('ExpressionChangedAfterItHasBeenCheckedError')) {
    //   return;
    // }

    JL().fatalException('Uncaught Exception', error);
    const router = this.injector.get(Router);
    router.navigate(['login']);
  }
}
