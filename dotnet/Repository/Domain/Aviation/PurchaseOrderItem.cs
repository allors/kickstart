namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;

    public partial class PurchaseOrderItem
    {
        #region Allors
        [Id("d702434b-144e-4281-ba8d-69362c428f2d")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public WorkTask WorkTask{ get; set; }
    }
}