// <copyright file="Custom.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Commands
{
    using Allors;
    using Allors.Database;
    using Allors.Database.Derivations;
    using Allors.Database.Domain;
    using McMaster.Extensions.CommandLineUtils;
    using NLog;
    using SkiaSharp;
    using System;
    using System.Collections.Generic;
    using System.IO;
    using System.Linq;
    using System.Net.NetworkInformation;

    [Command(Description = "Execute custom code")]
    public class Custom
    {
        public Program Parent { get; set; }

        public Logger Logger => LogManager.GetCurrentClassLogger();

        private ITransaction transaction;

        public int OnExecute(CommandLineApplication app)
        {
            this.transaction = this.Parent.Database.CreateTransaction();

            this.Logger.Info("Begin");

            transaction.Services.Get<IUserService>().User = new AutomatedAgents(transaction).System;

            var m = this.Parent.M;

            // Custom code

            return this.DoSomething();

            //return this.AddUser();
            //return this.PrintSalesInvoice();
            //return this.PrintSalesOrder();
            //return this.PrintWorkOrderSalesInvoice();
            //return this.PrintProductQuote();
            //return this.PrintPartQuote();
            //return this.PrintSparePartsBarcode();
            //return this.PrintWorkTask();
            //return this.PrintWorkTaskForWorker();
            //return this.PrintPurchaseOrder();
            //return this.PrintPurchaseInvoice();
            //return this.ProcessImage();
            //return this.ResetProductQuotePrint();

            transaction.Derive();
            transaction.Commit();

            this.Logger.Info("End");

            return ExitCode.Success;
        }

        private int ResetProductQuotePrint()
        {
            using (var transaction = this.transaction)
            {
                this.Logger?.Info($"Starting {System.Reflection.MethodBase.GetCurrentMethod().Name}");

                var singleton = transaction.GetSingleton();
                var organisations = new Organisations(transaction).Extent().ToArray();
                foreach (var internalOrganisation in organisations.Where(v => v.IsInternalOrganisation))
                {
                    internalOrganisation.ProductQuoteTemplate =
                       singleton.CreateOpenDocumentTemplate<Allors.Database.Domain.Print.ProductQuoteModel.Model>("ProductQuote.odt",
                           singleton.GetResourceBytes("Templates.ProductQuote.odt"));
                }

                transaction.Derive();

                foreach (ProductQuote productQuote in new ProductQuotes(transaction).Extent())
                {
                    productQuote.ResetPrintDocument();
                }

                transaction.Commit();

                this.Logger?.Info($"End {System.Reflection.MethodBase.GetCurrentMethod().Name}");
            }

            return ExitCode.Success;
        }

        private int DoSomething()
        {
            using (var transaction = this.Parent.Database.CreateTransaction())
            {
                this.Logger.Info("Begin");

                var m = this.Parent.M;
                var derivation = transaction.Database.Services.Get<IDerivationService>().CreateDerivation(transaction);
                var security = transaction.Database.Services.Get<ISecurity>();

                this.Logger.Info("End");
            }

            return ExitCode.Success;
        }

        private int ResetSalesOrderPrint()
        {
            this.Logger?.Info($"Starting {System.Reflection.MethodBase.GetCurrentMethod().Name}");

            using (var transaction = this.transaction)
            {
                var singleton = transaction.GetSingleton();
                var organisations = new Organisations(transaction).Extent().ToArray();
                foreach (var internalOrganisation in organisations.Where(v => v.IsInternalOrganisation))
                {
                    internalOrganisation.SalesOrderTemplate =
                        singleton.CreateOpenDocumentTemplate<Allors.Database.Domain.Print.SalesOrderModel.Model>("SalesOrder.odt",
                            singleton.GetResourceBytes("Templates.SalesOrder.odt"));
                }

                transaction.Derive();

                foreach (SalesOrder salesOrder in new SalesOrders(transaction).Extent())
                {
                    salesOrder.ResetPrintDocument();
                }

                transaction.Commit();
            }

            this.Logger?.Info($"End {System.Reflection.MethodBase.GetCurrentMethod().Name}");

            return ExitCode.Success;
        }

        private void ResetWorkOrderSalesInvoicePrint()
        {
            this.Logger?.Info($"Starting {System.Reflection.MethodBase.GetCurrentMethod().Name}");

            using (var transaction = this.transaction)
            {
                var singleton = transaction.GetSingleton();
                var organisations = new Organisations(transaction).Extent().ToArray();
                foreach (var internalOrganisation in organisations.Where(v => v.IsInternalOrganisation))
                {
                    internalOrganisation.WorkOrderSalesInvoiceTemplate =
                        singleton.CreateOpenDocumentTemplate<Allors.Database.Domain.Print.WorkOrderSalesInvoiceModel.Model>("WorkOrderSalesInvoice.odt",
                            singleton.GetResourceBytes("Templates.WorkOrderSalesInvoice.odt"));
                }

                transaction.Derive();

                foreach (WorkTask workTask in new WorkTasks(transaction).Extent().Where(v => v.ExistSalesInvoicesWhereWorkEffort))
                {
                    foreach (SalesInvoice salesInvoice in workTask.SalesInvoicesWhereWorkEffort)
                    {
                        salesInvoice.ResetPrintDocument();
                    }
                }

                transaction.Commit();
            }

            this.Logger?.Info($"End {System.Reflection.MethodBase.GetCurrentMethod().Name}");
        }

        private void ResetWorkTaskPrint()
        {
            this.Logger?.Info($"Starting {System.Reflection.MethodBase.GetCurrentMethod().Name}");

            using (var transaction = this.transaction)
            {
                var singleton = transaction.GetSingleton();
                var organisations = new Organisations(transaction).Extent().ToArray();
                foreach (var internalOrganisation in organisations.Where(v => v.IsInternalOrganisation))
                {
                    internalOrganisation.WorkTaskTemplate =
                        singleton.CreateOpenDocumentTemplate<Allors.Database.Domain.Print.WorkTaskModel.Model>("WorkTask.odt",
                            singleton.GetResourceBytes("Templates.WorkTask.odt"));
                }

                transaction.Derive();

                foreach (WorkTask workTask in new WorkTasks(transaction).Extent())
                {
                    workTask.ResetPrintDocument();
                }

                transaction.Commit();
            }

            this.Logger?.Info($"End {System.Reflection.MethodBase.GetCurrentMethod().Name}");
        }

        private int PrintSalesOrder()
        {
            using (var transaction = this.Parent.Database.CreateTransaction())
            {
                this.Logger.Info("Begin");

                var m = this.Parent.M;

                transaction.Services.Get<IUserService>().User = new AutomatedAgents(transaction).System;

                var singleton = transaction.GetSingleton();

                var template = singleton.CreateOpenDocumentTemplate<Allors.Database.Domain.Print.SalesOrderModel.Model>("SalesOrder.odt", singleton.GetResourceBytes("Templates.SalesOrder.odt"));

                transaction.Derive();

                var templateFilePath = "domain/templates/SalesOrder.odt";
                var templateFileInfo = new FileInfo(templateFilePath);
                var prefix = string.Empty;
                while (!templateFileInfo.Exists)
                {
                    prefix += "../";
                    templateFileInfo = new FileInfo(prefix + templateFilePath);
                }

                var salesOrder = new SalesOrders(transaction).FindBy(m.SalesOrder.OrderNumber, "AMSO-2022-2542");

                using (var memoryStream = new MemoryStream())
                using (var fileStream = new FileStream(
                    templateFileInfo.FullName,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite))
                {
                    fileStream.CopyTo(memoryStream);
                    template.Media.InData = memoryStream.ToArray();
                }

                transaction.Derive();

                var logo = salesOrder.TakenBy?.ExistLogoImage == true ?
                               salesOrder.TakenBy.LogoImage.MediaContent.Data :
                               singleton.LogoImage.MediaContent.Data;

                var images = new Dictionary<string, byte[]> { { "Logo", logo }, };

                if (salesOrder.ExistOrderNumber)
                {
                    var barcodeService = transaction.Database.Services.Get<IBarcodeGenerator>();
                    var barcode = barcodeService.Generate(salesOrder.OrderNumber, BarcodeType.CODE_128, 320, 80, pure: true);
                    images.Add("Barcode", barcode);
                }

                var printModel = new Allors.Database.Domain.Print.SalesOrderModel.Model(salesOrder);
                salesOrder.RenderPrintDocument(template, printModel, images);

                transaction.Derive();

                var result = salesOrder.PrintDocument;

                var desktopDir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                var outputFile = File.Create(Path.Combine(desktopDir, "SalesOrder.odt"));
                using (var stream = new MemoryStream(result.Media.MediaContent.Data))
                {
                    stream.CopyTo(outputFile);
                }

                this.Logger.Info("End");
            }

            return ExitCode.Success;
        }

        private int PrintSalesInvoice()
        {
            using (var transaction = this.Parent.Database.CreateTransaction())
            {
                this.Logger.Info("Begin");

                var m = this.Parent.M;

                transaction.Services.Get<IUserService>().User = new AutomatedAgents(transaction).System;
                var singleton = transaction.GetSingleton();

                var template = singleton.CreateOpenDocumentTemplate<Allors.Database.Domain.Print.SalesInvoiceModel.Model>("SalesInvoice.odt", singleton.GetResourceBytes("Templates.SalesInvoice.odt"));

                transaction.Derive();

                var templateFilePath = "domain/templates/SalesInvoice.odt";
                var templateFileInfo = new FileInfo(templateFilePath);
                var prefix = string.Empty;
                while (!templateFileInfo.Exists)
                {
                    prefix += "../";
                    templateFileInfo = new FileInfo(prefix + templateFilePath);
                }

                var invoice = new SalesInvoices(transaction).FindBy(m.SalesInvoice.InvoiceNumber, "1");
                invoice.PrintCondensed = true;

                using (var memoryStream = new MemoryStream())
                using (var fileStream = new FileStream(
                    templateFileInfo.FullName,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite))
                {
                    fileStream.CopyTo(memoryStream);
                    template.Media.InData = memoryStream.ToArray();
                }

                transaction.Derive();

                var logo = invoice.BilledFrom?.ExistLogoImage == true ?
                            invoice.BilledFrom.LogoImage.MediaContent.Data :
                            singleton.LogoImage.MediaContent.Data;

                var images = new Dictionary<string, byte[]> { { "Logo", logo }, };

                if (invoice.ExistInvoiceNumber)
                {
                    var barcodeService = transaction.Database.Services.Get<IBarcodeGenerator>();
                    var barcode = barcodeService.Generate(invoice.InvoiceNumber, BarcodeType.CODE_128, 320, 80, pure: true);
                    images.Add("Barcode", barcode);
                }

                var printModel = new Allors.Database.Domain.Print.SalesInvoiceModel.Model(invoice);
                invoice.RenderPrintDocument(template, printModel, images);

                transaction.Derive();

                var result = invoice.PrintDocument;

                var desktopDir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                var outputFile = File.Create(Path.Combine(desktopDir, "SalesInvoice.odt"));
                using (var stream = new MemoryStream(result.Media.MediaContent.Data))
                {
                    stream.CopyTo(outputFile);
                }

                this.Logger.Info("End");
            }

            return ExitCode.Success;
        }

        private int PrintWorkOrderSalesInvoice()
        {
            using (var transaction = this.Parent.Database.CreateTransaction())
            {
                this.Logger.Info("Begin");

                var m = this.Parent.M;

                transaction.Services.Get<IUserService>().User = new AutomatedAgents(transaction).System;
                var singleton = transaction.GetSingleton();

                var template = singleton.CreateOpenDocumentTemplate<Allors.Database.Domain.Print.WorkOrderSalesInvoiceModel.Model>("WorkOrderSalesInvoice.odt", singleton.GetResourceBytes("Templates.WorkOrderSalesInvoice.odt"));

                transaction.Derive();

                var templateFilePath = "domain/templates/WorkOrderSalesInvoice.odt";
                var templateFileInfo = new FileInfo(templateFilePath);
                var prefix = string.Empty;
                while (!templateFileInfo.Exists)
                {
                    prefix += "../";
                    templateFileInfo = new FileInfo(prefix + templateFilePath);
                }

                var invoice = new SalesInvoices(transaction).FindBy(m.SalesInvoice.InvoiceNumber, "261");

                using (var memoryStream = new MemoryStream())
                using (var fileStream = new FileStream(
                    templateFileInfo.FullName,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite))
                {
                    fileStream.CopyTo(memoryStream);
                    template.Media.InData = memoryStream.ToArray();
                }

                transaction.Derive();

                var logo = invoice.BilledFrom?.ExistLogoImage == true ?
                            invoice.BilledFrom.LogoImage.MediaContent.Data :
                            singleton.LogoImage.MediaContent.Data;

                var images = new Dictionary<string, byte[]> { { "Logo", logo }, };

                if (invoice.ExistInvoiceNumber)
                {
                    var barcodeService = transaction.Database.Services.Get<IBarcodeGenerator>();
                    var barcode = barcodeService.Generate(invoice.InvoiceNumber, BarcodeType.CODE_128, 320, 80, pure: true);
                    images.Add("Barcode", barcode);
                }

                foreach (var workEffort in invoice.WorkEfforts)
                {
                    var workTask = (WorkTask)workEffort;
                    var barcodeService = transaction.Database.Services.Get<IBarcodeGenerator>();
                    var barcode = barcodeService.Generate(workTask.WorkEffortNumber, BarcodeType.CODE_128, 320, 80, pure: true);
                    images.Add(Allors.Database.Domain.Print.WorkTaskModel.Model.BarcodeName(workTask), barcode);
                }

                var printModel = new Allors.Database.Domain.Print.WorkOrderSalesInvoiceModel.Model(invoice);
                invoice.RenderPrintDocument(template, printModel, images);

                transaction.Derive();

                var result = invoice.PrintDocument;

                var desktopDir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                var outputFile = File.Create(Path.Combine(desktopDir, "WorkOrderSalesInvoice.odt"));
                using (var stream = new MemoryStream(result.Media.MediaContent.Data))
                {
                    stream.CopyTo(outputFile);
                }

                this.Logger.Info("End");
            }

            return ExitCode.Success;
        }

        private int PrintProductQuote()
        {
            using (var transaction = this.Parent.Database.CreateTransaction())
            {
                this.Logger.Info("Begin");

                var m = this.Parent.M;

                transaction.Services.Get<IUserService>().User = new AutomatedAgents(transaction).System;
                var singleton = transaction.GetSingleton();

                var quote = new ProductQuotes(transaction).Extent().First(v => v.QuoteNumber == "NLQ-1928");

                var logo = quote.Issuer?.ExistLogoImage == true ?
                   quote.Issuer.LogoImage.MediaContent.Data :
                   singleton.LogoImage.MediaContent.Data;

                var images = new Dictionary<string, byte[]> { { "Logo", logo }, };

                if (quote.ExistQuoteNumber)
                {
                    var barcodeService = transaction.Database.Services.Get<IBarcodeGenerator>();
                    var barcode = barcodeService.Generate(quote.QuoteNumber, BarcodeType.CODE_128, 320, 80, pure: true);
                    images.Add("Barcode", barcode);
                }

                if (quote.QuoteItems.Any(v => v.ExistProduct && v.Product.GetType().Name.Equals(typeof(NonUnifiedPart).Name)))
                {
                    var template = singleton.CreateOpenDocumentTemplate<Allors.Database.Domain.Print.PartQuoteModel.Model>("ProductQuote.odt", singleton.GetResourceBytes("Templates.ProductQuote.odt"));

                    transaction.Derive();

                    var templateFilePath = "custom/database/domain/templates/ProductQuote.odt";
                    var templateFileInfo = new FileInfo(templateFilePath);
                    var prefix = string.Empty;
                    while (!templateFileInfo.Exists)
                    {
                        prefix += "../";
                        templateFileInfo = new FileInfo(prefix + templateFilePath);
                    }

                    using (var memoryStream = new MemoryStream())
                    using (var fileStream = new FileStream(
                        templateFileInfo.FullName,
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.ReadWrite))
                    {
                        fileStream.CopyTo(memoryStream);
                        template.Media.InData = memoryStream.ToArray();
                    }

                    transaction.Derive();
                    var printModel = new Allors.Database.Domain.Print.PartQuoteModel.Model(quote, images);
                    quote.RenderPrintDocument(template, printModel, images);
                }
                else
                {
                    var template = singleton.CreateOpenDocumentTemplate<Allors.Database.Domain.Print.ProductQuoteModel.Model>("ProductQuote.odt", singleton.GetResourceBytes("Templates.ProductQuote.odt"));

                    transaction.Derive();

                    var templateFilePath = "custom/database/domain/templates/ProductQuote.odt";
                    var templateFileInfo = new FileInfo(templateFilePath);
                    var prefix = string.Empty;
                    while (!templateFileInfo.Exists)
                    {
                        prefix += "../";
                        templateFileInfo = new FileInfo(prefix + templateFilePath);
                    }

                    using (var memoryStream = new MemoryStream())
                    using (var fileStream = new FileStream(
                        templateFileInfo.FullName,
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.ReadWrite))
                    {
                        fileStream.CopyTo(memoryStream);
                        template.Media.InData = memoryStream.ToArray();
                    }

                    transaction.Derive();
                    var printModel = new Allors.Database.Domain.Print.ProductQuoteModel.Model(quote, images);
                    quote.RenderPrintDocument(template, printModel, images);
                }

                transaction.Derive();

                var result = quote.PrintDocument;

                var desktopDir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                var outputFile = File.Create(Path.Combine(desktopDir, "quote.odt"));
                using (var stream = new MemoryStream(result.Media.MediaContent.Data))
                {
                    stream.CopyTo(outputFile);
                }

                this.Logger.Info("End");
            }

            return ExitCode.Success;
        }

        private int PrintPartQuote()
        {
            using (var transaction = this.Parent.Database.CreateTransaction())
            {
                this.Logger.Info("Begin");

                var m = this.Parent.M;

                transaction.Services.Get<IUserService>().User = new AutomatedAgents(transaction).System;
                var singleton = transaction.GetSingleton();

                var quote = new ProductQuotes(transaction).Extent().First(v => v.QuoteNumber == "NLQ-1928");

                var logo = quote.Issuer?.ExistLogoImage == true ?
                   quote.Issuer.LogoImage.MediaContent.Data :
                   singleton.LogoImage.MediaContent.Data;

                var images = new Dictionary<string, byte[]> { { "Logo", logo }, };

                if (quote.ExistQuoteNumber)
                {
                    var barcodeService = transaction.Database.Services.Get<IBarcodeGenerator>();
                    var barcode = barcodeService.Generate(quote.QuoteNumber, BarcodeType.CODE_128, 320, 80, pure: true);
                    images.Add("Barcode", barcode);
                }

                if (quote.QuoteItems.Any(v => v.ExistProduct && v.Product.GetType().Name.Equals(typeof(NonUnifiedPart).Name)))
                {
                    var template = singleton.CreateOpenDocumentTemplate<Allors.Database.Domain.Print.PartQuoteModel.Model>("PartQuote.odt", singleton.GetResourceBytes("Templates.PartQuote.odt"));

                    transaction.Derive();

                    var templateFilePath = "custom/database/domain/templates/PartQuote.odt";
                    var templateFileInfo = new FileInfo(templateFilePath);
                    var prefix = string.Empty;
                    while (!templateFileInfo.Exists)
                    {
                        prefix += "../";
                        templateFileInfo = new FileInfo(prefix + templateFilePath);
                    }

                    using (var memoryStream = new MemoryStream())
                    using (var fileStream = new FileStream(
                        templateFileInfo.FullName,
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.ReadWrite))
                    {
                        fileStream.CopyTo(memoryStream);
                        template.Media.InData = memoryStream.ToArray();
                    }

                    transaction.Derive();
                    var printModel = new Allors.Database.Domain.Print.PartQuoteModel.Model(quote, images);
                    quote.RenderPrintDocument(template, printModel, images);
                }
                else
                {
                    var template = singleton.CreateOpenDocumentTemplate<Allors.Database.Domain.Print.ProductQuoteModel.Model>("ProductQuote.odt", singleton.GetResourceBytes("Templates.ProductQuote.odt"));

                    transaction.Derive();

                    var templateFilePath = "custom/database/domain/templates/ProductQuote.odt";
                    var templateFileInfo = new FileInfo(templateFilePath);
                    var prefix = string.Empty;
                    while (!templateFileInfo.Exists)
                    {
                        prefix += "../";
                        templateFileInfo = new FileInfo(prefix + templateFilePath);
                    }

                    using (var memoryStream = new MemoryStream())
                    using (var fileStream = new FileStream(
                        templateFileInfo.FullName,
                        FileMode.Open,
                        FileAccess.Read,
                        FileShare.ReadWrite))
                    {
                        fileStream.CopyTo(memoryStream);
                        template.Media.InData = memoryStream.ToArray();
                    }

                    transaction.Derive();
                    var printModel = new Allors.Database.Domain.Print.ProductQuoteModel.Model(quote, images);
                    quote.RenderPrintDocument(template, printModel, images);
                }

                transaction.Derive();

                var result = quote.PrintDocument;

                var desktopDir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                var outputFile = File.Create(Path.Combine(desktopDir, "quote.odt"));
                using (var stream = new MemoryStream(result.Media.MediaContent.Data))
                {
                    stream.CopyTo(outputFile);
                }

                this.Logger.Info("End");
            }

            return ExitCode.Success;
        }

        private int PrintSparePartsBarcode()
        {
            using (var transaction = this.Parent.Database.CreateTransaction())
            {
                this.Logger.Info("Begin");

                var m = this.Parent.M;

                transaction.Services.Get<IUserService>().User = new AutomatedAgents(transaction).System;
                var singleton = transaction.GetSingleton();

                var template = singleton.CreateOpenDocumentTemplate<Allors.Database.Domain.Print.NonUnifiedPartBarcodePrint.Model>("SparePartsBarcode.odt", singleton.GetResourceBytes("Templates.SparePartsBarcode.odt"));

                transaction.Derive();

                var organisations = new Organisations(transaction).Extent();
                organisations.Filter.AddEquals(m.Organisation.ExternalPrimaryKey, "AVIATION REPAIRS AND MAINTENANCE, S.L.ESB55744833");
                var arm = organisations.First;

                var partByName = new NonSerialisedInventoryItems(transaction).Extent()
                    .Where(v => v.Facility.Equals(arm.FacilitiesWhereOwner.First()))
                    .Select(v => v)
                    .ToDictionary(v =>
                    {
                        var part = (NonUnifiedPart)v.Part;
                        return $"{part?.Name}, {part?.UnitOfMeasure.Name}";
                    });

                var images = new Dictionary<string, byte[]>();
                foreach (var keyValuePair in partByName)
                {
                    var barcodeService = transaction.Database.Services.Get<IBarcodeGenerator>();
                    var barcode = barcodeService.Generate(keyValuePair.Value.Part.PartIdentification(), BarcodeType.CODE_128, 320, 80, pure: true);
                    var barcodeName = Allors.Database.Domain.Print.PartModel.Model.BarcodeName(keyValuePair.Value.Part);
                    if (!images.ContainsKey(barcodeName))
                    {
                        images.Add(barcodeName, barcode);
                    }
                }

                var partByNameSorted = new SortedDictionary<string, NonSerialisedInventoryItem>(partByName);

                var printModel = new Allors.Database.Domain.Print.NonUnifiedPartBarcodePrint.Model(partByNameSorted);
                var result = template?.Render(printModel, images);

                transaction.Derive();

                var desktopDir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                var outputFile = File.Create(Path.Combine(desktopDir, "SparePartsBarcode.odt"));
                using (var stream = new MemoryStream(result))
                {
                    stream.CopyTo(outputFile);
                }

                this.Logger.Info("End");
            }

            return ExitCode.Success;
        }

        private int PrintWorkTask()
        {
            using (var transaction = this.Parent.Database.CreateTransaction())
            {
                this.Logger.Info("Begin");

                var m = this.Parent.M;

                transaction.Services.Get<IUserService>().User = new AutomatedAgents(transaction).System;
                var singleton = transaction.GetSingleton();

                var template = singleton.CreateOpenDocumentTemplate<Allors.Database.Domain.Print.WorkTaskModel.Model>("WorkTask.odt", singleton.GetResourceBytes("Templates.WorkTask.odt"));

                transaction.Derive();

                var templateFilePath = "domain/templates/WorkTask.odt";
                var templateFileInfo = new FileInfo(templateFilePath);
                var prefix = string.Empty;
                while (!templateFileInfo.Exists)
                {
                    prefix += "../";
                    templateFileInfo = new FileInfo(prefix + templateFilePath);
                }

                var workTasks = new WorkTasks(transaction).Extent();
                var workTask = workTasks.First(v => v.WorkEffortNumber == "RMWO-605");

                workTask.DerivationTrigger = Guid.NewGuid();

                transaction.Derive();
                transaction.Commit();

                using (var memoryStream = new MemoryStream())
                using (var fileStream = new FileStream(
                    templateFileInfo.FullName,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite))
                {
                    fileStream.CopyTo(memoryStream);
                    template.Media.InData = memoryStream.ToArray();
                }

                transaction.Derive();

                var logo = workTask.TakenBy?.ExistLogoImage == true ?
                    workTask.TakenBy.LogoImage.MediaContent.Data :
                    singleton.LogoImage.MediaContent.Data;

                var images = new Dictionary<string, byte[]> { { "Logo", logo }, };

                if (workTask.ExistWorkEffortNumber)
                {
                    var barcodeService = transaction.Database.Services.Get<IBarcodeGenerator>();
                    var barcode = barcodeService.Generate(workTask.WorkEffortNumber, BarcodeType.CODE_128, 320, 80, pure: true);
                    images.Add("Barcode", barcode);
                }

                var printModel = new Allors.Database.Domain.Print.WorkTaskModel.Model(workTask);
                workTask.RenderPrintDocument(template, printModel, images);

                transaction.Derive();

                var result = workTask.PrintDocument;

                var desktopDir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                var outputFile = File.Create(Path.Combine(desktopDir, "workTask.odt"));
                using (var stream = new MemoryStream(result.Media.MediaContent.Data))
                {
                    stream.CopyTo(outputFile);
                }

                this.Logger.Info("End");
            }

            return ExitCode.Success;
        }

        private int PrintWorkTaskForWorker()
        {
            using (var transaction = this.Parent.Database.CreateTransaction())
            {
                this.Logger.Info("Begin");

                var m = this.Parent.M;

                transaction.Services.Get<IUserService>().User = new AutomatedAgents(transaction).System;
                var singleton = transaction.GetSingleton();

                var template = singleton.CreateOpenDocumentTemplate<Allors.Database.Domain.Print.WorkTaskModel.Model>("WorkTaskWorker.odt", singleton.GetResourceBytes("Templates.WorkTaskWorker.odt"));

                transaction.Derive();

                var templateFilePath = "domain/templates/WorkTaskWorker.odt";
                var templateFileInfo = new FileInfo(templateFilePath);
                var prefix = string.Empty;
                while (!templateFileInfo.Exists)
                {
                    prefix += "../";
                    templateFileInfo = new FileInfo(prefix + templateFilePath);
                }

                var workTasks = new WorkTasks(transaction).Extent();
                var workTask = workTasks.First(v => v.WorkEffortNumber == "RMWO-605");

                using (var memoryStream = new MemoryStream())
                using (var fileStream = new FileStream(
                    templateFileInfo.FullName,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite))
                {
                    fileStream.CopyTo(memoryStream);
                    template.Media.InData = memoryStream.ToArray();
                }

                transaction.Derive();

                var images = new Dictionary<string, byte[]> { { "Logo", transaction.GetSingleton().LogoImage.MediaContent.Data }, };

                if (workTask.ExistWorkEffortNumber)
                {
                    var barcodeService = transaction.Database.Services.Get<IBarcodeGenerator>();
                    var barcode = barcodeService.Generate(workTask.WorkEffortNumber, BarcodeType.CODE_128, 320, 80, pure: true);
                    images.Add("Barcode", barcode);
                }

                var printModel = new Allors.Database.Domain.Print.WorkTaskModel.Model(workTask);
                workTask.RenderPrintDocument(template, printModel, images);

                transaction.Derive();

                var result = workTask.PrintDocument;

                var desktopDir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                var outputFile = File.Create(Path.Combine(desktopDir, "workTaskWorker.odt"));
                using (var stream = new MemoryStream(result.Media.MediaContent.Data))
                {
                    stream.CopyTo(outputFile);
                }

                this.Logger.Info("End");
            }

            return ExitCode.Success;
        }

        private int PrintPurchaseOrder()
        {
            using (var transaction = this.Parent.Database.CreateTransaction())
            {
                this.Logger.Info("Begin");

                var m = this.Parent.M;

                transaction.Services.Get<IUserService>().User = new AutomatedAgents(transaction).System;
                var singleton = transaction.GetSingleton();

                var template = singleton.CreateOpenDocumentTemplate<Allors.Database.Domain.Print.PurchaseOrderModel.Model>("PurchaseOrder.odt", singleton.GetResourceBytes("Templates.PurchaseOrder.odt"));

                transaction.Derive();

                var templateFilePath = "domain/templates/PurchaseOrder.odt";
                var templateFileInfo = new FileInfo(templateFilePath);
                var prefix = string.Empty;
                while (!templateFileInfo.Exists)
                {
                    prefix += "../";
                    templateFileInfo = new FileInfo(prefix + templateFilePath);
                }

                var purchaseOrder = new PurchaseOrders(transaction).FindBy(m.PurchaseOrder.OrderNumber, "RMPO-2020-100");

                using (var memoryStream = new MemoryStream())
                using (var fileStream = new FileStream(
                    templateFileInfo.FullName,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite))
                {
                    fileStream.CopyTo(memoryStream);
                    template.Media.InData = memoryStream.ToArray();
                }

                transaction.Derive();
                var logo = purchaseOrder.OrderedBy?.ExistLogoImage == true ?
                    purchaseOrder.OrderedBy.LogoImage.MediaContent.Data :
                    singleton.LogoImage.MediaContent.Data;

                var images = new Dictionary<string, byte[]> { { "Logo", logo }, };

                if (purchaseOrder.ExistOrderNumber)
                {
                    var barcodeService = transaction.Database.Services.Get<IBarcodeGenerator>();
                    var barcode = barcodeService.Generate(purchaseOrder.OrderNumber, BarcodeType.CODE_128, 320, 80, pure: true);
                    images.Add("Barcode", barcode);
                }

                var printModel = new Allors.Database.Domain.Print.PurchaseOrderModel.Model(purchaseOrder);
                purchaseOrder.RenderPrintDocument(template, printModel, images);

                transaction.Derive();

                var result = purchaseOrder.PrintDocument;

                var desktopDir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                var outputFile = File.Create(Path.Combine(desktopDir, "PurchaseOrder.odt"));
                using (var stream = new MemoryStream(result.Media.MediaContent.Data))
                {
                    stream.CopyTo(outputFile);
                }

                this.Logger.Info("End");
            }

            return ExitCode.Success;
        }

        private int PrintPurchaseInvoice()
        {
            using (var transaction = this.Parent.Database.CreateTransaction())
            {
                this.Logger.Info("Begin");

                var m = this.Parent.M;

                transaction.Services.Get<IUserService>().User = new AutomatedAgents(transaction).System;
                var singleton = transaction.GetSingleton();

                var template = singleton.CreateOpenDocumentTemplate<Allors.Database.Domain.Print.PurchaseInvoiceModel.Model>("PurchaseInvoice.odt", singleton.GetResourceBytes("Templates.PurchaseInvoice.odt"));

                transaction.Derive();

                var templateFilePath = "domain/templates/PurchaseInvoice.odt";
                var templateFileInfo = new FileInfo(templateFilePath);
                var prefix = string.Empty;
                while (!templateFileInfo.Exists)
                {
                    prefix += "../";
                    templateFileInfo = new FileInfo(prefix + templateFilePath);
                }

                var purchaseInvoice = new PurchaseInvoices(transaction).FindBy(m.PurchaseInvoice.InvoiceNumber, "RMPI-2022-204");

                using (var memoryStream = new MemoryStream())
                using (var fileStream = new FileStream(
                    templateFileInfo.FullName,
                    FileMode.Open,
                    FileAccess.Read,
                    FileShare.ReadWrite))
                {
                    fileStream.CopyTo(memoryStream);
                    template.Media.InData = memoryStream.ToArray();
                }

                transaction.Derive();

                var logo = purchaseInvoice.BilledTo?.ExistLogoImage == true ?
                    purchaseInvoice.BilledTo.LogoImage.MediaContent.Data :
                    singleton.LogoImage.MediaContent.Data;

                var images = new Dictionary<string, byte[]> { { "Logo", logo }, };

                if (purchaseInvoice.ExistInvoiceNumber)
                {
                    var barcodeService = transaction.Database.Services.Get<IBarcodeGenerator>();
                    var barcode = barcodeService.Generate(purchaseInvoice.InvoiceNumber, BarcodeType.CODE_128, 320, 80, pure: true);
                    images.Add("Barcode", barcode);
                }

                var printModel = new Allors.Database.Domain.Print.PurchaseInvoiceModel.Model(purchaseInvoice);
                purchaseInvoice.RenderPrintDocument(template, printModel, images);

                transaction.Derive();

                var result = purchaseInvoice.PrintDocument;

                var desktopDir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                var outputFile = File.Create(Path.Combine(desktopDir, "PurchaseInvoice.odt"));
                using (var stream = new MemoryStream(result.Media.MediaContent.Data))
                {
                    stream.CopyTo(outputFile);
                }

                this.Logger.Info("End");
            }

            return ExitCode.Success;
        }

        private int ProcessImage()
        {
            using (var transaction = this.Parent.Database.CreateTransaction())
            {
                var singleton = transaction.GetSingleton();
                var unifiedGood = new UnifiedGoods(transaction).Extent().First();

                var photo = unifiedGood.PrimaryPhoto;
                var watermark = singleton.Watermark;

                var processed = photo.Processed;
                using var image = SKImage.FromBitmap(processed);
                using var data = image.Encode(SKEncodedImageFormat.Png, 100);

                var desktopDir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
                var outputFile = File.Create(Path.Combine(desktopDir, "Processed.png"));
                using (var stream = new MemoryStream(data.ToArray()))
                {
                    stream.CopyTo(outputFile);
                }
            }

            return 0;
        }
    }
}