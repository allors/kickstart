using System;
using System.Drawing;
using System.Linq;
using Allors.Excel;
using Allors.Workspace;
using Allors.Workspace.Data;
using Allors.Workspace.Meta;
using Task = System.Threading.Tasks.Task;

namespace Application.Sheets
{
    public class PeopleSheet : ISheet, ISaveable
    {
        public PeopleSheet(Program program)
        {
            this.Sheet = program.ActiveWorkbook.AddWorksheet();
            this.Binder = new Binder(this.Sheet, new Style(Color.DeepSkyBlue, Color.Black));
            this.Binder.ToDomained += Binder_ToDomained;

            this.M = program.M;
            this.MessageService = program.Workspace.Services.Get<IMessageService>();
            this.ErrorService = program.Workspace.Services.Get<IErrorService>();

            this.Session = program.Workspace.CreateSession();
        }

        private async void Binder_ToDomained(object sender, EventArgs e)
        {
            await this.Sheet.Flush();
        }

        public M M { get; set; }

        public ISession Session { get; }

        public IWorksheet Sheet { get; }

        public Binder Binder { get; set; }

        public IMessageService MessageService { get; set; }

        public IErrorService ErrorService { get; }

        public async Task Load()
        {
            var pull = new Pull
            {
                Extent = new Filter(this.M.User),
            };

            var result = await this.Session.PullAsync(pull);
            this.Session.Reset();

            var people = result.GetCollection<Allors.Workspace.Domain.Person>() ?? Array.Empty<Allors.Workspace.Domain.Person>();

            var index = 0;
            var firstName = new Column { Header = "First Name", Index = index++, NumberFormat = "@" };
            var lastName = new Column { Header = "Last Name", Index = index++, NumberFormat = "@" };

            var columns = new[]
            {
                firstName,
                lastName,
            };

            foreach (var column in columns)
            {
                this.Sheet[0, column.Index].Value = column.Header;
                this.Sheet[0, column.Index].Style = new Style(Color.LightBlue, Color.Black);
            }

            var row = 1;
            foreach (var customer in people)
            {
                foreach (var column in columns)
                {
                    this.Sheet[row, column.Index].NumberFormat = column.NumberFormat;
                    this.Sheet[row, column.Index].Style = new Style(Color.Aqua, Color.BurlyWood);
                }

                this.Binder.Set(row, firstName.Index, new RoleTypeBinding(customer, M.Person.FirstName));
                this.Binder.Set(row, lastName.Index, new RoleTypeBinding(customer, M.Person.LastName));

                row++;
            }

            this.Binder.ResetChangedCells();
            var obsoleteCells = this.Binder.ToCells();
            foreach (var obsoleteCell in obsoleteCells)
            {
                obsoleteCell.Clear();
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
