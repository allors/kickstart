namespace Allors.Repository
{
    using Allors.Repository.Attributes;
    using static Workspaces;

    [Workspace(Default)]
    public partial class UnifiedGood
    {
        #region Allors
        [Id("DD3B9393-5CD0-49F2-B793-4E30985636BE")]
        
        
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Derived]
        [Workspace(Default)]
        public IataGseCode IataGseCode { get; set; }

        #region Allors
        [Id("d60dfc9d-5ee2-407a-9618-d0192eda29ab")]
        #endregion
        [Workspace(Default)]
        public decimal AssignedAssumedMonthlyOperatingHours { get; set; }

        #region Allors
        [Id("205d5f32-611e-4087-8c93-9e78d061c050")]
        #endregion
        [Derived]
        [Workspace(Default)]
        public decimal DerivedAssumedMonthlyOperatingHours { get; set; }

        #region inherited properties
        public string ExternalPrimaryKey { get; set; }

        public string SpanishName { get; set; }

        public string SpanishUOM { get; set; }

        public string DutchName { get; set; }

        public string DutchUOM { get; set; }

        #endregion

        #region inherited methods

        #endregion
    }
}