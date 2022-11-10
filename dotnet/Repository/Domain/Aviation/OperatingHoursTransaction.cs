namespace Allors.Repository
{
    using System;
    using Attributes;
    using static Workspaces;

    #region Allors

    [Id("05d99d23-7cd5-45a2-a3f6-1b49b53cfc7b")]

    #endregion
    [Workspace(Default)]
    public partial class OperatingHoursTransaction : UniquelyIdentifiable, Auditable
    {
        #region inherited properties

        public SecurityToken[] SecurityTokens { get; set; }

        public Guid UniqueId { get; set; }

        public User CreatedBy { get; set; }

        public User LastModifiedBy { get; set; }

        public DateTime CreationDate { get; set; }

        public DateTime LastModifiedDate { get; set; }

        public Revocation[] Revocations { get; set; }

        #endregion

        #region Allors
        [Id("dcefa22b-bec5-4fe6-ad81-bbc6c454919f")]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Required]
        [Workspace(Default)]
        public SerialisedItem SerialisedItem { get; set; }

        #region Allors
        [Id("19348544-ef18-409e-a63e-72c9ec2cd89b")]
        #endregion
        [Required]
        [Workspace(Default)]
        public DateTime RecordingDate { get; set; }

        #region Allors
        [Id("38264152-655d-4e06-9502-c385576725ce")]
        #endregion
        [Required]
        [Workspace(Default)]
        public decimal Value { get; set; }

        #region Allors
        [Id("63b0fb90-8643-41a5-8a64-343197a9c7e3")]
        #endregion
        [Derived]
        [Workspace(Default)]
        public decimal Delta { get; set; }

        #region Allors
        [Id("f3ea2bd7-b244-4778-8a83-94880e978846")]
        #endregion
        [Derived]
        [Workspace(Default)]
        public int Days { get; set; }

        #region Allors
        [Id("3ce6a684-dfbd-48c2-ab19-834c95acea0b")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Workspace(Default)]
        public OperatingHoursTransaction PreviousTransaction{ get; set; }

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