// <copyright file="PurchaseOrderApprovalLevel1Tests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using TestPopulation;
    using Xunit;

    public class PurchaseOrderApprovalLevel1Tests : DomainTest, IClassFixture<Fixture>
    {
        public PurchaseOrderApprovalLevel1Tests(Fixture fixture) : base(fixture)
        {

        }

        [Fact]
        public void OnCreatedPurchaseOrderApprovalLevel1DeriveEmptyParticipants()
        {
            var purchaseOrder = new PurchaseOrderBuilder(this.Transaction).WithDefaults(this.InternalOrganisation).Build();

            this.Transaction.Derive(false);

            var approval = new PurchaseOrderApprovalLevel1Builder(this.Transaction).WithPurchaseOrder(purchaseOrder).Build();

            this.Transaction.Derive(false);

            Assert.Empty(approval.Participants);
        }

        [Fact]
        public void OnCreatedPurchaseInvoiceApprovalLevel1DeriveParticipants()
        {
            var approver = new PersonBuilder(this.Transaction).Build();
            this.InternalOrganisation.AddPurchaseOrderApproversLevel1(approver);
            this.Transaction.Derive(false);

            var purchaseOrder = this.InternalOrganisation.CreatePurchaseOrderWithNonSerializedItem(this.Transaction.Faker());

            var supplierRelationship = purchaseOrder.TakenViaSupplier.SupplierRelationshipsWhereSupplier.First(v => v.InternalOrganisation == purchaseOrder.OrderedBy);
            supplierRelationship.NeedsApproval = true;
            supplierRelationship.ApprovalThresholdLevel1 = 1;

            this.Transaction.Derive(false);

            purchaseOrder.SetReadyForProcessing();

            this.Transaction.Derive(false);

            Assert.NotEmpty(purchaseOrder.PurchaseOrderApprovalsLevel1WherePurchaseOrder.First().Participants);
        }
    }
}
