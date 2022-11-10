using System;

namespace Allors.Database.Domain
{
    public partial class Organisations
    {
        protected override void AviationPrepare(Setup setup)
        {
            setup.AddDependency(this.Meta, this.M.Role);
            setup.AddDependency(this.Meta, this.M.Grant);
            setup.AddDependency(this.Meta, this.M.UserGroup);
            setup.AddDependency(this.Meta, this.M.SecurityToken);
        }
    }
}
