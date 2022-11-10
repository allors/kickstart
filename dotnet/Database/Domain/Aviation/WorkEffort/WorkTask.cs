// <copyright file="WorkTask.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using System.Linq;

namespace Allors.Database.Domain
{
    public partial class WorkTask
    {
        public Expression CleaningExpression => this.ExistCleaningCalculation ? new Expression(this.CleaningCalculation) : null;

        public Expression SundriesExpression => this.ExistSundriesCalculation ? new Expression(this.SundriesCalculation) : null;

        public void AviationOnInit(ObjectOnInit method)
        {
            var transaction = this.strategy.Transaction;

            var settings = this.Strategy.Transaction.GetSingleton().Settings;

            if (this.Customer is Organisation customer)
            {
                if (!customer.ExcludeCleaning)
                {
                    new WorkEffortInvoiceItemAssignmentBuilder(transaction)
                        .WithAssignment(this)
                        .WithWorkEffortInvoiceItem(new WorkEffortInvoiceItemBuilder(transaction)
                                                .WithInvoiceItemType(new InvoiceItemTypes(transaction).Cleaning)
                                                .WithDescription("Parts to clean GSE")
                                                .Build())
                        .Build();

                    this.CleaningCalculation = customer.ExistCleaningCalculation ? customer.CleaningCalculation : settings.CleaningCalculation;
                }

                if (!customer.ExcludeSundries)
                {
                    new WorkEffortInvoiceItemAssignmentBuilder(transaction)
                        .WithAssignment(this)
                        .WithWorkEffortInvoiceItem(new WorkEffortInvoiceItemBuilder(transaction)
                                                .WithInvoiceItemType(new InvoiceItemTypes(transaction).Sundries)
                                                .WithDescription("Small material (sundries)")
                                                .Build())
                        .Build();

                    this.SundriesCalculation = customer.ExistSundriesCalculation ? customer.SundriesCalculation : settings.SundriesCalculation;
                }            }
            else
            {
                this.CleaningCalculation = settings.CleaningCalculation;
                this.SundriesCalculation = settings.SundriesCalculation;
            }
        }

        public void AviationPrintForWorker(WorkTaskPrintForWorker method)
        {
            if (!method.IsPrinted)
            {
                var singleton = this.Strategy.Transaction.GetSingleton();
                var logo = this.TakenBy?.ExistLogoImage == true ?
                               this.TakenBy.LogoImage.MediaContent.Data :
                               singleton.LogoImage.MediaContent.Data;

                var images = new Dictionary<string, byte[]>
                                 {
                                     { "Logo", logo },
                                 };

                if (this.ExistWorkEffortNumber)
                {
                    var barcodeGenerator = this.Strategy.Transaction.Database.Services.Get<IBarcodeGenerator>();
                    var barcode = barcodeGenerator.Generate(this.WorkEffortNumber, BarcodeType.CODE_128, 320, 80, pure: true);
                    images["Barcode"] = barcode;
                }

                var model = new Print.WorkTaskModel.Model(this);

                var document = this.TakenBy?.WorkTaskWorkerTemplate?.Render(model, images);
                if (document != null)
                {
                    if (!this.ExistPrintWorkerDocument)
                    {
                        this.PrintWorkerDocument = new PrintDocumentBuilder(this.Strategy.Transaction).Build();
                    }

                    if (!this.PrintWorkerDocument.ExistMedia)
                    {
                        this.PrintWorkerDocument.Media = new MediaBuilder(this.Strategy.Transaction).Build();
                    }

                    this.PrintWorkerDocument.Media.InData = document;
                }
                else
                {
                    this.ResetPrintDocument();
                }

                this.PrintWorkerDocument.Media.InFileName = $"{this.WorkEffortNumber} for worker.odt";

                method.IsPrinted = true;
            }
        }

        public void ResetPrintWorkerDocument()
        {
            if (!this.ExistPrintWorkerDocument)
            {
                this.PrintWorkerDocument = new PrintDocumentBuilder(this.Strategy.Transaction).Build();
            }

            this.PrintWorkerDocument.Media?.Delete();
        }
    }
}