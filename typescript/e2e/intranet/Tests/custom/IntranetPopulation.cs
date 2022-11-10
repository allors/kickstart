// <copyright file="IntranetPopulation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>


using Allors.Database.Meta;

namespace Tests.E2E
{
    using System;
    using System.IO;
    using System.Linq;
    using System.Reflection;
    using Allors;
    using Allors.Database;
    using Allors.Database.Domain;
    using Allors.Database.Domain.TestPopulation;

    public class IntranetPopulation
    {
        private readonly ITransaction Transaction;

        private readonly DirectoryInfo DataPath;

        private readonly MetaPopulation M;

        public IntranetPopulation(ITransaction transaction, DirectoryInfo dataPath, MetaPopulation m)
        {
            this.Transaction = transaction;
            this.DataPath = dataPath;
            this.M = m;
        }

        public void Execute()
        {
            var singleton = this.Transaction.GetSingleton();
            var dutchLocale = new Locales(this.Transaction).DutchNetherlands;
            singleton.AddAdditionalLocale(dutchLocale);

            var euro = new Currencies(this.Transaction).FindBy(M.Currency.IsoCode, "EUR");
            var usd = new Currencies(this.Transaction).FindBy(M.Currency.IsoCode, "USD");

            new ExchangeRateBuilder(this.Transaction).WithValidFrom(new DateTime(2021, 04, 19, 12, 0, 0, DateTimeKind.Utc)).WithFromCurrency(euro).WithToCurrency(usd).WithRate(1.2027861M).Build();

            var faker = this.Transaction.Faker();

            var internalOrganisation = new Organisations(this.Transaction).Extent().First(v => v.IsInternalOrganisation);

            singleton.Settings.DefaultFacility = internalOrganisation.FacilitiesWhereOwner.FirstOrDefault();

            internalOrganisation.CreateEmployee("letmein", faker);
            internalOrganisation.CreateAdministrator("letmein", faker);
            var allorsB2BCustomer = internalOrganisation.CreateB2BCustomer(this.Transaction.Faker());
            var allorsB2CCustomer = internalOrganisation.CreateB2CCustomer(this.Transaction.Faker());
            internalOrganisation.CreateSupplier(this.Transaction.Faker());
            internalOrganisation.CreateSubContractor(this.Transaction.Faker());

            this.Transaction.Derive();

            var facility = new FacilityBuilder(this.Transaction)
                .WithName("Allors warehouse 2")
                .WithFacilityType(new FacilityTypes(this.Transaction).Warehouse)
                .WithOwner(internalOrganisation)
                .Build();

            // TODO: Martien
            //var store = new StoreBuilder(this.Transaction).WithName("store")
            //    .WithInternalOrganisation(allors)
            //    .WithDefaultFacility(facility)
            //    .WithDefaultShipmentMethod(new ShipmentMethods(this.Transaction).Ground)
            //    .WithDefaultCarrier(new Carriers(this.Transaction).Fedex)
            //    .Build();

            var productType = new ProductTypeBuilder(this.Transaction)
                .WithName($"Gizmo Serialised")
                .WithSerialisedItemCharacteristicType(new SerialisedItemCharacteristicTypeBuilder(this.Transaction)
                                            .WithName("Size")
                                            .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("Afmeting").WithLocale(dutchLocale).Build())
                                            .Build())
                .WithSerialisedItemCharacteristicType(new SerialisedItemCharacteristicTypeBuilder(this.Transaction)
                                            .WithName("Weight")
                                            .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("Gewicht").WithLocale(dutchLocale).Build())
                                            .WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Kilogram)
                                            .Build())
                .Build();

            var vatRate = new VatRateBuilder(this.Transaction).WithRate(21).Build();

            var brand = new BrandBuilder(this.Transaction).WithDefaults().Build();

            var good_1 = new UnifiedGoodBuilder(this.Transaction).WithNonSerialisedDefaults(internalOrganisation).Build();

            this.Transaction.Derive();

            new InventoryItemTransactionBuilder(this.Transaction)
                .WithPart(good_1)
                .WithQuantity(100)
                .WithReason(new InventoryTransactionReasons(this.Transaction).Unknown)
                .Build();

            var good_2 = new UnifiedGoodBuilder(this.Transaction).WithSerialisedDefaults(internalOrganisation).Build();

            var serialisedItem1 = new SerialisedItemBuilder(this.Transaction).WithDefaults(internalOrganisation).Build();

            good_2.AddSerialisedItem(serialisedItem1);

            this.Transaction.Derive();

            new SerialisedInventoryItemBuilder(this.Transaction)
                .WithPart(good_2)
                .WithSerialisedItem(serialisedItem1)
                .WithFacility(internalOrganisation.StoresWhereInternalOrganisation.First().DefaultFacility)
                .Build();

            var good_3 = new NonUnifiedGoodBuilder(this.Transaction).WithNonSerialisedDefaults(internalOrganisation).Build();

            var good_4 = new UnifiedGoodBuilder(this.Transaction).WithSerialisedDefaults(internalOrganisation).Build();

            var productCategory_1 = new ProductCategoryBuilder(this.Transaction)
                .WithInternalOrganisation(internalOrganisation)
                .WithName("Best selling gizmo's")
                .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("Meest verkochte gizmo's").WithLocale(dutchLocale).Build())
                .Build();

            var productCategory_2 = new ProductCategoryBuilder(this.Transaction)
                .WithInternalOrganisation(internalOrganisation)
                .WithName("Big Gizmo's")
                .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("Grote Gizmo's").WithLocale(dutchLocale).Build())
                .Build();

            var productCategory_3 = new ProductCategoryBuilder(this.Transaction)
                .WithInternalOrganisation(internalOrganisation)
                .WithName("Small gizmo's")
                .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("Kleine gizmo's").WithLocale(dutchLocale).Build())
                .WithProduct(good_1)
                .WithProduct(good_2)
                .WithProduct(good_3)
                .WithProduct(good_4)
                .Build();

            new CatalogueBuilder(this.Transaction)
                .WithInternalOrganisation(internalOrganisation)
                .WithName("New gizmo's")
                .WithLocalisedName(new LocalisedTextBuilder(this.Transaction).WithText("Nieuwe gizmo's").WithLocale(dutchLocale).Build())
                .WithDescription("Latest in the world of Gizmo's")
                .WithLocalisedDescription(new LocalisedTextBuilder(this.Transaction).WithText("Laatste in de wereld van Gizmo's").WithLocale(dutchLocale).Build())
                .WithProductCategory(productCategory_1)
                .Build();

            this.Transaction.Derive();

            new FaceToFaceCommunicationBuilder(this.Transaction).WithDefaults(internalOrganisation).Build();
            new EmailCommunicationBuilder(this.Transaction).WithDefaults(internalOrganisation).Build();
            new LetterCorrespondenceBuilder(this.Transaction).WithDefaults(internalOrganisation).Build();
            new PhoneCommunicationBuilder(this.Transaction).WithDefaults(internalOrganisation).Build();

            var salesOrder_1 = internalOrganisation.CreateB2BSalesOrder(faker);
            var salesOrder_2 = internalOrganisation.CreateB2CSalesOrder(faker);

            new SalesInvoiceBuilder(this.Transaction).WithSalesExternalB2BInvoiceDefaults(internalOrganisation).Build();

            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(good_1)
                .WithSupplier(internalOrganisation.ActiveSuppliers.FirstOrDefault())
                .WithFromDate(this.Transaction.Now().AddMinutes(-1))
                .WithUnitOfMeasure(new UnitsOfMeasure(this.Transaction).Piece)
                .WithPrice(7)
                .WithCurrency(euro)
                .Build();

            new PurchaseInvoiceBuilder(this.Transaction).WithExternalB2BInvoiceDefaults(internalOrganisation).Build();

            internalOrganisation.CreatePurchaseOrderWithBothItems(faker);

            var workTask = new WorkTaskBuilder(this.Transaction)
                .WithTakenBy(internalOrganisation)
                .WithCustomer(internalOrganisation.ActiveCustomers.FirstOrDefault())
                .WithName("maintenance")
                .Build();

            new PositionTypeBuilder(this.Transaction)
                .WithTitle("Mechanic")
                .WithUniqueId(new Guid("E62A8F4B-8045-472E-AB18-E39C51A02696"))
                .Build();

            new PositionTypeRateBuilder(this.Transaction)
                .WithRate(100)
                .WithRateType(new RateTypes(this.Transaction).StandardRate)
                .WithFrequency(new TimeFrequencies(this.Transaction).Hour)
                .Build();

            this.Transaction.Derive();

            // Serialized RFQ with Serialized Unified-Good
            var serializedRFQ = new RequestForQuoteBuilder(this.Transaction).WithSerializedDefaults(internalOrganisation).Build();

            // NonSerialized RFQ with NonSerialized Unified-Good
            var nonSerializedRFQ = new RequestForQuoteBuilder(this.Transaction).WithNonSerializedDefaults(internalOrganisation).Build();

            var quote = new ProductQuoteBuilder(this.Transaction).WithSerializedDefaults(internalOrganisation).Build();

            this.Transaction.Derive();

            var salesOrderItem_1 = new SalesOrderItemBuilder(this.Transaction).WithNonSerialisedPartItemDefaults().Build();

            salesOrder_2.AddSalesOrderItem(salesOrderItem_1);

            this.Transaction.Derive();

            new CustomerShipmentBuilder(this.Transaction).WithDefaults(internalOrganisation).Build();

            this.Transaction.Derive();

            new PurchaseShipmentBuilder(this.Transaction).WithDefaults(internalOrganisation).Build();

            this.Transaction.Derive();
            this.Transaction.Commit();
        }

        private byte[] GetResourceBytes(string name)
        {
            var assembly = this.GetType().GetTypeInfo().Assembly;
            var manifestResourceName = assembly.GetManifestResourceNames().First(v => v.Contains(name));
            var resource = assembly.GetManifestResourceStream(manifestResourceName);
            if (resource != null)
            {
                using var ms = new MemoryStream();
                resource.CopyTo(ms);
                return ms.ToArray();
            }

            return null;
        }

        private Template CreateOpenDocumentTemplate(byte[] content)
        {
            var media = new MediaBuilder(this.Transaction).WithInData(content).Build();
            var templateType = new TemplateTypes(this.Transaction).OpenDocumentType;
            var template = new TemplateBuilder(this.Transaction).WithMedia(media).WithTemplateType(templateType).Build();
            return template;
        }
    }
}
