// <copyright file="NonUnifiedPartBrandDerivation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;
    using Meta;
    using Database.Derivations;
    using Resources;
    using Allors.Database.Domain.Derivations.Rules;

    public class NonUnifiedPartBrandRule : Rule
    {
        public NonUnifiedPartBrandRule(MetaPopulation m) : base(m, new Guid("e4df67cf-57bb-4655-8762-827bdb6130e7")) =>
            this.Patterns = new[]
            {
                m.NonUnifiedPart.RolePattern(v => v.Brand),
                m.NonUnifiedPart.RolePattern(v => v.Model),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<NonUnifiedPart>())
            {
                var doubles = @this.Brand?.PartsWhereBrand.Where(v => v.ExistModel && v.Model.Equals(@this.Model)).ToArray();
                if (doubles?.Length > 1)
                {
                    validation.AddError($"{@this}, {this.M.NonUnifiedPart.Brand}, {ErrorMessages.ProductExists}");
                }
            }
        }
    }
}
