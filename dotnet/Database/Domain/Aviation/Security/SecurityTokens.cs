// <copyright file="Singletons.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;

    public partial class SecurityTokens
    {
        public static readonly Guid AllCustomersSecurityTokenId = new Guid("0d075519-319f-48cc-9c91-6e41eb43a0b7");

        public SecurityToken AllCustomersSecurityToken => this.Cache[AllCustomersSecurityTokenId];

        protected override void AviationSetup(Setup setup)
        {
            var merge = this.Cache.Merger().Action();

            var grants = new Grants(this.Transaction);

            merge(InitialSecurityTokenId, v =>
            {
                if (setup.Config.SetupSecurity)
                {
                    v.AddGrant(grants.CustomerContactCreator);
                }
            });

            merge(DefaultSecurityTokenId, v =>
            {
                if (setup.Config.SetupSecurity)
                {
                    v.AddGrant(grants.SalesAccountManagerGlobal);
                    v.AddGrant(grants.LocalAdministratorGlobal);
                    v.AddGrant(grants.ProductManager);
                    v.AddGrant(grants.GenericCustomerContact);
                }
            });

            merge(AllCustomersSecurityTokenId, v =>
            {
                if (setup.Config.SetupSecurity)
                {
                    v.AddGrant(grants.CustomerContactCreator);
                }
            });
        }
    }
}
