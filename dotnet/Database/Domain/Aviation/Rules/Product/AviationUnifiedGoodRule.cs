// <copyright file="UnifiedGoodCustomDerivation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using Allors.Database.Meta;
using Allors.Database.Derivations;
using Resources;
using Allors.Database.Domain.Derivations.Rules;

namespace Allors.Database.Domain
{
    public class AviationUnifiedGoodRule : Rule
    {
        public AviationUnifiedGoodRule(MetaPopulation m) : base(m, new Guid("d2c49ce7-dff3-4e85-8114-fcbd7d7bacb4")) =>
            this.Patterns = new Pattern[]
            {
                m.UnifiedGood.RolePattern(v => v.Brand),
                m.UnifiedGood.RolePattern(v => v.Model),
                m.Model.RolePattern(v => v.Name, v => v.PartsWhereModel.Part, M.UnifiedGood),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<UnifiedGood>())
            {
                @this.Name = @this.Brand?.Name + " " + @this.Model?.Name;

                var doubles = @this.Brand?.PartsWhereBrand.Where(v => v.ExistModel && v.Model.Equals(@this.Model)).ToArray();
                if (doubles?.Length > 1)
                {
                    validation.AddError($"{@this}, {this.M.UnifiedGood.Brand}, {ErrorMessages.ProductExists}");
                }
            }
        }
    }
}
