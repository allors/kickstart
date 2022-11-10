namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;

    [Workspace(Default)]
    public partial class QuoteItem : ExternalWithPrimaryKey
    {
        #region inherited properties
        public string ExternalPrimaryKey { get; set; }

        #endregion

        #region Allors
        [Id("0e803915-7dcb-4dba-8806-a6156f93f796")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public SaleKind SaleKind { get; set; }

        #region Allors
        [Id("4c9200cd-e9e8-4ee5-a02f-e7630a2165d7")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public RentalType RentalType { get; set; }

        /// <summary>
        /// Use when spare part is not known in the application at the time the quote is created. Mutually exclusive with QuoteItem.Product.
        /// Spare part is automatically created when sales order is created.
        /// </summary>
        #region Allors
        [Id("05a3984d-5b8d-4b35-b01c-838883bed4f0")]
        #endregion
        [Workspace(Default)]
        public string SparePartDescription { get; set; }

        #region inherited methods

        #endregion
    }
}