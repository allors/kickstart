using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Allors.Excel;
using Allors.Workspace;
using Allors.Workspace.Data;
using Allors.Workspace.Domain;
using Allors.Workspace.Meta;
using Payment = Allors.Workspace.Domain.Payment;
using PurchaseInvoice = Allors.Workspace.Domain.PurchaseInvoice;
using Task = System.Threading.Tasks.Task;

namespace Application.Sheets
{
    public class PurchaseInvoicesSheet : ISheet, ISaveable
    {
        private readonly Style defaultPurchaseInvoiceState;
        private readonly Dictionary<Guid, Style> styleByPurchaseInvoiceState;

        public PurchaseInvoicesSheet(Program program)
        {
            this.Sheet = program.ActiveWorkbook.AddWorksheet();

            this.defaultPurchaseInvoiceState = new Style(Color.Empty, Color.Empty);
            this.styleByPurchaseInvoiceState = new Dictionary<Guid, Style>
            {
                {PurchaseInvoiceStates.PaidId, new Style(Color.LightPink, Color.Red )},
                {PurchaseInvoiceStates.AwaitingApprovalId, new Style(Color.Khaki, Color.SandyBrown )},
                {PurchaseInvoiceStates.NotPaidId, new Style(Color.White, Color.Black )},
            };

            this.M = program.M;
            this.MessageService = program.Workspace.Services.Get<IMessageService>();
            this.ErrorService = program.Workspace.Services.Get<IErrorService>();

            this.Session = program.Workspace.CreateSession();
        }

        public M M { get; set; }

        public ISession Session { get; }

        public IWorksheet Sheet { get; }

        public IMessageService MessageService { get; set; }

        public IErrorService ErrorService { get; }

        public async Task Load()
        {
            var now = System.DateTime.Now;

            var pulls = new Pull[]
            {
                new Pull
                {
                    Extent = new Filter(M.PurchaseInvoice),

                    Results = new[]
                    {
                        new Result
                        {
                            Select = new Select
                            {
                                Include = new []
                                {
                                    new Node(M.PurchaseInvoice.BilledTo),
                                    new Node(M.PurchaseInvoice.BilledFrom),
                                    new Node(M.PurchaseInvoice.DerivedCurrency),
                                    new Node(M.PurchaseInvoice.PurchaseInvoiceState),
                                }
                            },
                        },
                    },
                },
                new Pull
                {
                    Extent = new Filter(M.Payment),

                    Results = new[]
                    {
                        new Result
                        {
                            Select = new Select
                            {
                                Include = new Node[]
                                {
                                    new Node(M.Payment.PaymentApplications, new []
                                    {
                                        new Node(M.PaymentApplication.Invoice),
                                    }),
                                }
                            },
                        },
                    },
                }
            };

            var result = await this.Session.PullAsync(pulls);
            this.Session.Reset();

            var purchaseInvoices = result.GetCollection<PurchaseInvoice>("PurchaseInvoices") ?? Array.Empty<PurchaseInvoice>();
            var payments = result.GetCollection<Payment>("Payments") ?? Array.Empty<Payment>();

            var index = 0;
            var org = new Column { Header = "Org.", Index = index++, NumberFormat = "@" };
            var refNr = new Column { Header = "Ref Nr.", Index = index++, NumberFormat = "@" };
            var supplierName = new Column { Header = "Supplier Name", Index = index++, NumberFormat = "@" };
            var invoiceNr = new Column { Header = "Invoice Nr.", Index = index++, NumberFormat = "@" };
            var invoiceDate = new Column { Header = "Invoice Date", Index = index++, NumberFormat = "dd/mm/yyyy" };
            var description = new Column { Header = "Description", Index = index++, NumberFormat = "@" };
            var amountNet = new Column { Header = "Amount Net", Index = index++, NumberFormat = "@" };
            var amount = new Column { Header = "Amount", Index = index++, NumberFormat = "@" };
            var amountSundries = new Column { Header = "Amount Sundries", Index = index++, NumberFormat = "@" };
            var currency = new Column { Header = "Currency", Index = index++, NumberFormat = "@" };
            var status = new Column { Header = "Status", Index = index++, NumberFormat = "@" };
            var dueDate = new Column { Header = "Due Date", Index = index++, NumberFormat = "@" };
            var paymentDate = new Column { Header = "Payment Date", Index = index++, NumberFormat = "@" };
            var notes = new Column { Header = "Notes", Index = index++, NumberFormat = "@" };

            var columns = new[]
            {
                org,
                refNr,
                supplierName,
                invoiceNr,
                invoiceDate,
                description,
                amountNet,
                amount,
                amountSundries,
                currency,
                status,
                dueDate,
                paymentDate,
                notes,
            };

            foreach (var column in columns)
            {
                this.Sheet[0, column.Index].Value = column.Header;
                this.Sheet[0, column.Index].Style = new Style(Color.LightBlue, Color.Black);
            }

            var row = 1;
            foreach (var purchaseInvoice in purchaseInvoices)
            {
                foreach (var column in columns)
                {
                    this.Sheet[row, column.Index].NumberFormat = column.NumberFormat;
                    this.Sheet[row, column.Index].Style = new Style(Color.Aqua, Color.BurlyWood);
                }

                this.Sheet[row, org.Index].Value = purchaseInvoice.BilledTo?.DisplayName;
                this.Sheet[row, refNr.Index].Value = purchaseInvoice.InvoiceNumber;
                this.Sheet[row, supplierName.Index].Value = purchaseInvoice.BilledFrom?.DisplayName;
                this.Sheet[row, invoiceNr.Index].Value = purchaseInvoice.CustomerReference;
                this.Sheet[row, invoiceDate.Index].Value = purchaseInvoice.InvoiceDate.ToLocalTime().ToString("dd/MM/yyyy");
                this.Sheet[row, description.Index].Value = purchaseInvoice.Description;
                this.Sheet[row, amountNet.Index].Value = purchaseInvoice.TotalExVat;
                this.Sheet[row, amount.Index].Value = purchaseInvoice.TotalIncVat;
                this.Sheet[row, amountSundries.Index].Value = purchaseInvoice.AmountSundries;
                this.Sheet[row, currency.Index].Value = purchaseInvoice.DerivedCurrency?.IsoCode;

                Style style = null;
                if (purchaseInvoice.PurchaseInvoiceState?.UniqueId != null)
                {
                    this.styleByPurchaseInvoiceState.TryGetValue(purchaseInvoice.PurchaseInvoiceState.UniqueId, out style);
                }
                this.Sheet[row, status.Index].Style = style ?? this.defaultPurchaseInvoiceState;
                this.Sheet[row, status.Index].Value = purchaseInvoice.PurchaseInvoiceState?.Name;

                if (purchaseInvoice.DueDate.HasValue)
                {
                    this.Sheet[row, dueDate.Index].Value = purchaseInvoice.DueDate.Value.ToLocalTime().ToString("dd/MM/yyyy");
                }
                else
                {
                    this.Sheet[row, dueDate.Index].NumberFormat = "@";
                    this.Sheet[row, dueDate.Index].Value = string.Empty;
                }

                var payment = payments
                    .OrderByDescending(v => v.EffectiveDate)
                    .FirstOrDefault(v => v.PaymentApplications.Any(w => w.Invoice is PurchaseInvoice && Equals(w.Invoice, purchaseInvoice)));

                if (payment != null)
                {
                    this.Sheet[row, paymentDate.Index].Value = payment.EffectiveDate.ToLocalTime().ToString("dd/MM/yyyy");
                }
                else
                {
                    this.Sheet[row, paymentDate.Index].NumberFormat = "@";
                    this.Sheet[row, paymentDate.Index].Value = string.Empty;
                }

                this.Sheet[row, notes.Index].Value = purchaseInvoice.InternalComment;

                row++;
            }

            await this.Sheet.Flush();
        }

        public async Task Save()
        {
            var response = await this.Session.PushAsync();
            if (response.HasErrors)
            {
                this.ErrorService.Handle(response, this.Session);
            }
            else
            {
                this.MessageService.Show("Successfully saved", "Info");
            }

            await this.Load();
        }

        public async Task Refresh()
        {
            await this.Load();
        }
    }
}
