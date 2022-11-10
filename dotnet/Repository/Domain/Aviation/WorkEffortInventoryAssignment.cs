// <copyright file="WorkEffortInventoryAssignment.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;
    using Allors.Repository.Attributes;
    using static Workspaces;

    [Workspace(Default)]
    public partial class WorkEffortInventoryAssignment
    {
        #region Allors
        [Id("7A7D176E-A18D-44BC-8E05-9FA592A3A911")]


        #endregion
        [Derived]
        [Required]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        public decimal UnitPurchasePrice { get; set; }

        /// <summary>
        /// Gets or sets the Quantity of the Part for this WorkEffortInventoryAssignment.
        /// </summary>
        #region Allors
        [Id("070e786b-8605-4d98-949d-fa736c190257")]


        #endregion
        [Derived]
        [Workspace(Default)]
        public decimal WorkEffortStandardQuantity { get; set; }


        /// <summary>
        /// Gets or sets the Quantity of the Part for this WorkEffortInventoryAssignment.
        /// </summary>
        #region Allors
        [Id("be0ec050-669e-411e-8a62-0ac7745b6329")]


        #endregion
        [Derived]
        [Workspace(Default)]
        public decimal ExtraQuantity { get; set; }
    }
}
