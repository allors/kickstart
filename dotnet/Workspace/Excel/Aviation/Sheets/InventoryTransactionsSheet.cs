using System;
using System.Drawing;
using Allors.Excel;
using Allors.Workspace;
using Allors.Workspace.Data;
using Task = System.Threading.Tasks.Task;
using InventoryItemTransaction = Allors.Workspace.Domain.InventoryItemTransaction;
using WorkEffortInventoryAssignment = Allors.Workspace.Domain.WorkEffortInventoryAssignment;
using Part = Allors.Workspace.Domain.Part;
using Allors.Workspace.Meta;
using System.Collections.Generic;

namespace Application.Sheets
{
    public class InventoryTransactionsSheet : ISheet, ISaveable
    {
        public InventoryTransactionsSheet(Program program)
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
            var pulls = new Pull[]
            {
                new Pull()
                {
                    Extent = new Filter(M.InventoryItemTransaction)
                    {
                        Predicate = new Exists(M.InventoryItemTransaction.NonSerialisedInventoryItemState),
                        Sorting = new []
                        {
                            new Sort(M.InventoryItemTransaction.PartNumber) { SortDirection = Allors.SortDirection.Ascending},
                            new Sort(M.InventoryItemTransaction.FacilityName),
                            new Sort(M.InventoryItemTransaction.InventoryItemStateName),
                            new Sort(M.InventoryItemTransaction.TransactionDate) { SortDirection = Allors.SortDirection.Ascending},
                        }
                    },

                    Results = new[]
                    {
                        new Result()
                        {
                            Select = new Select
                            {
                                Include =  new []
                                {
                                    new Node(M.InventoryItemTransaction.Reason),
                                    new Node(M.InventoryItemTransaction.InventoryItem),
                                    new Node(M.InventoryItemTransaction.UnitOfMeasure),
                                    new Node(M.InventoryItemTransaction.ShipmentItem, new []
                                    {
                                        new Node(M.ShipmentItem.ShipmentWhereShipmentItem),
                                    }),
                                }
                            }
                        }
                    }
                },
            };

            var result = await this.Session.PullAsync(pulls);
            this.Session.Reset();

            var transactions = result.GetCollection<InventoryItemTransaction>("InventoryItemTransactions") ?? Array.Empty<InventoryItemTransaction>();

            var index = 0;
            var productId = new Column { Header = "PartNo", NumberFormat = "@", Index = index++ };
            var name = new Column { Header = "Name", NumberFormat = "@", Index = index++ };
            var facility = new Column { Header = "Facility", NumberFormat = "@", Index = index++ };
            var state = new Column { Header = "State", NumberFormat = "@", Index = index++ };
            var reason = new Column { Header = "Reason", NumberFormat = "@", Index = index++ };
            var date = new Column { Header = "date", NumberFormat = "dd/MM/yyyy;@", Index = index++ };
            var quantity = new Column { Header = "quantity", NumberFormat = "@", Index = index++ };
            var uom = new Column { Header = "UOM", NumberFormat = "@", Index = index++ };
            var shipment = new Column { Header = "Shipment", NumberFormat = "@", Index = index++ };
            var workOrder = new Column { Header = "Work Order", NumberFormat = "@", Index = index++ };
            var cost = new Column { Header = "Cost", NumberFormat = "@", Index = index++ };
            var instock = new Column { Header = "In Stock", NumberFormat = "@", Index = index++ };

            var columns = new[] {
                productId,
                name,
                facility,
                state,
                reason,
                date,
                quantity,
                uom,
                shipment,
                workOrder,
                cost,
                instock
            };

            foreach (var item in columns)
            {
                this.Sheet[0, item.Index].Value = item.Header;
                this.Sheet[0, item.Index].Style = new Style(Color.LightBlue, Color.Black);
            }

            var row = 1;
            var partNumber = "";
            var qoh = 0M;

            // list nonserialised item only
            foreach (var transaction in transactions)
            {
                foreach (var column in columns)
                {
                    this.Sheet[row, column.Index].NumberFormat = column.NumberFormat;
                }

                if (partNumber != transaction.PartNumber)
                {
                    partNumber = transaction.PartNumber;
                    qoh = 0M;
                }

                this.Sheet[row, productId.Index].Value = transaction.PartNumber;
                this.Sheet[row, name.Index].Value = transaction.PartDisplayName;
                this.Sheet[row, facility.Index].Value = transaction.FacilityName;
                this.Sheet[row, state.Index].Value = transaction.InventoryItemStateName;
                this.Sheet[row, reason.Index].Value = transaction.Reason?.Name;
                this.Sheet[row, date.Index].Value = transaction.TransactionDate;

                var transactionQuantity = 0M;
                if (transaction.Reason?.IncreasesQuantityOnHand == true)
                {
                    transactionQuantity = transaction.Quantity;
                }
                else if (transaction.Reason?.IncreasesQuantityOnHand == false)
                {
                    transactionQuantity = 0 - transaction.Quantity;
                }

                this.Sheet[row, quantity.Index].Value = transactionQuantity;
                this.Sheet[row, uom.Index].Value = transaction.UnitOfMeasure?.Abbreviation;

                if (transaction.ExistShipmentItem)
                {
                    this.Sheet[row, shipment.Index].Value = transaction.ShipmentItem.ShipmentWhereShipmentItem?.ShipmentNumber;
                }

                this.Sheet[row, workOrder.Index].Value = transaction.WorkEffortNumber;
                this.Sheet[row, cost.Index].Value = transaction.Cost;

                qoh += transactionQuantity;
                this.Sheet[row, instock.Index].Value = qoh;

                ++row;
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
