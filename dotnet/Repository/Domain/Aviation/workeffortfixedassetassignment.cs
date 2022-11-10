// <copyright file="WorkEffortFixedAssetAssignment.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;

    public partial class WorkEffortFixedAssetAssignment
    {
        #region Allors
        [Id("eb31a2fe-cfbc-4919-9b9c-f7987f0604a6")]
        #endregion
        [Workspace(Default)]
        public decimal OperatingHours { get; set; }
    }
}
