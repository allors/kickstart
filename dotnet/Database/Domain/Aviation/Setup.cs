// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Setup.cs" company="Allors bvba">
//   Copyright 2002-2012 Allors bvba.
//
// Dual Licensed under
//   a) the General Public Licence v3 (GPL)
//   b) the Allors License
//
// The GPL License is included in the file gpl.txt.
// The Allors License is an addendum to your contract.
//
// Allors Applications is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// For more information visit http://www.allors.com/legal
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Linq;
using Allors.Database.Meta;
using Allors.Database.Services;

namespace Allors.Database.Domain
{
    public partial class Setup
    {
        private void AviationOnPrePrepare()
        {
        }

        private void AviationOnPostPrepare()
        {
        }

        private void AviationOnPreSetup()
        {
        }

        private void AviationOnPostSetup(Config config)
        {
            var databaseServices = this.transaction.Database.Services;
            var m = databaseServices.Get<MetaPopulation>();

            databaseServices.Get<IPermissions>().Sync(this.transaction);

            if (this.Config.SetupSecurity)
            {
                // Give Administrators access
                var employeeUserGroup = new UserGroups(this.transaction).Employees;
                foreach (Person administrator in new UserGroups(this.transaction).Administrators.Members)
                {
                    employeeUserGroup.AddMember(administrator);

                    foreach (var @this in new Organisations(this.transaction).Extent().Where(v => v.IsInternalOrganisation))
                    {
                        new EmploymentBuilder(this.transaction).WithEmployee(administrator).WithEmployer(@this).Build();

                        @this.AddProductQuoteApprover(administrator);
                        @this.AddPurchaseOrderApproversLevel1(administrator);
                        @this.AddPurchaseOrderApproversLevel2(administrator);
                        @this.AddPurchaseInvoiceApprover(administrator);
                    }
                }
            }

            var internalOrganisation = new Organisations(this.transaction).Extent().First(v => v.IsInternalOrganisation);

            // Single Catalogue
            var catalogue = new Catalogues(this.transaction).FindBy(m.Catalogue.ExternalPrimaryKey, "Import");

            if (catalogue == null)
            {
                catalogue = new CatalogueBuilder(this.transaction)
                    .WithInternalOrganisation(internalOrganisation)
                    .WithName("Catalogue")
                    .WithCatScope(new Scopes(this.transaction).Public)
                    .WithExternalPrimaryKey("Import")
                    .Build();
            }

            var store = internalOrganisation.StoresWhereInternalOrganisation.First();
            store?.AddCatalogue(catalogue);

            var settings = this.transaction.GetSingleton().Settings;
            settings.DefaultFacility = store.DefaultFacility;
        }
    }
}
