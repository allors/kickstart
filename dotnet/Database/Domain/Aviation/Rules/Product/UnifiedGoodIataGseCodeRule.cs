// <copyright file="UnifiedGoodIataGseCodeDerivation.cs" company="Allors bvba">
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
    public class UnifiedGoodIataGseCodeRule : Rule
    {
        public UnifiedGoodIataGseCodeRule(MetaPopulation m) : base(m, new Guid("10ea30c3-2cb7-4886-8e6f-a4a4a8921be3")) =>
            this.Patterns = new Pattern[]
            {
                m.ProductCategory.RolePattern(v => v.IataGseCode, v => v.Products.UnifiedProduct.AsUnifiedGood),
                m.Product.AssociationPattern(v => v.ProductCategoriesWhereProduct, M.UnifiedGood),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<UnifiedGood>())
            {
                // constraint in place limiting product to 1 category
                @this.IataGseCode = @this.ProductCategoriesWhereProduct.FirstOrDefault()?.IataGseCode;
            }
        }
    }
}
