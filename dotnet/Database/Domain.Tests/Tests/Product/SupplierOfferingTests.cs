// <copyright file="SupplierOfferingTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

using Allors.Database.Domain;
using Allors.Database.Domain.TestPopulation;
using Allors.Database.Meta;

namespace Allors.Database.Domain.Tests
{
    using System;
    using System.Linq;
    using Xunit;

    public class SupplierOfferingTests : DomainTest, IClassFixture<Fixture>
    {
        public SupplierOfferingTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void GivenSupplierOffering_WhenCalculatingUnitSellingPrice_ThenConsiderHighestHistoricalPurchaseRate()
        {
            var m = this.Transaction.Database.Services.Get<MetaPopulation>();
            var settings = this.Transaction.GetSingleton().Settings;

            var supplier_1 = new OrganisationBuilder(this.Transaction).WithName("supplier uno").Build();
            var supplier_2 = new OrganisationBuilder(this.Transaction).WithName("supplier dos").Build();
            var supplier_3 = new OrganisationBuilder(this.Transaction).WithName("supplier tres").Build();
            var supplier_4 = new OrganisationBuilder(this.Transaction).WithName("supplier cuatro").Build();

            new SupplierRelationshipBuilder(this.Transaction)
                .WithSupplier(supplier_1)
                .WithInternalOrganisation(this.InternalOrganisation)
                .WithFromDate(Transaction.Now().AddYears(-3))
                .Build();
            new SupplierRelationshipBuilder(this.Transaction)
                .WithSupplier(supplier_2)
                .WithInternalOrganisation(this.InternalOrganisation)
                .WithFromDate(Transaction.Now().AddYears(-2))
                .Build();
            new SupplierRelationshipBuilder(this.Transaction)
                .WithSupplier(supplier_3)
                .WithInternalOrganisation(this.InternalOrganisation)
                .WithFromDate(Transaction.Now().AddYears(-1))
                .Build();
            new SupplierRelationshipBuilder(this.Transaction)
                .WithSupplier(supplier_4)
                .WithInternalOrganisation(this.InternalOrganisation)
                .WithFromDate(Transaction.Now().AddMonths(-6))
                .Build();

            var finishedGood = new NonUnifiedPartBuilder(this.Transaction)
                .WithNonSerialisedDefaults(this.InternalOrganisation)
                .Build();

            this.Transaction.Derive();

            new InventoryItemTransactionBuilder(this.Transaction).WithQuantity(100).WithReason(new InventoryTransactionReasons(this.Transaction).Unknown).WithPart(finishedGood).Build();
            this.Transaction.Derive(true);

            var euro = new Currencies(this.Transaction).FindBy(m.Currency.IsoCode, "EUR");
            var piece = new UnitsOfMeasure(this.Transaction).Piece;
            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(finishedGood)
                .WithSupplier(supplier_1)
                .WithFromDate(Transaction.Now().AddMonths(-6))
                .WithThroughDate(Transaction.Now().AddMonths(-3))
                .WithUnitOfMeasure(piece)
                .WithPrice(100)
                .WithCurrency(euro)
                .Build();

            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(finishedGood)
                .WithSupplier(supplier_2)
                .WithFromDate(Transaction.Now().AddYears(-1))
                .WithThroughDate(Transaction.Now().AddDays(-1))
                .WithUnitOfMeasure(piece)
                .WithPrice(120)
                .WithCurrency(euro)
                .Build();

            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(finishedGood)
                .WithSupplier(supplier_3)
                .WithFromDate(this.Transaction.Now())
                .WithUnitOfMeasure(piece)
                .WithPrice(99)
                .WithCurrency(euro)
                .Build();

            new SupplierOfferingBuilder(this.Transaction)
                .WithPart(finishedGood)
                .WithSupplier(supplier_4)
                .WithFromDate(Transaction.Now().AddDays(7))
                .WithThroughDate(Transaction.Now().AddDays(30))
                .WithUnitOfMeasure(piece)
                .WithPrice(135)
                .WithCurrency(euro)
                .Build();

            this.Transaction.Derive();

            var customer = this.InternalOrganisation.CreateB2BCustomer(this.Transaction.Faker());

            var workEffort = new WorkTaskBuilder(this.Transaction)
                .WithName("Activity")
                .WithCustomer(customer)
                .WithTakenBy(this.InternalOrganisation)
                .Build();

            var workEffortInventoryAssignement = new WorkEffortInventoryAssignmentBuilder(this.Transaction)
                .WithAssignment(workEffort)
                .WithInventoryItem(finishedGood.InventoryItemsWherePart.First())
                .WithQuantity(1)
                .Build();

            this.Transaction.Derive();

            //Purchase price times InternalSurchargePercentage
            var sellingPrice = Math.Round(135 * (1 + this.Transaction.GetSingleton().Settings.PartSurchargePercentage / 100), 2);

            Assert.Equal(sellingPrice, workEffortInventoryAssignement.UnitSellingPrice);
        }
    }
}
