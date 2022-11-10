namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;

    [Workspace(Default)]
    public partial class NonSerialisedInventoryItem
    {
        #region Allors
        [Id("82178056-8594-4eff-8527-810810db86f2")]
        
        
        [Indexed]
        #endregion
        [Derived]
        [Origin(Origin.Session)]
        [Workspace(Default)]
        public string SpanishPartDisplayName { get; set; }

        #region Allors
        [Id("bfb13f6c-e7ab-4fdb-85ed-d99d3d2d5c93")]
        
        
        [Indexed]
        #endregion
        [Derived]
        [Origin(Origin.Session)]
        [Workspace(Default)]
        public string DutchPartDisplayName { get; set; }
    }
}