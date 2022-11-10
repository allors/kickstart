﻿using System;
using System.Drawing;
using System.Linq;
using Allors.Excel;
using Allors.Workspace;
using Allors.Workspace.Data;
using Allors.Workspace.Meta;
using Task = System.Threading.Tasks.Task;

namespace Application.Sheets
{
    public class MediasSheet : ISheet, ISaveable
    {
        public MediasSheet(Program program)
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

            var medias = result.GetCollection<Allors.Workspace.Domain.Media>() ?? Array.Empty<Allors.Workspace.Domain.Media>();

            var index = 0;
            var name = new Column { Header = "Name", Index = index++, NumberFormat = "@" };
            var fileName = new Column { Header = "File Name", Index = index++, NumberFormat = "@" };

            var columns = new[]
            {
                name,
                fileName,
            };

            foreach (var column in columns)
            {
                this.Sheet[0, column.Index].Value = column.Header;
                this.Sheet[0, column.Index].Style = new Style(Color.LightBlue, Color.Black);
            }

            var row = 1;
            foreach (var customer in medias)
            {
                foreach (var column in columns)
                {
                    this.Sheet[row, column.Index].NumberFormat = column.NumberFormat;
                    this.Sheet[row, column.Index].Style = new Style(Color.Aqua, Color.BurlyWood);
                }

                this.Binder.Set(row, name.Index, new RoleTypeBinding(customer, M.Media.Name));
                this.Binder.Set(row, fileName.Index, new RoleTypeBinding(customer, M.Media.FileName));

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
