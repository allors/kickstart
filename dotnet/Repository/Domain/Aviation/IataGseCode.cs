namespace Allors.Repository
{
    using System;
    using Attributes;
    using static Workspaces;

    #region Allors

    [Id("1EE0FCE4-4D7E-4B86-BEB8-88DC3CED3F7C")]

    #endregion
    [Workspace(Default)]
    public partial class IataGseCode : Enumeration, IDisplayName
    {
        #region inherited properties

        public SecurityToken[] SecurityTokens { get; set; }

        public Guid UniqueId { get; set; }

        public string Name { get; set; }
        public LocalisedText[] LocalisedNames { get; set; }
        public bool IsActive { get; set; }

        public Revocation[] Revocations { get; set; }
        public string DisplayName { get; set; }

        #endregion

        #region Allors
        [Id("3DC22C17-2AEF-4E5C-8109-ED489EB876D0")]


        [Indexed]
        #endregion
        [Required]
        [Workspace(Default)]
        public string Code { get; set; }

        #region Allors
        [Id("3A1AD367-44EE-420D-8189-C189A5CF00A6")]


        [Indexed]
        #endregion
        [Required]
        [Workspace(Default)]
        public string SwissportDescription { get; set; }

        #region inherited methods

        public void OnBuild()
        {
        }

        public void OnPostBuild()
        {

        }

        public void OnInit()
        {

        }

        public void OnPostDerive()
        {

        }
        #endregion
    }
}