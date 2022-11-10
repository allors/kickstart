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
using DateTime = System.DateTime;
using ProductCategory = Allors.Workspace.Domain.ProductCategory;
using Task = System.Threading.Tasks.Task;

namespace Application.Sheets
{
    public class ProductCategoriesSheet : ISheet, ISaveable
    {
        private int StartRowNewItems;

        public ProductCategoriesSheet(Program program)
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

        public ISession Session { get; }

        public IWorksheet Sheet { get; }

        public Binder Binder { get; set; }

        public Controls Controls { get; }

        public IMessageService MessageService { get; set; }

        public IErrorService ErrorService { get; }

        private bool IsWorksheetUpToDate { get; set; }

        public async Task Init()
        {
            var pull =
                new Pull
                {
                    Extent = new Filter(M.ProductCategory)
                    {
                        Predicate = new Not(new Exists(M.ProductCategory.Children))
                    },
                    Results = new[]
                    {
                        new Result
                        {
                            Select = new Select
                            {Include = new []
                                {
                                    new Node(M.ProductCategory.PrimaryParent, new[]
                                    {
                                        new Node(M.ProductCategory.PrimaryParent),
                                    }),
                                    new Node(M.ProductCategory.IataGseCode),
                                }
                            }
                        }
                    }
                };

            var result = await this.Session.PullAsync(pull);

            var productCategories = result.GetCollection<ProductCategory>("ProductCategories") ?? Array.Empty<ProductCategory>();

            var index = 0;
            var categoryAndGroupName = new Column { Header = "Group & Category", Index = index++, NumberFormat = "@" };
            var familyCode = new Column { Header = "Family Code", Index = index++, NumberFormat = "@" };
            var familyName = new Column { Header = "Family Name", Index = index++, NumberFormat = "@" };
            var groupCode = new Column { Header = "Group Code", Index = index++, NumberFormat = "@" };
            var groupName = new Column { Header = "Group Name", Index = index++, NumberFormat = "@" };
            var categoryCode = new Column { Header = "Category Code", Index = index++, NumberFormat = "@" };
            var categoryName = new Column { Header = "Category Name", Index = index++, NumberFormat = "@" };
            var motorised = new Column { Header = "Motorised", Index = index++, NumberFormat = "@" };
            var iataTerminology = new Column { Header = "Iata Terminology", Index = index++, NumberFormat = "@" };
            var iataNomenclature = new Column { Header = "Iata Nomenclature", Index = index++, NumberFormat = "@" };

            var columns = new[]
            {
                categoryAndGroupName,
                categoryAndGroupName,
                familyCode,
                familyName,
                groupCode,
                groupName,
                categoryCode,
                categoryName,
                motorised,
                iataTerminology,
                iataNomenclature
            };

            foreach (var column in columns)
            {
                this.Sheet[0, column.Index].Value = column.Header;
            }

            var row = 1;
            foreach (var category in productCategories)
            {
                foreach (var column in columns)
                {
                    this.Sheet[row, column.Index].NumberFormat = column.NumberFormat;
                    this.Sheet[row, column.Index].Style = new Style(Color.Aqua, Color.BurlyWood);
                }

                this.Controls.Static(row, categoryAndGroupName.Index, category.ExistPrimaryParent ? string.Concat(category.PrimaryParent.Name, '/', category.Name) : category.Name);
                this.Controls.Label(row, categoryCode.Index, category, M.ProductCategory.Code);
                this.Controls.Label(row, categoryName.Index, category, M.ProductCategory.Name);

                if (category.IsSubGroup)
                {
                    this.Controls.Label(row, familyCode.Index, category.PrimaryParent.PrimaryParent, M.ProductCategory.Code);
                    this.Controls.Label(row, familyName.Index, category.PrimaryParent.PrimaryParent, M.ProductCategory.Name);
                }
                else if (category.IsGroup)
                {
                    this.Controls.Label(row, familyCode.Index, category.PrimaryParent, M.ProductCategory.Code);
                    this.Controls.Label(row, familyName.Index, category.PrimaryParent, M.ProductCategory.Name);
                }

                if (category.IsSubGroup)
                {
                    this.Controls.Label(row, groupCode.Index, category.PrimaryParent, M.ProductCategory.Code);
                    this.Controls.Label(row, groupName.Index, category.PrimaryParent, M.ProductCategory.Name);
                }

                this.Controls.Label(row, motorised.Index, category, M.ProductCategory.Motorised);
                this.Controls.Static(row, iataTerminology.Index, category.IataGseCode?.Code);
                this.Controls.Static(row, iataNomenclature.Index, category.IataGseCode?.Name);

                row++;
            }

            this.Controls.Bind();

            await this.Sheet.Flush().ConfigureAwait(false);
        }

        public async Task Update()
        {
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

        private int GetWeekNumber(DateTime date)
        {
            CultureInfo ciCurr = CultureInfo.CurrentCulture;
            int weekNum = ciCurr.Calendar.GetWeekOfYear(date, CalendarWeekRule.FirstFourDayWeek, System.DayOfWeek.Monday);
            return weekNum;
        }
    }
}
