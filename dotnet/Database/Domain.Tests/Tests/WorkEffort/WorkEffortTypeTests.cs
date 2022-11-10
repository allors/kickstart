using Allors.Database.Domain.TestPopulation;
using Xunit;

namespace Allors.Database.Domain.Tests
{
    public class WorkEffortTypeProductCategoryDisplayNameRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public WorkEffortTypeProductCategoryDisplayNameRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedProductCategoryDeriveProductCategoryDisplayName()
        {
            var workEffortType = new WorkEffortTypeBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var category = new ProductCategoryBuilder(this.Transaction).WithName("name").Build();
            this.Transaction.Derive(false);

            workEffortType.ProductCategory = category;
            this.Transaction.Derive(false);

            Assert.Equal(category.Name, workEffortType.ProductCategoryDisplayName);
        }

        [Fact]
        public void DeleteProductCategoryDeriveProductCategoryDisplayName()
        {
            var workEffortType = new WorkEffortTypeBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var category = new ProductCategoryBuilder(this.Transaction).WithName("name").Build();
            this.Transaction.Derive(false);

            workEffortType.ProductCategory = category;
            this.Transaction.Derive(false);

            Assert.Equal(category.Name, workEffortType.ProductCategoryDisplayName);

            workEffortType.RemoveProductCategory();
            this.Transaction.Derive(false);

            Assert.False(workEffortType.ExistProductCategoryDisplayName);
        }

        [Fact]
        public void ChangeProductCategoryNameDeriveProductCategoryDisplayName()
        {
            var workEffortType = new WorkEffortTypeBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var category = new ProductCategoryBuilder(this.Transaction).WithName("name").Build();
            this.Transaction.Derive(false);

            workEffortType.ProductCategory = category;
            this.Transaction.Derive(false);

            category.Name = "changed";
            this.Transaction.Derive(false);

            Assert.Equal("changed", workEffortType.ProductCategoryDisplayName);
        }
    }

    public class WorkEffortTypeUnifiedGoodDisplayNameRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public WorkEffortTypeUnifiedGoodDisplayNameRuleTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedUnifiedGoodDeriveUnifiedGoodDisplayName()
        {
            var workEffortType = new WorkEffortTypeBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var good = new UnifiedGoodBuilder(this.Transaction).WithName("name").Build();
            this.Transaction.Derive(false);

            workEffortType.UnifiedGood = good;
            this.Transaction.Derive(false);

            Assert.Equal(good.Name, workEffortType.UnifiedGoodDisplayName);
        }

        [Fact]
        public void DeleteUnifiedGoodDeriveUnifiedGoodDisplayName()
        {
            var workEffortType = new WorkEffortTypeBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var good = new UnifiedGoodBuilder(this.Transaction).WithName("name").Build();
            this.Transaction.Derive(false);

            workEffortType.UnifiedGood = good;
            this.Transaction.Derive(false);

            Assert.Equal(good.Name, workEffortType.UnifiedGoodDisplayName);

            workEffortType.RemoveUnifiedGood();
            this.Transaction.Derive(false);

            Assert.False(workEffortType.ExistUnifiedGoodDisplayName);
        }

        [Fact]
        public void ChangeUnifiedGoodNameDeriveUnifiedGoodDisplayName()
        {
            var workEffortType = new WorkEffortTypeBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var good = new UnifiedGoodBuilder(this.Transaction).WithName("name").Build();
            this.Transaction.Derive(false);

            workEffortType.UnifiedGood = good;
            this.Transaction.Derive(false);

            good.Name = "changed";
            this.Transaction.Derive(false);

            Assert.Equal("changed", workEffortType.UnifiedGoodDisplayName);
        }
    }

    [Trait("Category", "Security")]
    public class WorkEffortTypeDeniedPermissionRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public WorkEffortTypeDeniedPermissionRuleTests(Fixture fixture) : base(fixture) => this.deleteRevocation = new Revocations(this.Transaction).WorkEffortTypeDeleteRevocation;

        public override Config Config => new Config { SetupSecurity = true };

        private readonly Revocation deleteRevocation;

        [Fact]
        public void OnChangedMaintenanceAgreementWorkEffortTypeDeriveDeletePermissionAllowed()
        {
            var workEffortType = new WorkEffortTypeBuilder(this.Transaction).Build();
            var agreement = new MaintenanceAgreementBuilder(this.Transaction).WithWorkEffortType(workEffortType).Build();
            this.Transaction.Derive(false);

            agreement.RemoveWorkEffortType();
            this.Transaction.Derive(false);

            Assert.DoesNotContain(this.deleteRevocation, workEffortType.Revocations);
        }

        [Fact]
        public void OnChangedMaintenanceAgreementWorkEffortTypeDeriveDeletePermissionDenied()
        {
            var workEffortType = new WorkEffortTypeBuilder(this.Transaction).Build();
            var agreement = new MaintenanceAgreementBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            agreement.WorkEffortType = workEffortType;
            this.Transaction.Derive(false);

            Assert.Contains(this.deleteRevocation, workEffortType.Revocations);
        }
    }
}
