import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult } from '@allors/system/workspace/domain';
import {
  Enumeration,
  GenderType,
  Person,
  Salutation,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
  SingletonId,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

@Component({
  templateUrl: './user-form.component.html',
  providers: [ContextService],
})
export class UserFormComponent extends AllorsFormComponent<Person> {
  public m: M;

  person: Person;
  genders: Enumeration[];
  salutations: Enumeration[];

  public confirmPassword: string;

  constructor(
    @Self() public allors: ContextService,
    errorService: ErrorService,
    form: NgForm,
    private singletonId: SingletonId
  ) {
    super(allors, errorService, form);
    this.m = allors.metaPopulation as M;
  }

  onPrePull(pulls: Pull[]): void {
    const { m } = this;
    const { pullBuilder: p } = m;

    if (this.editRequest) {
      pulls.push(
        p.Person({
          name: '_object',
          objectId: this.editRequest.objectId,
          include: {
            Gender: {},
            Salutation: {},
            Picture: {},
          },
        }),
        p.GenderType({
          predicate: {
            kind: 'Equals',
            propertyType: m.GenderType.IsActive,
            value: true,
          },
          sorting: [{ roleType: m.GenderType.Name }],
        }),
        p.Salutation({
          predicate: {
            kind: 'Equals',
            propertyType: m.Salutation.IsActive,
            value: true,
          },
          sorting: [{ roleType: m.Salutation.Name }],
        })
      );
    }

    this.onPrePullInitialize(pulls);
  }

  onPostPull(pullResult: IPullResult) {
    const { m } = this;

    this.object = this.editRequest
      ? pullResult.object('_object')
      : this.context.create(this.createRequest.objectType);

    this.onPostPullInitialize(pullResult);

    this.person = pullResult.object<Person>(m.Person);
    this.genders = pullResult.collection<GenderType>(m.GenderType);
    this.salutations = pullResult.collection<Salutation>(m.Salutation);
  }
}
