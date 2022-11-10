// <copyright file="CustomPurchaseOrderApprovalLevel1ParticipantsDerivation.cs" company="Allors bvba">
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
    public class AviationPurchaseOrderApprovalLevel1ParticipantsRule : Rule
    {
        public AviationPurchaseOrderApprovalLevel1ParticipantsRule(MetaPopulation m) : base(m, new Guid("bd4ccf2f-dd67-47ab-8e7a-9fd3d46ce9a7")) =>
            this.Patterns = new Pattern[]
            {
                m.PurchaseOrderApprovalLevel1.RolePattern(v => v.DateClosed),
                m.PurchaseOrderApprovalLevel1.RolePattern(v => v.PurchaseOrder),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<PurchaseOrderApprovalLevel1>())
            {
                var participants = @this.ExistDateClosed
                                       ? (IEnumerable<Person>)Array.Empty<Person>()
                                       : @this.PurchaseOrder.PurchaseOrderState.IsAwaitingApprovalLevel1 ? @this.PurchaseOrder.OrderedBy.PurchaseOrderApproversLevel1 : @this.PurchaseOrder.OrderedBy.PurchaseOrderApproversLevel2;
                @this.AssignParticipants(participants);
            }
        }
    }
}
