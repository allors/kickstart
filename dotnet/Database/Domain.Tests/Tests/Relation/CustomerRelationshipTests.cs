// <copyright file="CustomerRelationshipTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the MediaTests type.</summary>

using System.Linq;
using Xunit;

namespace Allors.Database.Domain.Tests
{
    public class CustomerRelationshipTests : DomainTest, IClassFixture<Fixture>
    {
        public CustomerRelationshipTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void ChangedCustomerDeriveCustomerName()
        {
            var relationship = new CustomerRelationshipBuilder(this.Transaction).Build();
            this.Transaction.Derive(false);

            relationship.Customer = this.Customer;
            this.Transaction.Derive(false);

            Assert.Equal(this.Customer.DisplayName, relationship.CustomerName);
        }

        [Fact]
        public void ChangedCustomerPartyNameDeriveCustomerName()
        {
            var customer = new OrganisationBuilder(this.Transaction).Build();
            var relationship = new CustomerRelationshipBuilder(this.Transaction).WithCustomer(customer).Build();
            this.Transaction.Derive(false);

            customer.Name = "changed";
            this.Transaction.Derive(false);

            Assert.Equal("changed", relationship.CustomerName);
        }
    }
}
