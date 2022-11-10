namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;

    [Workspace(Default)]
    public partial class NonUnifiedPart : ExternalWithPrimaryKey
    {
        #region inherited properties
        public string ExternalPrimaryKey { get; set; }

        public string SpanishName { get; set; }

        public string SpanishUOM { get; set; }

        public string DutchName { get; set; }

        public string DutchUOM { get; set; }

        #endregion

        #region Allors
        [Id("6b91d098-8350-40b7-a982-5e45769aeec8")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        [Derived]
        public decimal SuggestedExternalSellingPrice { get; set; }

        #region Allors
        [Id("9d229a68-745d-49df-887b-b69eb538535a")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        [Origin(Origin.Session)]
        [Derived]
        public decimal SuggestedInternalSellingPrice { get; set; }

        #region Allors
        [Id("127722e8-98dd-40de-b94b-e763bd562d47")]
        #endregion
        [Workspace(Default)]
        public bool IsSundries{ get; set; }
    }
}