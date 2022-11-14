// <copyright file="Organisation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;
    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("3a5dcec7-308f-48c7-afee-35d38415aa0b")]
    #endregion
    public partial class Organisation : Party, Deletable, Versioned
    {
        #region inherited properties
        public SecurityToken[] SecurityTokens { get; set; }
        public Revocation[] Revocations { get; set; }
        public Locale Locale { get; set; }
        public User CreatedBy { get; set; }
        public User LastModifiedBy { get; set; }
        public DateTime CreationDate { get; set; }
        public DateTime LastModifiedDate { get; set; }
        public Guid UniqueId { get; set; }
        public Currency PreferredCurrency { get; set; }
        public string DisplayName { get; set; }
        #endregion

        #region Versioning
        #region Allors
        [Id("275CFF8F-AD72-4237-AEBD-158A72650D25")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        public OrganisationVersion CurrentVersion { get; set; }

        #region Allors
        [Id("9BF20468-BF1D-410D-8D83-EBA561A5F066")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Workspace(Default)]
        public OrganisationVersion[] AllVersions { get; set; }
        #endregion

        #region Allors
        [Id("2cc74901-cda5-4185-bcd8-d51c745a8437")]
        #endregion
        [Indexed]
        [Required]
        [Size(256)]
        [Workspace(Default)]
        public string Name { get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit() { }

        public void OnPostDerive() { }

        public void Delete() { }

        #endregion

    }
}
