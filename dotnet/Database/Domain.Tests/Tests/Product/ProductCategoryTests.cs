// <copyright file="ProductCategoryTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

using Allors.Database.Derivations;
using Resources;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace Allors.Database.Domain.Tests
{
    public class ProductCategoryProductsRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public ProductCategoryProductsRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedProductCategoryProductsDeriveIataGseCode()
        {
            var iataGseCode = this.Transaction.Extent<IataGseCode>().First();
            var category1 = new ProductCategoryBuilder(this.Transaction).Build();
            var category2 = new ProductCategoryBuilder(this.Transaction).Build();
            var good = new UnifiedGoodBuilder(this.Transaction).Build();

            category1.AddProduct(good);
            this.Transaction.Derive(false);

            category2.AddProduct(good);

            var expectedMessage = $"{category2}, { this.M.ProductCategory.Products}, { ErrorMessages.ProductInMultipleCategories}";
            var errors = new List<IDerivationError>(this.Transaction.Derive(false).Errors);
            Assert.Contains(errors, e => e.Message.Equals(expectedMessage));
        }
    }

    public class ProductCategoryCustomRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public ProductCategoryCustomRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedIataGseCodeDeriveIataCode()
        {
            var iataGseCode = this.Transaction.Extent<IataGseCode>().First();
            var category = new ProductCategoryBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            category.IataGseCode = iataGseCode;
            this.Transaction.Derive(false);

            Assert.Equal(iataGseCode.Code, category.IataCode);
        }

        [Fact]
        public void ChangedIataGseCodeCodeDeriveIataCode()
        {
            var iataGseCode = this.Transaction.Extent<IataGseCode>().First();
            var category = new ProductCategoryBuilder(this.Transaction).WithIataGseCode(iataGseCode).Build();
            this.Transaction.Derive(false);

            iataGseCode.Code = "changed";
            this.Transaction.Derive(false);

            Assert.Equal("changed", category.IataCode);
        }

        [Fact]
        public void ChangedUniqueIdDeriveIsFamily()
        {
            var category = new ProductCategoryBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.True(category.IsFamily);
        }

        [Fact]
        public void ChangedPrimaryParentDeriveIsFamily()
        {
            var group = new ProductCategoryBuilder(this.Transaction).Build();

            group.PrimaryParent = new ProductCategoryBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            Assert.False(group.IsFamily);
        }

        [Fact]
        public void ChangedPrimaryParentDeriveIsGroup()
        {
            var family = new ProductCategoryBuilder(this.Transaction).Build();
            var group = new ProductCategoryBuilder(this.Transaction).Build();
            var subGroup = new ProductCategoryBuilder(this.Transaction).Build();

            subGroup.PrimaryParent = group;
            this.Transaction.Derive(false);

            group.PrimaryParent = family;
            this.Transaction.Derive(false);

            Assert.True(group.IsGroup);
        }

        [Fact]
        public void ChangedChildrenDeriveIsGroup()
        {
            var family = new ProductCategoryBuilder(this.Transaction).Build();
            var group = new ProductCategoryBuilder(this.Transaction).Build();
            var subGroup = new ProductCategoryBuilder(this.Transaction).Build();

            group.PrimaryParent = family;
            this.Transaction.Derive(false);

            subGroup.PrimaryParent = group;
            this.Transaction.Derive(false);

            Assert.True(group.IsGroup);
        }

        [Fact]
        public void ChangedPrimaryParentDeriveIsSubGroup()
        {
            var family = new ProductCategoryBuilder(this.Transaction).Build();
            var group = new ProductCategoryBuilder(this.Transaction).Build();
            var subGroup = new ProductCategoryBuilder(this.Transaction).Build();

            subGroup.PrimaryParent = group;
            this.Transaction.Derive(false);

            group.PrimaryParent = family;
            this.Transaction.Derive(false);

            Assert.True(subGroup.IsSubGroup);
        }

        [Fact]
        public void ChangedChildrenDeriveIsSubGroup()
        {
            var family = new ProductCategoryBuilder(this.Transaction).Build();
            var group = new ProductCategoryBuilder(this.Transaction).Build();
            var subGroup = new ProductCategoryBuilder(this.Transaction).Build();

            group.PrimaryParent = family;
            this.Transaction.Derive(false);

            subGroup.PrimaryParent = group;
            this.Transaction.Derive(false);

            Assert.True(subGroup.IsSubGroup);
        }
    }
}
