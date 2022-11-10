// <copyright file="PurchaseInvoiceTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using TestPopulation;
    using Xunit;

    public class PurchaseInvoiceApprovalRuleTests : DomainTest, IClassFixture<Fixture>
    {
        public PurchaseInvoiceApprovalRuleTests(Fixture fixture) : base(fixture) {}

        [Fact]
        public void OnCreatedPurchaseInvoiceApprovalDeriveEmptyParticipants()
        {
            var purchaseInvoice = new PurchaseInvoiceBuilder(this.Transaction).WithBilledTo(this.InternalOrganisation).Build();
            this.Transaction.Derive(false);

            var approval = new PurchaseInvoiceApprovalBuilder(this.Transaction).WithPurchaseInvoice(purchaseInvoice).Build();

            this.Transaction.Derive(false);

            Assert.Empty(approval.Participants);
        }

        [Fact]
        public void OnCreatedPurchaseInvoiceApprovalDeriveParticipants()
        {
            var approver = new PersonBuilder(this.Transaction).Build();
            this.InternalOrganisation.AddPurchaseInvoiceApprover(approver);
            this.Transaction.Derive(false);

            var purchaseInvoice = new PurchaseInvoiceBuilder(this.Transaction).WithBilledTo(this.InternalOrganisation).Build();
            this.Transaction.Derive(false);

            purchaseInvoice.Confirm();
            this.Transaction.Derive(false);

            Assert.NotEmpty(purchaseInvoice.PurchaseInvoiceApprovalsWherePurchaseInvoice.First().Participants);
        }

        [Fact]
        public void OnChangedPurchaseInvoiceApprovalDeriveParticipants()
        {
            var approver = new PersonBuilder(this.Transaction).Build();
            this.InternalOrganisation.AddPurchaseInvoiceApprover(approver);
            this.Transaction.Derive(false);

            var purchaseInvoice = new PurchaseInvoiceBuilder(this.Transaction).WithBilledTo(this.InternalOrganisation).Build();
            this.Transaction.Derive(false);

            purchaseInvoice.Confirm();
            this.Transaction.Derive(false);

            var approval = purchaseInvoice.PurchaseInvoiceApprovalsWherePurchaseInvoice.First();
            approval.Approve();
            this.Transaction.Derive(false);

            Assert.Empty(purchaseInvoice.PurchaseInvoiceApprovalsWherePurchaseInvoice.First().Participants);
        }
    }
}
