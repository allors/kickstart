namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;

    [Workspace(Default)]
    public partial class ProductCategory : ExternalWithPrimaryKey
    {
        #region inherited properties
        public string ExternalPrimaryKey { get; set; }

        #endregion

        #region Allors
        [Id("3c4d2d3c-32a4-43ab-a7b2-404d475b2595")]
        
        
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public IataGseCode IataGseCode { get; set; }

        #region Allors
        [Id("AB25D561-66E4-4B18-BCD0-D9305511BD4A")]
        
        
        [Indexed]
        #endregion
        [Required]
        [Workspace(Default)]
        public bool Motorised{ get; set; }

        #region Allors
        [Id("79b7dc54-b2e8-4699-a816-a23fb46f1872")]
        
        
        [Indexed]
        #endregion
        [Required]
        [Derived]
        [Workspace(Default)]
        public bool IsFamily { get; set; }

        #region Allors
        [Id("d646d78f-502a-4fdf-8c65-8b07f9ba89eb")]
        
        
        [Indexed]
        #endregion
        [Required]
        [Derived]
        [Workspace(Default)]
        public bool IsGroup { get; set; }

        #region Allors
        [Id("086c7856-a176-4144-87a5-b85f629c9037")]
        
        
        [Indexed]
        #endregion
        [Required]
        [Derived]
        [Workspace(Default)]
        public bool IsSubGroup { get; set; }

        #region Allors
        [Id("908273e9-8329-437b-b8b7-fab559c38546")]
        
        
        #endregion
        [Derived]
        [Workspace(Default)]
        public string FamilyName { get; set; }

        #region Allors
        [Id("b2190d53-ffe2-4153-8728-c815777adc03")]
        
        
        #endregion
        [Derived]
        [Workspace(Default)]
        public string GroupName { get; set; }

        #region Allors
        [Id("827c2d8d-44b2-440c-9630-7a9e68fa42c0")]
        
        
        #endregion
        [Derived]
        [Workspace(Default)]
        public string IataCode { get; set; }

        #region inherited methods

        #endregion
    }
}