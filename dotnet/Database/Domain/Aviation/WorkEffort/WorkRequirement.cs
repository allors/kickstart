// <copyright file="WorkRequirement.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class WorkRequirement
    {
        public bool ShouldEmail => this.RequirementState.IsFinished && !this.ExistEmailMessage;

        public void AviationOnInit(ObjectOnInit method)
        {
            var user = this.Strategy.Transaction.Services.Get<IUserService>().User as Person;
            var creationDate = this.Strategy.Transaction.Now();

            var description = $"<p>{this.Description}</p>"
                + $"<p>{this.Reason}</p>"
                + $"<p>Created by {user.DisplayName} on {creationDate:D}</p>";

            foreach(var person in this.ServicedBy.LocalAdministrators)
            {
                var notification = new NotificationBuilder(this.Transaction())
                    .WithTarget(this)
                    .WithTitle("New Service Request")
                    .WithDescription(description)
                    .Build();

                person.NotificationList.AddNotification(notification);
            }
        }
    }
}
