using System;
using System.Collections.Concurrent;
using System.Linq;
using Allors.Workspace;
using Allors.Workspace.Data;
using Allors.Workspace.Meta;
using Application.Sheets;
using Person = Allors.Workspace.Domain.Person;

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
                    case Actions.People:
                        var salesInvoicesSheet = new PeopleSheet(this);
                        this.SheetByWorksheet.Add(salesInvoicesSheet.Sheet, salesInvoicesSheet);
                        await salesInvoicesSheet.Load();
                        break;
                    case Actions.Medias:
                        var customersSheet = new MediasSheet(this);
                        this.SheetByWorksheet.Add(customersSheet.Sheet, customersSheet);
                        await customersSheet.Load();
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

                case "people":
                case "medias":
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