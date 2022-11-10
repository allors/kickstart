namespace Allors.Database.Domain
{
    public partial class WorkEffortTypes
    {
        protected override void AviationPrepare(Security security) => security.AddDependency(this.Meta, this.M.Revocation);


        protected override void AviationSecure(Security config)
        {
            var revocations = new Revocations(this.Transaction);
            var permissions = new Permissions(this.Transaction);

            revocations.WorkEffortTypeDeleteRevocation.DeniedPermissions = new[]
            {
                permissions.Get(this.Meta, this.Meta.Delete),
            };
        }
    }
}
