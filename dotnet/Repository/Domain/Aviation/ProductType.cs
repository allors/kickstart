namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;

    [Workspace(Default)]
    public partial class ProductType : ExternalWithPrimaryKey
    {
        #region inherited properties
        public string ExternalPrimaryKey { get; set; }

        #endregion

        #region Allors
        [Id("023f2e3e-b7f1-4ef1-971b-399b10675046")]
        #endregion
        [Workspace(Default)]
        public decimal AssumedMonthlyOperatingHours { get; set; }

        #region inherited methods
        #endregion
    }
}