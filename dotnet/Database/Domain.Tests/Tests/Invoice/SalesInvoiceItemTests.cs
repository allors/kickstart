namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using Xunit;

    public class SalesInvoiceItemTest : DomainTest, IClassFixture<Fixture>
    {
        public SalesInvoiceItemTest(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedSerialisedItemDeriveDescription()
        {
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();

            var invoice = new SalesInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new SalesInvoiceItemBuilder(this.Transaction).WithSerialisedItem(serialisedItem).Build();
            invoice.AddSalesInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            Assert.True(invoiceItem.ExistDescription);
        }

        [Fact]
        public void ChangedProductDeriveDescription()
        {
            var product = new UnifiedGoodBuilder(this.Transaction).Build();

            var invoice = new SalesInvoiceBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            var invoiceItem = new SalesInvoiceItemBuilder(this.Transaction).WithProduct(product).Build();
            invoice.AddSalesInvoiceItem(invoiceItem);
            this.Transaction.Derive(false);

            Assert.True(invoiceItem.ExistDescription);
        }

        [Fact]
        public void ChangedWorkTaskTotalLabourRevenueDeriveCleaningGSEAmount()
        {
            var workTask = new WorkTaskBuilder(this.Transaction).WithCustomer(this.Customer).Build();
            this.Transaction.Derive(false);

            var timeEntry = new TimeEntryBuilder(this.Transaction)
                .WithWorkEffort(workTask)
                .WithIsBillable(true)
                .WithBillingFrequency(new TimeFrequencies(this.Transaction).Hour)
                .WithAssignedBillingRate(10)
                .WithAssignedAmountOfTime(1.5M)
                .Build();
            this.Transaction.Derive(false);

            var cleaningGSE = workTask.WorkEffortInvoiceItemAssignmentsWhereAssignment.Where(v => v.WorkEffortInvoiceItem.InvoiceItemType.IsCleaning).Select(v => v.WorkEffortInvoiceItem).First();

            Assert.Equal(14.8M, cleaningGSE.Amount);
        }

        [Fact]
        public void ChangedWorkTaskTotalLabourRevenueDeriveCleaningGSEAmountMinimum()
        {
            this.Customer.CleaningMinimum = 20M;
            this.Transaction.Derive(false);

            var workTask = new WorkTaskBuilder(this.Transaction).WithCustomer(this.Customer).Build();
            this.Transaction.Derive(false);

            var timeEntry = new TimeEntryBuilder(this.Transaction)
                .WithWorkEffort(workTask)
                .WithIsBillable(true)
                .WithBillingFrequency(new TimeFrequencies(this.Transaction).Hour)
                .WithAssignedBillingRate(10)
                .WithAssignedAmountOfTime(1.5M)
                .Build();
            this.Transaction.Derive(false);

            var cleaningGSE = workTask.WorkEffortInvoiceItemAssignmentsWhereAssignment.Where(v => v.WorkEffortInvoiceItem.InvoiceItemType.IsCleaning).Select(v => v.WorkEffortInvoiceItem).First();

            Assert.Equal(this.Customer.CleaningMinimum, cleaningGSE.Amount);
        }

        [Fact]
        public void ChangedWorkTaskTotalLabourRevenueDeriveCleaningGSEAmountMaximum()
        {
            this.Customer.CleaningMaximum = 10M;
            this.Transaction.Derive(false);

            var workTask = new WorkTaskBuilder(this.Transaction).WithCustomer(this.Customer).Build();
            this.Transaction.Derive(false);

            var timeEntry = new TimeEntryBuilder(this.Transaction)
                .WithWorkEffort(workTask)
                .WithIsBillable(true)
                .WithBillingFrequency(new TimeFrequencies(this.Transaction).Hour)
                .WithAssignedBillingRate(10)
                .WithAssignedAmountOfTime(1.5M)
                .Build();
            this.Transaction.Derive(false);

            var cleaningGSE = workTask.WorkEffortInvoiceItemAssignmentsWhereAssignment.Where(v => v.WorkEffortInvoiceItem.InvoiceItemType.IsCleaning).Select(v => v.WorkEffortInvoiceItem).First();

            Assert.Equal(this.Customer.CleaningMaximum, cleaningGSE.Amount);
        }

        [Fact]
        public void ChangedWorkTaskTotalLabourRevenueDeriveCleaningGSEExcluded()
        {
            this.Customer.ExcludeCleaning = true;
            this.Transaction.Derive(false);

            var workTask = new WorkTaskBuilder(this.Transaction).WithCustomer(this.Customer).Build();
            this.Transaction.Derive(false);

            var timeEntry = new TimeEntryBuilder(this.Transaction)
                .WithWorkEffort(workTask)
                .WithIsBillable(true)
                .WithBillingFrequency(new TimeFrequencies(this.Transaction).Hour)
                .WithAssignedBillingRate(10)
                .WithAssignedAmountOfTime(1.5M)
                .Build();
            this.Transaction.Derive(false);

            var cleaningGSE = workTask.WorkEffortInvoiceItemAssignmentsWhereAssignment.Where(v => v.WorkEffortInvoiceItem.InvoiceItemType.IsCleaning).Select(v => v.WorkEffortInvoiceItem).FirstOrDefault();

            Assert.Null(cleaningGSE);
        }

        [Fact]
        public void ChangedWorkTaskTotalLabourRevenueDeriveSundriesAmount()
        {
            var workTask = new WorkTaskBuilder(this.Transaction).WithCustomer(this.Customer).Build();
            this.Transaction.Derive(false);

            var timeEntry = new TimeEntryBuilder(this.Transaction)
                .WithWorkEffort(workTask)
                .WithIsBillable(true)
                .WithBillingFrequency(new TimeFrequencies(this.Transaction).Hour)
                .WithAssignedBillingRate(10)
                .WithAssignedAmountOfTime(1.5M)
                .Build();
            this.Transaction.Derive(false);

            var sundries = workTask.WorkEffortInvoiceItemAssignmentsWhereAssignment.Where(v => v.WorkEffortInvoiceItem.InvoiceItemType.IsSundries).Select(v => v.WorkEffortInvoiceItem).First();

            Assert.Equal(8.85M, sundries.Amount);
        }

        [Fact]
        public void ChangedWorkTaskTotalLabourRevenueDeriveSundriesAmountMinimum()
        {
            this.Customer.SundriesMinimum = 20M;
            this.Transaction.Derive(false);

            var workTask = new WorkTaskBuilder(this.Transaction).WithCustomer(this.Customer).Build();
            this.Transaction.Derive(false);

            var timeEntry = new TimeEntryBuilder(this.Transaction)
                .WithWorkEffort(workTask)
                .WithIsBillable(true)
                .WithBillingFrequency(new TimeFrequencies(this.Transaction).Hour)
                .WithAssignedBillingRate(10)
                .WithAssignedAmountOfTime(1.5M)
                .Build();
            this.Transaction.Derive(false);

            var sundries = workTask.WorkEffortInvoiceItemAssignmentsWhereAssignment.Where(v => v.WorkEffortInvoiceItem.InvoiceItemType.IsSundries).Select(v => v.WorkEffortInvoiceItem).First();

            Assert.Equal(this.Customer.SundriesMinimum, sundries.Amount);
        }

        [Fact]
        public void ChangedWorkTaskTotalLabourRevenueDeriveSundriesAmountMaximum()
        {
            this.Customer.SundriesMaximum = 5M;
            this.Transaction.Derive(false);

            var workTask = new WorkTaskBuilder(this.Transaction).WithCustomer(this.Customer).Build();
            this.Transaction.Derive(false);

            var timeEntry = new TimeEntryBuilder(this.Transaction)
                .WithWorkEffort(workTask)
                .WithIsBillable(true)
                .WithBillingFrequency(new TimeFrequencies(this.Transaction).Hour)
                .WithAssignedBillingRate(10)
                .WithAssignedAmountOfTime(1.5M)
                .Build();
            this.Transaction.Derive(false);

            var sundries = workTask.WorkEffortInvoiceItemAssignmentsWhereAssignment.Where(v => v.WorkEffortInvoiceItem.InvoiceItemType.IsSundries).Select(v => v.WorkEffortInvoiceItem).First();

            Assert.Equal(this.Customer.SundriesMaximum, sundries.Amount);
        }

        [Fact]
        public void ChangedWorkTaskTotalLabourRevenueDeriveSundriesExcluded()
        {
            this.Customer.ExcludeSundries = true;
            this.Transaction.Derive(false);

            var workTask = new WorkTaskBuilder(this.Transaction).WithCustomer(this.Customer).Build();
            this.Transaction.Derive(false);

            var timeEntry = new TimeEntryBuilder(this.Transaction)
                .WithWorkEffort(workTask)
                .WithIsBillable(true)
                .WithBillingFrequency(new TimeFrequencies(this.Transaction).Hour)
                .WithAssignedBillingRate(10)
                .WithAssignedAmountOfTime(1.5M)
                .Build();
            this.Transaction.Derive(false);

            var sundries = workTask.WorkEffortInvoiceItemAssignmentsWhereAssignment.Where(v => v.WorkEffortInvoiceItem.InvoiceItemType.IsSundries).Select(v => v.WorkEffortInvoiceItem).FirstOrDefault();

            Assert.Null(sundries);
        }
    }
}
