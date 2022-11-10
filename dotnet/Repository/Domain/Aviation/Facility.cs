// <copyright file="Facility.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;

    using Attributes;
    using static Workspaces;

    public partial class Facility
    {
        #region Allors
        [Id("5bb6b9ca-333f-4320-8d00-5e04637c050c")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public Facility WorkshopWarehouse{ get; set; }

        #region Allors
        [Id("6701606d-a3fe-4b7b-93d5-03e50b4b8930")]
        #endregion
        [Workspace(Default)]
        public string WorkshopWarehouseName { get; set; }
    }
}
