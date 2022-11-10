import { Component, OnDestroy, OnInit } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import { map, of, Subscription, switchMap, tap } from 'rxjs';

import {
  AuthenticationService,
  ContextService,
  SingletonId,
} from '@allors/base/workspace/angular/foundation';
import { M } from '@allors/default/workspace/meta';
import {
  Organisation,
  Person,
  Singleton,
} from '@allors/default/workspace/domain';
import { InternalOrganisationId } from '@allors/apps-intranet/workspace/angular-material';
import { AllorsCommandService } from '@allors/aviation-intranet/workspace/angular-material';
import { MenuService } from '@allors/base/workspace/angular/application';
import { AppMenuService } from '../services/menu.service';

@Component({
  templateUrl: './login.component.html',
})
export class LoginComponent implements OnInit, OnDestroy {
  public loginForm = this.formBuilder.group({
    userName: ['', Validators.required],
    password: ['', Validators.required],
  });

  readonly m: M;

  busy: boolean;

  private subscription: Subscription;
  private commandSubscription: Subscription;
  private returnUrl: string;

  constructor(
    private authService: AuthenticationService,
    private route: ActivatedRoute,
    private router: Router,
    private allors: ContextService,
    private internalOrganisationId: InternalOrganisationId,
    private singletonId: SingletonId,
    public formBuilder: FormBuilder,
    public commandService: AllorsCommandService,
    private menuService: MenuService
  ) {
    this.m = allors.metaPopulation as M;
    this.busy = false;
  }

  public ngOnInit(): void {
    this.returnUrl = this.route.snapshot.queryParams['returnUrl'] || '/';

    this.commandSubscription = this.commandService.command$.subscribe(
      (command) => {
        if (command.name === 'login') {
          const [userName, password] = command.args;
          this.doLogin(userName + '@aviaco-gse.com', password);
        }
      }
    );
  }

  public login() {
    const userName = this.loginForm.controls['userName'].value;
    const password = this.loginForm.controls['password'].value;

    this.doLogin(userName, password);
  }

  private doLogin(userName: string, password: string) {
    const { m } = this;
    const { pullBuilder: p } = m;

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
                include: {
                  UserProfile: {
                    DefaultInternalOrganization: {},
                  },
                },
              }),
              p.Organisation({
                predicate: {
                  kind: 'Equals',
                  propertyType: m.Organisation.IsInternalOrganisation,
                  value: true,
                },
              }),
              p.Singleton({
                include: {
                  Settings: {},
                },
              }),
            ];

            return this.allors.context.pull(pulls).pipe(
              tap((loaded) => {
                const person = loaded.object<Person>(m.Person);
                const internalOrganisations = loaded.collection<Organisation>(
                  m.Organisation
                );

                const internalOrganisation =
                  person.UserProfile?.DefaultInternalOrganization ??
                  internalOrganisations.find((v) => v.canExecuteShowInMenu);

                this.internalOrganisationId.value =
                  internalOrganisation?.strategy.id;

                const singleton = loaded.collection<Singleton>(m.Singleton)[0];
                this.singletonId.value = singleton.strategy.id;
              }),
              map(() => true)
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
