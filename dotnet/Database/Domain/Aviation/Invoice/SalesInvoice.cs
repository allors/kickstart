// --------------------------------------------------------------------------------------------------------------------
// <copyright file="ProductQuote.cs" company="Allors bvba">
//   Copyright 2002-2012 Allors bvba.
// Dual Licensed under
//   a) the General Public Licence v3 (GPL)
//   b) the Allors License
// The GPL License is included in the file gpl.txt.
// The Allors License is an addendum to your contract.
// Allors Applications is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// For more information visit http://www.allors.com/legal
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.Linq;

namespace Allors.Database.Domain
{
    public partial class SalesInvoice
    {
        public void AviationPrint(PrintablePrint method)
        {
            if (this.InvoiceItems.Any(v => v.ExistWorkEffortBillingsWhereInvoiceItem))
            {
                var singleton = this.Strategy.Transaction.GetSingleton();
                var logo = this.BilledFrom?.ExistLogoImage == true ?
                    this.BilledFrom.LogoImage.MediaContent.Data :
                    singleton.LogoImage.MediaContent.Data;

                var images = new Dictionary<string, byte[]>
                {
                    { "Logo", logo },
                };

                var transaction = this.Strategy.Transaction;

                if (this.ExistInvoiceNumber)
                {
                    var barcodeGenerator = this.Strategy.Transaction.Database.Services.Get<IBarcodeGenerator>();
                    var barcode = barcodeGenerator.Generate(this.InvoiceNumber, BarcodeType.CODE_128, 320, 80, pure: true);
                    images.Add("Barcode", barcode);
                }

                foreach (var workEffort in this.WorkEfforts)
                {
                    if (workEffort is WorkTask workTask)
                    {
                        var barcodeGenerator = this.Strategy.Transaction.Database.Services.Get<IBarcodeGenerator>();
                        var barcode = barcodeGenerator.Generate(workTask.WorkEffortNumber, BarcodeType.CODE_128, 320, 80, pure: true);
                        images.Add(Allors.Database.Domain.Print.WorkTaskModel.Model.BarcodeName(workTask), barcode);
                    }
                }

                var printModel = new Print.WorkOrderSalesInvoiceModel.Model(this);
                this.RenderPrintDocument(this.BilledFrom?.WorkOrderSalesInvoiceTemplate, printModel, images);

                this.PrintDocument.Media.InFileName = $"{this.InvoiceNumber}.odt";

                method.StopPropagation = true;
            }
        }

        public SalesInvoice AviationCredit(SalesInvoiceCredit method)
        {
            var creditNote = new SalesInvoiceBuilder(this.Strategy.Transaction)
                .WithCreditedFromInvoice(this)
                .WithPurchaseInvoice(this.PurchaseInvoice)
                .WithBilledFrom(this.BilledFrom)
                .WithAssignedCurrency(this.DerivedCurrency)
                .WithAssignedBilledFromContactMechanism(this.DerivedBilledFromContactMechanism)
                .WithBilledFromContactPerson(this.BilledFromContactPerson)
                .WithBillToCustomer(this.BillToCustomer)
                .WithAssignedBillToContactMechanism(this.DerivedBillToContactMechanism)
                .WithBillToContactPerson(this.BillToContactPerson)
                .WithBillToEndCustomer(this.BillToEndCustomer)
                .WithAssignedBillToEndCustomerContactMechanism(this.DerivedBillToEndCustomerContactMechanism)
                .WithBillToEndCustomerContactPerson(this.BillToEndCustomerContactPerson)
                .WithShipToCustomer(this.ShipToCustomer)
                .WithAssignedShipToAddress(this.DerivedShipToAddress)
                .WithShipToContactPerson(this.ShipToContactPerson)
                .WithShipToEndCustomer(this.ShipToEndCustomer)
                .WithAssignedShipToEndCustomerAddress(this.DerivedShipToEndCustomerAddress)
                .WithShipToEndCustomerContactPerson(this.ShipToEndCustomerContactPerson)
                .WithDescription(this.Description)
                .WithStore(this.Store)
                .WithInvoiceDate(this.Transaction().Now())
                .WithSalesChannel(this.SalesChannel)
                .WithSalesInvoiceType(new SalesInvoiceTypes(this.Strategy.Transaction).CreditNote)
                .WithAssignedVatRegime(this.AssignedVatRegime)
                .WithAssignedIrpfRegime(this.AssignedIrpfRegime)
                .WithCustomerReference(this.CustomerReference)
                .WithAssignedPaymentMethod(this.DerivedPaymentMethod)
                .WithBillingAccount(this.BillingAccount)
                .Build();

            foreach (OrderAdjustment orderAdjustment in this.OrderAdjustments)
            {
                OrderAdjustment newAdjustment = null;
                if (orderAdjustment.GetType().Name.Equals(typeof(DiscountAdjustment).Name))
                {
                    newAdjustment = new DiscountAdjustmentBuilder(this.Transaction()).Build();
                }

                if (orderAdjustment.GetType().Name.Equals(typeof(SurchargeAdjustment).Name))
                {
                    newAdjustment = new SurchargeAdjustmentBuilder(this.Transaction()).Build();
                }

                if (orderAdjustment.GetType().Name.Equals(typeof(Fee).Name))
                {
                    newAdjustment = new FeeBuilder(this.Transaction()).Build();
                }

                if (orderAdjustment.GetType().Name.Equals(typeof(ShippingAndHandlingCharge).Name))
                {
                    newAdjustment = new ShippingAndHandlingChargeBuilder(this.Transaction()).Build();
                }

                if (orderAdjustment.GetType().Name.Equals(typeof(MiscellaneousCharge).Name))
                {
                    newAdjustment = new MiscellaneousChargeBuilder(this.Transaction()).Build();
                }

                newAdjustment.Amount ??= orderAdjustment.Amount * -1;
                creditNote.AddOrderAdjustment(newAdjustment);
            }

            foreach (SalesInvoiceItem salesInvoiceItem in this.SalesInvoiceItems)
            {
                var invoiceItem = new SalesInvoiceItemBuilder(this.Strategy.Transaction)
                    .WithInvoiceItemType(salesInvoiceItem.InvoiceItemType)
                    .WithAssignedUnitPrice(salesInvoiceItem.UnitPrice)
                    .WithProduct(salesInvoiceItem.Product)
                    .WithQuantity(salesInvoiceItem.Quantity)
                    .WithAssignedVatRegime(salesInvoiceItem.DerivedVatRegime)
                    .WithAssignedIrpfRegime(salesInvoiceItem.DerivedIrpfRegime)
                    .WithDescription(salesInvoiceItem.Description)
                    .WithSerialisedItem(salesInvoiceItem.SerialisedItem)
                    .WithComment(salesInvoiceItem.Comment)
                    .WithInternalComment(salesInvoiceItem.InternalComment)
                    .WithFacility(salesInvoiceItem.Facility)
                    .WithSerialisedItemVersionBeforeSale(salesInvoiceItem.SerialisedItemVersionBeforeSale)
                    .Build();

                invoiceItem.ProductFeatures = salesInvoiceItem.ProductFeatures;
                creditNote.AddSalesInvoiceItem(invoiceItem);

                foreach (WorkEffortBilling workEffortBilling in salesInvoiceItem.WorkEffortBillingsWhereInvoiceItem)
                {
                    new WorkEffortBillingBuilder(this.Strategy.Transaction)
                        .WithWorkEffort(workEffortBilling.WorkEffort)
                        .WithInvoiceItem(invoiceItem)
                        .Build();

                    workEffortBilling.WorkEffort.CanInvoice = true;
                }
            }

            //A credit note issued to other internal organisation should be received there as a purchase return
            if (creditNote.BillToCustomer is InternalOrganisation organisation
                && organisation.IsInternalOrganisation)
            {
                var purchaseReturn = new PurchaseInvoiceBuilder(this.Strategy.Transaction)
                    .WithBilledFrom((Organisation)this.BilledFrom)
                    .WithAssignedBilledFromContactMechanism(this.DerivedBilledFromContactMechanism)
                    .WithBilledFromContactPerson(this.BilledFromContactPerson)
                    .WithBilledTo(organisation)
                    .WithBilledToContactPerson(this.BillToContactPerson)
                    .WithDescription(this.Description)
                    .WithInvoiceDate(this.Transaction().Now())
                    .WithPurchaseInvoiceType(new PurchaseInvoiceTypes(this.Strategy.Transaction).PurchaseReturn)
                    .WithAssignedVatRegime(this.DerivedVatRegime)
                    .WithAssignedIrpfRegime(this.DerivedIrpfRegime)
                    .WithCustomerReference(this.CustomerReference)
                    .Build();

                foreach (OrderAdjustment orderAdjustment in this.OrderAdjustments)
                {
                    OrderAdjustment newAdjustment = null;
                    if (orderAdjustment.GetType().Name.Equals(typeof(DiscountAdjustment).Name))
                    {
                        newAdjustment = new DiscountAdjustmentBuilder(this.Transaction()).Build();
                    }

                    if (orderAdjustment.GetType().Name.Equals(typeof(SurchargeAdjustment).Name))
                    {
                        newAdjustment = new SurchargeAdjustmentBuilder(this.Transaction()).Build();
                    }

                    if (orderAdjustment.GetType().Name.Equals(typeof(Fee).Name))
                    {
                        newAdjustment = new FeeBuilder(this.Transaction()).Build();
                    }

                    if (orderAdjustment.GetType().Name.Equals(typeof(ShippingAndHandlingCharge).Name))
                    {
                        newAdjustment = new ShippingAndHandlingChargeBuilder(this.Transaction()).Build();
                    }

                    if (orderAdjustment.GetType().Name.Equals(typeof(MiscellaneousCharge).Name))
                    {
                        newAdjustment = new MiscellaneousChargeBuilder(this.Transaction()).Build();
                    }

                    newAdjustment.Amount ??= orderAdjustment.Amount * -1;
                    purchaseReturn.AddOrderAdjustment(newAdjustment);
                }

                foreach (SalesInvoiceItem salesInvoiceItem in this.SalesInvoiceItems)
                {
                    var invoiceItem = new PurchaseInvoiceItemBuilder(this.Strategy.Transaction)
                        .WithInvoiceItemType(salesInvoiceItem.InvoiceItemType)
                        .WithAssignedUnitPrice(salesInvoiceItem.UnitPrice)
                        .WithPart(salesInvoiceItem.DerivedPart)
                        .WithQuantity(salesInvoiceItem.Quantity)
                        .WithAssignedVatRegime(salesInvoiceItem.DerivedVatRegime)
                        .WithAssignedIrpfRegime(salesInvoiceItem.DerivedIrpfRegime)
                        .WithDescription(salesInvoiceItem.Description)
                        .WithSerialisedItem(salesInvoiceItem.SerialisedItem)
                        .WithComment(salesInvoiceItem.Comment)
                        .WithInternalComment(salesInvoiceItem.InternalComment)
                        .Build();

                    purchaseReturn.AddPurchaseInvoiceItem(invoiceItem);
                }
            }

            method.StopPropagation = true;

            return creditNote;
        }
    }
}