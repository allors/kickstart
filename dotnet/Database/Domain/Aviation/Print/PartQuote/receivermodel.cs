// <copyright file="ReceiverModel.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Print.PartQuoteModel
{
    public class ReceiverModel
    {
        public ReceiverModel(ProductQuote quote)
        {
            var receiver = quote.Receiver;

            var billTo = receiver;
            var billToOrganisation = billTo as Organisation;
            if (billTo != null)
            {
                this.Name = billTo.DisplayName;
                this.TaxId = billToOrganisation?.TaxNumber;
            }

            this.Contact = quote.ContactPerson?.DisplayName;

            if (quote.FullfillContactMechanism is PostalAddress postalAddress)
            {
                this.Address = postalAddress.Address1;
                if (!string.IsNullOrWhiteSpace(postalAddress.Address2))
                {
                    this.Address = $"\n{postalAddress.Address2}";
                }

                if (!string.IsNullOrWhiteSpace(postalAddress.Address3))
                {
                    this.Address = $"\n{postalAddress.Address3}";
                }

                this.City = postalAddress.Locality;
                this.State = postalAddress.Region;
                this.PostalCode = postalAddress.PostalCode;
                this.Country = postalAddress.Country?.Name;
                this.PrintPostalCode = !string.IsNullOrEmpty(this.PostalCode);
                this.PrintCity = !this.PrintPostalCode;
            }

            if (quote.FullfillContactMechanism is ElectronicAddress electronicAddress)
            {
                this.Address = electronicAddress.ElectronicAddressString;
            }
        }

        public bool PrintPostalCode { get; }

        public bool PrintCity { get; }

        public string Name { get; }

        public string Address { get; }

        public string City { get; }

        public string State { get; }

        public string Country { get; }

        public string PostalCode { get; }

        public string TaxId { get; }

        public string Contact { get; }
    }
}
