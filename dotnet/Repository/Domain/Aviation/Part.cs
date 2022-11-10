namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;

    public partial interface Part
    {
        #region Allors
        [Id("3f4fe327-5b9c-423a-a146-e4304ea62410")]
        
        
        [Indexed]
        #endregion
        [Workspace(Default)]
        string SpanishName { get; set; }

        #region Allors
        [Id("b40746b3-b50f-4c73-9bec-c8b48dbb369f")]
        
        
        [Indexed]
        #endregion
        [Workspace(Default)]
        string SpanishUOM { get; set; }

        #region Allors
        [Id("f597f7f8-7eff-4d65-91ac-2165909715fc")]
        
        
        [Indexed]
        #endregion
        [Workspace(Default)]
        string DutchName { get; set; }

        #region Allors
        [Id("ebdb5731-19b9-484a-98c3-e8e5845be174")]
        
        
        [Indexed]
        #endregion
        [Workspace(Default)]
        string DutchUOM { get; set; }
    }
}