// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Model.cs" company="Allors bvba">
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

using System.Collections.Generic;

namespace Allors.Database.Domain.Print.StatementOfWorkModel
{
    using System.Linq;

    public class Model
    {
        public Model(StatementOfWork sow, Dictionary<string, byte[]> images)
        {
            this.StatementOfWork = new StatementOfWorkModel(sow);

            this.Issuer = new IssuerModel(sow);
            this.BillTo = new BillToModel(sow);
            this.Receiver = new ReceiverModel(sow);
            this.StatemenOfWorkGSE = new StatemenOfWorkGSEModel(sow.SerialisedItem);
            this.QuoteItems = sow.QuoteItems.Select(v => new QuoteItemModel(v, images)).ToArray();
        }

        public StatementOfWorkModel StatementOfWork { get; }

        public IssuerModel Issuer { get; }

        public BillToModel BillTo { get; }

        public ReceiverModel Receiver { get; }

        public QuoteItemModel[] QuoteItems { get; }

        public StatemenOfWorkGSEModel StatemenOfWorkGSE { get; }
    }
}
