// <copyright file="Grants.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class Grants
    {
        public static readonly Guid SalesAccountManagerGlobalId = new Guid("2E0C80D3-A416-4227-B92D-2EB66FF40BD1");
        public static readonly Guid LocalAdministratorGlobalId = new Guid("51CF85DD-F10B-48A6-9B8A-EA516BDC03D0");
        public static readonly Guid ProductManagerId = new Guid("0ee92303-c28d-449c-8d66-b77a88ba7ffd");
        public static readonly Guid GenericCustomerContactId = new Guid("dbae0483-b794-4d9a-8c75-0e621d12cd8d");
        public static readonly Guid SpecificCustomerContactId = new Guid("2ad2d4fb-2042-447f-a06a-5afc7d818822");
        public static readonly Guid CustomerContactCreatorId = new Guid("0bfd8d91-deba-48e3-bc50-2da3b1d8d615");

        public Grant SalesAccountManagerGlobal => this.Cache[SalesAccountManagerGlobalId];

        public Grant LocalAdministratorGlobal => this.Cache[LocalAdministratorGlobalId];

        public Grant ProductManager => this.Cache[ProductManagerId];

        public Grant GenericCustomerContact => this.Cache[GenericCustomerContactId];

        public Grant SpecificCustomerContact => this.Cache[SpecificCustomerContactId];

        public Grant CustomerContactCreator => this.Cache[CustomerContactCreatorId];

        protected override void AviationSetup(Setup setup)
        {
            if (setup.Config.SetupSecurity)
            {
                var merge = this.Cache.Merger().Action();

                var roles = new Roles(this.Transaction);
                var userGroups = new UserGroups(this.Transaction);

                merge(SalesAccountManagerGlobalId, v =>
                {
                    v.Role = roles.SalesAccountManagerGlobal;
                    v.AddSubjectGroup(userGroups.SalesAccountManagersGlobal);
                });

                merge(LocalAdministratorGlobalId, v =>
                {
                    v.Role = roles.LocalAdministratorGlobal;
                    v.AddSubjectGroup(userGroups.LocalAdministratorsGlobal);
                });

                merge(ProductManagerId, v =>
                {
                    v.Role = roles.ProductManager;
                    v.AddSubjectGroup(userGroups.ProductManagers);
                });

                merge(GenericCustomerContactId, v =>
                {
                    v.Role = roles.GenericCustomerContact;
                    v.AddSubjectGroup(userGroups.GenericCustomerContacts);
                });

                merge(SpecificCustomerContactId, v =>
                {
                    v.Role = roles.SpecificCustomerContact;
                    v.AddSubjectGroup(userGroups.SpecificCustomerContacts);
                });

                merge(CustomerContactCreatorId, v =>
                {
                    v.Role = roles.CustomerContactCreator;
                    v.AddSubjectGroup(userGroups.GenericCustomerContacts);
                });
            }
        }
    }
}