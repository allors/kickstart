// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Locales.cs" company="Allors bvba">
//   Copyright 2002-2016 Allors bvba.
//
// Dual Licensed under
//   a) the General Public Licence v3 (GPL)
//   b) the Allors License
//
// The GPL License is included in the file gpl.txt.
// The Allors License is an addendum to your contract.
//
// Allors Applications is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// For more information visit http://www.allors.com/legal
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

namespace Allors.Database.Domain
{
    public partial class Countries
    {
        protected override void AviationSetup(Setup setup)
        {
            // TODO: Add to countries

            var undefined = new Countries(this.Transaction).FindBy(M.Country.IsoCode, "__");
            if (undefined == null)
            {
                new CountryBuilder(this.Transaction).WithName("Undefined").WithIsoCode("__").Build();
            }

            var xk = new Countries(this.Transaction).FindBy(M.Country.IsoCode, "XK");
            if (xk == null)
            {
                new CountryBuilder(this.Transaction).WithName("Kosovo").WithIsoCode("XK").Build();
            }
        }
    }
}