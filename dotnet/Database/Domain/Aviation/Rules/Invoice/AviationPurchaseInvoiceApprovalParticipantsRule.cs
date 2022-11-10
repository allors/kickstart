// <copyright file="CustomPurchaseInvoiceApprovalParticipantsDerivation.cs" company="Allors bvba">
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
    public class AviationPurchaseInvoiceApprovalParticipantsRule : Rule
    {
        public AviationPurchaseInvoiceApprovalParticipantsRule(MetaPopulation m) : base(m, new Guid("b27e663e-39ef-49e6-899d-afdc069e43b9")) =>
            this.Patterns = new Pattern[]
            {
                m.PurchaseInvoiceApproval.RolePattern(v => v.DateClosed),
                m.PurchaseInvoiceApproval.RolePattern(v => v.PurchaseInvoice),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<PurchaseInvoiceApproval>())
            {
                var participants = @this.ExistDateClosed
                                       ? (IEnumerable<Person>)Array.Empty<Person>()
                                       : @this.PurchaseInvoice?.BilledTo?.ExistPurchaseInvoiceApprovers == true ? @this.PurchaseInvoice?.BilledTo?.PurchaseInvoiceApprovers : @this.PurchaseInvoice?.BilledTo.LocalAdministrators;
                @this.AssignParticipants(participants);
            }
        }
    }
}
