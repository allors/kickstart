// <copyright file="CustomPurchaseOrderApprovalLevel2ParticipantsDerivation.cs" company="Allors bvba">
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

    public class AviationPurchaseOrderApprovalLevel2ParticipantsRule : Rule
    {
        public AviationPurchaseOrderApprovalLevel2ParticipantsRule(MetaPopulation m) : base(m, new Guid("94038f6a-e550-4d6f-a0a9-eed9a5ae515b")) =>
            this.Patterns = new Pattern[]
            {
                m.PurchaseOrderApprovalLevel2.RolePattern(v => v.DateClosed),
                m.PurchaseOrderApprovalLevel2.RolePattern(v => v.PurchaseOrder),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<PurchaseOrderApprovalLevel2>())
            {
                var participants = @this.ExistDateClosed
                                       ? (IEnumerable<Person>)Array.Empty<Person>()
                                       : @this.PurchaseOrder.PurchaseOrderState.IsAwaitingApprovalLevel2 ? @this.PurchaseOrder.OrderedBy.PurchaseOrderApproversLevel2 : @this.PurchaseOrder.OrderedBy.PurchaseOrderApproversLevel2;
                @this.AssignParticipants(participants);
            }
        }
    }
}
