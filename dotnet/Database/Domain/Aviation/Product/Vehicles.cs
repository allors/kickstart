namespace Allors.Database.Domain
{
    public partial class Vehicles
    {
        protected override void AviationPrepare(Security security) => security.AddDependency(this.Meta, this.M.Revocation);


        protected override void AviationSecure(Security config)
        {
            var revocations = new Revocations(this.Transaction);
            var permissions = new Permissions(this.Transaction);

            revocations.VehicleDeleteRevocation.DeniedPermissions = new[]
            {
                permissions.Get(this.Meta, this.Meta.Delete),
            };
        }
    }
}
