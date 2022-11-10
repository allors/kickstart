// <copyright file="CustomProductQuoteApprovalParticipantsDerivation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;
    using Database.Derivations;
    using Allors.Database.Domain.Derivations.Rules;
    public class AviationProductQuoteApprovalParticipantsRule : Rule
    {
        public AviationProductQuoteApprovalParticipantsRule(MetaPopulation m) : base(m, new Guid("409f1258-d263-41fc-a7e9-69a6ef660745")) =>
            this.Patterns = new Pattern[]
            {
                m.ProductQuoteApproval.RolePattern(v => v.DateClosed),
                m.ProductQuoteApproval.RolePattern(v => v.ProductQuote),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<ProductQuoteApproval>())
            {
                var participants = @this.ExistDateClosed
                                       ? (IEnumerable<Person>)Array.Empty<Person>()
                                       : @this.ProductQuote.Issuer.ProductQuoteApprovers;
                @this.AssignParticipants(participants);
            }
        }
    }
}
