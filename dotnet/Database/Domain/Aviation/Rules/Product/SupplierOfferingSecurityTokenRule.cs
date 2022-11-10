// <copyright file="OrganisationCalculationRule.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using Allors.Database.Meta;
using Allors.Database.Derivations;
using Allors.Database.Domain.Derivations.Rules;

namespace Allors.Database.Domain
{
    public class SupplierOfferingSecurityTokenRule : Rule
    {
        public SupplierOfferingSecurityTokenRule(MetaPopulation m) : base(m, new Guid("59b5b564-35d9-4a03-830e-4d641f6cedfd")) =>
            this.Patterns = new Pattern[]
            {
                m.SupplierOffering.RolePattern(v => v.Supplier),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SupplierOffering>())
            {
                @this.DeriveSupplierOfferingSecurityToken(validation);
            }
        }
    }

    public static class SupplierOfferingSecurityTokenRuleExtensions
    {
        public static void DeriveSupplierOfferingSecurityToken(this SupplierOffering @this, IValidation validation)
        {
            @this.SecurityTokens = new[]
            {
                new SecurityTokens(@this.Strategy.Transaction).DefaultSecurityToken,
            };

            foreach (InternalOrganisation internalOrganisation in @this.Supplier.SupplierRelationshipsWhereSupplier.Select(v => v.InternalOrganisation))
            {
                @this.AddSecurityToken(internalOrganisation.LocalAdministratorSecurityToken);
                @this.AddSecurityToken(internalOrganisation.LocalEmployeeSecurityToken);
            }

            if (@this.ExistPart)
            {
                var internalOrganisation = @this.Part.DefaultFacility?.Owner;

                @this.AddSecurityToken(internalOrganisation.LocalEmployeeSecurityToken);
                @this.AddSecurityToken(internalOrganisation.LocalAdministratorSecurityToken);
            }
        }
    }
}
