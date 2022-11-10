// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Custom.cs" company="Allors bvba">
//   Copyright 2002-2017 Allors bvba.
// 
// Dual Licensed under
//   a) the General Public Licence v3 (GPL)
//   b) the Allors License
// 
// The GPL License is included in the file gpl.txt.
// The Allors License is an addendum to your contract.
// 
// Allors Applications is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// For more information visit http://www.allors.com/legal
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using Allors.Database.Domain;
using NLog;

namespace Commands
{
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using McMaster.Extensions.CommandLineUtils;

    using Microsoft.Extensions.Configuration;
    using Microsoft.Extensions.DependencyInjection;
    using NLog;

    [Command(Description = "Print Product Quote")]
    public class PrintProductQuote
    {
        public Program Parent { get; set; }

        public Logger Logger => LogManager.GetCurrentClassLogger();

        public int OnExecute(CommandLineApplication app)
        {
            using (var transaction = this.Parent.Database.CreateTransaction())
            {
                this.Logger.Info("Begin");

                transaction.Services.Get<IUserService>().User = new AutomatedAgents(transaction).System;

                var singleton = transaction.GetSingleton();

                var template = singleton.CreateOpenDocumentTemplate<Allors.Database.Domain.Print.ProductQuoteModel.Model>("ProductQuote.odt", singleton.GetResourceBytes("Templates.ProductQuote.odt"));

                transaction.Derive();

                var templateFilePath = "domain/templates/ProductQuote.odt";
                var templateFileInfo = new FileInfo(templateFilePath);
                var prefix = string.Empty;
                while (!templateFileInfo.Exists)
                {
                    prefix += "../";
                    templateFileInfo = new FileInfo(prefix + templateFilePath);
                }

                var quote = new ProductQuotes(transaction).Extent().Last();

                using (var memoryStream = new MemoryStream())
                using (var fileStream = new FileStream(templateFileInfo.FullName, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    fileStream.CopyTo(memoryStream);
                    template.Media.InData = memoryStream.ToArray();
                }

                transaction.Derive();

                var logo = quote.Issuer?.ExistLogoImage == true ?
                               quote.Issuer.LogoImage.MediaContent.Data :
                               transaction.GetSingleton().LogoImage.MediaContent.Data;

                var images = new Dictionary<string, byte[]>
                                 {
                                     { "Logo", logo },
                                 };

                if (quote.ExistQuoteNumber)
                {
                    var barcodeGenerator = transaction.Database.Services.Get<IBarcodeGenerator>();
                    var barcode = barcodeGenerator.Generate(quote.QuoteNumber, BarcodeType.CODE_128, 320, 80, pure: true);
                    images.Add("Barcode", barcode);
                }

                var printModel = new Allors.Database.Domain.Print.ProductQuoteModel.Model(quote, images);
                quote.RenderPrintDocument(template, printModel, images);

                transaction.Derive();

                var result = quote.PrintDocument;

                var desktopDir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                var outputFile = File.Create(Path.Combine(desktopDir, "productQuote.odt"));
                using (var stream = new MemoryStream(result.Media.MediaContent.Data))
                {
                    stream.CopyTo(outputFile);
                }

                this.Logger.Info("End");
            }

            return ExitCode.Success;
        }
    }
}