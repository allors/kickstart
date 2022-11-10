using System;
using System.Collections.Concurrent;
using System.Linq;
using Allors.Workspace;
using Allors.Workspace.Data;
using Allors.Workspace.Meta;
using Application.Sheets;
using Person = Allors.Workspace.Domain.Person;
using UserGroup = Allors.Workspace.Domain.UserGroup;

namespace Application
{
    using System.Collections.Generic;
    using System.Threading.Tasks;
    using Allors.Excel;
    using WindowsForms;

    public class Program : IProgram
    {
        public Program(IWorkspace workspace)
        {
            this.Workspace = workspace;
            this.M = this.Workspace.Services.Get<M>();

            this.Roles = new Roles();

            this.Workbooks = new List<IWorkbook>();
            this.Worksheets = new List<IWorksheet>();
            this.SheetByWorksheet = new ConcurrentDictionary<IWorksheet, ISheet>();

            this.Logger = workspace.Services.Get<ILoggerService>();
        }

        public ILoggerService Logger { get; set; }

        public IWorkspace Workspace { get; }

        public M M { get; }

        public Roles Roles { get; private set; }

        public IAddIn AddIn { get; private set; }

        public IList<IWorkbook> Workbooks { get; }

        public IList<IWorksheet> Worksheets { get; }

        public IWorkbook ActiveWorkbook => this.AddIn.Workbooks.FirstOrDefault(v => v.IsActive);

        public IWorksheet ActiveWorksheet => this.ActiveWorkbook?.Worksheets.FirstOrDefault(v => v.IsActive);

        public IDictionary<IWorksheet, ISheet> SheetByWorksheet;

        public bool IsLoggedIn => this.Workspace.Services.Get<IUserIdService>().IsLoggedIn;

        public async Task OnHandle(string controlId, params object[] argument)
        {
            using (new UIBlocker())
            {
                switch (controlId)
                {
                    case Actions.Save:
                        await this.OnSave();
                        break;
                    case Actions.Refresh:
                        await this.OnRefresh();
                        break;
                    case Actions.Customers:
                        var customersSheet = new CustomersSheet(this);
                        this.SheetByWorksheet.Add(customersSheet.Sheet, customersSheet);
                        await customersSheet.Load();
                        break;
                    case Actions.SalesInvoices:
                        var salesInvoicesSheet = new SalesInvoicesSheet(this);
                        this.SheetByWorksheet.Add(salesInvoicesSheet.Sheet, salesInvoicesSheet);
                        await salesInvoicesSheet.Load();
                        break;
                    case Actions.ProductCategories:
                        var productCategoriesSheet = new ProductCategoriesSheet(this);
                        this.SheetByWorksheet.Add(productCategoriesSheet.Sheet, productCategoriesSheet);
                        await productCategoriesSheet.Init();
                        break;
                    case Actions.PurchaseInvoices:
                        var purchaseInvoicesSheet = new PurchaseInvoicesSheet(this);
                        this.SheetByWorksheet.Add(purchaseInvoicesSheet.Sheet, purchaseInvoicesSheet);
                        await purchaseInvoicesSheet.Load();
                        break;
                    case Actions.SerialisedItems:
                        var serialisedItemsSheet = new SerialisedItemsSheet(this);
                        this.SheetByWorksheet.Add(serialisedItemsSheet.Sheet, serialisedItemsSheet);
                        await serialisedItemsSheet.Init();
                        break;
                    case Actions.Rentals:
                        var rentalsSheet = new RentalsSheet(this);
                        this.SheetByWorksheet.Add(rentalsSheet.Sheet, rentalsSheet);
                        await rentalsSheet.Load();
                        break;
                    case Actions.SpareParts:
                        var sparePartsSheet = new SparePartsSheet(this);
                        this.SheetByWorksheet.Add(sparePartsSheet.Sheet, sparePartsSheet);
                        await sparePartsSheet.Load();
                        break;
                    case Actions.InventoryTransactions:
                        var inventoryTransactionsSheet = new InventoryTransactionsSheet(this);
                        this.SheetByWorksheet.Add(inventoryTransactionsSheet.Sheet, inventoryTransactionsSheet);
                        await inventoryTransactionsSheet.Load();
                        break;
                    case Actions.WorkOrders:
                        var workOrdersSheet = new WorkOrdersSheet(this);
                        this.SheetByWorksheet.Add(workOrdersSheet.Sheet, workOrdersSheet);
                        await workOrdersSheet.Init();
                        break;
                    case Actions.TimeEntries:
                        var timeEntriesSheet = new TimeEntriesSheet(this);
                        this.SheetByWorksheet.Add(timeEntriesSheet.Sheet, timeEntriesSheet);
                        await timeEntriesSheet.Init();
                        break;
                }
            }
        }

        public async Task OnLogin()
        {
            var session = this.Workspace.CreateSession();

            var userIdService = this.Workspace.Services.Get<IUserIdService>().UserId;

            var pulls = new[]
            {
                new Pull
                {
                    ObjectId = userIdService,
                    Results = new[]
                    {
                        new Result
                        {
                            Select= new Select
                            {
                                Include = this.M.Person.Nodes(v => new []
                                {
                                    v.UserGroupsWhereMember.Node(),
                                    v.InternalOrganisationsWhereLocalAdministrator.Node()
                                })
                            }
                        }
                    }
                },
            };

            var result = await session.PullAsync(pulls);

            var person = result.GetObject<Person>();
            var groups = person?.UserGroupsWhereMember;

            this.Roles = new Roles
            {
                IsAdministrator = groups?.Any(v => v.UniqueId == Roles.AdministratorsId) == true,
                IsSalesAccountManager = groups?.Any(v => v.UniqueId == Roles.SalesAccountManagersId) == true,
                IsLocalAdministrator = person?.InternalOrganisationsWhereLocalAdministrator.Any() == true,
            };

            var ribbonService = this.Workspace.Services.Get<IRibbonService>();
            ribbonService.UserLabel = person?.FirstName;
            ribbonService.AuthenticationLabel = "Logoff";
            ribbonService.Invalidate();
        }

        public async Task OnLogout()
        {
            this.Roles = new Roles();

            var ribbonService = this.Workspace.Services.Get<IRibbonService>();

            ribbonService.UserLabel = "Not logged in";
            ribbonService.AuthenticationLabel = "Login";
            ribbonService.Invalidate();
        }

        public bool IsEnabled(string controlId, string controlTag)
        {
            if (this.ActiveWorksheet == null)
            {
                return false;
            }

            var isLoggedIn = this.IsLoggedIn;

            switch (controlId)
            {
                case "save":
                case "refresh":
                    return isLoggedIn;

                case "customers":
                    return isLoggedIn && (this.Roles.IsAdministrator);

                case "salesInvoices":
                case "rentals":
                    return isLoggedIn && (this.Roles.IsAdministrator || this.Roles.IsSalesAccountManager);

                case "spareParts":
                case "workOrders":
                case "timeEntries":
                case "inventoryTransactions":
                    return isLoggedIn && (this.Roles.IsAdministrator || this.Roles.IsLocalAdministrator);

                case "purchaseInvoices":
                case "inventoryItems":
                case "serialisedItems":
                case "productCategories":
                    return isLoggedIn && (this.Roles.IsAdministrator);

                default:
                    throw new Exception($"Unhandled control with id {controlId}");

            }
        }

        public async Task OnSave()
        {
            using (new UIBlocker())
            {
                var activeWorksheet = this.ActiveWorksheet;

                if (activeWorksheet != null)
                {
                    if (this.SheetByWorksheet.TryGetValue(activeWorksheet, out var sheet))
                    {
                        if (sheet is ISaveable saveable)
                        {
                            await saveable.Save();
                        }
                    }
                }
            }
        }

        public async Task OnRefresh()
        {
            using (new UIBlocker())
            {
                var activeWorksheet = this.ActiveWorksheet;

                if (activeWorksheet != null)
                {
                    if (this.SheetByWorksheet.TryGetValue(activeWorksheet, out var sheet))
                    {
                        if (sheet is ISaveable saveable)
                        {
                            await saveable.Refresh();
                        }
                    }
                }
            }
        }

        #region Application
        public async Task OnStart(IAddIn addIn)
        {
            this.AddIn = addIn;

            this.Logger.Info("Started");
        }

        public async Task OnStop()
        {
            this.Logger.Info("Stopped");
        }
        #endregion

        #region Workbook
        public async Task OnNew(IWorkbook workbook)
        {
            this.Workbooks.Add(workbook);

            var ribbonService = this.Workspace.Services.Get<IRibbonService>();
            ribbonService.Invalidate();
        }

        public void OnClose(IWorkbook workbook, ref bool cancel)
        {
            this.Workbooks.Remove(workbook);

            var ribbonService = this.Workspace.Services.Get<IRibbonService>();
            ribbonService.Invalidate();
        }
        #endregion

        #region Worksheet
        public async Task OnNew(IWorksheet worksheet)
        {
            this.Worksheets.Add(worksheet);

            var ribbonService = this.Workspace.Services.Get<IRibbonService>();
            ribbonService.Invalidate();
        }

        public async Task OnBeforeDelete(IWorksheet worksheet)
        {
            this.SheetByWorksheet.Remove(worksheet);
            this.Worksheets.Remove(worksheet);

            var ribbonService = this.Workspace.Services.Get<IRibbonService>();
            ribbonService.Invalidate();
        }

        #endregion
    }
}