// --------------------------------------------------------------------------------------------------------------------
// <copyright file="PositionTypes.cs" company="Allors bvba">
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

    public partial class PositionTypes
    {
        private static readonly Guid MechanicId = new Guid("2BFBA311-83F9-4739-B19C-47DE9D1AF043");

        public PositionType Mechanic => this.Cache[MechanicId];

        protected override void AviationSetup(Setup setup)
        {
            new PositionTypeBuilder(this.Transaction)
                .WithTitle("Mechanic")
                .WithUniqueId(MechanicId)
                .Build();
        }
    }
}
