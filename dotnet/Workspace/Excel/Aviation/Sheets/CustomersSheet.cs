using System;
using System.Drawing;
using System.Linq;
using Allors.Excel;
using Allors.Workspace;
using Allors.Workspace.Data;
using Allors.Workspace.Meta;
using Organisation = Allors.Workspace.Domain.Organisation;
using PostalAddress = Allors.Workspace.Domain.PostalAddress;
using Task = System.Threading.Tasks.Task;

namespace Application.Sheets
{
    public class CustomersSheet : ISheet, ISaveable
    {
        public CustomersSheet(Program program)
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
                Extent = new Filter(this.M.Organisation),

                Results = new[]
                {
                    new Result
                    {
                        Select = new Select
                        {
                            Include = new []
                            {
                                new Node(M.Organisation.PartyContactMechanismsWhereParty, new[]
                                {
                                    new Node(M.PartyContactMechanism.ContactPurposes),
                                    new Node(M.PartyContactMechanism.ContactMechanism, new[]
                                    {
                                        new Node(M.PostalAddress.Country),
                                    }),
                                }),
                                new Node(M.Organisation.CurrentOrganisationContactRelationships, new[]
                                {
                                    new Node(M.OrganisationContactRelationship.Organisation),
                                    new Node(M.OrganisationContactRelationship.Contact, new[]
                                    {
                                        new Node(M.Person.Salutation),
                                        new Node(M.Person.GeneralCorrespondence, new[]
                                        {
                                            new Node(M.ContactMechanism.ContactMechanismType),
                                        }),
                                    }),
                                }),
                            }
                        },
                    },
                },
            };

            var result = await this.Session.PullAsync(pull);
            this.Session.Reset();

            var customers = result.GetCollection<Organisation>("Organisations") ?? Array.Empty<Organisation>();

            var index = 0;
            var companyName = new Column { Header = "Company Name", Index = index++, NumberFormat = "@" };
            var address = new Column { Header = "Address", Index = index++, NumberFormat = "@" };
            var locality = new Column { Header = "Plaats", Index = index++, NumberFormat = "@" };
            var country = new Column { Header = "Country", Index = index++, NumberFormat = "@" };
            var postCode = new Column { Header = "PostCode", Index = index++, NumberFormat = "@" };
            var contact = new Column { Header = "Contact", Index = index++, NumberFormat = "@" };
            var vat = new Column { Header = "Customer VAT", Index = index++, NumberFormat = "@" };

            var columns = new[]
            {
                companyName,
                address,
                locality,
                country,
                postCode,
                contact,
                vat,
            };

            foreach (var column in columns)
            {
                this.Sheet[0, column.Index].Value = column.Header;
                this.Sheet[0, column.Index].Style = new Style(Color.LightBlue, Color.Black);
            }

            var row = 1;
            foreach (var customer in customers)
            {
                foreach (var column in columns)
                {
                    this.Sheet[row, column.Index].NumberFormat = column.NumberFormat;
                    this.Sheet[row, column.Index].Style = new Style(Color.Aqua, Color.BurlyWood);
                }

                this.Binder.Set(row, companyName.Index, new RoleTypeBinding(customer, M.Organisation.Name));
                // this.Sheet[row, companyName.Index].Value = customer.Name;

                var contactMechanism = customer.PartyContactMechanismsWhereParty.FirstOrDefault(v => v.ContactPurposes.Any(w => string.Equals(w.Name, "General correspondence address", StringComparison.OrdinalIgnoreCase)))?.ContactMechanism;

                if (contactMechanism is PostalAddress postalAddress)
                {
                    this.Binder.Set(row, address.Index, new RoleTypeBinding(postalAddress, M.PostalAddress.Address1));
                    this.Binder.Set(row, locality.Index, new RoleTypeBinding(postalAddress, M.PostalAddress.Locality));

                    this.Binder.Set(row, country.Index, new RoleTypeBinding(postalAddress.Country, M.Country.Name, oneWayBinding: true));
                    this.Binder.Set(row, postCode.Index, new RoleTypeBinding(postalAddress, M.PostalAddress.PostalCode));
                }

                var contacts = customer.CurrentOrganisationContactRelationships.Select(v => v.Contact);
                this.Sheet[row, contact.Index].Value = string.Join("\n", contacts.Select(v => $"{v?.Salutation?.Name} {v?.DisplayName}"));

                this.Binder.Set(row, vat.Index, new RoleTypeBinding(customer, M.Organisation.TaxNumber));

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
