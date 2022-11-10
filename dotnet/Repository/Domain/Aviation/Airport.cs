namespace Allors.Repository
{
    using System;
    using Attributes;
    using static Workspaces;
    #region Allors

    [Id("B521D33E-847C-418A-B043-6FC1888DC67F")]

    #endregion

    [Workspace(Default)]
    public partial class Airport : GeoLocatable
    {
        #region inherited properties

        public SecurityToken[] SecurityTokens { get; set; }

        public Guid UniqueId { get; set; }

        public double Latitude { get; set; }

        public double Longitude { get; set; }

        public Revocation[] Revocations { get; set; }

        #endregion

        #region Allors
        [Id("58FD2D54-ADD2-483B-85C6-674797F7FCA8")]


        [Workspace(Default)]
        #endregion
        public string Name { get; set; }

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