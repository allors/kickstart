
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Security.cs" company="Allors bvba">
//   Copyright 2002-2016 Allors bvba.
// 
// Dual Licensed under
//   a) the General Public Licence v3 (GPL)
//   b) the Allors License
// 
// The GPL License is included in the file gpl.txt.
// The Allors License is an addendum to your contract.
// 
// Allors Applications is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// For more information visit http://www.allors.com/legal
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Allors.Database.Domain
{
    using Allors.Database.Meta;

    public partial class Security
    {
        public void GrantEmployee(ObjectType objectType, params Operations[] operations)
        {
            this.Grant(Roles.EmployeeId, objectType, operations);
        }

        public void GrantBlueCollarWorker(ObjectType objectType, params Operations[] operations) => this.Grant(Roles.BlueCollarWorkerId, objectType, operations);

        public void GrantExceptBlueCollarWorker(ObjectType objectType, ICollection<IOperandType> excepts, params Operations[] operations) => this.GrantExcept(Roles.BlueCollarWorkerId, objectType, excepts, operations);

        public void GrantStockManager(ObjectType objectType, params Operations[] operations) => this.Grant(Roles.StockManagerId, objectType, operations);

        public void GrantExceptStockManager(ObjectType objectType, ICollection<IOperandType> excepts, params Operations[] operations) => this.GrantExcept(Roles.StockManagerId, objectType, excepts, operations);

        public void GrantProductQuoteApprover(ObjectType objectType, params Operations[] operations) => this.Grant(Roles.ProductQuoteApproverId, objectType, operations);

        public void GrantPurchaseOrderApproverLevel1(ObjectType objectType, params Operations[] operations) => this.Grant(Roles.PurchaseOrderApproverLevel1Id, objectType, operations);

        public void GrantPurchaseOrderApproverLevel2(ObjectType objectType, params Operations[] operations) => this.Grant(Roles.PurchaseOrderApproverLevel2Id, objectType, operations);

        public void GrantLocalAdministrator(ObjectType objectType, params Operations[] operations) => this.Grant(Roles.LocalAdministratorId, objectType, operations);

        public void GrantExceptLocalAdministrator(ObjectType objectType, ICollection<IOperandType> excepts, params Operations[] operations) => this.GrantExcept(Roles.LocalAdministratorId, objectType, excepts, operations);

        public void GrantLocalAdministratorGlobal(ObjectType objectType, params Operations[] operations) => this.Grant(Roles.LocalAdministratorGlobalId, objectType, operations);

        public void GrantExceptLocalAdministratorGlobal(ObjectType objectType, ICollection<IOperandType> excepts, params Operations[] operations) => this.GrantExcept(Roles.LocalAdministratorGlobalId, objectType, excepts, operations);

        public void GrantSalesAccountManagerGlobal(ObjectType objectType, params Operations[] operations) => this.Grant(Roles.SalesAccountManagerGlobalId, objectType, operations);

        public void GrantExceptSalesAccountManagerGlobal(ObjectType objectType, ICollection<IOperandType> excepts, params Operations[] operations) => this.GrantExcept(Roles.SalesAccountManagerGlobalId, objectType, excepts, operations);

        public void GrantLocalSalesAccountManager(ObjectType objectType, params Operations[] operations) => this.Grant(Roles.LocalSalesAccountManagerId, objectType, operations);

        public void GrantExceptLocalSalesAccountManager(ObjectType objectType, ICollection<IOperandType> excepts, params Operations[] operations) => this.GrantExcept(Roles.LocalSalesAccountManagerId, objectType, excepts, operations);

        public void GrantProductManager(ObjectType objectType, params Operations[] operations) => this.Grant(Roles.ProductManagerId, objectType, operations);

        public void GrantExceptProductManager(ObjectType objectType, ICollection<IOperandType> excepts, params Operations[] operations) => this.GrantExcept(Roles.ProductManagerId, objectType, excepts, operations);

        public void GrantGenericCustomerContact(ObjectType objectType, params Operations[] operations) => this.Grant(Roles.GenericCustomerContactId, objectType, operations);

        public void GrantSpecificCustomerContact(ObjectType objectType, params Operations[] operations) => this.Grant(Roles.SpecificCustomerContactId, objectType, operations);

        public void GrantCustomerContactCreator(ObjectType objectType, params Operations[] operations) => this.Grant(Roles.CustomerContactCreatorId, objectType, operations);

        public void GrantLocalEmployee(ObjectType objectType, params Operations[] operations) => this.Grant(Roles.LocalEmployeeId, objectType, operations);

        public void GrantExceptLocalEmployee(ObjectType objectType, ICollection<IOperandType> excepts, params Operations[] operations) => this.GrantExcept(Roles.LocalEmployeeId, objectType, excepts, operations);

        private void AviationOnPreSetup()
        {
        }

        private void AviationOnPostSetup()
        {
        }
    }
}