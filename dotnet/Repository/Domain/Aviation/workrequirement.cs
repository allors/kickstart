// <copyright file="Requirement.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;
    public partial class WorkRequirement
    {
        #region Allors
        [Id("c90d60b6-160d-431b-962d-c6449e6177bd")]
        #endregion
        [Indexed]
        [Derived]
        [Workspace(Default)]
        public string FleetCode { get; set; }

        #region Allors
        [Id("518a5673-49c6-4b21-b46d-176141bfd7c4")]
        #endregion
        [Derived]
        [Workspace(Default)]
        public bool IsActive{ get; set; }
    }
}
