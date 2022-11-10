// <copyright file="Model.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Print.SalesInvoiceModel
{
    using System.Collections.Generic;
    using System.Linq;

    public class Model
    {
        public Model(SalesInvoice invoice)
        {
            var transaction = invoice.Strategy.Transaction;

            this.Invoice = new InvoiceModel(invoice);
            this.BilledFrom = new BilledFromModel((Organisation)invoice.BilledFrom, invoice.DerivedCurrency);
            this.BillTo = new BillToModel(invoice);
            this.ShipTo = new ShipToModel(invoice);

            if (invoice.PrintCondensed)
            {
                var invoiceItemByProduct = invoice.ValidInvoiceItems
                    .Where(v => ((SalesInvoiceItem)v).InvoiceItemType.IsProductItem)
                    .GroupBy(v => ((SalesInvoiceItem)v).Product)
                    .ToDictionary(v => v.Key, v => v);

                var invoiceItemByPart = invoice.ValidInvoiceItems
                    .Where(v => ((SalesInvoiceItem)v).InvoiceItemType.IsPartItem)
                    .GroupBy(v => ((SalesInvoiceItem)v).Part)
                    .ToDictionary(v => v.Key, v => v);

                var lines = new List<InvoiceItemModel>();

                foreach (var productItem in invoiceItemByProduct)
                {
                    var quantity = invoice.ValidInvoiceItems
                        .Where(v => ((SalesInvoiceItem)v).InvoiceItemType.IsProductItem && ((SalesInvoiceItem)v).Product.Equals(productItem.Key))
                        .Sum(v => v.Quantity);

                    var amount = invoice.ValidInvoiceItems
                        .Where(v => ((SalesInvoiceItem)v).InvoiceItemType.IsProductItem && ((SalesInvoiceItem)v).Product.Equals(productItem.Key))
                        .Sum(v => v.TotalExVat);

                    lines.Add(new InvoiceItemModel(new InvoiceItemTypes(transaction).ProductItem.Name, productItem.Key.Name, quantity, amount));
                }

                foreach (var partItem in invoiceItemByPart)
                {
                    var quantity = invoice.ValidInvoiceItems
                        .Where(v => ((SalesInvoiceItem)v).InvoiceItemType.IsPartItem && ((SalesInvoiceItem)v).Part.Equals(partItem.Key))
                        .Sum(v => v.Quantity);

                    var amount = invoice.ValidInvoiceItems
                        .Where(v => ((SalesInvoiceItem)v).InvoiceItemType.IsPartItem && ((SalesInvoiceItem)v).Part.Equals(partItem.Key))
                        .Sum(v => v.TotalExVat);

                    lines.Add(new InvoiceItemModel(new InvoiceItemTypes(transaction).PartItem.Name, partItem.Key.Name, quantity, amount));
                }

                var otherItems = invoice.ValidInvoiceItems
                    .Where(v => !((SalesInvoiceItem)v).InvoiceItemType.IsProductItem && !((SalesInvoiceItem)v).InvoiceItemType.IsPartItem)
                    .Select(v => v)
                    .ToList();

                lines.AddRange(otherItems.Select(v => new InvoiceItemModel((SalesInvoiceItem)v)));

                this.InvoiceItems = lines.ToArray();
            }
            else
            {
                this.InvoiceItems = invoice.ValidInvoiceItems.Select(v => new InvoiceItemModel((SalesInvoiceItem)v)).ToArray();
            }

            if (invoice.ExistOrderAdjustments)
            {
                this.OrderAdjustments = invoice.OrderAdjustments.Select(v => new OrderAdjustmentModel(v)).ToArray();
            }

            var paymentTerm = new InvoiceTermTypes(transaction).PaymentNetDays;
            this.SalesTerms = invoice.SalesTerms.Where(v => !v.TermType.Equals(paymentTerm)).Select(v => new SalesTermModel(v)).ToArray();
        }

        public InvoiceModel Invoice { get; }

        public BilledFromModel BilledFrom { get; }

        public BillToModel BillTo { get; }

        public ShipToModel ShipTo { get; }

        public InvoiceItemModel[] InvoiceItems { get; }

        public SalesTermModel[] SalesTerms { get; }

        public OrderAdjustmentModel[] OrderAdjustments { get; }
    }
}
