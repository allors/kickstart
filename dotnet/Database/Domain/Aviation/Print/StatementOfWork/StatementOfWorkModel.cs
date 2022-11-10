// <copyright file="InvoiceItemModel.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Collections.Generic;
using System.Globalization;

namespace Allors.Database.Domain.Print.StatementOfWorkModel
{
    public class StatementOfWorkModel
    {
        public StatementOfWorkModel(StatementOfWork sow)
        {
            this.Description = sow.Description?.Split('\n');
            this.Number = sow.QuoteNumber;
            this.IssueDate = sow.IssueDate.ToString("yyyy-MM-dd");
            this.ValidFromDate = (sow.ValidFromDate ?? sow.IssueDate).ToString("yyyy-MM-dd");
            this.ValidThroughDate = sow.ValidThroughDate?.ToString("yyyy-MM-dd");

            // TODO: Where does the currency come from?
            var currency = "€";
            this.SubTotal = sow.TotalListPrice.ToString("N2", new CultureInfo("nl-BE")) + " " + currency;
            this.TotalExVat = sow.TotalExVat.ToString("N2", new CultureInfo("nl-BE")) + " " + currency;
            this.VatCharge = sow.DerivedVatRate?.Rate.ToString("n2");
            this.TotalVat = sow.TotalVat.ToString("N2", new CultureInfo("nl-BE")) + " " + currency;
            this.IrpfRate = sow.DerivedIrpfRate?.Rate.ToString("n2");

            // IRPF is subtracted for total amount to pay
            var totalIrpf = sow.TotalIrpf * -1;
            this.TotalIrpf = totalIrpf.ToString("N2", new CultureInfo("nl-BE")) + " " + currency;
            this.PrintIrpf = sow.TotalIrpf != 0;

            this.TotalIncVat = sow.TotalIncVat.ToString("N2", new CultureInfo("nl-BE")) + " " + currency;
            this.GrandTotal = sow.GrandTotal.ToString("N2", new CultureInfo("nl-BE")) + " " + currency;
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
