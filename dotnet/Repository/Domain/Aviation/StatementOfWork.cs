// <copyright file="WorkEffortFixedAssetAssignment.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;

    public partial class StatementOfWork
    {
        #region Allors
        [Id("6036d928-7a19-467e-b9f7-b90b78e3b13d")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Required]
        [Workspace(Default)]
        public SerialisedItem SerialisedItem { get; set; }
    }
}
