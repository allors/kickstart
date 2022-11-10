// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ObjectsBase.v.cs" company="Allors bvba">
//   Copyright 2002-2013 Allors bvba.
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
    public abstract partial class ObjectsBase<T> where T : IObject
    {
        public void Prepare(Setup setup)
        {
            this.CorePrepare(setup);
            this.BasePrepare(setup);
            this.CustomPrepare(setup);
        }

        public void Setup(Setup setup)
        {
            this.CoreSetup(setup);
            this.BaseSetup(setup);
            this.CustomSetup(setup);

            this.Transaction.Derive(true);
        }

        public void Prepare(Security security)
        {
            this.CorePrepare(security);
            this.BasePrepare(security);
            this.CustomPrepare(security);
        }

        public void Secure(Security security)
        {
            this.CoreSecure(security);
            this.BaseSecure(security);
            this.CustomSecure(security);
        }
    }
}
