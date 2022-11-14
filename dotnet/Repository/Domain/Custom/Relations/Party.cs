// <copyright file="Party.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;

    #region Allors
    [Id("3bba6e5a-dc2d-4838-b6c4-881f6c8c3013")]
    #endregion
    [Plural("Parties")]
    public partial interface Party : Localised, Auditable, Displayable, UniquelyIdentifiable
    {
        #region Allors
        [Id("f0de022f-b94e-4d29-8cdf-99d39ad9add6")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        Currency PreferredCurrency { get; set; }

    }
}
