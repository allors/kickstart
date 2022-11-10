// <copyright file="CustomWorkTaskDerivation.cs" company="Allors bvba">
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
    public class AviationWorkTaskRule : Rule
    {
        public AviationWorkTaskRule(MetaPopulation m) : base(m, new Guid("89be28a0-cbb9-40ef-84cb-94a624e9558c")) =>
            this.Patterns = new Pattern[]
            {
                m.WorkTask.RolePattern(v => v.Customer),
                m.WorkTask.RolePattern(v => v.WorkEffortState),
                m.WorkEffortFixedAssetAssignment.RolePattern(v => v.FixedAsset, v => v.Assignment, m.WorkTask),
                m.SerialisedItem.RolePattern(v => v.OwnedBy, v => v.WorkEffortFixedAssetAssignmentsWhereFixedAsset.WorkEffortFixedAssetAssignment.Assignment, m.WorkTask),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            foreach (var @this in matches.Cast<WorkTask>())
            {
                var asset = @this.WorkEffortFixedAssetAssignmentsWhereAssignment.FirstOrDefault()?.FixedAsset as SerialisedItem;
                var customer = @this.Customer as Organisation;

                // when workorder is not in initial state then do not change customer
                // It can be the case that unit is sold while refurbishment is still in progress. 
                // This would chnage the ownership and then the Customer was automatically changed in the WO
                if (asset != null
                    && customer?.IsInternalOrganisation == true
                    && @this.WorkEffortState.IsCreated
                    && (asset.OwnedBy as InternalOrganisation)?.IsInternalOrganisation == true)
                {
                    @this.Customer = asset.OwnedBy;
                }
            }
        }
    }
}
