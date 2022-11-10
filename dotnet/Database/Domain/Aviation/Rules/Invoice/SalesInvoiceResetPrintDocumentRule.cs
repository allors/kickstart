// <copyright file="SalesInvoiceItemDescriptionDerivation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using Allors.Database.Meta;
using Allors.Database.Derivations;
using System.Text;
using Allors.Database.Domain.Derivations.Rules;

namespace Allors.Database.Domain
{
    public class SalesInvoiceResetPrintDocumentRule : Rule
    {
        public SalesInvoiceResetPrintDocumentRule(MetaPopulation m) : base(m, new Guid("787c53a2-dc67-49fb-9a32-9a31f9c8e0f6")) =>
            this.Patterns = new Pattern[]
            {
                m.SalesInvoice.RolePattern(v => v.PrintCondensed),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;
            var changeSet = cycle.ChangeSet;

            foreach (var @this in matches.Cast<SalesInvoice>())
            {
                @this.DeriveSalesInvoicePrintCondensed(validation);
            }
        }
    }

    public static class SalesInvoicePrintCondensedRuleExtensions
    {
        public static void DeriveSalesInvoicePrintCondensed(this SalesInvoice @this, IValidation validation)
        {
            @this.ResetPrintDocument();
        }
    }
}
