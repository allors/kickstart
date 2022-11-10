// <copyright file="WorkEffortTypeDeniedPermissionDerivation.cs" company="Allors bvba">
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
    public class WorkEffortTypeProductCategoryDisplayNameRule : Rule
    {
        public WorkEffortTypeProductCategoryDisplayNameRule(MetaPopulation m) : base(m, new Guid("67ddfcc8-c4b5-4c46-8047-8b9a7f84ee3f")) =>
            this.Patterns = new Pattern[]
            {
                m.WorkEffortType.RolePattern(v => v.ProductCategory),
                m.ProductCategory.RolePattern(v => v.DisplayName, v => v.WorkEffortTypesWhereProductCategory),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<WorkEffortType>())
            {
                @this.DeriveWorkEffortTypeProductCategoryDisplayName(validation);
            }
        }
    }

    public static class WorkEffortTypeProductCategoryDisplayNameRuleExtensions
    {
        public static void DeriveWorkEffortTypeProductCategoryDisplayName(this WorkEffortType @this, IValidation validation)
        {
            @this.ProductCategoryDisplayName = @this.ProductCategory?.DisplayName;
        }
    }
}
