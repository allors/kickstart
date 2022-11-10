// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Good.cs" company="Allors bvba">
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
using System.Collections.Generic;
using System.Linq;

namespace Allors.Database.Domain
{
    public partial class Organisation
    {
        public Expression CleaningExpression => this.ExistCleaningCalculation ? new Expression(this.CleaningCalculation) : null;

        public Expression SundriesExpression => this.ExistSundriesCalculation ? new Expression(this.SundriesCalculation) : null;

        public void AviationCreateWorkEffortInvoice(InternalOrganisationCreateWorkEffortInvoice method)
        {
            var customers = new Parties(this.Transaction()).Extent();
            customers.Filter.AddEquals(M.Party.CollectiveWorkEffortInvoice, true);

            var workTasks = new WorkTasks(this.Transaction()).Extent();
            workTasks.Filter.AddEquals(M.WorkEffort.ExecutedBy, this);
            workTasks.Filter.AddEquals(M.WorkEffort.WorkEffortState, new WorkEffortStates(this.Transaction()).Completed);
            workTasks.Filter.AddContainedIn(M.WorkEffort.Customer, (Extent)customers);

            var workTasksByCustomer = workTasks.Select(v => v.Customer).Distinct()
                .ToDictionary(v => v, v => v.WorkEffortsWhereCustomer
                .Where(w => w.WorkEffortState.Equals(new WorkEffortStates(this.Transaction()).Completed) && w.ExecutedBy.Equals(this))
                .ToArray());

            SalesInvoice salesInvoice = null;

            foreach (var customerWorkTasks in workTasksByCustomer)
            {
                var customer = customerWorkTasks.Key;

                var customerWorkTasksByInternalOrganisation = customerWorkTasks.Value
                    .GroupBy(v => v.TakenBy)
                    .Select(v => v)
                    .ToArray();

                if (customerWorkTasks.Value.Any(v => v.CanInvoice))
                {
                    foreach (var group in customerWorkTasksByInternalOrganisation)
                    {
                        if (group.Any(v => v.CanInvoice))
                        {
                            salesInvoice = new SalesInvoiceBuilder(this.Transaction())
                                .WithBilledFrom(group.Key)
                                .WithBillToCustomer(customer)
                                .WithInvoiceDate(this.strategy.Transaction.Database.Services.Get<ITime>().Now())
                                .WithSalesInvoiceType(new SalesInvoiceTypes(this.Transaction()).SalesInvoice)
                                .Build();
                        }

                        foreach (var workEffort in group)
                        {
                            if (workEffort.CanInvoice)
                            {
                                if (string.IsNullOrEmpty(salesInvoice.CustomerReference))
                                {
                                    salesInvoice.CustomerReference = $"WorkOrder(s): {workEffort.WorkEffortNumber}";
                                }
                                else
                                {
                                    salesInvoice.CustomerReference += $", {workEffort.WorkEffortNumber}";
                                }

                                var totalAmount = workEffort.TotalRevenue;

                                foreach (WorkEffort childWorkEffort in workEffort.Children)
                                {
                                    totalAmount += childWorkEffort.TotalRevenue;
                                }

                                var item = workEffort.WorkEffortFixedAssetAssignmentsWhereAssignment.FirstOrDefault()?.FixedAsset as SerialisedItem;

                                var invoiceItem = new SalesInvoiceItemBuilder(this.Transaction())
                                    .WithInvoiceItemType(new InvoiceItemTypes(this.Transaction()).Rm)
                                    .WithAssignedUnitPrice(totalAmount)
                                    .WithQuantity(1)
                                    .WithDescription($"Work Order {workEffort.WorkEffortNumber} - {item?.DisplayName} - {item?.ItemNumber}")
                                    .Build();

                                salesInvoice.AddSalesInvoiceItem(invoiceItem);

                                foreach (TimeEntry billableEntry in workEffort.BillableTimeEntries())
                                {
                                    new TimeEntryBillingBuilder(this.Transaction())
                                        .WithTimeEntry(billableEntry)
                                        .WithInvoiceItem(invoiceItem)
                                        .Build();
                                }

                                new WorkEffortBillingBuilder(this.Transaction())
                                    .WithWorkEffort(workEffort)
                                    .WithInvoiceItem(invoiceItem)
                                    .Build();

                                foreach (WorkEffort childWorkEffort in workEffort.Children)
                                {
                                    var childTimeEntries = childWorkEffort.ServiceEntriesWhereWorkEffort.OfType<TimeEntry>()
                                        .Where(v => v.WorkEffort.CanInvoice && v.IsBillable &&
                                                    (!v.BillableAmountOfTime.HasValue && v.AmountOfTime.HasValue) || v.BillableAmountOfTime.HasValue)
                                        .Select(v => v)
                                        .ToArray();

                                    foreach (TimeEntry billableEntry in childTimeEntries)
                                    {
                                        new TimeEntryBillingBuilder(this.Transaction())
                                            .WithTimeEntry(billableEntry)
                                            .WithInvoiceItem(invoiceItem)
                                            .Build();
                                    }

                                    new WorkEffortBillingBuilder(this.Transaction())
                                        .WithWorkEffort(childWorkEffort)
                                        .WithInvoiceItem(invoiceItem)
                                        .Build();
                                }
                            }

                            workEffort.WorkEffortState = new WorkEffortStates(this.Transaction()).Finished;
                        }
                    }
                }
            }

            method.StopPropagation = true;
        }
    }
}