// <copyright file="QuoteItemPrintDocumentDerivation.cs" company="Allors bvba">
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
    public class QuoteItemPrintDocumentRule : Rule
    {
        public QuoteItemPrintDocumentRule(MetaPopulation m) : base(m, new Guid("57267858-d2af-4f32-ad0a-1e654b7bed29")) =>
            this.Patterns = new Pattern[]
            {
                m.SerialisedItem.RolePattern(v => v.PrimaryPhoto, v => v.QuoteItemsWhereSerialisedItem),
                m.SerialisedItem.RolePattern(v => v.SecondaryPhotos, v => v.QuoteItemsWhereSerialisedItem),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<QuoteItem>())
            {
                @this.QuoteWhereQuoteItem.RemovePrintDocument();
            }
        }
    }
}
