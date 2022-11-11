// <copyright file="Displayable.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using System;

namespace Allors.Repository
{
    using Attributes;

    #region Allors
    [Id("383E7A1C-A382-4D6C-AEEC-6638BCDF7E2C")]
    #endregion
    public partial class DummyPeriod : Period
    {
        #region inherited
        public SecurityToken[] SecurityTokens { get; set; }
        public Revocation[] Revocations { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ThroughDate { get; set; }

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit() { }

        public void OnPostDerive() { }
        #endregion
    }
}