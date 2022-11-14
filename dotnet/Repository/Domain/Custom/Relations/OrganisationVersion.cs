// <copyright file="OrganisationVersion.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;
    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("E1AFA103-7032-416B-AC7B-274A7E35381A")]
    #endregion
    public partial class OrganisationVersion : Version
    {
        #region inherited properties

        public Revocation[] Revocations { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public Guid DerivationId { get; set; }

        public DateTime DerivationTimeStamp { get; set; }

        #endregion

        #region Allors
        [Id("089768C8-5084-4917-8B21-3B185B9FADE6")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public User CreatedBy { get; set; }

        #region Allors
        [Id("2BAD0A81-70F0-44DF-8539-280A34206DF6")]
        #endregion
        [Workspace(Default)]
        public DateTime CreationDate { get; set; }

        #region Allors
        [Id("22CB9D7A-487F-4E8E-9FFC-3A2F3B1AE2C5")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public Currency PreferredCurrency { get; set; }

        #region Allors
        [Id("B5552663-3D07-4EEA-BA8A-DEB40D264D48")]
        #endregion
        [Indexed]
        [Size(256)]
        [Workspace(Default)]
        public string Name { get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit() { }

        public void OnPostDerive() { }

        #endregion
    }
}
