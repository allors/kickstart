// <copyright file="NonUnifiedPartUomNameDerivation.cs" company="Allors bvba">
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
    public class PartLocalisedNameRule : Rule
    {
        public PartLocalisedNameRule(MetaPopulation m) : base(m, new Guid("570b6a88-2262-463f-9704-05cc3411b2b2")) =>
            this.Patterns = new Pattern[]
            {
                m.NonUnifiedPart.RolePattern(v => v.UnitOfMeasure),
                m.LocalisedText.RolePattern(v => v.Text, v => v.UnifiedProductWhereLocalisedName, m.NonUnifiedPart),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<NonUnifiedPart>())
            {
                @this.DerivePartLocalisedName(validation);
            }
        }
    }

    public static class PartLocalisedNameRuleExtensions
    {
        public static void DerivePartLocalisedName(this NonUnifiedPart @this, IValidation validation)
        {
            var dutchLocale = new Locales(@this.Transaction()).DutchNetherlands;
            var spanishLocale = new Locales(@this.Transaction()).Spanish;

            var spanishName = @this.LocalisedNames.FirstOrDefault(v => v.Locale.Equals(spanishLocale))?.Text;
            var dutchName = @this.LocalisedNames.FirstOrDefault(v => v.Locale.Equals(dutchLocale))?.Text;

            @this.SpanishName = !string.IsNullOrEmpty(spanishName) ? spanishName : @this.Name;
            @this.DutchName = !string.IsNullOrEmpty(dutchName) ? dutchName : @this.Name;
        }
    }
}
