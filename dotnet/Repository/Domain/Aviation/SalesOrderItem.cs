namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;

    [Workspace(Default)]
    public partial class SalesOrderItem
    {
        #region inherited properties

        #endregion

        #region Allors
        [Id("01745085-0259-44ff-b812-95895de6d448")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public SaleKind SaleKind { get; set; }

        #region Allors
        [Id("93016a2d-1af4-4ec1-a4e8-223eade57d56")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public RentalType RentalType { get; set; }

        #region inherited methods

        #endregion
    }
}