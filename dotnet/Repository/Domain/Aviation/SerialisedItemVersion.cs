namespace Allors.Repository
{
    using Allors.Repository.Attributes;
    using static Workspaces;

    [Workspace(Default)]
    public partial class SerialisedVersion
    {
        #region Allors
        [Id("6babbcd1-5d10-44b4-b409-718fead11941")]
        
        
        #endregion
        [Workspace(Default)]
        public decimal EstimatedRefurbishCost { get; set; }

        #region Allors
        [Id("a96287ce-8a36-4660-ae7a-d03c0d9e0da2")]
        
        
        #endregion
        [Workspace(Default)]
        public decimal ActualRefurbishCost { get; set; }

        #region Allors
        [Id("938a89e8-b7e5-4cd5-87d4-73df530513bc")]
        
        
        #endregion
        [Workspace(Default)]
        public decimal EstimatedTransportCost { get; set; }

        #region Allors
        [Id("e672b27b-8d37-4648-b544-fc9bae4122e6")]
        
        
        #endregion
        [Workspace(Default)]
        public decimal ActualTransportCost { get; set; }

        #region Allors
        [Id("fd1af436-d84e-49a7-b08b-0cbcf0ca5346")]
        
        
        #endregion
        [Workspace(Default)]
        public decimal ExpectedRentalPriceFullService { get; set; }

        #region Allors
        [Id("F767f8064-1b3e-4302-bfa6-8623c2fcceee")]
        
        
        #endregion
        [Workspace(Default)]
        public decimal ExpectedRentalPriceDryLease { get; set; }

        #region Allors
        [Id("1bde9112-c617-4cdb-8c2a-c25d660db4fe")]
        
        
        #endregion
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        public decimal SellingPrice { get; set; }
    }
}