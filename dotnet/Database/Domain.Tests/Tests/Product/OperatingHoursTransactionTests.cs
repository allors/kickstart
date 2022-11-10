// <copyright file="OperatingHoursTransactionTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

using System.Linq;
using Xunit;

namespace Allors.Database.Domain.Tests
{
    public class OperatingHoursTransactionTests : DomainTest, IClassFixture<Fixture>
    {
        public OperatingHoursTransactionTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenSerialisedItem_WhenInitialTransactionIsCreated_ThenOperatingHoursIsSet()
        {
            var productType = new ProductTypeBuilder(this.Transaction).WithSerialisedItemCharacteristicType(new SerialisedItemCharacteristicTypes(this.Transaction).OperatingHours).Build();
            var part = new UnifiedGoodBuilder(this.Transaction).WithProductType(productType).Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            part.AddSerialisedItem(serialisedItem);
            this.Transaction.Derive(false);

            new OperatingHoursTransactionBuilder(this.Transaction).WithSerialisedItem(serialisedItem).WithValue(10).Build();
            this.Transaction.Derive(false);

            var hoursCharacteristic = serialisedItem.SerialisedItemCharacteristics.FirstOrDefault(v => v.SerialisedItemCharacteristicType.Equals(new SerialisedItemCharacteristicTypes(this.Transaction).OperatingHours));

            Assert.Equal("10", hoursCharacteristic.Value);
        }

        [Fact]
        public void GivenSerialisedItem_WhenInitialTransactionIsCreated_ThenTransactionIsSynced()
        {
            var productType = new ProductTypeBuilder(this.Transaction).WithSerialisedItemCharacteristicType(new SerialisedItemCharacteristicTypes(this.Transaction).OperatingHours).Build();
            var part = new UnifiedGoodBuilder(this.Transaction).WithProductType(productType).Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            part.AddSerialisedItem(serialisedItem);
            this.Transaction.Derive(false);

            new OperatingHoursTransactionBuilder(this.Transaction).WithSerialisedItem(serialisedItem).WithValue(10).Build();
            this.Transaction.Derive(false);

            var hoursCharacteristic = serialisedItem.SerialisedItemCharacteristics.FirstOrDefault(v => v.SerialisedItemCharacteristicType.Equals(new SerialisedItemCharacteristicTypes(this.Transaction).OperatingHours));

            Assert.Single(serialisedItem.SyncedOperatingHoursTransactions);
        }

        [Fact]
        public void GivenSerialisedItem_WhenTransactionIsCreated_ThenOperatingHoursIsSet()
        {
            var productType = new ProductTypeBuilder(this.Transaction).WithSerialisedItemCharacteristicType(new SerialisedItemCharacteristicTypes(this.Transaction).OperatingHours).Build();
            var part = new UnifiedGoodBuilder(this.Transaction).WithProductType(productType).Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            part.AddSerialisedItem(serialisedItem);
            this.Transaction.Derive(false);

            new OperatingHoursTransactionBuilder(this.Transaction).WithSerialisedItem(serialisedItem).WithValue(10).Build();
            this.Transaction.Derive(false);

            new OperatingHoursTransactionBuilder(this.Transaction).WithSerialisedItem(serialisedItem).WithValue(110).Build();
            this.Transaction.Derive(false);

            var hoursCharacteristic = serialisedItem.SerialisedItemCharacteristics.FirstOrDefault(v => v.SerialisedItemCharacteristicType.Equals(new SerialisedItemCharacteristicTypes(this.Transaction).OperatingHours));

            Assert.Equal("110", hoursCharacteristic.Value);
        }

        [Fact]
        public void GivenSerialisedItem_WhenTransactionIsCreated_ThenDeltaIsDerived()
        {
            var productType = new ProductTypeBuilder(this.Transaction).WithSerialisedItemCharacteristicType(new SerialisedItemCharacteristicTypes(this.Transaction).OperatingHours).Build();
            var part = new UnifiedGoodBuilder(this.Transaction).WithProductType(productType).Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            part.AddSerialisedItem(serialisedItem);
            this.Transaction.Derive(false);

            new OperatingHoursTransactionBuilder(this.Transaction).WithSerialisedItem(serialisedItem).WithValue(10).Build();
            this.Transaction.Derive(false);

            var secondTransaction = new OperatingHoursTransactionBuilder(this.Transaction).WithSerialisedItem(serialisedItem).WithValue(110).Build();
            this.Transaction.Derive(false);

            var hoursCharacteristic = serialisedItem.SerialisedItemCharacteristics.FirstOrDefault(v => v.SerialisedItemCharacteristicType.Equals(new SerialisedItemCharacteristicTypes(this.Transaction).OperatingHours));

            Assert.Equal(100, secondTransaction.Delta);
        }

        [Fact]
        public void GivenSerialisedItem_WhenTransactionIsCreated_ThenDaysIsDerived()
        {
            var productType = new ProductTypeBuilder(this.Transaction).WithSerialisedItemCharacteristicType(new SerialisedItemCharacteristicTypes(this.Transaction).OperatingHours).Build();
            var part = new UnifiedGoodBuilder(this.Transaction).WithProductType(productType).Build();
            var serialisedItem = new SerialisedItemBuilder(this.Transaction).Build();
            part.AddSerialisedItem(serialisedItem);
            this.Transaction.Derive(false);

            new OperatingHoursTransactionBuilder(this.Transaction).WithRecordingDate(this.Transaction.Now().AddDays(-10)).WithSerialisedItem(serialisedItem).WithValue(10).Build();
            this.Transaction.Derive(false);

            var secondTransaction = new OperatingHoursTransactionBuilder(this.Transaction).WithRecordingDate(this.Transaction.Now()).WithSerialisedItem(serialisedItem).WithValue(110).Build();
            this.Transaction.Derive(false);

            var hoursCharacteristic = serialisedItem.SerialisedItemCharacteristics.FirstOrDefault(v => v.SerialisedItemCharacteristicType.Equals(new SerialisedItemCharacteristicTypes(this.Transaction).OperatingHours));

            Assert.Equal(10, secondTransaction.Days);
        }
    }
}
