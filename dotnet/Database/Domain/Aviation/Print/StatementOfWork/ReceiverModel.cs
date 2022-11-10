// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ReceiverModel.cs" company="Allors bvba">
//   Copyright 2002-2012 Allors bvba.
// Dual Licensed under
//   a) the General Public Licence v3 (GPL)
//   b) the Allors License
// The GPL License is included in the file gpl.txt.
// The Allors License is an addendum to your contract.
// Allors Applications is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// For more information visit http://www.allors.com/legal
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;

namespace Allors.Database.Domain.Print.StatementOfWorkModel
{
    public class ReceiverModel
    {
        public ReceiverModel(StatementOfWork sow)
        {
            var transaction = sow.Strategy.Transaction;

            var receiver = sow.Receiver;
            var organisationReceiver = sow.Receiver as Organisation;
            var personReceiver = sow.Receiver as Person;

            if (receiver != null)
            {
                this.Name = receiver.DisplayName;
                this.TaxId = organisationReceiver?.TaxNumber;
            }

            if (organisationReceiver != null)
            {
                this.Contact = sow.ContactPerson?.DisplayName;
                this.ContactLastName = sow.ContactPerson?.LastName;
                this.Salutation = sow.ContactPerson?.Salutation?.Name;
            }
            
            if (personReceiver != null)
            {
                this.Contact = personReceiver.DisplayName;
                this.ContactLastName = personReceiver.LastName;
                this.Salutation = personReceiver.Salutation?.Name;
            }

            if (!string.IsNullOrEmpty(this.Salutation))
            {
                this.ContactPrintName = $"{this.Salutation} {this.Contact}";
            }
            else
            {
                this.ContactPrintName = this.Contact;
            }

            if (!string.IsNullOrEmpty(this.Salutation) && !string.IsNullOrEmpty(this.ContactLastName))
            {
                this.Addressing = $"{this.Salutation} {this.ContactLastName}";
            }
            else if (!string.IsNullOrEmpty(this.ContactLastName))
            {
                this.Addressing = $"Mr. / Ms {this.ContactLastName}";
            }
            else
            {
                this.Addressing = "Mr. / Ms";
            }

            this.Addressing += ",";

            this.ContactFunction = sow.ContactPerson?.Function;

            if (sow.ContactPerson?.CurrentPartyContactMechanisms.FirstOrDefault(v => v.ContactMechanism.GetType().Name == typeof(EmailAddress).Name)?.ContactMechanism is EmailAddress emailAddress)
            {
                this.ContactEmail = emailAddress.ElectronicAddressString;
            }

            if (sow.ContactPerson?.CurrentPartyContactMechanisms.FirstOrDefault(v => v.ContactMechanism.GetType().Name == typeof(TelecommunicationsNumber).Name)?.ContactMechanism is TelecommunicationsNumber phone)
            {
                this.ContactPhone = phone.ToString();
            }
        }

        public string Name { get; }

        public string ContactPrintName { get; }

        public string Addressing { get; }

        public string Salutation { get; }

        public string Contact { get; }

        public string ContactLastName { get; }

        public string ContactFunction { get; }

        public string ContactEmail { get; }

        public string ContactPhone { get; }

        public string TaxId { get; }
    }
}
