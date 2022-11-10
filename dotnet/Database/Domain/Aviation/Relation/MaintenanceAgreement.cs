// <copyright file="MaintenanceAgreement.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class MaintenanceAgreement
    {
        public bool IsDeletable =>
           !this.ExistWorkTasksWhereMaintenanceAgreement;
    }
}
