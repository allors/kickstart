using SkiaSharp;
using System.IO;

namespace Allors.Database.Domain
{
    public partial class Singleton
    {
        private static SKBitmap watermark;

        public void AviationOnBuild(ObjectOnBuild method)
        {
            if (!this.ExistNonUnifiedPartBarcodePrintTemplate)
            {
                this.NonUnifiedPartBarcodePrintTemplate =
                    this.CreateOpenDocumentTemplate<Allors.Database.Domain.Print.NonUnifiedPartBarcodePrint.Model>("NonUnifiedPartBarcodePrint.odt", this.GetResourceBytes("Templates.NonUnifiedPartBarcodePrint.odt"));
            }
        }

        public void AviationOnInit(ObjectOnInit method)
        {
            if (!this.ExistLogoImage)
            {
                this.LogoImage = new MediaBuilder(this.strategy.Transaction).WithInFileName("Aviation-Logo.jpg").WithInData(this.GetResourceBytes("Aviation-Logo.jpg")).Build();
            }
        }

        public SKBitmap Watermark
        {
            get
            {
                if (watermark == null)
                {
                    var data = this.GetResourceBytes("Resources.watermark.png");
                    using var memoryStream = new MemoryStream(data);
                    watermark = SKBitmap.Decode(memoryStream);
                }

                return watermark;
            }
        }

        public void AviationRepeatingPurchaseInvoicing(SingletonRepeatingPurchaseInvoicing method)
        {
            RepeatingPurchaseInvoices.Daily(this.Transaction());

            method.StopPropagation = true;
        }

        public void AviationRepeatingSalesInvoicing(SingletonRepeatingSalesInvoicing method)
        {
            RepeatingSalesInvoices.Daily(this.Transaction());

            method.StopPropagation = true;
        }
    }
}
