// <copyright file="ProductQuote.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using System.Linq;

namespace Allors.Database.Domain
{
    public partial class ProductQuote
    {
        public bool IsSparePartsQuote =>
            this.QuoteItems.Any(v => (v.ExistProduct && v.Product.GetType().Name.Equals(typeof(NonUnifiedPart).Name)) 
                                    || v.ExistSparePartDescription);

        public void AviationPrint(PrintablePrint method)
        {
            var singleton = this.Strategy.Transaction.GetSingleton();
            var logo = this.Issuer?.ExistLogoImage == true ?
                            this.Issuer.LogoImage.MediaContent.Data :
                            singleton.LogoImage.MediaContent.Data;

            var images = new Dictionary<string, byte[]>
                                {
                                    { "Logo1", logo },
                                    { "Logo2", logo },
                                };

            if (this.ExistQuoteNumber)
            {
                var transaction = this.Strategy.Transaction;
                var barcodeGenerator = transaction.Database.Services.Get<IBarcodeGenerator>();
                var barcode = barcodeGenerator.Generate(this.QuoteNumber, BarcodeType.CODE_128, 320, 80, pure: true);
                images.Add("Barcode", barcode);
            }

            if (this.IsSparePartsQuote)
            {
                var printModel = new Print.PartQuoteModel.Model(this, images);
                this.RenderPrintDocument(this.Issuer?.PartQuoteTemplate, printModel, images);
            }
            else
            {
                var printModel = new Print.ProductQuoteModel.Model(this, images);
                this.RenderPrintDocument(this.Issuer?.ProductQuoteTemplate, printModel, images);
            }

            this.PrintDocument.Media.InFileName = $"{this.QuoteNumber}.odt";

            method.StopPropagation = true;
        }
    }
}
