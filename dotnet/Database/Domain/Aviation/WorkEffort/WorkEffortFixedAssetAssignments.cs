// <copyright file="WorkEffortFixedAssetAssignments.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System.Linq;

    public partial class WorkEffortFixedAssetAssignments
    {
        protected override void AviationSecure(Security config)
        {
            var revocations = new Revocations(this.Transaction);
            var permissions = new Permissions(this.Transaction);

            revocations.WorkEffortFixedAssetAssignmentDeleteRevocation.DeniedPermissions = new[]
            {
                permissions.Get(this.Meta, this.Meta.Delete),
            };

            revocations.WorkEffortFixedAssetAssignmentWriteFixedAssetRevocation.DeniedPermissions = new[]
            {
               permissions.Get(this.Meta, this.Meta.FixedAsset, Operations.Write)
            };
        }
    }
}
