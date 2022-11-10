using Allors.Repository.Attributes;

namespace Allors.Repository
{
    using static Workspaces;

    [Workspace(Default)]
    public partial class Brand : ExternalWithPrimaryKey
    {
        #region inherited properties

        public string ExternalPrimaryKey { get; set; }

        #endregion

        #region inherited methods
        #endregion
    }
}