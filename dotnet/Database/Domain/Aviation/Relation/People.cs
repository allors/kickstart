namespace Allors.Database.Domain
{
    using System.Linq;

    public partial class People
    {
        protected override void AviationPrepare(Setup setup)
        {
            setup.AddDependency(this.Meta, M.UserGroup);
        }

        protected override void AviationSetup(Setup setup)
        {
            var internalOrganisations = new Organisations(this.Transaction).Extent().Where(v => Equals(v.IsInternalOrganisation, true));

            if (new People(this.Transaction).FindBy(M.Person.UserName, "martien@dipu.com") == null)
            {
                var userEmail = new EmailAddressBuilder(this.Transaction).WithElectronicAddressString("martien@dipu.com").Build();

                var martien = new PersonBuilder(this.Transaction)
                    .WithUserName(userEmail.ElectronicAddressString)
                    .WithFirstName("Martien")
                    .WithMiddleName("van")
                    .WithLastName("Knippenberg")
                    .WithUserEmail(userEmail.ElectronicAddressString)
                    .WithUserEmailConfirmed(true)
                    .WithEmailFrequency(new EmailFrequencies(this.Transaction).Immediate)
                    .Build();

                martien.SetPassword("l3tm31n!");

                new PartyContactMechanismBuilder(this.Transaction)
                    .WithParty(martien)
                    .WithContactMechanism(userEmail)
                    .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).PersonalEmailAddress)
                    .WithUseAsDefault(true)
                    .Build();

                new UserGroups(this.Transaction).Administrators.AddMember(martien);
                new UserGroups(this.Transaction).Creators.AddMember(martien);
            }

            if (new People(this.Transaction).FindBy(M.Person.UserName, "koen@dipu.com") == null)
            {
                var userEmail = new EmailAddressBuilder(this.Transaction).WithElectronicAddressString("koen@dipu.com").Build();

                var koen = new PersonBuilder(this.Transaction)
                    .WithUserName(userEmail.ElectronicAddressString)
                    .WithFirstName("Koen")
                    .WithLastName("Van Exem")
                    .WithUserEmail(userEmail.ElectronicAddressString)
                    .WithUserEmailConfirmed(true)
                    .WithEmailFrequency(new EmailFrequencies(this.Transaction).Immediate)
                    .Build();

                koen.SetPassword("l3tm31n!");

                new PartyContactMechanismBuilder(this.Transaction)
                    .WithParty(koen)
                    .WithContactMechanism(userEmail)
                    .WithContactPurpose(new ContactMechanismPurposes(this.Transaction).PersonalEmailAddress)
                    .WithUseAsDefault(true)
                    .Build();

                new UserGroups(this.Transaction).Administrators.AddMember(koen);
                new UserGroups(this.Transaction).Creators.AddMember(koen);
            }
        }
    }
}
