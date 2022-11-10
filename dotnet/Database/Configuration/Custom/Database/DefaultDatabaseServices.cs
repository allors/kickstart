// <copyright file="DefaultDatabaseScope.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the DomainTest type.</summary>

namespace Allors.Database.Configuration
{
    using Database.Derivations;
    using Derivations.Default;
    using Domain;
    using Microsoft.AspNetCore.Http;
    using Microsoft.Extensions.Configuration;

    public class DefaultDatabaseServices : DatabaseServices
    {
        public DefaultDatabaseServices(Engine engine, IConfiguration configuration, IHttpContextAccessor httpContextAccessor = null) : base(engine, httpContextAccessor) { }

        protected override IPasswordHasher CreatePasswordHasher() => new PasswordHasher();

        protected override IDerivationService CreateDerivationFactory() => new DerivationService(this.Engine);
    }
}