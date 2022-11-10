// --------------------------------------------------------------------------------------------------------------------
// <copyright file="SalesInvoices.cs" company="Allors bvba">
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

using Allors.Database.Meta;
using System.Collections.Generic;

namespace Allors.Database.Domain
{
    public partial class SalesInvoices
    {
        protected override void AviationSecure(Security config)
        {
            var notPaid = new SalesInvoiceStates(this.Transaction).NotPaid;
            var paid = new SalesInvoiceStates(this.Transaction).Paid;
            var partiallyPaid = new SalesInvoiceStates(this.Transaction).PartiallyPaid;
            var writtenOff = new SalesInvoiceStates(this.Transaction).WrittenOff;
            var cancelled = new SalesInvoiceStates(this.Transaction).Cancelled;

            var grants = new HashSet<IOperandType>
            {
                this.Meta.PrintCondensed,
            };

            config.Grant(this.ObjectType, notPaid, grants, Operations.Write);
            config.Grant(this.ObjectType, partiallyPaid, grants, Operations.Write);
            config.Grant(this.ObjectType, paid, grants, Operations.Write);
            config.Grant(this.ObjectType, writtenOff, grants, Operations.Write);
            config.Grant(this.ObjectType, cancelled, grants, Operations.Write);
        }
    }
}