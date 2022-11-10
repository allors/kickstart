using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using Allors.Excel;
using Allors.Extra;
using Allors.Workspace;
using Allors.Workspace.Data;
using Allors.Workspace.Meta;
using Application.Excel;
using Organisation = Allors.Workspace.Domain.Organisation;
using TimeEntry = Allors.Workspace.Domain.TimeEntry;
using UnifiedGood = Allors.Workspace.Domain.UnifiedGood;
using Task = System.Threading.Tasks.Task;

namespace Application.Sheets
{
    public class TimeEntriesSheet : ISheet, ISaveable
    {
        private int StartRowNewItems;

        public TimeEntriesSheet(Program program)
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

        private UnifiedGood[] parts;

        private Organisation[] organisations;

        public async Task Init()
        {
            var now = System.DateTime.Now;

            var pulls = new List<Pull>()
            {
                new Pull
                {
                    Extent = new Filter(M.TimeEntry),

                    Results = new[]
                    {
                        new Result
                        {
                            Select = new Select
                            {
                                Include = new []
                                {
                                    new Node(M.TimeEntry.WorkEffort, new []
                                    {
                                        new Node(M.WorkEffort.Customer),
                                    }),
                                    new Node(M.TimeEntry.Worker, new []
                                    {
                                        new Node(M.Person.EmploymentsWhereEmployee, new []
                                        {
                                            new Node(M.Employment.Employer),
                                        }),
                                    }),
                                    new Node(M.TimeEntry.TimeFrequency),
                                    new Node(M.TimeEntry.RateType),
                                }
                            }
                        }
                    }
                },
            };

            var result = await this.Session.PullAsync(pulls.ToArray());

            var timeEntries = result.GetCollection<TimeEntry>("TimeEntries") ?? Array.Empty<TimeEntry>();

            var index = 0;
            var workOrder = new Column { Header = "Work Order", Index = index++, NumberFormat = "@" };
            var customer = new Column { Header = "Customer", Index = index++, NumberFormat = "@" };
            var worker = new Column { Header = "Worker", Index = index++, NumberFormat = "@" };
            var internalOrganisation = new Column { Header = "InternalOrganisation", Index = index++, NumberFormat = "@" };
            var rate = new Column { Header = "Billing rate", Index = index++, NumberFormat = "@" };
            var week = new Column { Header = "Week", Index = index++, NumberFormat = "@" };
            var month = new Column { Header = "Month", Index = index++, NumberFormat = "@" };
            var year = new Column { Header = "Year", Index = index++, NumberFormat = "@" };
            var from = new Column { Header = "From", Index = index++, NumberFormat = "dd/MM/yyyy HH:mm;@" };
            var through = new Column { Header = "Through", Index = index++, NumberFormat = "dd/MM/yyyy HH:mm;@" };
            var amountOfTime = new Column { Header = "Time", Index = index++, NumberFormat = "@" };
            var billableAmountOfTime = new Column { Header = "Billable Time", Index = index++, NumberFormat = "@" };
            var timeFrequency = new Column { Header = "Time freq.", Index = index++, NumberFormat = "@" };
            var isBillable = new Column { Header = "Billable", Index = index++, NumberFormat = "@" };

            var columns = new[]
            {
                workOrder,
                customer,
                worker,
                internalOrganisation,
                rate,
                week,
                month,
                year,
                from,
                through,
                amountOfTime,
                billableAmountOfTime,
                timeFrequency,
                isBillable,
            };

            foreach (var column in columns)
            {
                this.Sheet[0, column.Index].Value = column.Header;
                this.Sheet[0, column.Index].Style = new Style(Color.LightBlue, Color.Black);
            }

            var row = 1;
            foreach (var timeEntry in timeEntries)
            {
                foreach (var column in columns)
                {
                    this.Sheet[row, column.Index].NumberFormat = column.NumberFormat;
                }

                this.Controls.Label(row, workOrder.Index, timeEntry.WorkEffort, M.WorkEffort.WorkEffortNumber);
                this.Controls.Label(row, customer.Index, timeEntry.WorkEffort.Customer, M.Party.DisplayName);
                this.Controls.Label(row, worker.Index, timeEntry.Worker, M.Party.DisplayName);
                this.Controls.Label(row, internalOrganisation.Index, timeEntry.Worker.EmploymentsWhereEmployee.First().Employer, M.Party.DisplayName);
                this.Controls.Label(row, rate.Index, timeEntry.RateType, M.RateType.Name);
                this.Controls.Static(row, week.Index, this.GetWeekNumber(timeEntry.FromDate));
                this.Controls.Static(row, month.Index, timeEntry.FromDate.Month);
                this.Controls.Static(row, year.Index, timeEntry.FromDate.Year);
                this.Controls.Static(row, from.Index, timeEntry.FromDate);
                if (timeEntry.ExistThroughDate)
                {
                    this.Controls.Static(row, through.Index, timeEntry.ThroughDate.Value);
                }
                this.Controls.Label(row, amountOfTime.Index, timeEntry, M.TimeEntry.AmountOfTime);
                this.Controls.Label(row, billableAmountOfTime.Index, timeEntry, M.TimeEntry.BillableAmountOfTime);
                this.Controls.Label(row, timeFrequency.Index, timeEntry.TimeFrequency, M.TimeFrequency.Name);
                this.Controls.Label(row, isBillable.Index, timeEntry, M.TimeEntry.IsBillable);

                row++;
            }

            this.Controls.Bind();

            await this.Sheet.Flush().ConfigureAwait(false);
        }

        public async Task Update()
        {
            //var pulls = new List<Pull>();
            //this.AddLookupPulls(pulls);

            //var result = await this.Context.Load(pulls.ToArray());

            // TODO: Koen
            //this.Session.Refresh();

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

        private int GetWeekNumber(System.DateTime date)
        {
            CultureInfo ciCurr = CultureInfo.CurrentCulture;
            int weekNum = ciCurr.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, System.DayOfWeek.Monday);
            return weekNum;
        }
    }
}
