// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ContactMechanismPurposes.cs" company="Allors bvba">
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
namespace Allors.Database.Domain
{
    using System;

    public partial class ContactMechanismPurposes
    {
        private static readonly Guid SalesMarketingId = new Guid("ACB6F606-960F-4D7A-8BAB-F32CBC01FA99");
        private static readonly Guid OperationsFinanceId = new Guid("D0FDF4A2-AD21-498E-8420-99E887338DF2");
        private static readonly Guid FinanceAdministrationId = new Guid("96E827A2-A415-4561-B606-8F9C830C19E2");

        public ContactMechanismPurpose SalesMarketing => this.Cache[SalesMarketingId];

        public ContactMechanismPurpose OperationsFinance => this.Cache[OperationsFinanceId];

        public ContactMechanismPurpose FinanceAdministration => this.Cache[FinanceAdministrationId];

        protected override void AviationSetup(Setup setup)
        {
            var dutchLocale = new Locales(this.Transaction).DutchNetherlands;

            new ContactMechanismPurposeBuilder(this.Transaction)
                .WithName("(After) Sales & Marketing")
                .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("(After) Sales & Marketing").WithLocale(dutchLocale).Build())
                .WithUniqueId(SalesMarketingId)
                .WithIsActive(true)
                .Build();

            new ContactMechanismPurposeBuilder(this.Transaction)
                .WithName("Operations & Finance")
                .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("Operations & Finance").WithLocale(dutchLocale).Build())
                .WithUniqueId(OperationsFinanceId)
                .WithIsActive(true)
                .Build();

            new ContactMechanismPurposeBuilder(this.Transaction)
                .WithName("Finance & Administration")
                .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("Finance & Administration").WithLocale(dutchLocale).Build())
                .WithUniqueId(FinanceAdministrationId)
                .WithIsActive(true)
                .Build();
        }
    }
}
