using System;
using System.Drawing;
using System.Linq;
using Allors.Excel;
using Allors.Workspace;
using Allors.Workspace.Data;
using Allors.Excel;
using Allors.Workspace.Meta;
using Payment = Allors.Workspace.Domain.Payment;
using SalesInvoice = Allors.Workspace.Domain.SalesInvoice;
using Task = System.Threading.Tasks.Task;

namespace Application.Sheets
{
    public class SalesInvoicesSheet : ISheet, ISaveable
    {
        public SalesInvoicesSheet(Program program)
        {
            this.Sheet = program.ActiveWorkbook.AddWorksheet();

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

            var x = M.Party.AsPerson;

            var pulls = new Pull[]
            {
                new Pull
                {
                    Extent = new Filter(M.SalesInvoice),

                    Results = new[]
                    {
                        new Result()
                        {
                            Select = new Select
                            {
                                Include =  new []
                                {
                                    new Node(M.SalesInvoice.BilledFrom),
                                    new Node(M.SalesInvoice.BillToCustomer),
                                    new Node(M.SalesInvoice.BillToContactPerson, new []
                                    {
                                        new Node(M.Party.AsPerson.Salutation),
                                        new Node(M.Party.GeneralCorrespondence),
                                    }),
                                    new Node(M.SalesInvoice.DerivedCurrency),
                                    new Node(M.SalesInvoice.SalesInvoiceType),
                                    new Node(M.SalesInvoice.DerivedVatRegime, new []
                                    {
                                        new Node(M.VatRegime.VatRates),
                                    }),
                                    new Node(M.SalesInvoice.SalesInvoiceState),
                                }
                            }
                        }
                    }
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
                                Include = new []
                                {
                                    new Node(M.Payment.PaymentApplications, new []
                                    {
                                        new Node(M.PaymentApplication.Invoice),
                                    }),
                                }
                            }
                        } ,
                    }
                }
            };

            var result = await this.Session.PullAsync(pulls);
            this.Session.Reset();

            var salesInvoices = result.GetCollection<SalesInvoice>("SalesInvoices") ?? Array.Empty<SalesInvoice>();
            var payments = result.GetCollection<Payment>("Payments") ?? Array.Empty<Payment>();

            var index = 0;
            var entity = new Column { Header = "Entity", Index = index++, NumberFormat = "@" };
            var invoice = new Column { Header = "Invoice", Index = index++, NumberFormat = "@" };
            var customerName = new Column { Header = "Customer Name", Index = index++, NumberFormat = "@" };
            var invoiceDate = new Column { Header = "Invoice Date", Index = index++, NumberFormat = "dd/MM/yyyy;@" };
            var dueDate = new Column { Header = "Due date", Index = index++, NumberFormat = "dd/MM/yyyy;@" };
            var description = new Column { Header = "Description", Index = index++, NumberFormat = "@" };
            var vatPercent = new Column { Header = "Vat %", Index = index++, NumberFormat = "@" };
            var totalExVat = new Column { Header = "Total ex. VAT", Index = index++, NumberFormat = "@" };
            var vatAmount = new Column { Header = "VAT", Index = index++, NumberFormat = "@" };
            var totalIncVat = new Column { Header = "Total inc. VAT", Index = index++, NumberFormat = "@" };
            var currency = new Column { Header = "Currency", Index = index++, NumberFormat = "@" };
            var poRefNr = new Column { Header = "PO/REF nr.", Index = index++, NumberFormat = "@" };
            var paymentStatus = new Column { Header = "Payment Status", Index = index++, NumberFormat = "@" };
            var amountPaid = new Column { Header = "Amount Paid", Index = index++, NumberFormat = "@" };
            var paymentDate = new Column { Header = "Payment Date", Index = index++, NumberFormat = "dd/MM/yyyy;@" };
            var comment = new Column { Header = "Comment", Index = index++, NumberFormat = "@" };

            var columns = new[]
            {
                entity,
                invoice,
                customerName,
                invoiceDate,
                dueDate,
                description,
                vatPercent,
                totalExVat,
                vatAmount,
                totalIncVat,
                currency,
                poRefNr,
                paymentStatus,
                amountPaid,
                paymentDate,
                comment,
            };

            foreach (var column in columns)
            {
                this.Sheet[0, column.Index].Value = column.Header;
                this.Sheet[0, column.Index].Style = new Style(Color.LightBlue, Color.Black);
            }

            var row = 1;
            foreach (var salesInvoice in salesInvoices)
            {
                foreach (var column in columns)
                {
                    this.Sheet[row, column.Index].NumberFormat = column.NumberFormat;
                }

                this.Sheet[row, entity.Index].Value = salesInvoice.BilledFrom?.DisplayName;
                this.Sheet[row, invoice.Index].Value = salesInvoice.InvoiceNumber;
                this.Sheet[row, customerName.Index].Value = salesInvoice.BillToCustomer?.DisplayName;
                this.Sheet[row, invoiceDate.Index].Value = salesInvoice.InvoiceDate;

                this.Sheet[row, dueDate.Index].Value = salesInvoice.DueDate.Value;
                this.Sheet[row, description.Index].Value = salesInvoice.Description;
                this.Sheet[row, vatPercent.Index].Value = string.Join(", ", salesInvoice.DerivedVatRegime?.VatRates?.Select(v => v.Rate) ?? Array.Empty<decimal>());

                this.Sheet[row, totalExVat.Index].Value = salesInvoice.TotalExVat;
                this.Sheet[row, vatAmount.Index].Value = salesInvoice.TotalVat;
                this.Sheet[row, totalIncVat.Index].Value = salesInvoice.TotalIncVat;
                this.Sheet[row, currency.Index].Value = salesInvoice.DerivedCurrency.IsoCode;
                this.Sheet[row, poRefNr.Index].Value = salesInvoice.CustomerReference;
                this.Sheet[row, paymentStatus.Index].Value = salesInvoice.SalesInvoiceState?.Name;
                this.Sheet[row, amountPaid.Index].Value = salesInvoice.AmountPaid;

                var payment = payments
                    .OrderByDescending(v => v.EffectiveDate)
                    .FirstOrDefault(v => v.PaymentApplications.Any(w => w.Invoice is SalesInvoice && Equals(w.Invoice, salesInvoice)));

                if (payment != null)
                {
                    this.Sheet[row, paymentDate.Index].Value = payment.EffectiveDate;
                }
                else
                {
                    this.Sheet[row, paymentDate.Index].NumberFormat = "@";
                    this.Sheet[row, paymentDate.Index].Value = string.Empty;
                }

                // TODO add aantal dagen vervallen

                this.Sheet[row, comment.Index].Value = salesInvoice.InternalComment;

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
