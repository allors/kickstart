// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SecurityToken.cs" company="Allors bvba">
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
    using System.Linq;

    public partial class SecurityToken
    {
        public string DebuggerDisplay
        {
            get
            {
                if (this.UniqueId == Domain.SecurityTokens.InitialSecurityTokenId)
                {
                    return "Initial";
                }

                if (this.UniqueId == Domain.SecurityTokens.DefaultSecurityTokenId)
                {
                    return "Default";
                }

                if (this.UniqueId == Domain.SecurityTokens.AdministratorSecurityTokenId)
                {
                    return "Administrator";
                }

                var toString = string.Join(",", this.Grants.ToArray().Select(v => v.ToString()));
                return $"{toString} [{this.strategy.ObjectId}]";
            }
        }
    }
}