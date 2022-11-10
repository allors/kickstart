namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;

    [Workspace(Default)]
    public partial class CustomerRelationship
    {
        #region Allors
        [Id("3a56be99-4aa9-42e3-ac5b-78fd02b20670")]
        
        
        [Indexed]
        #endregion
        [Derived]
        [Workspace(Default)]
        public string CustomerName{ get; set; }
    }
}