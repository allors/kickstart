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

    public class FacilitySupplierOfferingRule : Rule
    {
        public FacilitySupplierOfferingRule(MetaPopulation m) : base(m, new Guid("ddf4f1e8-ca21-459b-8cfd-a2e12dc59875")) =>
            this.Patterns = new Pattern[]
            {
                m.Facility.RolePattern(v => v.FacilityType),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<Facility>())
            {
                @this.DeriveFacilitySupplierOffering(validation);
            }
        }
    }

    public static class FacilitySupplierOfferingRuleExtensions
    {
        public static void DeriveFacilitySupplierOffering(this Facility @this, IValidation validation)
        {
            if (@this.FacilityType.IsWorkshop)
            {
                foreach (SupplierOffering supplierOffering in @this.Strategy.Transaction.Extent<SupplierOffering>().ToArray())
                {
                    supplierOffering.DeriveSupplierOfferingSecurityToken(validation);
                }
            }
        }
    }
}
