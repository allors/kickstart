// <copyright file="Settings.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;

    public partial class Settings
    {
        #region Allors
        [Id("933d558b-d592-4dc4-9595-37b2c88cd024")]
        #endregion
        [Required]
        [Workspace(Default)]
        public string CleaningCalculation { get; set; }

        #region Allors
        [Id("1beb80a3-b556-4774-be55-bb92a74e9968")]
        #endregion
        [Required]
        [Workspace(Default)]
        public string SundriesCalculation { get; set; }
    }
}
