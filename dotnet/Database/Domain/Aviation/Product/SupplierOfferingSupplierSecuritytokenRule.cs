// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database.Derivations;
    using Meta;
    using Derivations.Rules;

    public class SupplierOfferingSupplierSecuritytokenRule : Rule
    {
        public SupplierOfferingSupplierSecuritytokenRule(MetaPopulation m) : base(m, new Guid("3a73732a-8e9d-4d3d-8745-f732fc754d1e")) =>
            this.Patterns = new Pattern[]
            {
                m.SupplierOffering.RolePattern(v => v.Supplier),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SupplierOffering>())
            {
                @this.DeriveSupplierOfferingSupplieSecuritytoken(validation);
            }
        }
    }

    public static class SupplierOfferingSupplieSecuritytokenRuleExtensions
    {
        public static void DeriveSupplierOfferingSupplieSecuritytoken(this SupplierOffering @this, IValidation validation)
        {
            if (@this.ExistSupplier && @this.ExistPart)
            {
                var internalOrganisation = @this.Part.DefaultFacility?.Owner;

                @this.Supplier.AddSecurityToken(internalOrganisation?.LocalAdministratorSecurityToken);
                @this.Supplier.AddSecurityToken(internalOrganisation?.LocalEmployeeSecurityToken);
            }
        }
    }
}
