// <copyright file="FacilityType.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class FacilityType
    {
        public bool IsWorkshop => this.UniqueId == FacilityTypes.WorkshopId;
        public bool IsWarehouse => this.UniqueId == FacilityTypes.WarehouseId;
        public bool IsStorageLocation => this.UniqueId == FacilityTypes.StorageLocationId;
    }
}
