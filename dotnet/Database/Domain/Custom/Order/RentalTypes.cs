// --------------------------------------------------------------------------------------------------------------------
// <copyright file="RentalTypes.cs" company="Allors bvba">
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

    public partial class RentalTypes
    {
        public static readonly Guid ShortTermId = new Guid("22e53e2e-6e5e-4a43-af92-70444cea8c72");
        public static readonly Guid LongTermId = new Guid("1d8bf67d-0a59-4a76-812a-720de42f8f4b");

        public RentalType ShortTerm => this.Cache[ShortTermId];

        public RentalType LongTerm => this.Cache[LongTermId];

        private UniquelyIdentifiableCache<RentalType> cache;
       
        private UniquelyIdentifiableCache<RentalType> Cache => this.cache ??= new UniquelyIdentifiableCache<RentalType>(this.Transaction);

        protected override void CustomSetup(Setup setup)
        {
            var dutchLocale = new Locales(this.Transaction).DutchNetherlands;
            var merge = this.Cache.Merger().Action();
            var localisedName = new LocalisedTextAccessor(this.Meta.LocalisedNames);

            merge(ShortTermId, v =>
            {
                v.Name = "Short term";
                localisedName.Set(v, dutchLocale, "Korte termijn");
                v.IsActive = true;
            });

            merge(LongTermId, v =>
            {
                v.Name = "Long term";
                localisedName.Set(v, dutchLocale, "Lange termijn");
                v.IsActive = true;
            });
        }
    }
}
