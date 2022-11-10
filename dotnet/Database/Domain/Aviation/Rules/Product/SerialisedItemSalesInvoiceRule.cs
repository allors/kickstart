// <copyright file="Domain.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Database.Derivations;
    using Meta;
    using Derivations.Rules;

    public class SerialisedItemSalesInvoiceRule : Rule
    {
        public SerialisedItemSalesInvoiceRule(MetaPopulation m) : base(m, new Guid("87ff1b93-e6b7-45bc-8e97-ce4ceedc4565")) =>
            this.Patterns = new Pattern[]
            {
                m.SalesInvoiceItem.RolePattern(v => v.SerialisedItem, v => v.SerialisedItem.SerialisedItem),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SerialisedItem>())
            {
                @this.DeriveSerialisedItemSalesInvoice(validation);
            }
        }
    }

    public static class SerialisedItemSalesInvoiceRuleExtensions
    {
        public static void DeriveSerialisedItemSalesInvoice(this SerialisedItem @this, IValidation validation)
        {
            var salesInvoice = @this.SalesInvoiceItemsWhereSerialisedItem.Select(v => v.SalesInvoiceWhereSalesInvoiceItem).OrderByDescending(v => v.InvoiceDate).FirstOrDefault();

            @this.SalesInvoiceNumber = salesInvoice?.InvoiceNumber;
            @this.BillToCustomerName = salesInvoice?.BillToCustomer?.DisplayName;

            var postalAddress = salesInvoice?.BillToCustomer?.GeneralCorrespondence as PostalAddress;
            @this.BillToCustomerCountryCode = postalAddress?.Country?.IsoCode;
        }
    }
}
