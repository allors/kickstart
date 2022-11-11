import { Component, Self } from '@angular/core';
import { NgForm } from '@angular/forms';

import { Pull, IPullResult } from '@allors/system/workspace/domain';
import {
  Locale,
  Person,
  User,
  UserGroup,
} from '@allors/default/workspace/domain';
import { M } from '@allors/default/workspace/meta';
import {
  ErrorService,
  AllorsFormComponent,
  SingletonId,
} from '@allors/base/workspace/angular/foundation';
import { ContextService } from '@allors/base/workspace/angular/foundation';

@Component({
  selector: 'person-form',
  templateUrl: './person-form.component.html',
  providers: [ContextService],
})
export class PersonFormComponent extends AllorsFormComponent<Person> {
  readonly m: M;

  userGroups: UserGroup[];
  selectedUserGroups: UserGroup[] = [];

  locales: Locale[];

  public confirmPassword: string;

  canWriteMembers: boolean;
  creatorUserGroup: UserGroup;

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

    pulls.push(
      p.Singleton({
        objectId: this.singletonId.value,
        select: {
          Locales: {
            include: {
              Language: {},
              Country: {},
            },
          },
        },
      }),
      p.UserGroup({
        predicate: {
          kind: 'Equals',
          propertyType: m.UserGroup.IsSelectable,
          value: true,
        },
        include: {
          InMembers: {},
          OutMembers: {},
        },
        sorting: [{ roleType: m.Role.Name }],
      }),
      p.UserGroup({
        name: 'CreatorUserGroup',
        predicate: {
          kind: 'Equals',
          propertyType: m.UserGroup.UniqueId,
          value: 'f0d8132b-79d6-4a30-a866-ef6e5c952761',
        },
        include: {
          InMembers: {},
          OutMembers: {},
        },
      })
    );

    if (this.editRequest) {
      pulls.push(
        p.Person({
          name: '_object',
          objectId: this.editRequest.objectId,
          include: {
            Locale: {},
          },
        }),
        p.Person({
          name: 'ActiveUserGroups',
          objectId: this.editRequest.objectId,
          select: {
            UserGroupsWhereMember: {},
          },
        })
      );
    }

    this.onPrePullInitialize(pulls);
  }
  onPostPull(pullResult: IPullResult) {
    this.object = this.editRequest
      ? pullResult.object('_object')
      : this.context.create(this.createRequest.objectType);

    this.onPostPullInitialize(pullResult);

    this.locales =
      pullResult.collection<Locale>(this.m.Singleton.Locales) || [];

    this.userGroups = pullResult.collection<UserGroup>(this.m.UserGroup);
    this.creatorUserGroup =
      pullResult.collection<UserGroup>('CreatorUserGroup')[0];
    this.canWriteMembers = this.creatorUserGroup.canWriteMembers;

    // if (this.createRequest) {
    // }

    if (this.editRequest) {
      this.selectedUserGroups =
        pullResult.collection<UserGroup>('ActiveUserGroups') ?? [];
    }
  }

  public onUserGroupChange(event): void {
    const userGroup = event.source.value as UserGroup;
    const user = this.object as User;

    if (event.isUserInput) {
      if (event.source.selected) {
        userGroup.addInMember(user);
      } else {
        userGroup.addOutMember(user);
      }
    }
  }

  public override save(): void {
    this.onSave();
    super.save();
  }

  private onSave(): void {
    if (this.selectedUserGroups.length == 0) {
      this.creatorUserGroup.addOutMember(this.object);
    } else {
      this.creatorUserGroup.addInMember(this.object);
    }
  }
}
