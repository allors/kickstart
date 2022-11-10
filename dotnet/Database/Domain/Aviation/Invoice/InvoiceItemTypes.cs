// --------------------------------------------------------------------------------------------------------------------
// <copyright file="InvoiceItemTypes.cs" company="Allors bvba">
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
    using System.Linq;

    public partial class InvoiceItemTypes
    {
        public static readonly Guid OtherId = new Guid("8AB1F56A-B07E-4552-83A7-CA2DA2043740");

        public static readonly Guid RmId = new Guid("F2D9770B-F933-48B0-A495-DF80CB702FCE");
        public static readonly Guid TransportId = new Guid("96c1c0ff-b0f1-480f-91a7-4658bebe6674");
        public static readonly Guid CleaningId = new Guid("c5bb91f5-f624-4cd4-88ea-7bc4f9ff13cd");
        public static readonly Guid SundriesId = new Guid("d88933a3-b311-41ee-9790-71c55eaf54e8");
        public static readonly Guid VehicleId = new Guid("cef3043d-8892-4114-ab39-71c5954d3b2a");
        public static readonly Guid GseUnmotorizedId = new Guid("c9362657-b081-4030-ac94-9622a2bbde08");

        public InvoiceItemType Other => this.Cache[OtherId];

        public InvoiceItemType Rm => this.Cache[RmId];

        public InvoiceItemType Transport => this.Cache[TransportId];

        public InvoiceItemType Cleaning => this.Cache[CleaningId];

        public InvoiceItemType Sundries => this.Cache[SundriesId];

        public InvoiceItemType Vehicle => this.Cache[VehicleId];

        public InvoiceItemType GseUnmotorized => this.Cache[GseUnmotorizedId];

        protected override void AviationSetup(Setup setup)
        {
            var dutchLocale = new Locales(this.Transaction).DutchNetherlands;

            new InvoiceItemTypeBuilder(this.Transaction)
                .WithName("Other")
                .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("Overige").WithLocale(dutchLocale).Build())
                .WithUniqueId(OtherId)
                .WithIsActive(true)
                .Build();

            // Repair and Maintenance
            new InvoiceItemTypeBuilder(this.Transaction)
                .WithName("R&M")
                .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("R&M").WithLocale(dutchLocale).Build())
                .WithUniqueId(RmId)
                .WithIsActive(true)
                .Build();

            new InvoiceItemTypeBuilder(this.Transaction)
                .WithName("Transport")
                .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("Transport").WithLocale(dutchLocale).Build())
                .WithUniqueId(TransportId)
                .WithIsActive(true)
                .Build();

            new InvoiceItemTypeBuilder(this.Transaction)
                .WithName("Cleaning GSE")
                .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("Poetsen GSE").WithLocale(dutchLocale).Build())
                .WithUniqueId(CleaningId)
                .WithIsActive(true)
                .Build();

            new InvoiceItemTypeBuilder(this.Transaction)
                .WithName("Sundries")
                .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("Klein materiaal").WithLocale(dutchLocale).Build())
                .WithUniqueId(SundriesId)
                .WithIsActive(true)
                .Build();

            new InvoiceItemTypeBuilder(this.Transaction)
                .WithName("Vehicle")
                .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("Voertuig").WithLocale(dutchLocale).Build())
                .WithUniqueId(VehicleId)
                .WithIsActive(true)
                .Build();

            new InvoiceItemTypeBuilder(this.Transaction)
                .WithName("GSE(U)")
                .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("GSE(U)").WithLocale(dutchLocale).Build())
                .WithUniqueId(GseUnmotorizedId)
                .WithIsActive(true)
                .Build();

            new InvoiceItemTypes(this.Transaction).ProductFeatureItem.IsActive = false;
            new InvoiceItemTypes(this.Transaction).Service.IsActive = false;

            var productItem = new InvoiceItemTypes(this.Transaction).ProductItem;
            productItem.Name = "GSE";
            productItem.LocalisedNames.First(v => v.Locale.Equals(dutchLocale)).Text = "Reserveonderdeel";

            var partItem = new InvoiceItemTypes(this.Transaction).PartItem;
            partItem.Name = "Spare Part";
            partItem.LocalisedNames.First(v => v.Locale.Equals(dutchLocale)).Text = "Reserveonderdeel";
        }
    }
}
