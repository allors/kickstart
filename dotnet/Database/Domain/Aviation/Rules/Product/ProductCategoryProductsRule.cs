// <copyright file="ProductCategoryProductsDerivation.cs" company="Allors bvba">
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
    public class ProductCategoryProductsRule : Rule
    {
        public ProductCategoryProductsRule(MetaPopulation m) : base(m, new Guid("f6fc51f8-7da3-4a25-823a-25a516e3c269")) =>
            this.Patterns = new Pattern[]
            {
                m.ProductCategory.RolePattern(v => v.Products),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<ProductCategory>())
            {
                foreach (Good product in @this.Products)
                {
                    if (product.ProductCategoriesWhereProduct?.Count() > 1)
                    {
                        validation.AddError($"{@this}, {this.M.ProductCategory.Products}, {ErrorMessages.ProductInMultipleCategories}");
                    }
                }
            }
        }
    }
}
