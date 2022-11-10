import { Component, OnDestroy } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { of, Subscription, switchMap, pipe, map, tap } from 'rxjs';

import {
  AuthenticationService,
  ContextService,
  SingletonId,
} from '@allors/base/workspace/angular/foundation';
import { CustomerOrganisationId } from '@allors/aviation-extranet/workspace/angular-material';
import { M } from '@allors/default/workspace/meta';
import { Party, Singleton } from '@allors/default/workspace/domain';

@Component({
  templateUrl: './login.component.html',
})
export class LoginComponent implements OnDestroy {
  public loginForm = this.formBuilder.group({
    userName: ['', Validators.required],
    password: ['', Validators.required],
  });

  readonly m: M;

  private subscription: Subscription;

  constructor(
    private authService: AuthenticationService,
    private router: Router,
    public formBuilder: FormBuilder,
    private customerOrganisationId: CustomerOrganisationId,
    private singletonId: SingletonId,
    private allors: ContextService
  ) {
    this.m = allors.metaPopulation as M;
  }

  public login() {
    const { m } = this;
    const { pullBuilder: p } = m;

    const userName = this.loginForm.controls['userName'].value;
    const password = this.loginForm.controls['password'].value;

    if (this.subscription) {
      this.subscription.unsubscribe();
    }

    this.subscription = this.authService
      .login$(userName, password)
      .pipe(
        switchMap((result) => {
          if (result.a) {
            const pulls = [
              p.Person({
                objectId: result.u,
                select: {
                  PartiesWhereCurrentContact: {},
                },
              }),
              p.Singleton({}),
            ];

            return this.allors.context.pull(pulls).pipe(
              tap((loaded) => {
                const organisation = loaded.collection<Party>(
                  m.Person.PartiesWhereCurrentContact
                )[0];
                this.customerOrganisationId.value = organisation.strategy.id;
                const singleton = loaded.collection<Singleton>(m.Singleton)[0];
                this.singletonId.value = singleton.strategy.id;
              }),
              map((v) => true)
            );
          } else {
            return of(false);
          }
        })
      )
      .subscribe(
        (authenticated) => {
          if (authenticated) {
            this.router.navigate(['/']);
          } else {
            alert('Could not log in');
          }
        },
        (error) => alert(JSON.stringify(error))
      );
  }

  public ngOnDestroy() {
    if (this.subscription) {
      this.subscription.unsubscribe();
    }
  }
}
