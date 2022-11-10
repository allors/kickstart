// <copyright file="ProductQuoteApprovalTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

namespace Allors.Database.Domain.Tests
{
    using System.Linq;
    using Xunit;

    public class ProductQuoteApprovalTests : DomainTest, IClassFixture<Fixture>
    {
        public ProductQuoteApprovalTests(Fixture fixture) : base(fixture)
        {
        }

        [Fact]
        public void ChangedProductQuoteDeriveEmptyParticipants()
        {
            var quote = new ProductQuoteBuilder(this.Transaction).WithIssuer(this.InternalOrganisation).Build();
            this.Transaction.Derive(false);

            var approval = new ProductQuoteApprovalBuilder(this.Transaction).WithProductQuote(quote).Build();
            this.Transaction.Derive(false);

            Assert.Empty(approval.Participants);
        }

        [Fact]
        public void ChangedProductQuoteQuoteStateDeriveParticipants()
        {
            var approver = new PersonBuilder(this.Transaction).Build();
            this.InternalOrganisation.AddProductQuoteApprover(approver);
            this.Transaction.Derive(false);

            var quote = new ProductQuoteBuilder(this.Transaction).WithIssuer(this.InternalOrganisation).Build();
            this.Transaction.Derive(false);

            quote.QuoteState = new QuoteStates(this.Transaction).AwaitingApproval;
            this.Transaction.Derive(false);

            Assert.NotEmpty(quote.ProductQuoteApprovalsWhereProductQuote.First().Participants);
        }
    }
}
