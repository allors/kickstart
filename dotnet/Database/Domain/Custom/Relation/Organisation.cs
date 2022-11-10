// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Good.cs" company="Allors bvba">
//   Copyright 2002-2012 Allors bvba.
// Dual Licensed under
//   a) the General Public Licence v3 (GPL)
//   b) the Allors License
// The GPL License is included in the file gpl.txt.
// The Allors License is an addendum to your contract.
// Allors Applications is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// For more information visit http://www.allors.com/legal
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace Allors.Database.Domain
{
    public partial class Organisation
    {
        public void CustomOnPostDerive(ObjectOnPostDerive method)
        {
            var transaction = this.Strategy.Transaction;

            if (this.IsInternalOrganisation)
            {
                var groupName = $"{this.Name} ProductQuote approvers";


                if (!this.ExistProductQuoteApproverUserGroup)
                {
                    this.ProductQuoteApproverUserGroup = new UserGroups(transaction).FindBy(M.UserGroup.Name, groupName)
                                               ?? new UserGroupBuilder(transaction).WithName(groupName).WithIsSelectable(true).Build();
                }

                if (!this.ExistProductQuoteApproverSecurityToken)
                {
                    this.ProductQuoteApproverSecurityToken = new SecurityTokenBuilder(transaction).Build();
                }

                if (!this.ExistProductQuoteApproverGrant)
                {
                    var role = new Roles(transaction).ProductQuoteApprover;

                    this.ProductQuoteApproverGrant =
                        new GrantBuilder(transaction).WithRole(role)
                            .WithSubjectGroup(this.ProductQuoteApproverUserGroup)
                            .Build();

                    this.ProductQuoteApproverSecurityToken.AddGrant(this.ProductQuoteApproverGrant);
                }

                groupName = $"{this.Name} PurchaseOrder approvers level 1";

                if (!this.ExistPurchaseOrderApproverLevel1UserGroup)
                {
                    this.PurchaseOrderApproverLevel1UserGroup = new UserGroups(transaction).FindBy(M.UserGroup.Name, groupName)
                                                         ?? new UserGroupBuilder(transaction).WithName(groupName).WithIsSelectable(true).Build();
                }

                if (!this.ExistPurchaseOrderApproverLevel1SecurityToken)
                {
                    this.PurchaseOrderApproverLevel1SecurityToken = new SecurityTokenBuilder(transaction).Build();
                }

                if (!this.ExistPurchaseOrderApproverLevel1Grant)
                {
                    var role = new Roles(transaction).PurchaseOrderApproverLevel1;

                    this.PurchaseOrderApproverLevel1Grant =
                        new GrantBuilder(transaction).WithRole(role)
                            .WithSubjectGroup(this.PurchaseOrderApproverLevel1UserGroup)
                            .Build();

                    this.PurchaseOrderApproverLevel1SecurityToken.AddGrant(this.PurchaseOrderApproverLevel1Grant);
                }

                groupName = $"{this.Name} PurchaseOrder approvers level 2";

                if (!this.ExistPurchaseOrderApproverLevel2UserGroup)
                {
                    this.PurchaseOrderApproverLevel2UserGroup = new UserGroups(transaction).FindBy(M.UserGroup.Name, groupName)
                                                                ?? new UserGroupBuilder(transaction).WithName(groupName).WithIsSelectable(true).Build();
                }

                if (!this.ExistPurchaseOrderApproverLevel2SecurityToken)
                {
                    this.PurchaseOrderApproverLevel2SecurityToken = new SecurityTokenBuilder(transaction).Build();
                }

                if (!this.ExistPurchaseOrderApproverLevel2Grant)
                {
                    var role = new Roles(transaction).PurchaseOrderApproverLevel2;

                    this.PurchaseOrderApproverLevel2Grant =
                        new GrantBuilder(transaction).WithRole(role)
                            .WithSubjectGroup(this.PurchaseOrderApproverLevel2UserGroup)
                            .Build();

                    this.PurchaseOrderApproverLevel2SecurityToken.AddGrant(this.PurchaseOrderApproverLevel2Grant);
                }

                groupName = $"{this.Name} PurchaseInvoice approvers";

                if (!this.ExistPurchaseInvoiceApproverUserGroup)
                {
                    this.PurchaseInvoiceApproverUserGroup = new UserGroups(transaction).FindBy(M.UserGroup.Name, groupName)
                                                                ?? new UserGroupBuilder(transaction).WithName(groupName).WithIsSelectable(true).Build();
                }

                if (!this.ExistPurchaseInvoiceApproverSecurityToken)
                {
                    this.PurchaseInvoiceApproverSecurityToken = new SecurityTokenBuilder(transaction).Build();
                }

                if (!this.ExistPurchaseInvoiceApproverGrant)
                {
                    var role = new Roles(transaction).PurchaseInvoiceApprover;

                    this.PurchaseInvoiceApproverGrant =
                        new GrantBuilder(transaction).WithRole(role)
                            .WithSubjectGroup(this.PurchaseInvoiceApproverUserGroup)
                            .Build();

                    this.PurchaseInvoiceApproverSecurityToken.AddGrant(this.PurchaseInvoiceApproverGrant);
                }

                groupName = $"{this.Name} Blue-collar workers";

                if (!this.ExistBlueCollarWorkerUserGroup)
                {
                    this.BlueCollarWorkerUserGroup = new UserGroups(transaction).FindBy(M.UserGroup.Name, groupName)
                                                         ?? new UserGroupBuilder(transaction).WithName(groupName).WithIsSelectable(true).Build();
                }

                if (!this.ExistBlueCollarWorkerSecurityToken)
                {
                    this.BlueCollarWorkerSecurityToken = new SecurityTokenBuilder(transaction).Build();
                }

                if (!this.ExistBlueCollarWorkerGrant)
                {
                    var role = new Roles(transaction).BlueCollarWorker;

                    this.BlueCollarWorkerGrant =
                        new GrantBuilder(transaction).WithRole(role)
                            .WithSubjectGroup(this.BlueCollarWorkerUserGroup)
                            .Build();

                    this.BlueCollarWorkerSecurityToken.AddGrant(this.BlueCollarWorkerGrant);
                }

                groupName = $"{this.Name} Stock Managers";

                if (!this.ExistStockManagerUserGroup)
                {
                    this.StockManagerUserGroup = new UserGroups(transaction).FindBy(M.UserGroup.Name, groupName)
                                                     ?? new UserGroupBuilder(transaction).WithName(groupName).WithIsSelectable(true).Build();
                }

                if (!this.ExistStockManagerSecurityToken)
                {
                    this.StockManagerSecurityToken = new SecurityTokenBuilder(transaction).Build();
                }

                if (!this.ExistStockManagerGrant)
                {
                    var role = new Roles(transaction).StockManager;

                    this.StockManagerGrant =
                        new GrantBuilder(transaction).WithRole(role)
                            .WithSubjectGroup(this.StockManagerUserGroup)
                            .Build();

                    this.StockManagerSecurityToken.AddGrant(this.StockManagerGrant);
                }

                groupName = $"{this.Name} administrators";

                if (!this.ExistLocalAdministratorUserGroup)
                {
                    this.LocalAdministratorUserGroup = new UserGroups(transaction).FindBy(M.UserGroup.Name, groupName)
                                                     ?? new UserGroupBuilder(transaction).WithName(groupName).WithIsSelectable(true).Build();
                }

                if (!this.ExistLocalAdministratorSecurityToken)
                {
                    this.LocalAdministratorSecurityToken = new SecurityTokenBuilder(transaction).Build();
                }

                if (!this.ExistLocalAdministratorGrant)
                {
                    var role = new Roles(transaction).LocalAdministrator;

                    this.LocalAdministratorGrant =
                        new GrantBuilder(transaction).WithRole(role)
                            .WithSubjectGroup(this.LocalAdministratorUserGroup)
                            .Build();

                    this.LocalAdministratorSecurityToken.AddGrant(this.LocalAdministratorGrant);
                }

                groupName = $"{this.Name} employees";

                if (!this.ExistLocalEmployeeUserGroup)
                {
                    this.LocalEmployeeUserGroup = new UserGroups(transaction).FindBy(M.UserGroup.Name, groupName)
                                                     ?? new UserGroupBuilder(transaction).WithName(groupName).WithIsSelectable(true).Build();
                }

                if (!this.ExistLocalEmployeeSecurityToken)
                {
                    this.LocalEmployeeSecurityToken = new SecurityTokenBuilder(transaction).Build();
                }

                if (!this.ExistLocalEmployeeGrant)
                {
                    var role = new Roles(transaction).LocalEmployee;

                    this.LocalEmployeeGrant =
                        new GrantBuilder(transaction).WithRole(role)
                            .WithSubjectGroup(this.LocalEmployeeUserGroup)
                            .Build();

                    this.LocalEmployeeSecurityToken.AddGrant(this.LocalEmployeeGrant);
                }

                groupName = $"{this.Name} sales account managers";

                if (!this.ExistLocalSalesAccountManagerUserGroup)
                {
                    this.LocalSalesAccountManagerUserGroup = new UserGroups(transaction).FindBy(M.UserGroup.Name, groupName)
                                                     ?? new UserGroupBuilder(transaction).WithName(groupName).WithIsSelectable(true).Build();
                }

                if (!this.ExistLocalSalesAccountManagerSecurityToken)
                {
                    this.LocalSalesAccountManagerSecurityToken = new SecurityTokenBuilder(transaction).Build();
                }

                if (!this.ExistLocalSalesAccountManagerGrant)
                {
                    var role = new Roles(transaction).LocalSalesAccountManager;

                    this.LocalSalesAccountManagerGrant =
                        new GrantBuilder(transaction).WithRole(role)
                            .WithSubjectGroup(this.LocalSalesAccountManagerUserGroup)
                            .Build();

                    this.LocalSalesAccountManagerSecurityToken.AddGrant(this.LocalSalesAccountManagerGrant);
                }

                this.ProductQuoteApproverUserGroup.Members = this.ProductQuoteApprovers.ToArray();
                this.PurchaseOrderApproverLevel1UserGroup.Members = this.PurchaseOrderApproversLevel1.ToArray();
                this.PurchaseOrderApproverLevel2UserGroup.Members = this.PurchaseOrderApproversLevel2.ToArray();
                this.PurchaseInvoiceApproverUserGroup.Members = this.PurchaseInvoiceApprovers.ToArray();
                this.BlueCollarWorkerUserGroup.Members = this.BlueCollarWorkers.ToArray();
                this.StockManagerUserGroup.Members = this.StockManagers.ToArray();
                this.LocalAdministratorUserGroup.Members = this.LocalAdministrators.ToArray();
                this.LocalEmployeeUserGroup.Members = this.LocalEmployees.ToArray();
                this.LocalSalesAccountManagerUserGroup.Members = this.LocalSalesAccountManagers.ToArray();
            }
            else
            {
                var groupName = $"Customer contacts at {this.Name ?? "Unknown"} ({this.UniqueId})";

                if (!this.ExistContactsUserGroup)
                {
                    this.ContactsUserGroup = new UserGroupBuilder(transaction).WithName(groupName).WithIsSelectable(false).Build();
                }

                if (!this.ExistContactsSecurityToken)
                {
                    this.ContactsSecurityToken = new SecurityTokenBuilder(transaction).Build();
                }

                if (!this.ExistContactsGrant)
                {
                    var role = new Roles(transaction).SpecificCustomerContact;

                    this.ContactsGrant = new GrantBuilder(transaction)
                        .WithRole(role)
                        .WithSubjectGroup(this.ContactsUserGroup)
                        .Build();

                    this.ContactsSecurityToken.AddGrant(this.ContactsGrant);
                }

                this.ContactsUserGroup.Members = this.CurrentContacts.ToArray();

                foreach (var member in this.CurrentContacts)
                {
                    new UserGroups(transaction).GenericCustomerContacts.AddMember(member);
                }

                foreach (var member in this.InactiveContacts)
                {
                    new UserGroups(transaction).GenericCustomerContacts.RemoveMember(member);
                }
            }

            this.SecurityTokens = new[]
            {
                new SecurityTokens(this.Transaction()).DefaultSecurityToken,
                this.ProductQuoteApproverSecurityToken,
                this.PurchaseOrderApproverLevel1SecurityToken,
                this.PurchaseOrderApproverLevel2SecurityToken,
                this.PurchaseInvoiceApproverSecurityToken,
                this.BlueCollarWorkerSecurityToken,
                this.StockManagerSecurityToken,
                this.LocalAdministratorSecurityToken,
                this.LocalEmployeeSecurityToken,
                this.LocalSalesAccountManagerSecurityToken,
                this.ContactsSecurityToken
            };

            var internalOrganisations = transaction.Extent<Organisation>().Where(v => v.IsInternalOrganisation).ToArray();

            if (this.IsInternalOrganisation)
            {
                this.AddSecurityToken(new SecurityTokens(this.Transaction()).AllCustomersSecurityToken);

                foreach(InternalOrganisation @this in internalOrganisations)
                {
                    foreach (InternalOrganisation other in internalOrganisations.Except(new List<InternalOrganisation>() { @this }))
                    {
                        @this.AddSecurityToken(other.LocalEmployeeSecurityToken);
                    }
                }
            }
            else
            {
                foreach (InternalOrganisation @this in internalOrganisations)
                {
                    this.AddSecurityToken(@this.LocalAdministratorSecurityToken);
                    this.AddSecurityToken(@this.LocalEmployeeSecurityToken);
                }
            }
        }
    }
}