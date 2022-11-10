// --------------------------------------------------------------------------------------------------------------------
// <copyright file="WorkEffortExtensions.cs" company="Allors bvba">
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

using System;
using System.Linq;

namespace Allors.Database.Domain
{
    public static partial class WorkEffortExtensions
    {
        public static void AviationComplete(this WorkEffort @this, WorkEffortComplete method)
        {
            // When state is Complete, invoicing is enabled. 
            // When working on you own equipment you do not want to invoice, so set state directly to finished.
            if (@this.ExecutedBy.Equals(@this.Customer))
            {
                @this.WorkEffortState = new WorkEffortStates(@this.Strategy.Transaction).Finished;
            }
            else
            {
                @this.WorkEffortState = new WorkEffortStates(@this.Strategy.Transaction).Completed;
            }

            method.StopPropagation = true;
        }

        public static void AviationInvoice(this WorkEffort @this, WorkEffortInvoice method)
        {
            if (@this.CanInvoice)
            {
                @this.WorkEffortState = new WorkEffortStates(@this.Strategy.Transaction).Finished;
                @this.AviationInvoiceThis();
                @this.CanInvoice = false;
            }

            method.StopPropagation = true;
        }

        private static SalesInvoice AviationInvoiceThis(this WorkEffort @this)
        {
            var transaction = @this.Strategy.Transaction;

            var salesInvoice = new SalesInvoiceBuilder(transaction)
                .WithBilledFrom(@this.TakenBy)
                .WithBillToCustomer(@this.Customer)
                .WithBillToContactPerson(@this.ContactPerson)
                .WithInvoiceDate(DateTime.UtcNow)
                .WithSalesInvoiceType(new SalesInvoiceTypes(transaction).SalesInvoice)
                .WithCustomerReference(@this.WorkEffortNumber)
                .Build();

            var totalAmount = @this.TotalRevenue;

            foreach (WorkEffort childWorkEffort in @this.Children)
            {
                totalAmount += childWorkEffort.TotalRevenue;
            }

            var item = @this.WorkEffortFixedAssetAssignmentsWhereAssignment.FirstOrDefault()?.FixedAsset as SerialisedItem;

            var invoiceItem = new SalesInvoiceItemBuilder(transaction)
                .WithInvoiceItemType(new InvoiceItemTypes(transaction).Rm)
                .WithAssignedUnitPrice(totalAmount)
                .WithQuantity(1)
                .WithDescription($"Work Order {@this.WorkEffortNumber} - {item?.DisplayName} - {item?.ItemNumber}")
                .Build();

            salesInvoice.AddSalesInvoiceItem(invoiceItem);

            foreach (TimeEntry billableEntry in @this.BillableTimeEntries())
            {
                new TimeEntryBillingBuilder(transaction)
                    .WithTimeEntry(billableEntry)
                    .WithInvoiceItem(invoiceItem)
                    .Build();
            }

            new WorkEffortBillingBuilder(transaction)
                .WithWorkEffort(@this)
                .WithInvoiceItem(invoiceItem)
                .Build();

            foreach (WorkEffort childWorkEffort in @this.Children)
            {
                var childTimeEntries = childWorkEffort.ServiceEntriesWhereWorkEffort.OfType<TimeEntry>()
                    .Where(v => v.WorkEffort.CanInvoice && v.IsBillable &&
                                (!v.BillableAmountOfTime.HasValue && v.AmountOfTime.HasValue) || v.BillableAmountOfTime.HasValue)
                    .Select(v => v)
                    .ToArray();

                foreach (TimeEntry billableEntry in childTimeEntries)
                {
                    new TimeEntryBillingBuilder(transaction)
                        .WithTimeEntry(billableEntry)
                        .WithInvoiceItem(invoiceItem)
                        .Build();
                }

                new WorkEffortBillingBuilder(transaction)
                    .WithWorkEffort(childWorkEffort)
                    .WithInvoiceItem(invoiceItem)
                    .Build();
            }

            return salesInvoice;
        }
    }
}
