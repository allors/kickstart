// <copyright file="Revocations.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>
// <summary>Defines the role type.</summary>

namespace Allors.Database.Domain
{
    using System;

    public partial class Revocations
    {
        public static readonly Guid MaintenanceAgreementDeleteRevocationId = new Guid("ce429933-111d-40fc-8a80-07089a96cee5");
        public static readonly Guid WorkEffortFixedAssetAssignmentDeleteRevocationId = new Guid("c9dc980f-da1e-4f69-a3a3-7a5faeabcb93");
        public static readonly Guid WorkEffortFixedAssetAssignmentWriteFixedAssetRevocationId = new Guid("cd20eda9-7520-4ae7-95f6-1d00f01fed58");
        public static readonly Guid WorkEffortTypeDeleteRevocationId = new Guid("951f1f64-b02b-446f-a7ad-c71186c1a9f4");
        public static readonly Guid VehicleDeleteRevocationId = new Guid("70e74b37-9640-4447-b17a-6c87c1ab0d43");

        public Revocation MaintenanceAgreementDeleteRevocation => this.Cache[MaintenanceAgreementDeleteRevocationId];

        public Revocation WorkEffortFixedAssetAssignmentDeleteRevocation => this.Cache[WorkEffortFixedAssetAssignmentDeleteRevocationId];

        public Revocation WorkEffortFixedAssetAssignmentWriteFixedAssetRevocation => this.Cache[WorkEffortFixedAssetAssignmentWriteFixedAssetRevocationId];

        public Revocation WorkEffortTypeDeleteRevocation => this.Cache[WorkEffortTypeDeleteRevocationId];

        public Revocation VehicleDeleteRevocation => this.Cache[VehicleDeleteRevocationId];

        protected override void AviationSecure(Security security)
        {
            var merge = this.Cache.Merger().Action();

            merge(MaintenanceAgreementDeleteRevocationId, _ => { });
            merge(VehicleDeleteRevocationId, _ => { });
            merge(WorkEffortFixedAssetAssignmentDeleteRevocationId, _ => { });
            merge(WorkEffortFixedAssetAssignmentWriteFixedAssetRevocationId, _ => { });
            merge(WorkEffortTypeDeleteRevocationId, _ => { });
        }
    }
}
