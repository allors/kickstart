namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;

    public partial class SalesInvoice
    {
        #region Allors
        [Id("be191fa6-9908-4973-b5ef-950ba0116232")]
        [Indexed]
        #endregion
        [Required]
        [Workspace(Default)]
        public bool PrintCondensed{ get; set; }
    }
}