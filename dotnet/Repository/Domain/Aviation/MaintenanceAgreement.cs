// <copyright file="MaintenanceAgreement.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using System;
    using Allors.Repository.Attributes;
    using static Workspaces;

    #region Allors
    [Id("d394c3c4-09f0-491d-8ef6-410af3f3318b")]
    #endregion
    [Workspace(Default)]
    public partial class MaintenanceAgreement : Agreement, IDisplayName
    {
        #region inherited properties
        public DateTime AgreementDate { get; set; }

        public Addendum[] Addenda { get; set; }

        public string Description { get; set; }

        public AgreementTerm[] AgreementTerms { get; set; }

        public string Text { get; set; }

        public AgreementItem[] AgreementItems { get; set; }

        public string AgreementNumber { get; set; }

        public SecurityToken[] SecurityTokens { get; set; }

        public Guid UniqueId { get; set; }

        public DateTime FromDate { get; set; }

        public DateTime ThroughDate { get; set; }

        public Revocation[] Revocations { get; set; }

        public string DisplayName { get; set; }

        #endregion

        #region Allors
        [Id("803225bc-1d4b-4a83-a7f6-e04908b5b06c")]


        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Required]
        [Indexed]
        [Workspace(Default)]
        public WorkEffortType WorkEffortType { get; set; }

        #region Allors
        [Id("ca123bf5-df72-4ae3-ba6e-abcf9f82c0dc")]


        #endregion
        [Precision(19)]
        [Scale(2)]
        [Required]
        [Workspace(Default)]
        public decimal FixedPrice { get; set; }

        #region Allors
        [Id("bacbd2b6-988c-44c4-b6b7-fd75c54c5b93")]


        #endregion
        [Required]
        [Workspace(Default)]
        public decimal HourlyRate { get; set; }

        #region Allors
        [Id("2d7017fe-d8f9-463b-987a-e715ffb6d20f")]


        #endregion
        [Required]
        [Workspace(Default)]
        public decimal PartSurchargePercentage { get; set; }

        #region Allors
        [Id("c7c2609f-e3df-49b5-8bf9-92e0c3d5b4e3")]


        #endregion
        [Derived]
        [Origin(Origin.Session)]
        [Workspace(Default)]
        public string WorkEffortTypeDisplayName { get; set; }

        #region Allors
        [Id("f76c31cb-aec1-4dd5-9132-0a0b45ad2e0a")]


        #endregion
        [Derived]
        [Workspace(Default)]
        public string CustomerName { get; set; }

        #region Allors
        [Id("7a346fcd-513b-47b8-a83e-36ed160bb880")]


        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Required]
        [Workspace(Default)]
        public InternalOrganisation InternalOrganisation { get; set; }

        #region inherited methods

        public void OnBuild() { }

        public void OnPostBuild() { }

        public void OnInit()
        {
        }

        public void OnPostDerive() { }

        public void Delete() { }

        #endregion
    }
}

