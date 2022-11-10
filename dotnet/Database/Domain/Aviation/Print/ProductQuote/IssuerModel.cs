// --------------------------------------------------------------------------------------------------------------------
// <copyright file="BilledFromModel.cs" company="Allors bvba">
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

namespace Allors.Database.Domain.Print.ProductQuoteModel
{
    using System.Linq;

    public class IssuerModel
    {
        public IssuerModel(Quote quote, Dictionary<string, byte[]> imageByImageName)
        {
            if (quote.Issuer is Organisation issuer)
            {
                this.Name = issuer.DisplayName;
                this.Email = issuer.GeneralEmail?.ElectronicAddressString;
                this.Website = issuer.InternetAddress?.ElectronicAddressString;
                this.TaxId = issuer.TaxNumber;

                var phoneNumbers = issuer?.CurrentPartyContactMechanisms.Where(v => v.ContactMechanism.GetType().Name == typeof(TelecommunicationsNumber).Name).Select(v => v.ContactMechanism).ToArray();
                if (phoneNumbers.Length > 0)
                {
                    this.Telephone = phoneNumbers[0].ToString();
                }

                if (phoneNumbers.Length > 1)
                {
                    this.Telephone2 = phoneNumbers[1].ToString();
                }

                if (issuer.GeneralCorrespondence is PostalAddress generalAddress)
                {
                    var address = generalAddress.Address1;
                    if (!string.IsNullOrWhiteSpace(generalAddress.Address2))
                    {
                        address += $"\n{generalAddress.Address2}";
                    }

                    if (!string.IsNullOrWhiteSpace(generalAddress.Address3))
                    {
                        address += $"\n{generalAddress.Address3}";
                    }

                    this.Address = address.Split('\n');
                    this.City = generalAddress.Locality;
                    this.State = generalAddress.Region;
                    this.PostalCode = generalAddress.PostalCode;
                    this.Country = generalAddress.Country?.Name;
                    this.PrintPostalCode = !string.IsNullOrEmpty(this.PostalCode);
                    this.PrintCity = !this.PrintPostalCode;
                }

                var bankAccount = issuer.BankAccounts.FirstOrDefault(v => v.ExistIban);
                if (bankAccount != null)
                {
                    this.IBAN = bankAccount.Iban;

                    var bank = bankAccount.Bank;
                    this.Bank = bank?.Name;
                    this.Swift = bank?.SwiftCode ?? bank?.Bic;
                }
            }
        }

        public bool PrintPostalCode { get; }

        public bool PrintCity { get; }

        public string Name { get; }

        public string[] Address { get; }

        public string City { get; }

        public string State { get; }

        public string Country { get; }

        public string PostalCode { get; }

        public string Telephone { get; }

        public string Telephone2 { get; }

        public string Email { get; }

        public string Website { get; }

        public string Bank { get; }

        public string BankAccount { get; }

        public string IBAN { get; }

        public string Swift { get; }

        public string TaxId { get; }
    }
}
