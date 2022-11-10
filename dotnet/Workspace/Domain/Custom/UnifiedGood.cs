// <copyright file="PurchaseInvoiceStates.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Workspace.Domain
{
    using System.Linq;

    public partial class UnifiedGood
    {
        public string LocalisedName => this.LocalisedNames.FirstOrDefault(v => v.Locale?.MatchCurrentLanguage == true)?.Text ?? this.Name;
    }
}
