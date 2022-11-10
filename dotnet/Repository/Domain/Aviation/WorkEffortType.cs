namespace Allors.Repository
{
    using Allors.Repository.Attributes;
    using static Workspaces;

    [Workspace(Default)]
    public partial class WorkEffortType : Object, IDisplayName
    {
        #region inherited properties

        public string DisplayName { get; set; }

        #endregion

        #region Allors
        [Id("77a29645-afea-4d52-96f7-8d81ab402e09")]
        
        
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public UnifiedGood UnifiedGood { get; set; }

        #region Allors
        [Id("7bc25c17-a681-451c-a73b-316531182975")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Indexed]
        [Workspace(Default)]
        public ProductCategory ProductCategory { get; set; }

        #region Allors
        [Id("8f3d2612-e14e-4c29-a557-73c7d71e4c12")]
        
        
        #endregion
        [Derived]
        [Workspace(Default)]
        public string UnifiedGoodDisplayName { get; set; }

        #region Allors
        [Id("a8736027-da55-4307-bbe6-d9f141b8a9ef")]
        #endregion
        [Derived]
        [Workspace(Default)]
        public string ProductCategoryDisplayName { get; set; }
    }
}
