namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;

    [Workspace(Default)]
    public partial class SerialisedItemCharacteristicType : ExternalWithPrimaryKey
    {
        #region inherited properties
        public string ExternalPrimaryKey { get; set; }
        
        #endregion

        #region inherited methods
        #endregion
    }
}