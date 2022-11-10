// <copyright file="SalesOrder.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Linq;

namespace Allors.Database.Domain
{
    public partial class SalesOrder
    {
        public void AviationSetReadyForPosting(SalesOrderSetReadyForPosting method)
        {
            var orderThreshold = this.Store.OrderThreshold;
            var partyFinancial = this.BillToCustomer.PartyFinancialRelationshipsWhereFinancialParty.FirstOrDefault(v => Equals(v.InternalOrganisation, this.TakenBy));

            var amountOverDue = partyFinancial?.AmountOverDue;
            var creditLimit = partyFinancial?.CreditLimit ?? (this.Store.ExistCreditLimit ? this.Store.CreditLimit : 0);

            if (amountOverDue > creditLimit || this.TotalExVat < orderThreshold) // Theshold is minimum order amount required.
            {
                this.SalesOrderState = new SalesOrderStates(this.Strategy.Transaction).RequestsApproval;
            }
            else
            {
                this.SalesOrderState = new SalesOrderStates(this.Strategy.Transaction).InProcess;
            }

            method.StopPropagation = true;
        }

        public void AviationApprove(OrderApprove method)
        {
            this.SalesOrderState = new SalesOrderStates(this.Strategy.Transaction).InProcess;
            method.StopPropagation = true;
        }
    }
}
