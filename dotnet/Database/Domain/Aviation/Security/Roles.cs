// <copyright file="Roles.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class Roles
    {
        public static readonly Guid ProductQuoteApproverId = new Guid("07D39583-C82C-4EA0-92F1-288FB8E17FA3");
        public static readonly Guid PurchaseOrderApproverLevel1Id = new Guid("E7F5FB30-4B12-4BF4-8D14-8640FD21ED4A");
        public static readonly Guid PurchaseOrderApproverLevel2Id = new Guid("2E5A295D-36CB-498C-A6B4-7E2DCA14B030");
        public static readonly Guid PurchaseInvoiceApproverId = new Guid("79F13FC8-7E8A-4D01-9AC1-089807F97640");
        public static readonly Guid BlueCollarWorkerId = new Guid("3C2D223E-6056-447A-A3F9-AED2413D717D");
        public static readonly Guid StockManagerId = new Guid("A220F64A-9C2E-4A4E-8AEB-0663B578D487");
        public static readonly Guid LocalSalesAccountManagerId = new Guid("1e0094c8-53ea-4f88-95e1-19b5537b26f1");
        public static readonly Guid SalesAccountManagerGlobalId = new Guid("213B1C2C-742A-4F74-9AE2-9587266D9EB9");
        public static readonly Guid LocalAdministratorId = new Guid("1F8DCC58-5647-4969-ABF2-C4A380880995");
        public static readonly Guid LocalAdministratorGlobalId = new Guid("be44d287-ef6a-4995-bc6e-3fd8e0ab1d7c");
        public static readonly Guid ProductManagerId = new Guid("e370b190-a062-45ef-8ece-ab039cd52ed1");
        public static readonly Guid GenericCustomerContactId = new Guid("2bb49af0-9b51-47d0-853d-23625f8eef65");
        public static readonly Guid SpecificCustomerContactId = new Guid("a51cb3ef-f0c2-4dbf-9447-67c6acbfafde");
        public static readonly Guid CustomerContactCreatorId = new Guid("0440295d-88ec-4b82-aba1-3e6cc7a25d27");
        public static readonly Guid LocalEmployeeId = new Guid("3d7211b4-c30d-4fac-b5c8-decd06cf74d5");

        public Role ProductQuoteApprover => this.Cache[ProductQuoteApproverId];

        public Role PurchaseOrderApproverLevel1 => this.Cache[PurchaseOrderApproverLevel1Id];

        public Role PurchaseOrderApproverLevel2 => this.Cache[PurchaseOrderApproverLevel2Id];

        public Role PurchaseInvoiceApprover => this.Cache[PurchaseInvoiceApproverId];

        public Role BlueCollarWorker => this.Cache[BlueCollarWorkerId];

        public Role StockManager => this.Cache[StockManagerId];

        public Role LocalSalesAccountManager => this.Cache[LocalSalesAccountManagerId];

        public Role SalesAccountManagerGlobal => this.Cache[SalesAccountManagerGlobalId];

        public Role LocalAdministrator => this.Cache[LocalAdministratorId];

        public Role LocalAdministratorGlobal => this.Cache[LocalAdministratorGlobalId];

        public Role ProductManager => this.Cache[ProductManagerId];

        public Role GenericCustomerContact => this.Cache[GenericCustomerContactId];

        public Role SpecificCustomerContact => this.Cache[SpecificCustomerContactId];

        public Role CustomerContactCreator => this.Cache[CustomerContactCreatorId];

        public Role LocalEmployee => this.Cache[LocalEmployeeId];

        protected override void AviationSetup(Setup setup)
        {
            var merge = this.Cache.Merger().Action();

            merge(ProductQuoteApproverId, v => v.Name = "ProductQuote approver");
            merge(PurchaseOrderApproverLevel1Id, v => v.Name = "PurchaseOrder approver level 1");
            merge(PurchaseOrderApproverLevel2Id, v => v.Name = "PurchaseOrder approver level 2");
            merge(PurchaseInvoiceApproverId, v => v.Name = "PurchaseInvoice approver");
            merge(BlueCollarWorkerId, v => v.Name = "Blue-collar worker");
            merge(StockManagerId, v => v.Name = "Stock Manager");
            merge(LocalSalesAccountManagerId, v => v.Name = "Sales Account Manager");
            merge(SalesAccountManagerGlobalId, v => v.Name = "Sales Account Manager Global");
            merge(LocalAdministratorId, v => v.Name = "Administrator");
            merge(LocalAdministratorGlobalId, v => v.Name = "Administrator Global");
            merge(ProductManagerId, v => v.Name = "Product Manager");
            merge(GenericCustomerContactId, v => v.Name = "Generic Customer contact");
            merge(SpecificCustomerContactId, v => v.Name = "Specific Customer contact");
            merge(CustomerContactCreatorId, v => v.Name = "All customer contacts as creator");
            merge(LocalEmployeeId, v => v.Name = "Employee");
        }
    }
}
