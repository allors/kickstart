// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Setup.cs" company="Allors bvba">
//   Copyright 2002-2012 Allors bvba.
//
// Dual Licensed under
//   a) the General Public Licence v3 (GPL)
//   b) the Allors License
//
// The GPL License is included in the file gpl.txt.
// The Allors License is an addendum to your contract.
//
// Allors Applications is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// For more information visit http://www.allors.com/legal
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.IO;
using System.Linq;
using System.Reflection;
using Allors.Database;
using Allors.Database.Domain;
using Allors.Database.Domain.TestPopulation;

namespace Allors
{
    public partial class GatsbySetup
    {
        private readonly ITransaction transaction;

        public GatsbySetup(ITransaction transaction)
        {
            this.transaction = transaction;
        }

        public void Apply()
        {
            var administrator = new PersonBuilder(transaction)
                .WithFirstName("Jane")
                .WithLastName("Doe")
                .WithUserName("administrator")
                .Build();

            new UserGroups(transaction).Administrators.AddMember(administrator);

            transaction.Services.Get<IUserService>().User = administrator;

            // Give Administrator access
            foreach (var @this in new Organisations(this.transaction).Extent().Where(v => v.IsInternalOrganisation))
            {
                new EmploymentBuilder(this.transaction).WithEmployee(administrator).WithEmployer(@this).Build();
                @this.AddProductQuoteApprover(administrator);
                @this.AddPurchaseOrderApproversLevel1(administrator);
                @this.AddPurchaseOrderApproversLevel2(administrator);
            }

            var internalOrganisation = new Organisations(this.transaction).Extent().First(v => v.IsInternalOrganisation);

            var catalogue = new Catalogues(this.transaction).Extent().Single();

            Media CreateMedia(string name)
            {
                return new MediaBuilder(transaction).WithInFileName(name).WithInData(this.GetResourceBytes(name)).Build();
            }

            // Brand and Models
            var dummyBrand = new BrandBuilder(transaction).WithName("Dummy Brand").Build();

            var tldTmx100Model = new ModelBuilder(transaction).WithName("TMX 100").Build();
            var tld808DupModel = new ModelBuilder(transaction).WithName("808-DUP").Build();
            var tldMX450Model = new ModelBuilder(transaction).WithName("MX450").Build();
            var tldBrand = new BrandBuilder(transaction)
                .WithName("TLD")
                .WithLogoImage(CreateMedia("TLD.jpg"))
                .WithModel(tldTmx100Model)
                .WithModel(tld808DupModel)
                .WithDescription(@"TLD provides its customers with a complete range of Ground Support Equipment through the most extensive Worldwide Sales and Service network in the industry.
                                   TLD products are in operation at most airports around the world. Well-designed and adapted to the requirements of our customers, the equipment is consistently maintained to the highest standards by our large engineering teams. Performance and dynamic efficiency, as well as simplicity, high reliability and low maintenance costs, continuously drive our ongoing engineering efforts.")
                .Build();

            var mulagComet10aModel = new ModelBuilder(transaction).WithName("COMET 10A").Build();
            var mulagComet12aModel = new ModelBuilder(transaction).WithName("COMET 12A").Build();
            var mulagBrand = new BrandBuilder(transaction)
              .WithName("MULAG")
              .WithLogoImage(CreateMedia("MULAG.png"))
              .WithModel(mulagComet10aModel)
              .WithModel(mulagComet12aModel)
              .WithDescription(@"With their well-conceived design, the MULAG airport vehicles are extremely efficient and economical: They help to complete ground support tasks quickly and flexibly. The high production quality prevents extended downtimes and lengthy repairs.")
              .Build();

            // ProductCategories
            ProductCategory CreateProductCategory(string name, string image = null, ProductCategory parent = null)
            {
                Media media = image != null ? CreateMedia(image) : null;

                return new ProductCategoryBuilder(this.transaction)
                    .WithCatScope(new Scopes(this.transaction).Public)
                    .WithDescription("### This is a description")
                    .WithInternalOrganisation(internalOrganisation)
                    .WithPrimaryParent(parent)
                    .WithCategoryImage(media)
                    .WithName(name)
                    .Build();
            }

            // Top level
            var pushbacksAndTractors = CreateProductCategory(name: "PUSHBACKS & TRACTORS", image: "PUSHBACKS & TRACTORS.png");
            var acuAsuGpu = CreateProductCategory(name: "ACU, ASU, GPU", image: "ACU, ASU, GPU.png");

            catalogue.AddProductCategory(pushbacksAndTractors);
            catalogue.AddProductCategory(acuAsuGpu);

            // Second level
            var pushbackTractor = CreateProductCategory(name: "PUSHBACK TRACTOR", parent: pushbacksAndTractors);
            var towbarlessTractor = CreateProductCategory(name: "TOWBARLESS TRACTOR", parent: pushbacksAndTractors);
            var tractor = CreateProductCategory(name: "TRACTOR", parent: pushbacksAndTractors);

            var acu = CreateProductCategory(name: "ACU", parent: acuAsuGpu);
            var asu = CreateProductCategory(name: "ASU", parent: acuAsuGpu);
            var gpu = CreateProductCategory(name: "GPU", parent: acuAsuGpu);

            // Third level
            var narrowBody = CreateProductCategory(name: "NARROW BODY", parent: pushbackTractor);
            var mediumBody = CreateProductCategory(name: "MEDIUM BODY", parent: pushbackTractor);
            var wideBody = CreateProductCategory(name: "WIDE BODY", parent: pushbackTractor);

            var types = new SerialisedItemCharacteristicTypes(transaction);
            var nonSerialisedProductType = new ProductTypeBuilder(transaction)
                  .WithName("nonSerialisedProductType")
                  .WithSerialisedItemCharacteristicType(types.Height)
                  .WithSerialisedItemCharacteristicType(types.Length)
                  .WithSerialisedItemCharacteristicType(types.Width)
                  .WithSerialisedItemCharacteristicType(types.Weight)
                  .WithSerialisedItemCharacteristicType(types.OperatingHours)
                  .WithSerialisedItemCharacteristicType(types.EngineBrand)
                  .WithSerialisedItemCharacteristicType(types.EngineModel)
                  .Build();

            // Products
            UnifiedGood CreateUnifiedGood(string name, string primaryPhoto, Model model, ProductCategory productCategory)
            {
                var product = new UnifiedGoodBuilder(this.transaction)
                    .WithProductType(nonSerialisedProductType)
                    .WithDescription("### This is a description")
                    .WithName(name)
                    .WithInventoryItemKind(new InventoryItemKinds(this.transaction).Serialised)
                    .WithVatRegime(new VatRegimes(this.transaction).ZeroRated)
                    .WithBrand(model.BrandWhereModel)
                    .WithModel(model)
                    .WithPrimaryPhoto(CreateMedia(primaryPhoto))
                    .Build();

                productCategory.AddProduct(product);
                return product;
            }

            SerialisedItemCharacteristic CreateCharacteristic(string value, SerialisedItemCharacteristicType type)
            {
                var serializedItemCharacteristic = new SerialisedItemCharacteristicBuilder(transaction)
                .WithValue(value)
                .WithSerialisedItemCharacteristicType(type)
                .Build();

                return serializedItemCharacteristic;
            }

            var tldTmx100 = CreateUnifiedGood("TLD TMX100", "TLD TMX100.jpg", tldTmx100Model, narrowBody);
            tldTmx100.AddSerialisedItemCharacteristic(CreateCharacteristic("5", types.Length));
            tldTmx100.AddSerialisedItemCharacteristic(CreateCharacteristic("3", types.Width));
            tldTmx100.AddSerialisedItemCharacteristic(CreateCharacteristic("2", types.Height));
            tldTmx100.AddSerialisedItemCharacteristic(CreateCharacteristic("100", types.Weight));

            var tld808Dup = CreateUnifiedGood("TLD 808-DUP", "TLD 808-DUP.jpg", tld808DupModel, narrowBody);
            tld808Dup.SalesDiscontinuationDate = this.transaction.Now().AddYears(1);
            tld808Dup.AddSerialisedItemCharacteristic(CreateCharacteristic("10", types.Length));
            tld808Dup.AddSerialisedItemCharacteristic(CreateCharacteristic("6", types.Width));
            tld808Dup.AddPublicElectronicDocument(CreateMedia("Technical- TLD TMX-100_datasheet.pdf"));

            var tldMx450 = CreateUnifiedGood("TLD MX450", "TLD MX450.jpg", tldMX450Model, narrowBody);
            tldMx450.SalesDiscontinuationDate = this.transaction.Now().AddDays(-1);

            var mulagComet10a = CreateUnifiedGood("MULAG COMET 10A", "MULAG COMET10A.jpg", mulagComet10aModel, narrowBody);
            var mulagComet12a = CreateUnifiedGood("MULAG COMET 12A", "MULAG COMET12A.jpg", mulagComet12aModel, narrowBody);

            // Serialised Items
            var tldTmx100ItemAV11352 = new SerialisedItemBuilder(transaction).WithShowOnFrontPage(true).WithDefaults(internalOrganisation).Build();
            tldTmx100.AddSerialisedItem(tldTmx100ItemAV11352);
            tldTmx100ItemAV11352.AvailableForSale = true;
            tldTmx100ItemAV11352.PrimaryPhoto = CreateMedia("AV11352_0.jpg");
            tldTmx100ItemAV11352.AddSecondaryPhoto(CreateMedia("AV11352_1.jpg"));
            tldTmx100ItemAV11352.AddSecondaryPhoto(CreateMedia("AV11352_2.jpg"));
            tldTmx100ItemAV11352.AddSecondaryPhoto(CreateMedia("AV11352_3.jpg"));
            tldTmx100ItemAV11352.AddSecondaryPhoto(CreateMedia("AV11352_4.jpg"));
            tldTmx100ItemAV11352.AddSecondaryPhoto(CreateMedia("AV11352_5.jpg"));
            tldTmx100ItemAV11352.AddSerialisedItemCharacteristic(CreateCharacteristic("280", types.OperatingHours));
            tldTmx100ItemAV11352.AddSerialisedItemCharacteristic(CreateCharacteristic("Mercedes", types.EngineBrand));
            tldTmx100ItemAV11352.AddSerialisedItemCharacteristic(CreateCharacteristic("500X", types.EngineModel));

            var tldTmx100ItemAV12834 = new SerialisedItemBuilder(transaction).WithDefaults(internalOrganisation).Build();
            tldTmx100.AddSerialisedItem(tldTmx100ItemAV12834);
            tldTmx100ItemAV12834.AvailableForSale = false;

            var tld808DupAV10652 = new SerialisedItemBuilder(transaction).WithDefaults(internalOrganisation).Build();
            tld808DupAV10652.AvailableForSale = true;
            tld808DupAV10652.PrimaryPhoto = CreateMedia("AV10652_0.jpg");
            tld808DupAV10652.AddSecondaryPhoto(CreateMedia("AV10652_1.jpg"));
            tld808DupAV10652.AddSecondaryPhoto(CreateMedia("AV10652_2.jpg"));
            tld808Dup.AddSerialisedItem(tld808DupAV10652);
            tld808DupAV10652.AddSerialisedItemCharacteristic(CreateCharacteristic("201", types.OperatingHours));
            tld808DupAV10652.AddSerialisedItemCharacteristic(CreateCharacteristic("Fiat", types.EngineBrand));

            var tld808DupAV10653 = new SerialisedItemBuilder(transaction).WithShowOnFrontPage(true).WithDefaults(internalOrganisation).Build();
            tld808DupAV10653.AvailableForSale = true;
            tld808DupAV10653.PrimaryPhoto = CreateMedia("AV10653_0.jpg");
            tld808DupAV10653.AddSecondaryPhoto(CreateMedia("AV10653_1.jpg"));
            tld808Dup.AddSerialisedItem(tld808DupAV10653);
            tld808DupAV10653.AddSerialisedItemCharacteristic(CreateCharacteristic("200", types.OperatingHours));
            tld808DupAV10653.AddSerialisedItemCharacteristic(CreateCharacteristic("10", types.Length));
            tld808DupAV10653.AddSerialisedItemCharacteristic(CreateCharacteristic("6", types.Width));
            tld808DupAV10653.AddSerialisedItemCharacteristic(CreateCharacteristic("4", types.Height));
            tld808DupAV10653.AddSerialisedItemCharacteristic(CreateCharacteristic("200", types.Weight));

            this.transaction.Derive();
        }

        private byte[] GetResourceBytes(string name)
        {
            var assembly = this.GetType().GetTypeInfo().Assembly;
            var manifestResourceName = assembly.GetManifestResourceNames().First(v => v.Contains(name));
            var resource = assembly.GetManifestResourceStream(manifestResourceName);
            if (resource != null)
            {
                using (var ms = new MemoryStream())
                {
                    resource.CopyTo(ms);
                    return ms.ToArray();
                }
            }

            return null;
        }
    }
}
