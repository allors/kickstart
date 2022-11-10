using System;
using System.Drawing;
using System.Linq;
using Allors.Excel;
using Allors.Workspace;
using Allors.Workspace.Data;
using Allors.Workspace.Meta;
using PostalAddress = Allors.Workspace.Domain.PostalAddress;
using RepeatingSalesInvoice = Allors.Workspace.Domain.RepeatingSalesInvoice;
using Task = System.Threading.Tasks.Task;

namespace Application.Sheets
{
    public class RentalsSheet : ISheet, ISaveable
    {
        public RentalsSheet(Program program)
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

            var pulls = new Pull[]
            {
                new Pull {
                    Extent = new Filter(M.RepeatingSalesInvoice),

                    Results = new[]
                    {
                        new Result()
                        {
                            Select = new Select
                            {
                                Include =  new []
                                {
                                    new Node(M.RepeatingSalesInvoice.Source, new[]
                                    {
                                        new Node(M.SalesInvoice.BilledFrom),
                                        new Node(M.SalesInvoice.BillToCustomer, new[]
                                        {
                                            new Node(M.Party.PartyContactMechanismsWhereParty, new[]
                                            {
                                                new Node(M.PartyContactMechanism.ContactMechanism, new []
                                                {
                                                    new Node(M.ContactMechanism.AsPostalAddress.Country)
                                                })
                                            }),
                                        }),
                                        new Node(M.SalesInvoice.BillToContactPerson),
                                        new Node(M.SalesInvoice.DerivedBillToContactMechanism),
                                        new Node(M.SalesInvoice.SalesInvoiceItems, new []
                                        {
                                            new Node(M.SalesInvoiceItem.SerialisedItem, new[]
                                            {
                                                new Node(M.SerialisedItem.PartWhereSerialisedItem, new []
                                                {
                                                    new Node(M.Part.Brand),
                                                    new Node(M.Part.Model),
                                                }),
                                                new Node(M.SerialisedItem.SerialisedItemCharacteristics, new []
                                                {
                                                    new Node(M.SerialisedItemCharacteristic.SerialisedItemCharacteristicType),
                                                })
                                            }),
                                        }),
                                    }),
                                }
                            }
                        }
                    }
                },
            };

            var result = await this.Session.PullAsync(pulls);
            this.Session.Reset();

            var repeatingSalesInvoices = result.GetCollection<RepeatingSalesInvoice>("RepeatingSalesInvoices") ?? Array.Empty<RepeatingSalesInvoice>();

            var index = 0;
            var contractStatus = new Column { Header = "Contract Status", Index = index++, NumberFormat = "@" };
            var entity = new Column { Header = "Entity", Index = index++, NumberFormat = "@" };
            var customerName = new Column { Header = "Customer Name", Index = index++, NumberFormat = "@" };
            var country = new Column { Header = "Country", Index = index++, NumberFormat = "@" };
            var contact = new Column { Header = "Contact", Index = index++, NumberFormat = "@" };
            var equipment = new Column { Header = "Equipment", Index = index++, NumberFormat = "@" };
            var brand = new Column { Header = "Brand", Index = index++, NumberFormat = "@" };
            var model = new Column { Header = "Model", Index = index++, NumberFormat = "@" };
            var equipmentSN = new Column { Header = "Equipment SN", Index = index++, NumberFormat = "@" };
            var aviationNr = new Column { Header = "Aviation Nr.", Index = index++, NumberFormat = "@" };
            var deliveryDate = new Column { Header = "Delivery Date", Index = index++, NumberFormat = "dd/MM/yyyy;@" };
            var expectedReturnDate = new Column { Header = "Expected Return Date", Index = index++, NumberFormat = "dd/MM/yyyy;@" };
            var hourmeterOnDelivery = new Column { Header = "Hourmeter on delivery", Index = index++, NumberFormat = "@" };
            var rentalFrom = new Column { Header = "Rental from", Index = index++, NumberFormat = "dd/MM/yyyy;@" };
            var rentalPeriod = new Column { Header = "Rental Period", Index = index++, NumberFormat = "@" };
            var rentalUntil = new Column { Header = "Rental Until", Index = index++, NumberFormat = "dd/MM/yyyy;@" };
            var rentalPriceMnthEUR = new Column { Header = "Rental Price Mnth - EUR", Index = index++, NumberFormat = "@" };
            var transportPaid = new Column { Header = "Transport paid", Index = index++, NumberFormat = "@" };
            var toBeSentTo = new Column { Header = "to be sent to", Index = index++, NumberFormat = "@" };
            var comment = new Column { Header = "Comment", Index = index++, NumberFormat = "@" };

            var columns = new[]
            {
                contractStatus,
                entity,
                customerName,
                country,
                contact,
                equipment,
                brand,
                model,
                equipmentSN,
                aviationNr,
                deliveryDate,
                expectedReturnDate,
                hourmeterOnDelivery,
                rentalFrom,
                rentalPeriod,
                rentalUntil,
                rentalPriceMnthEUR,
                transportPaid,
                toBeSentTo,
                comment,
            };

            foreach (var column in columns)
            {
                this.Sheet[0, column.Index].Value = column.Header;
                this.Sheet[0, column.Index].Style = new Style(Color.LightBlue, Color.Black);
            }

            var row = 1;
            foreach (var repeatingSalesInvoice in repeatingSalesInvoices)
            {
                foreach (var column in columns)
                {
                    this.Sheet[row, column.Index].NumberFormat = column.NumberFormat;
                }

                this.Sheet[row, contractStatus.Index].Value = repeatingSalesInvoice.FinalExecutionDate < now
                    ? "Finished"
                    : "In force";

                var address = repeatingSalesInvoice.Source.BillToCustomer.PartyContactMechanismsWhereParty.FirstOrDefault(v => v.ContactMechanism.GetType().Name == typeof(PostalAddress).Name).ContactMechanism as PostalAddress;

                this.Sheet[row, entity.Index].Value = repeatingSalesInvoice.Source?.BilledFrom.DisplayName;
                this.Sheet[row, customerName.Index].Value = repeatingSalesInvoice.Source.BillToCustomer?.DisplayName;
                this.Sheet[row, country.Index].Value = address?.Country?.IsoCode;
                this.Sheet[row, contact.Index].Value = repeatingSalesInvoice.Source.BillToContactPerson?.DisplayName;
                this.Sheet[row, deliveryDate.Index].Value = repeatingSalesInvoice.Source.InvoiceDate;

                var serialisedItem = repeatingSalesInvoice.Source.SalesInvoiceItems.FirstOrDefault(v => v.ExistSerialisedItem)?.SerialisedItem;

                if (serialisedItem != null)
                {
                    this.Sheet[row, equipment.Index].Value = serialisedItem?.DisplayName;
                    this.Sheet[row, brand.Index].Value = serialisedItem.PartWhereSerialisedItem?.Brand?.Name;
                    this.Sheet[row, model.Index].Value = serialisedItem.PartWhereSerialisedItem?.Model?.Name;
                    this.Sheet[row, equipmentSN.Index].Value = serialisedItem.SerialNumber;
                    this.Sheet[row, aviationNr.Index].Value = serialisedItem.ItemNumber;

                    if (serialisedItem?.ExistExpectedReturnDate == true)
                    {
                        this.Sheet[row, expectedReturnDate.Index].Value = serialisedItem?.ExpectedReturnDate.Value;
                    }
                    else
                    {
                        this.Sheet[row, expectedReturnDate.Index].NumberFormat = "@";
                        this.Sheet[row, expectedReturnDate.Index].Value = string.Empty;
                    }

                    this.Sheet[row, hourmeterOnDelivery.Index].Value = serialisedItem?.SerialisedItemCharacteristics
                        .FirstOrDefault(v => string.Equals(v?.SerialisedItemCharacteristicType?.Name, "Operating Hours", StringComparison.OrdinalIgnoreCase))
                        ?.Value;

                    if (serialisedItem?.ExistRentalFromDate == true)
                    {
                        this.Sheet[row, rentalFrom.Index].Value = serialisedItem?.RentalFromDate.Value;
                    }
                    else
                    {
                        this.Sheet[row, rentalFrom.Index].NumberFormat = "@";
                        this.Sheet[row, rentalFrom.Index].Value = string.Empty;
                    }

                    // TODO: rentalPeriod

                    if (serialisedItem?.RentalThroughDate != null)
                    {
                        this.Sheet[row, rentalUntil.Index].Value = serialisedItem.RentalThroughDate.Value;
                    }
                    else
                    {
                        this.Sheet[row, rentalUntil.Index].NumberFormat = "@";
                        this.Sheet[row, rentalUntil.Index].Value = string.Empty;
                    }
                }

                this.Sheet[row, rentalPriceMnthEUR.Index].Value = repeatingSalesInvoice.Source.TotalExVat;

                // TODO: transportPaid

                this.Sheet[row, toBeSentTo.Index].Value = repeatingSalesInvoice.Source.BillToContactPerson?.DisplayName;
                this.Sheet[row, comment.Index].Value = repeatingSalesInvoice.Source.Comment;

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
