// <copyright file="CustomerContactTests.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using Allors.Database.Domain.TestPopulation;
using System.Linq;
using Xunit;

namespace Allors.Database.Domain.Tests
{
    public class CustomerContactTests : DomainTest, IClassFixture<Fixture>
    {
        public CustomerContactTests(Fixture fixture) : base(fixture) { }

        [Fact]
        public void UserGroup()
        {
            var organisation = new OrganisationBuilder(this.Transaction).WithName("organisation").Build();
            this.Transaction.Derive(true);

            Assert.True(organisation.ExistContactsUserGroup);

            organisation.RemoveContactsUserGroup();
            this.Transaction.Derive();

            Assert.True(organisation.ExistContactsUserGroup);
        }

        [Fact]
        public void Grant()
        {
            var organisation = new OrganisationBuilder(this.Transaction).WithName("organisation").Build();
            this.Transaction.Derive(true);

            Assert.True(organisation.ExistContactsGrant);
            Assert.Equal(new Roles(this.Transaction).SpecificCustomerContact, organisation.ContactsGrant.Role);
            Assert.Contains(organisation.ContactsUserGroup, organisation.ContactsGrant.SubjectGroups);

            organisation.RemoveContactsGrant();

            this.Transaction.Derive(true);

            Assert.True(organisation.ExistContactsGrant);
            Assert.Equal(new Roles(this.Transaction).SpecificCustomerContact, organisation.ContactsGrant.Role);
            Assert.Contains(organisation.ContactsUserGroup, organisation.ContactsGrant.SubjectGroups);
        }

        [Fact]
        public void SecurityToken()
        {
            var organisation = new OrganisationBuilder(this.Transaction).WithName("organisation").Build();
            this.Transaction.Derive(true);

            Assert.True(organisation.ExistContactsSecurityToken);
            Assert.Contains(organisation.ContactsGrant, organisation.ContactsSecurityToken.Grants);
        }

        [Fact]
        public void StockManagers()
        {
            var organisation = new OrganisationBuilder(this.Transaction).WithName("organisation").Build();
            this.Transaction.Derive(true);

            var contact= new PersonBuilder(this.Transaction)
                .WithUserName("contactuser")
                .WithLastName("contact")
                .Build();
            this.Transaction.Derive(true);

            new OrganisationContactRelationshipBuilder(this.Transaction).WithContact(contact).WithOrganisation(organisation).Build();
            this.Transaction.Derive(true);

            Assert.Contains(contact, organisation.ContactsUserGroup.Members);
        }
    }

    public class CustomerContactSecurityTests : DomainTest, IClassFixture<Fixture>
    {
        public CustomerContactSecurityTests(Fixture fixture) : base(fixture) { }

        public override Config Config => new Config { SetupSecurity = true };

        [Fact]
        public void WorkTaskOwnOrganisation()
        {
            var customer = new OrganisationBuilder(this.Transaction).WithName("Org1").Build();
            new CustomerRelationshipBuilder(this.Transaction).WithCustomer(customer).WithInternalOrganisation(this.InternalOrganisation).Build();

            var contact = new PersonBuilder(this.Transaction)
                .WithUserName("contactuser")
                .WithLastName("contact")
                .Build();

            new OrganisationContactRelationshipBuilder(this.Transaction).WithContact(contact).WithOrganisation(customer).Build();
            this.Transaction.Derive(true);

            var workTask = new WorkTaskBuilder(this.Transaction).WithName("Activity").WithCustomer(customer).WithTakenBy(this.InternalOrganisation).Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            this.Transaction.SetUser(contact);

            var acl = new DatabaseAccessControl(this.Security, contact)[workTask];
            Assert.False(acl.CanRead(M.WorkTask.Description));
            Assert.False(acl.CanWrite(M.WorkTask.Description));
        }

        [Fact]
        public void WorkTaskOtherOrganisation()
        {
            var customer = new OrganisationBuilder(this.Transaction).WithName("Org1").Build();
            new CustomerRelationshipBuilder(this.Transaction).WithCustomer(customer).WithInternalOrganisation(this.InternalOrganisation).Build();

            var anotherCustomer = new OrganisationBuilder(this.Transaction).WithName("Org2").Build();
            new CustomerRelationshipBuilder(this.Transaction).WithCustomer(anotherCustomer).WithInternalOrganisation(this.InternalOrganisation).Build();

            var contact = new PersonBuilder(this.Transaction)
                .WithUserName("contactuser")
                .WithLastName("contact")
                .Build();

            new OrganisationContactRelationshipBuilder(this.Transaction).WithContact(contact).WithOrganisation(customer).Build();
            this.Transaction.Derive(true);

            var workTask = new WorkTaskBuilder(this.Transaction).WithName("Activity").WithCustomer(anotherCustomer).WithTakenBy(this.InternalOrganisation).Build();

            this.Transaction.Derive();
            this.Transaction.Commit();

            this.Transaction.SetUser(contact);

            var acl = new DatabaseAccessControl(this.Security, contact)[workTask];
            Assert.False(acl.CanRead(M.WorkTask.Description));
            Assert.False(acl.CanWrite(M.WorkTask.Description));
        }

        [Fact]
        public void OwnOrganisation()
        {
            var customer = new OrganisationBuilder(this.Transaction).WithName("Org1").Build();
            new CustomerRelationshipBuilder(this.Transaction).WithCustomer(customer).WithInternalOrganisation(this.InternalOrganisation).Build();

            var contact = new PersonBuilder(this.Transaction)
                .WithUserName("contactuser")
                .WithLastName("contact")
                .Build();

            new OrganisationContactRelationshipBuilder(this.Transaction).WithContact(contact).WithOrganisation(customer).Build();
            this.Transaction.Derive(true);

            this.Transaction.SetUser(contact);

            var acl = new DatabaseAccessControl(this.Security, contact)[customer];
            Assert.True(acl.CanRead(M.Organisation.Name));
            Assert.False(acl.CanWrite(M.Organisation.Name));
        }

        [Fact]
        public void OtherOrganisation()
        {
            var customer = new OrganisationBuilder(this.Transaction).WithName("Org1").Build();
            new CustomerRelationshipBuilder(this.Transaction).WithCustomer(customer).WithInternalOrganisation(this.InternalOrganisation).Build();

            var anotherCustomer = new OrganisationBuilder(this.Transaction).WithName("Org2").Build();
            new CustomerRelationshipBuilder(this.Transaction).WithCustomer(anotherCustomer).WithInternalOrganisation(this.InternalOrganisation).Build();

            var contact = new PersonBuilder(this.Transaction)
                .WithUserName("contactuser")
                .WithLastName("contact")
                .Build();

            new OrganisationContactRelationshipBuilder(this.Transaction).WithContact(contact).WithOrganisation(customer).Build();
            this.Transaction.Derive(true);

            this.Transaction.SetUser(contact);

            var acl = new DatabaseAccessControl(this.Security, contact)[anotherCustomer];
            Assert.False(acl.CanRead(M.Organisation.Name));
            Assert.False(acl.CanWrite(M.Organisation.Name));
        }

        [Fact]
        public void OperatingHoursTransaction()
        {
            var customer = new OrganisationBuilder(this.Transaction).WithName("Org1").Build();
            new CustomerRelationshipBuilder(this.Transaction).WithCustomer(customer).WithInternalOrganisation(this.InternalOrganisation).Build();

            var contact = new PersonBuilder(this.Transaction)
                .WithUserName("contactuser")
                .WithLastName("contact")
                .Build();

            new OrganisationContactRelationshipBuilder(this.Transaction).WithContact(contact).WithOrganisation(customer).Build();
            this.Transaction.Derive(true);

            var serialisedItem = new SerialisedItemBuilder(this.Transaction).WithDefaults(this.InternalOrganisation).Build();
            serialisedItem.OwnedBy = customer;

            var operatingHoursTransaction = new OperatingHoursTransactionBuilder(this.Transaction).WithSerialisedItem(serialisedItem).Build();

            this.Transaction.Derive(false);
            
            this.Transaction.SetUser(contact);

            var acl = new DatabaseAccessControl(this.Security, contact)[operatingHoursTransaction];
            Assert.True(acl.CanRead(M.OperatingHoursTransaction.Value));
            Assert.True(acl.CanWrite(M.OperatingHoursTransaction.Value));
        }
    }
}
