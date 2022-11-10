// <copyright file="MaintenanceAgreementDeniedPermissionDerivation.cs" company="Allors bvba">
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
    public class MaintenanceAgreementDeniedPermissionRule : Rule
    {
        public MaintenanceAgreementDeniedPermissionRule(MetaPopulation m) : base(m, new Guid("d7d36e76-4827-4f21-aca2-d7baf6fbdc4c")) =>
            this.Patterns = new Pattern[]
            {
                m.MaintenanceAgreement.AssociationPattern(v => v.WorkTasksWhereMaintenanceAgreement),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<MaintenanceAgreement>())
            {
                var revocation = new Revocations(@this.Strategy.Transaction).MaintenanceAgreementDeleteRevocation;
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
