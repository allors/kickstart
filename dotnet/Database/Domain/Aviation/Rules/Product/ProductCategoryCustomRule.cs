// <copyright file="ProductCategoryCustomDerivation.cs" company="Allors bvba">
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
    public class ProductCategoryCustomRule : Rule
    {
        public ProductCategoryCustomRule(MetaPopulation m) : base(m, new Guid("fb69a8a5-920f-499f-be51-5725305f3ee3")) =>
            this.Patterns = new Pattern[]
            {
                m.ProductCategory.RolePattern(v => v.UniqueId),
                m.ProductCategory.RolePattern(v => v.IataGseCode),
                m.ProductCategory.RolePattern(v => v.PrimaryParent),
                m.ProductCategory.RolePattern(v => v.Children),
                m.IataGseCode.RolePattern(v => v.Code, v => v.ProductCategoriesWhereIataGseCode),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<ProductCategory>())
            {
                @this.IsFamily = !@this.ExistPrimaryParent;
                @this.IsGroup = @this.ExistPrimaryParent && @this.ExistChildren;
                @this.IsSubGroup = @this.ExistPrimaryParent && !@this.ExistChildren;

                if (@this.IsSubGroup)
                {
                    @this.FamilyName = @this.PrimaryParent?.PrimaryParent?.Name;
                }
                else if (@this.IsGroup)
                {
                    @this.FamilyName = @this.PrimaryParent?.Name;
                }

                if (@this.IsSubGroup)
                {
                    @this.GroupName = @this.PrimaryParent?.Name;
                }

                @this.IataCode = @this.IataGseCode?.Code;
            }
        }
    }
}
