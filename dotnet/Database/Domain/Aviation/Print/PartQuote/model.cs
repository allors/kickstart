// <copyright file="Model.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain.Print.PartQuoteModel
{
    using System.Collections.Generic;
    using System.Linq;

    public class Model
    {
        public Model(ProductQuote quote, Dictionary<string, byte[]> images)
        {
            var transaction = quote.Strategy.Transaction;

            this.Quote = new QuoteModel(quote, images);

            this.Issuer = new IssuerModel((Organisation)quote.Issuer);
            this.Receiver = new ReceiverModel(quote);

            this.QuoteItems = quote.QuoteItems.Select(v => new QuoteItemModel(v)).ToArray();

            var paymentTerm = new InvoiceTermTypes(transaction).PaymentNetDays;
            this.SalesTerms = quote.SalesTerms.Where(v => !v.TermType.Equals(paymentTerm)).Select(v => new SalesTermModel(v)).ToArray();
        }

        public QuoteModel Quote { get; }

        public IssuerModel Issuer { get; }

        public ReceiverModel Receiver { get; }

        public QuoteItemModel[] QuoteItems { get; }

        public SalesTermModel[] SalesTerms { get; }
    }
}
