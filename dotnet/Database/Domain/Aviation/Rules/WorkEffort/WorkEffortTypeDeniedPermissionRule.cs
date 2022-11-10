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
    public class WorkEffortTypeDeniedPermissionRule : Rule
    {
        public WorkEffortTypeDeniedPermissionRule(MetaPopulation m) : base(m, new Guid("9b3da736-77a0-4a73-bafe-2b666eba9be7")) =>
            this.Patterns = new Pattern[]
            {
                m.WorkEffortType.AssociationPattern(v => v.MaintenanceAgreementsWhereWorkEffortType),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<WorkEffortType>())
            {
                var revocation = new Revocations(@this.Strategy.Transaction).WorkEffortTypeDeleteRevocation;
                if (@this.IsDeletable)
                {
                    @this.RemoveRevocation(revocation);
                }
                else
                {
                    @this.AddRevocation(revocation);
                }
            }
        }
    }
}
