// <copyright file="InvoiceItemModel.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace Allors.Database.Domain.Print.ProductQuoteModel
{
    public class QuoteModel
    {
        public QuoteModel(Quote quote, Dictionary<string, byte[]> imageByImageName)
        {
            this.Description = quote.Description?.Split('\n');
            this.Number = quote.QuoteNumber;
            this.IssueDate = quote.IssueDate.ToString("yyyy-MM-dd");
            this.ValidFromDate = (quote.ValidFromDate ?? quote.IssueDate).ToString("yyyy-MM-dd");
            this.ValidThroughDate = quote.ValidThroughDate?.ToString("yyyy-MM-dd");

            // TODO: Where does the currency come from?
            var currency = "€";
            this.SubTotal = quote.TotalListPrice.ToString("N2", new CultureInfo("nl-BE")) + " " + currency;
            this.TotalExVat = quote.TotalExVat.ToString("N2", new CultureInfo("nl-BE")) + " " + currency;
            this.VatCharge = quote.DerivedVatRate?.Rate.ToString("n2");
            this.TotalVat = quote.TotalVat.ToString("N2", new CultureInfo("nl-BE")) + " " + currency;
            this.IrpfRate = quote.DerivedIrpfRate?.Rate.ToString("n2");

            // IRPF is subtracted for total amount to pay
            var totalIrpf = quote.TotalIrpf * -1;
            this.TotalIrpf = totalIrpf.ToString("N2", new CultureInfo("nl-BE")) + " " + currency;
            this.PrintIrpf = quote.TotalIrpf != 0;

            this.TotalIncVat = quote.TotalIncVat.ToString("N2", new CultureInfo("nl-BE")) + " " + currency;
            this.GrandTotal = quote.GrandTotal.ToString("N2", new CultureInfo("nl-BE")) + " " + currency;
        }

        public string[] Description { get; }

        public string Number { get; }

        public string IssueDate { get; }

        public string ValidFromDate { get; }

        public string ValidThroughDate { get; }

        public string SubTotal { get; }

        public string TotalExVat { get; }

        public string VatCharge { get; }

        public string TotalVat { get; }

        public string IrpfRate { get; }

        public string TotalIrpf { get; }

        public string TotalIncVat { get; }

        public string GrandTotal { get; }

        public bool PrintIrpf { get; }

    }
}
