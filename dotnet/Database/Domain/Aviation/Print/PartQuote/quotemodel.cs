// <copyright file="QuoteModel.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Print.PartQuoteModel
{
    using System;
    using System.Collections.Generic;
    using System.Globalization;

    public class QuoteModel
    {
        public QuoteModel(ProductQuote quote, Dictionary<string, byte[]> imageByImageName)
        {
            var currencyIsoCode = quote.DerivedCurrency.IsoCode;

            this.Description = quote.Description?.Split('\n');
            this.Currency = currencyIsoCode;
            this.Number = quote.QuoteNumber;
            this.Date = quote.IssueDate.ToString("yyyy-MM-dd");

            // TODO: Where does the currency come from?
            this.SubTotal = quote.TotalBasePrice.ToString("N2", new CultureInfo("nl-BE"));
            this.TotalExVat = quote.TotalExVat.ToString("N2", new CultureInfo("nl-BE"));
            this.VatRate = quote.DerivedVatRate?.Rate.ToString("n2");
            this.TotalVat = quote.TotalVat.ToString("N2", new CultureInfo("nl-BE"));

            // IRPF is subtracted for total amount to pay
            var totalIrpf = quote.TotalIrpf * -1;
            this.IrpfRate = quote.DerivedIrpfRate?.Rate.ToString("n2");
            this.TotalIrpf = totalIrpf.ToString("N2", new CultureInfo("nl-BE"));
            this.PrintIrpf = quote.TotalIrpf != 0;

            this.TotalIncVat = quote.TotalIncVat.ToString("N2", new CultureInfo("nl-BE"));
            this.GrandTotal = currencyIsoCode + " " + quote.GrandTotal.ToString("N2", new CultureInfo("nl-BE"));
        }

        public string[] Description { get; }

        public string Number { get; }

        public string Date { get; }

        public string SubTotal { get; }

        public string TotalExVat { get; }

        public string VatRate { get; }

        public string TotalVat { get; }

        public string IrpfRate { get; }

        public string TotalIrpf { get; }

        public string TotalIncVat { get; }

        public string GrandTotal { get; }

        public string Currency { get; }

        public bool PrintIrpf { get; }
    }
}
