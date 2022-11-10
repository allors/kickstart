// <copyright file="RequestExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using Allors.Database.Meta;

namespace Allors.Database.Domain
{
    using System.Linq;

    public static partial class RequestExtensions
    {
        public static void AviationOnInit(this Request @this, ObjectOnInit method)
        {
            var m = @this.Strategy.Transaction.Database.Services.Get<MetaPopulation>();

            if (@this.RequestState.Equals(new RequestStates(@this.Transaction()).Anonymous))
            {
                var emailAddress = new EmailAddresses(@this.Transaction()).FindBy(m.EmailAddress.ElectronicAddressString, @this.EmailAddress);
                
                var salutation = "Dear Sir or Madam";
                if (emailAddress?.PartyContactMechanismsWhereContactMechanism.FirstOrDefault().Party is Person receiver)
                {
                    salutation = $"Dear {receiver.FirstName}";
                }

                new EmailMessageBuilder(@this.Transaction())
                    .WithRecipientEmailAddress(@this.EmailAddress)
                    .WithSubject($"Your request for quote submitted on {@this.Transaction().Now().ToShortDateString()}")
                    .WithBody($@"Dear {salutation},

Thank you so much for your kind request which is registered on number {@this.RequestNumber}.

We are at your entire disposal for any further information that you should need. 
Feel free to contact me at any time +32 471 942 780.

Waiting for your news
Very kind regards.

									Danny Vranckx,
									CEO
")
                    .Build();
            }
        }
    }
}
