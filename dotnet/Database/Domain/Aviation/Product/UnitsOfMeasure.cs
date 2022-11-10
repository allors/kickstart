// <copyright file="UnitsOfMeasure.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Linq;

namespace Allors.Database.Domain
{
    using System;

    public partial class UnitsOfMeasure
    {
        // Quantity
        protected override void AviationSetup(Setup setup)
        {
            var dutchLocale = new Locales(this.Transaction).DutchNetherlands;

            if (this.Piece != null)
            {
                this.Piece.Name = "each";
                this.Piece.Abbreviation = "EA";
                var dutchName = this.Piece.LocalisedNames.ToArray().First(v => v.Locale.Equals(dutchLocale));
                dutchName.Text = "elk";
            }
        }
    }
}
