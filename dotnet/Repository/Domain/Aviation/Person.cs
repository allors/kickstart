namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;

    [Workspace(Default)]
    public partial class Person : ExternalWithPrimaryKey
    {
        #region inherited properties
        public string ExternalPrimaryKey { get; set; }
        #endregion

        #region Allors
        [Id("56CB8890-AB35-4BB0-9EC8-A2DBCD4117DB")]
        
        
        [Indexed]
        [Size(256)]
        #endregion
        public string ExternalPersonKey { get; set; }

        #region Allors
        [Id("7b979b78-330d-4823-aa6a-00b1e11a6737")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Workspace(Default)]
        public TimeEntry LastTimeEntry { get; set; }

        #region inherited methods

        #endregion
    }
}