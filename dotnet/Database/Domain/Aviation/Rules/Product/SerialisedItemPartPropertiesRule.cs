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

    public class SerialisedPartPropertiesRule : Rule
    {
        public SerialisedPartPropertiesRule(MetaPopulation m) : base(m, new Guid("9d9cdcc1-5977-4818-9c8b-6d047c9d1646")) =>
            this.Patterns = new Pattern[]
            {
                m.SerialisedItem.AssociationPattern(v => v.PartWhereSerialisedItem),
                m.UnifiedGood.RolePattern(v => v.ReplacementValue, v => v.SerialisedItems.SerialisedItem),
                m.UnifiedGood.RolePattern(v => v.LifeTime, v => v.SerialisedItems.SerialisedItem),
                m.UnifiedGood.RolePattern(v => v.DepreciationYears, v => v.SerialisedItems.SerialisedItem),
                m.UnifiedGood.RolePattern(v => v.HsCode, v => v.SerialisedItems.SerialisedItem),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SerialisedItem>())
            {
                @this.DeriveSerialisedPartProperties(validation);
            }
        }
    }

    public static class SerialisedPartPropertiesRuleExtensions
    {
        public static void DeriveSerialisedPartProperties(this SerialisedItem @this, IValidation validation)
        {
            @this.ReplacementValue = ((UnifiedGood)@this.PartWhereSerialisedItem).ReplacementValue;
            @this.LifeTime = ((UnifiedGood)@this.PartWhereSerialisedItem).LifeTime;
            @this.DepreciationYears = ((UnifiedGood)@this.PartWhereSerialisedItem).DepreciationYears;
            @this.HsCode = ((UnifiedGood)@this.PartWhereSerialisedItem).HsCode;
        }
    }
}
