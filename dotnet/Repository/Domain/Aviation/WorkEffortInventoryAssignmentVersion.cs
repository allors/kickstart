// <copyright file="WorkEffortInventoryAssignmentVersion.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;
    using Allors.Repository.Attributes;
    using static Workspaces;

    [Workspace(Default)]
    public partial class WorkEffortInventoryAssignmentVersion : Version
    {
        #region Allors
        [Id("f399fe39-97fc-4a52-aecf-eac0b8443c9a")]
        
        
        #endregion
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        public decimal UnitPurchasePrice { get; set; }

        /// <summary>
        /// Gets or sets the Quantity of the Part for this WorkEffortInventoryAssignment.
        /// </summary>
        #region Allors
        [Id("51ab8b1d-a751-4ba5-9dec-1591d947ae4d")]
        
        
        #endregion
        [Workspace(Default)]
        public decimal WorkEffortStandardQuantity { get; set; }


        /// <summary>
        /// Gets or sets the Quantity of the Part for this WorkEffortInventoryAssignment.
        /// </summary>
        #region Allors
        [Id("ebd71610-20cc-4958-95a6-06bac887fe01")]
        
        
        #endregion
        [Workspace(Default)]
        public decimal ExtraQuantity { get; set; }
    }
}
