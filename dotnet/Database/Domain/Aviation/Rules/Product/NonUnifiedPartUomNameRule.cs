// <copyright file="NonUnifiedPartUomNameDerivation.cs" company="Allors bvba">
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
    using Allors.Database.Domain.Derivations.Rules;

    public class NonUnifiedPartUomNameRule : Rule
    {
        public NonUnifiedPartUomNameRule(MetaPopulation m) : base(m, new Guid("109335b1-e6ad-4965-a01e-a2070f00e89a")) =>
            this.Patterns = new Pattern[]
            {
                m.NonUnifiedPart.RolePattern(v => v.UnitOfMeasure),
                m.UnitOfMeasure.RolePattern(v => v.Name, v => v.UnifiedProductsWhereUnitOfMeasure, m.NonUnifiedPart),
                m.UnitOfMeasure.RolePattern(v => v.LocalisedNames, v => v.UnifiedProductsWhereUnitOfMeasure, m.NonUnifiedPart),
                m.LocalisedText.RolePattern(v => v.Text, v => v.EnumerationWhereLocalisedName.Enumeration.AsUnitOfMeasure.UnifiedProductsWhereUnitOfMeasure, m.NonUnifiedPart),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var dutchLocale = new Locales(transaction).DutchNetherlands;
            var spanishLocale = new Locales(transaction).Spanish;

            foreach (var @this in matches.Cast<NonUnifiedPart>())
            {
                var spanishUOM = @this.UnitOfMeasure.LocalisedNames.FirstOrDefault(v => v.Locale.Equals(spanishLocale))?.Text;
                var dutchUOM = @this.UnitOfMeasure.LocalisedNames.FirstOrDefault(v => v.Locale.Equals(dutchLocale))?.Text;

                @this.SpanishUOM = spanishUOM ?? @this.UnitOfMeasure.Name;
                @this.DutchUOM = dutchUOM ?? @this.UnitOfMeasure.Name;
            }
        }
    }
}
