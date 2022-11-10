// <copyright file="VehicleDeniedPermissionDerivation.cs" company="Allors bvba">
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
    public class VehicleDeniedPermissionRule : Rule
    {
        public VehicleDeniedPermissionRule(MetaPopulation m) : base(m, new Guid("2ab21b01-7eb8-49e7-8805-2de3c4918892")) =>
            this.Patterns = new Pattern[]
            {
                m.Vehicle.AssociationPattern(v => v.WorkEffortFixedAssetAssignmentsWhereFixedAsset),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<Vehicle>())
            {
                var revocation = new Revocations(@this.Strategy.Transaction).VehicleDeleteRevocation;
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
