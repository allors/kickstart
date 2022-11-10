namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;

    [Workspace(Default)]
    public partial class PurchaseInvoice
    {
        #region Allors
        [Id("8dcb1795-92f6-4815-b78b-b70a63df2d2f")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        [Derived]
        public decimal AmountSundries { get; set; }
    }
}