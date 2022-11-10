namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;

    [Workspace(Default)]
    public partial class Singleton
    {
        #region Allors
        [Id("6293171D-F8F2-4121-9A4A-19C6007154E3")]
        
        
        #endregion
        [Workspace(Default)]
        [Multiplicity(Multiplicity.OneToOne)]
        public Organisation DefaultInternalOrganisation { get; set; }

        #region Allors
        [Id("66a3ce82-503f-441b-9a91-dfd2cd17db31")]
        #endregion
        [Workspace(Default)]
        public void RepeatingSalesInvoicing() { }

        #region Allors
        [Id("eb3ca454-af07-478e-b3a5-a310d02d4aca")]
        #endregion
        [Workspace(Default)]
        public void RepeatingPurchaseInvoicing() { }
    }
}