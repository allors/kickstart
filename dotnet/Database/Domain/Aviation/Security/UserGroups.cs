// <copyright file="UserGroups.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the role type.</summary>

namespace Allors.Database.Domain
{
    using System;
    using Allors;

    public partial class UserGroups
    {
        public static readonly Guid LocalAdministratorsGlobalId = new Guid("02011FEF-D342-44AE-A001-6AC1D3670A6E");
        public static readonly Guid SalesAccountManagersGlobalId = new Guid("449EA7CE-124B-4E19-AFDF-46CAFB8D7B20");
        public static readonly Guid ProductManagersId = new Guid("eafb3aa1-2a4f-4f2f-b50d-9a730ad1c481");
        public static readonly Guid GenericCustomerContactsId = new Guid("429513cf-7c42-46ef-acd3-e28ccfd09b34");
        public static readonly Guid SpecificCustomerContactsId = new Guid("777cfc8b-4bea-4440-80c0-c42be59a3c29");

        public UserGroup LocalAdministratorsGlobal => this.Cache[LocalAdministratorsGlobalId];

        public UserGroup SalesAccountManagersGlobal => this.Cache[SalesAccountManagersGlobalId];

        public UserGroup ProductManagers => this.Cache[ProductManagersId];

        public UserGroup GenericCustomerContacts => this.Cache[GenericCustomerContactsId];

        public UserGroup SpecificCustomerContacts => this.Cache[SpecificCustomerContactsId];

        protected override void AviationSetup(Setup setup)
        {
            var merge = this.cache.Merger().Action();

            merge(EmployeesId, v =>
            {
                v.IsSelectable = false;
            });
            merge(LocalAdministratorsGlobalId, v =>
            {
                v.Name = "Local Administrators Global";
                v.IsSelectable = false;
            });
            merge(SalesAccountManagersGlobalId, v =>
            {
                v.Name = "Sales AccountManagers Global";
                v.IsSelectable = false;
            });
            merge(ProductManagersId, v =>
            {
                v.Name = "Product Managers";
                v.IsSelectable = true;
            });
            merge(GenericCustomerContactsId, v =>
            {
                v.Name = "Generic Customer Contacts";
                v.IsSelectable = false;
            });
            merge(SpecificCustomerContactsId, v =>
            {
                v.Name = "Specific Customer Contacts";
                v.IsSelectable = false;
            });
        }
    }
}
