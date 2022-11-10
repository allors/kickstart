// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SaleKind.cs" company="Allors bvba">
//   Copyright 2002-2012 Allors bvba.
// Dual Licensed under
//   a) the General Public Licence v3 (GPL)
//   b) the Allors License
// The GPL License is included in the file gpl.txt.
// The Allors License is an addendum to your contract.
// Allors Applications is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// For more information visit http://www.allors.com/legal
// </copyright>
// --------------------------------------------------------------------------------------------------------------------
namespace Allors.Database.Domain
{
    using System;

    public partial class SaleKinds
    {
        public static readonly Guid SaleId = new Guid("041c683d-10f2-41d6-b292-e3a64f470b29");
        public static readonly Guid RentalId = new Guid("816819c5-a16f-472e-bfc5-c6b99502a8ae");

        public SaleKind Sale => this.Cache[SaleId];

        public SaleKind Rental => this.Cache[RentalId];

        private UniquelyIdentifiableCache<SaleKind> cache;
       
        private UniquelyIdentifiableCache<SaleKind> Cache => this.cache ??= new UniquelyIdentifiableCache<SaleKind>(this.Transaction);

        protected override void AviationSetup(Setup setup)
        {
            var dutchLocale = new Locales(this.Transaction).DutchNetherlands;
            var merge = this.Cache.Merger().Action();
            var localisedName = new LocalisedTextAccessor(this.Meta.LocalisedNames);

            merge(SaleId, v =>
            {
                v.Name = "Sale";
                localisedName.Set(v, dutchLocale, "Verkoop");
                v.IsActive = true;
            });

            merge(RentalId, v =>
            {
                v.Name = "Rental";
                localisedName.Set(v, dutchLocale, "Verhuur");
                v.IsActive = true;
            });
        }
    }
}
