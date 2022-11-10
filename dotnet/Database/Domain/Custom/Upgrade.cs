using Allors.Database.Derivations;
using Allors.Database.Meta;
using NLog;
using NLog.Fluent;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using DateTime = System.DateTime;

namespace Allors.Database.Domain
{
    public class Upgrade
    {
        private readonly ITransaction transaction;

        private DirectoryInfo DataPath;

        public Logger Logger => LogManager.GetCurrentClassLogger();

        public Upgrade(ITransaction transaction, DirectoryInfo dataPath)
        {
            this.transaction = transaction;
            this.DataPath = dataPath;
        }

        public void Execute()
        {
            //var m = transaction.Database.Services.Get<MetaPopulation>();

            //this.ResetProductQuotePrint();
            //this.ResetPartQuotePrint();
            //this.ResetSalesOrderPrint();
            //this.ResetPurchaseOrderPrint();
            //this.ResetPurchaseInvoicePrint();
            //this.ResetSalesInvoicePrint();    
            //this.ResetWorkOrderSalesInvoicePrint();
            //this.ResetWorkTaskPrint();
            //this.ResetWorkTaskWorkerPrint();

            //this.ResetWorkEffortDeniedPermissions();
        }

        private void ResetProductQuotePrint()
        {
            this.Logger?.Info($"Starting {System.Reflection.MethodBase.GetCurrentMethod().Name}");

            using (var transaction = this.transaction)
            {
                var singleton = transaction.GetSingleton();
                var organisations = new Organisations(transaction).Extent().ToArray();
                foreach (var internalOrganisation in organisations.Where(v => v.IsInternalOrganisation))
                {
                    internalOrganisation.ProductQuoteTemplate =
                       singleton.CreateOpenDocumentTemplate<Domain.Print.ProductQuoteModel.Model>("ProductQuote.odt",
                           singleton.GetResourceBytes("Templates.ProductQuote.odt"));
                }

                transaction.Derive();

                foreach (ProductQuote productQuote in new ProductQuotes(transaction).Extent())
                {
                    productQuote.ResetPrintDocument();
                }

                transaction.Commit();
            }

            this.Logger?.Info($"End {System.Reflection.MethodBase.GetCurrentMethod().Name}");
        }

        private void ResetPartQuotePrint()
        {
            this.Logger?.Info($"Starting {System.Reflection.MethodBase.GetCurrentMethod().Name}");

            using (var transaction = this.transaction)
            {
                var singleton = transaction.GetSingleton();
                var organisations = new Organisations(transaction).Extent().ToArray();
                foreach (var internalOrganisation in organisations.Where(v => v.IsInternalOrganisation))
                {
                    internalOrganisation.PartQuoteTemplate =
                       singleton.CreateOpenDocumentTemplate<Domain.Print.ProductQuoteModel.Model>("PartQuote.odt",
                           singleton.GetResourceBytes("Templates.PartQuote.odt"));
                }

                transaction.Derive();

                foreach (ProductQuote productQuote in new ProductQuotes(transaction).Extent())
                {
                    productQuote.ResetPrintDocument();
                }

                transaction.Commit();
            }

            this.Logger?.Info($"End {System.Reflection.MethodBase.GetCurrentMethod().Name}");
        }

        private void ResetSalesOrderPrint()
        {
            this.Logger?.Info($"Starting {System.Reflection.MethodBase.GetCurrentMethod().Name}");

            using (var transaction = this.transaction)
            {
                var singleton = transaction.GetSingleton();
                var organisations = new Organisations(transaction).Extent().ToArray();
                foreach (var internalOrganisation in organisations.Where(v => v.IsInternalOrganisation))
                {
                    internalOrganisation.SalesOrderTemplate =
                        singleton.CreateOpenDocumentTemplate<Print.SalesOrderModel.Model>("SalesOrder.odt",
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
        }

        private void ResetPurchaseOrderPrint()
        {
            this.Logger?.Info($"Starting {System.Reflection.MethodBase.GetCurrentMethod().Name}");

            using (var transaction = this.transaction)
            {
                var singleton = transaction.GetSingleton();
                var organisations = new Organisations(transaction).Extent().ToArray();
                foreach (var internalOrganisation in organisations.Where(v => v.IsInternalOrganisation))
                {
                    internalOrganisation.PurchaseOrderTemplate =
                        singleton.CreateOpenDocumentTemplate<Print.PurchaseOrderModel.Model>("PurchaseOrder.odt",
                            singleton.GetResourceBytes("Templates.PurchaseOrder.odt"));
                }

                transaction.Derive();

                foreach (PurchaseOrder purchaseOrder in new PurchaseOrders(transaction).Extent())
                {
                    purchaseOrder.ResetPrintDocument();
                }

                transaction.Commit();
            }

            this.Logger?.Info($"End {System.Reflection.MethodBase.GetCurrentMethod().Name}");
        }

        private void ResetPurchaseInvoicePrint()
        {
            this.Logger?.Info($"Starting {System.Reflection.MethodBase.GetCurrentMethod().Name}");

            using (var transaction = this.transaction)
            {
                var singleton = transaction.GetSingleton();
                var organisations = new Organisations(transaction).Extent().ToArray();
                foreach (var internalOrganisation in organisations.Where(v => v.IsInternalOrganisation))
                {
                    internalOrganisation.PurchaseInvoiceTemplate =
                        singleton.CreateOpenDocumentTemplate<Print.PurchaseInvoiceModel.Model>("PurchaseInvoice.odt",
                            singleton.GetResourceBytes("Templates.PurchaseInvoice.odt"));
                }

                transaction.Derive();

                foreach (PurchaseInvoice purchaseInvoice in new PurchaseInvoices(transaction).Extent())
                {
                    purchaseInvoice.ResetPrintDocument();
                }

                transaction.Commit();
            }

            this.Logger?.Info($"End {System.Reflection.MethodBase.GetCurrentMethod().Name}");
        }

        private void ResetSalesInvoicePrint()
        {
            this.Logger?.Info($"Starting {System.Reflection.MethodBase.GetCurrentMethod().Name}");

            using (var transaction = this.transaction)
            {
                var singleton = transaction.GetSingleton();
                var organisations = new Organisations(transaction).Extent().ToArray();
                foreach (var internalOrganisation in organisations.Where(v => v.IsInternalOrganisation))
                {
                    internalOrganisation.SalesInvoiceTemplate =
                        singleton.CreateOpenDocumentTemplate<Print.SalesInvoiceModel.Model>("SalesInvoice.odt",
                            singleton.GetResourceBytes("Templates.SalesInvoice.odt"));
                }

                transaction.Derive();

                foreach (SalesInvoice salesInvoice in new SalesInvoices(transaction).Extent())
                {
                    salesInvoice.ResetPrintDocument();
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
                        singleton.CreateOpenDocumentTemplate<Print.WorkTaskModel.Model>("WorkTask.odt",
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

        private void ResetWorkTaskWorkerPrint()
        {
            this.Logger?.Info($"Starting {System.Reflection.MethodBase.GetCurrentMethod().Name}");

            using (var transaction = this.transaction)
            {
                var singleton = transaction.GetSingleton();
                var organisations = new Organisations(transaction).Extent().ToArray();
                foreach (var internalOrganisation in organisations.Where(v => v.IsInternalOrganisation))
                {
                    internalOrganisation.WorkTaskWorkerTemplate =
                        singleton.CreateOpenDocumentTemplate<Print.WorkTaskModel.Model>("WorkTaskWorker.odt",
                            singleton.GetResourceBytes("Templates.WorkTaskWorker.odt"));
                }

                transaction.Derive();

                foreach (WorkTask workTask in new WorkTasks(transaction).Extent())
                {
                    workTask.ResetPrintWorkerDocument();
                }

                transaction.Commit();
            }

            this.Logger?.Info($"End {System.Reflection.MethodBase.GetCurrentMethod().Name}");
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
                        singleton.CreateOpenDocumentTemplate<Print.WorkOrderSalesInvoiceModel.Model>("WorkOrderSalesInvoice.odt",
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

        //private void ResetWorkEffortDeniedPermissions()
        //{
        //    this.Logger?.Info($"Starting {System.Reflection.MethodBase.GetCurrentMethod().Name}");

        //    using (var transaction = this.transaction)
        //    {
        //        var derivation = new DefaultDerivation(transaction);

        //        foreach (WorkEffort workEffort in transaction.Extent<WorkEffort>())
        //        {
        //            derivation.Mark(workEffort);
        //        }

        //        derivation.Derive();
        //        transaction.Commit();
        //    }

        //    this.Logger?.Info($"End {System.Reflection.MethodBase.GetCurrentMethod().Name}");
        //}
    }
}