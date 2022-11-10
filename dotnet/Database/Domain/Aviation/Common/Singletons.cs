using Allors.Database.Meta;

namespace Allors.Database.Domain
{
    public partial class Singletons
    {
        protected override void AviationSetup(Setup setup)
        {
            var singleton = this.Instance;
            singleton.DefaultLocale = new Locales(this.Transaction).EnglishGreatBritain;
            singleton.AddAdditionalLocale(new Locales(this.Transaction).DutchNetherlands);

            singleton.NonUnifiedPartBarcodePrint = new NonUnifiedPartBarcodePrintBuilder(this.Transaction).Build();
        }
    }
}
