// <copyright file="UnifiedGoodTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

using Allors.Database.Derivations;
using Allors.Database.Domain.TestPopulation;
using Resources;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Allors.Database.Domain.Tests
{
    public class UnifiedGoodTests : DomainTest, IClassFixture<Fixture>
    {
        public UnifiedGoodTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenUnifiedGood_WhenAddingSecondCategory_ThenErrorMessage()
        {
            var cat1 = new ProductCategoryBuilder(this.Transaction).WithName("1").Build();
            var cat2 = new ProductCategoryBuilder(this.Transaction).WithName("2").Build();
            var good = new UnifiedGoodBuilder(this.Transaction).WithSerialisedDefaults(this.InternalOrganisation).Build();

            cat1.AddProduct(good);
            this.Transaction.Derive(true);

            cat2.AddProduct(good);

            var expectedErrorMessage = ErrorMessages.ProductInMultipleCategories;

            var errors = this.Transaction.Derive(false).Errors.ToList();
            Assert.Single(errors.FindAll(e => e.Message.Contains(expectedErrorMessage)));
        }
    }

    public class UnifiedGoodCustomRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public UnifiedGoodCustomRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedBrandThrowValidationError()
        {
            var brand = new BrandBuilder(this.Transaction).Build();
            var model = new ModelBuilder(this.Transaction).Build();
            brand.AddModel(model);

            var good1 = new UnifiedGoodBuilder(this.Transaction).WithBrand(brand).WithModel(model).Build();
            this.Transaction.Derive(false);

            var good2 = new UnifiedGoodBuilder(this.Transaction).WithBrand(brand).WithModel(model).Build();

            var expectedMessage = $"{good2}, { this.M.UnifiedGood.Brand}, { ErrorMessages.ProductExists}";
            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals(expectedMessage));
        }

        [Fact]
        public void ChangedModelThrowValidationError()
        {
            var brand = new BrandBuilder(this.Transaction).Build();
            var model1 = new ModelBuilder(this.Transaction).Build();
            var model2 = new ModelBuilder(this.Transaction).Build();
            brand.AddModel(model1);
            brand.AddModel(model2);

            var good1 = new UnifiedGoodBuilder(this.Transaction).WithBrand(brand).WithModel(model1).Build();
            this.Transaction.Derive(false);

            var good2 = new UnifiedGoodBuilder(this.Transaction).WithBrand(brand).WithModel(model2).Build();
            this.Transaction.Derive(false);

            good2.Model = model1;

            var expectedMessage = $"{good2}, { this.M.UnifiedGood.Brand}, { ErrorMessages.ProductExists}";
            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals(expectedMessage));
        }

        [Fact]
        public void ChangedBrandDeriveName()
        {
            var brand = new BrandBuilder(this.Transaction).WithName("brandname").Build();
            var model = new ModelBuilder(this.Transaction).WithName("modelname").Build();
            brand.AddModel(model);

            var good = new UnifiedGoodBuilder(this.Transaction)
                .WithName("<Automatically set on saving>")
                .WithBrand(brand)
                .WithModel(model)
                .Build();
            this.Transaction.Derive(false);

            Assert.Equal("brandname modelname", good.Name);
        }
    }

    public class UnifiedGoodIataGseCodeRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public UnifiedGoodIataGseCodeRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedProductCategoryProductsDeriveIataGseCode()
        {
            var iataGseCode = this.Transaction.Extent<IataGseCode>().First();
            var category = new ProductCategoryBuilder(this.Transaction).WithIataGseCode(iataGseCode).Build();
            var good = new UnifiedGoodBuilder(this.Transaction).Build();

            category.AddProduct(good);
            this.Transaction.Derive(false);

            Assert.Equal(iataGseCode, good.IataGseCode);
        }

        [Fact]
        public void ChangedProductCategoryIataGseCodeDeriveIataGseCode()
        {
            var iataGseCode = this.Transaction.Extent<IataGseCode>().First();
            var category = new ProductCategoryBuilder(this.Transaction).Build();
            var good = new UnifiedGoodBuilder(this.Transaction).Build();

            category.AddProduct(good);
            this.Transaction.Derive(false);

            category.IataGseCode = iataGseCode;
            this.Transaction.Derive(false);

            Assert.Equal(iataGseCode, good.IataGseCode);
        }
    }

    public class UnifiedGoodDerivedAssumedMonthlyOperatingHoursRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public UnifiedGoodDerivedAssumedMonthlyOperatingHoursRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedAssignedAssumedMonthlyOperatingHoursDeriveDerivedAssumedMonthlyOperatingHours()
        {
            var good = new UnifiedGoodBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            good.AssignedAssumedMonthlyOperatingHours = 100;
            this.Transaction.Derive(false);

            Assert.Equal(100, good.DerivedAssumedMonthlyOperatingHours);
        }

        [Fact]
        public void ChangedproductTypeAssumedMonthlyOperatingHoursDeriveDerivedAssumedMonthlyOperatingHours()
        {
            var productType = new ProductTypeBuilder(this.Transaction).Build();
            var good = new UnifiedGoodBuilder(this.Transaction).WithProductType(productType).Build();
            this.Transaction.Derive(false);

            productType.AssumedMonthlyOperatingHours = 100;
            this.Transaction.Derive(false);

            Assert.Equal(100, good.DerivedAssumedMonthlyOperatingHours);
        }
    }
}
