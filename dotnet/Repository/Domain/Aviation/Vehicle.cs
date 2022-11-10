namespace Allors.Repository
{
    using Allors.Repository.Attributes;
    using static Workspaces;

    [Workspace(Default)]
    public partial class Vehicle
    {
        #region Allors
        [Id("d89564d5-6d8e-4f88-810a-8271db77f354")]
        #endregion
        [Required]
        [Workspace(Default)]
        public string LicensePlateNumber { get; set; }

        #region Allors
        [Id("0c237777-6a53-4c14-ac26-b705cd0e78d6")]
        #endregion
        [Workspace(Default)]
        public string ChassisNumber { get; set; }

        #region Allors
        [Id("f05a937d-eadf-4183-9a79-2d6b75fafffd")]
        #endregion
        [Workspace(Default)]
        public string Mileage { get; set; }

        #region Allors
        [Id("249fd223-7a8e-49c3-9020-9e269622d942")]
        #endregion
        [Indexed]
        [Workspace(Default)]
        public string Make { get; set; }

        #region Allors
        [Id("2969228f-11ea-4ab9-8bea-818afe40726e")]
        #endregion
        [Indexed]
        [Workspace(Default)]
        public string Model { get; set; }

        #region Allors
        [Id("e8e33ba1-be83-45e7-ac26-c1c2683addc5")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public Party OwnedBy { get; set; }

        #region Allors
        [Id("bdb25c94-8a0d-4bfe-8d54-3c5a3e42dc44")]
        #endregion
        [Indexed]
        [Derived]
        [Workspace(Default)]
        public string OwnedByPartyName { get; set; }
    }
}