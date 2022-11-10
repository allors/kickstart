using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Allors.Excel;
using Allors.Workspace;
using Allors.Workspace.Data;
using Allors.Workspace.Meta;
using Application.Excel;
using Organisation = Allors.Workspace.Domain.Organisation;
using WorkTask = Allors.Workspace.Domain.WorkTask;
using TimeEntry = Allors.Workspace.Domain.TimeEntry;
using Task = System.Threading.Tasks.Task;
using Excel.Allors;

namespace Application.Sheets
{
    public class WorkOrdersSheet : ISheet, ISaveable
    {
        private int StartRowNewItems;

        public WorkOrdersSheet(Program program)
        {
            this.Sheet = program.ActiveWorkbook.AddWorksheet();
            this.Binder = new Binder(this.Sheet, Constants.ChangedStyle);
            this.Binder.ToDomained += this.Binder_ToDomained;

            this.Controls = new Controls(this.Sheet);

            this.Sheet.SheetActivated += this.Sheet_SheetActivated;

            this.M = program.M;
            this.MessageService = program.Workspace.Services.Get<IMessageService>();
            this.ErrorService = program.Workspace.Services.Get<IErrorService>();

            this.Session = program.Workspace.CreateSession();
        }

        public M M { get; set; }

        public ISession Session { get; private set; }

        public IWorksheet Sheet { get; }

        public Binder Binder { get; set; }

        public Controls Controls { get; }

        public IMessageService MessageService { get; set; }

        public IErrorService ErrorService { get; }

        private bool IsWorksheetUpToDate { get; set; }

        private Organisation[] organisations;

        public async Task Init()
        {
            var now = System.DateTime.Now;

            var pulls = new List<Pull>()
            {
                new Pull
                {
                    Extent = new Filter(M.WorkTask),

                    Results = new[]
                    {
                        new Result
                        {
                            Select = new Select
                            {
                                Include = new []
                                {
                                    new Node(M.WorkTask.Customer),
                                    new Node(M.WorkTask.ExecutedBy),
                                    new Node(M.WorkTask.WorkEffortState),
                                    new Node(M.WorkTask.WorkEffortPartyAssignmentsWhereAssignment, new []
                                    {
                                        new Node(M.WorkEffortPartyAssignment.Party),
                                    }),
                                    new Node(M.WorkTask.WorkEffortFixedAssetAssignmentsWhereAssignment, new []
                                    {
                                        new Node(M.WorkEffortFixedAssetAssignment.FixedAsset),
                                    }),
                                    new Node(M.WorkTask.WorkEffortInventoryAssignmentsWhereAssignment),
                                    new Node(M.WorkTask.ServiceEntriesWhereWorkEffort),
                                }
                            }
                        }
                    }
                },
            };

            var result = await this.Session.PullAsync(pulls.ToArray());

            var workOrders = result.GetCollection<WorkTask>("WorkTasks") ?? Array.Empty<WorkTask>();

            var index = 0;
            var workOrder = new Column { Header = "Work Order", Index = index++, NumberFormat = "@" };
            var name = new Column { Header = "Name", Index = index++, NumberFormat = "@" };
            var issueDate = new Column { Header = "Issue Date", Index = index++, NumberFormat = "dd/MM/yyyy;@" };
            var startDate = new Column { Header = "Start Date", Index = index++, NumberFormat = "dd/MM/yyyy;@" };
            var completionDate = new Column { Header = "End Date", Index = index++, NumberFormat = "dd/MM/yyyy;@" };
            var lastModified = new Column { Header = "Last Modified", Index = index++, NumberFormat = "dd/MM/yyyy;@" };
            var customer = new Column { Header = "Customer", Index = index++, NumberFormat = "@" };
            var executedBy = new Column { Header = "Executed By", Index = index++, NumberFormat = "@" };
            var state = new Column { Header = "State", Index = index++, NumberFormat = "@" };
            var equipment = new Column { Header = "Equipment", Index = index++, NumberFormat = "@" };
            var workers = new Column { Header = "Worker(s)", Index = index++, NumberFormat = "@" };
            var labour = new Column { Header = "Labour", Index = index++, NumberFormat = "@" };
            var parts = new Column { Header = "Parts", Index = index++, NumberFormat = "@" };
            var sublet = new Column { Header = "Sublet", Index = index++, NumberFormat = "@" };
            var other = new Column { Header = "Other", Index = index++, NumberFormat = "@" };
            var total = new Column { Header = "total", Index = index++, NumberFormat = "@" };
            var costOfGoodsSold = new Column { Header = "Cost of Goods sold", Index = index++, NumberFormat = "@" };
            var timeSpend = new Column { Header = "Time Spend dd:hh:mm", Index = index++, NumberFormat = "dd:hh:mm" };
            var subletCost = new Column { Header = "Sublet Cost", Index = index++, NumberFormat = "@" };

            var columns = new[]
            {
                workOrder,
                name,
                issueDate,
                startDate,
                completionDate,
                lastModified,
                customer,
                executedBy,
                state,
                equipment,
                workers,
                labour,
                parts,
                sublet,
                other,
                total,
                costOfGoodsSold,
                timeSpend,
                subletCost
            };

            foreach (var column in columns)
            {
                this.Sheet[0, column.Index].Value = column.Header;
                this.Sheet[0, column.Index].Style = new Style(Color.LightBlue, Color.Black);
            }

            var row = 1;
            foreach (var @this in workOrders)
            {
                foreach (var column in columns)
                {
                    this.Sheet[row, column.Index].NumberFormat = column.NumberFormat;
                }

                this.Controls.Label(row, workOrder.Index, @this, M.WorkTask.WorkEffortNumber);
                this.Controls.Label(row, name.Index, @this, M.WorkTask.Name);
                this.Sheet[row, issueDate.Index].Value = @this.IssueDate;
                this.Sheet[row, startDate.Index].Value = @this.ActualStart;
                this.Sheet[row, completionDate.Index].Value = @this.ActualCompletion;
                this.Sheet[row, lastModified.Index].Value = @this.LastModifiedDate;
                this.Controls.Label(row, customer.Index, @this.Customer, M.Party.DisplayName);
                this.Controls.Label(row, executedBy.Index, @this.ExecutedBy, M.Party.DisplayName);
                this.Controls.Label(row, state.Index, @this.WorkEffortState, M.WorkEffortState.Name);
                this.Controls.Static(row, equipment.Index, string.Join(", ", @this.WorkEffortFixedAssetAssignmentsWhereAssignment.Select(v => v.FixedAsset.DisplayName)));
                this.Controls.Static(row, workers.Index, string.Join(", ", @this.WorkEffortPartyAssignmentsWhereAssignment.Select(v => v.Party.DisplayName)));
                this.Controls.Static(row, labour.Index, @this.TotalLabourRevenue, NumberFormats.TwoDecimals);
                this.Controls.Static(row, parts.Index, @this.TotalMaterialRevenue, NumberFormats.TwoDecimals);
                this.Controls.Static(row, sublet.Index, @this.TotalSubContractedRevenue, NumberFormats.TwoDecimals);
                this.Controls.Static(row, other.Index, @this.TotalOtherRevenue, NumberFormats.TwoDecimals);
                this.Controls.Static(row, total.Index, @this.GrandTotal, NumberFormats.TwoDecimals);
                this.Controls.Static(row, costOfGoodsSold.Index, decimal.Round(@this.WorkEffortInventoryAssignmentsWhereAssignment.Sum(v => v.CostOfGoodsSold), 2), NumberFormats.TwoDecimals);

                var timeEntries = @this.ServiceEntriesWhereWorkEffort.Where(v => v.Strategy.Class == M.TimeEntry).Cast<TimeEntry>();
                this.Controls.Static(row, timeSpend.Index, decimal.Round(timeEntries.Sum(v => v.AmountOfTimeInMinutes), 2) / (24 * 60), NumberFormats.Time);
                this.Controls.Static(row, subletCost.Index, @this.TotalSubContractedCost, NumberFormats.TwoDecimals);

                row++;
            }

            this.Controls.Bind();

            await this.Sheet.Flush().ConfigureAwait(false);
        }

        public async Task Update()
        {
            this.Controls.Bind();

            await this.Sheet.Flush().ConfigureAwait(false);
        }

        public async Task Save()
        {
            var response = await this.Session.PushAsync();
            if (response.HasErrors)
            {
                if (response.AccessErrors?.Any() == true || response.MissingErrors?.Any() == true || response.VersionErrors?.Any() == true)
                {
                    this.ErrorService.Handle(response, this.Session);
                    this.MessageService.Show("Error was irrecoverable, sheet has been reset", "Info");
                    await this.Init();
                }
                else
                {
                    this.ErrorService.Handle(response, this.Session);
                }
            }
            else
            {
                this.MessageService.Show("Successfully saved", "Info");
                this.Sheet.DeleteRows(this.StartRowNewItems, 100);
                await this.Init();
            }
        }

        public async Task Refresh()
        {
            if (!this.Session.HasChanges)
            {
                await this.Init();
            }
            else
            {
                switch (this.MessageService.ShowDialog("Do you want to keep your changes", "Info"))
                {
                    case true:
                        await this.Update();
                        break;
                    case false:
                        await this.Init();
                        break;
                }
            }
        }

        private async void Sheet_SheetActivated(object sender, string e)
        {
            if (!this.IsWorksheetUpToDate)
            {
                await this.Refresh().ConfigureAwait(false);

                this.IsWorksheetUpToDate = true;
            }
        }

        private async void Binder_ToDomained(object sender, EventArgs e)
        {
            await this.Sheet.Flush().ConfigureAwait(false);
        }
    }
}
