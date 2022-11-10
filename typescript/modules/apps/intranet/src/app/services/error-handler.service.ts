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

    const message: string = error && error.message;
    if (message.includes('ExpressionChangedAfterItHasBeenCheckedError')) {
      return;
    }

    console.error(error);

    JL().fatalException('Uncaught Exception', error);

    alert(`Application will restart
    
${error}  
    `);

    window.location.href = '/';
  }
}
