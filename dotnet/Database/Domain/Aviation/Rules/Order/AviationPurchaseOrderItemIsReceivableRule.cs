// <copyright file="CustomPurchaseOrderItemIsReceivableDerivation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Meta;
    using Database.Derivations;
    using Allors.Database.Domain.Derivations.Rules;

    public class AviationPurchaseOrderItemIsReceivableRule : Rule
    {
        public AviationPurchaseOrderItemIsReceivableRule(MetaPopulation m) : base(m, new Guid("8944c195-6374-4f91-aee2-208f8a04483b")) =>
            this.Patterns = new Pattern[]
            {
                m.PurchaseOrderItem.RolePattern(v => v.InvoiceItemType),
                m.PurchaseOrderItem.RolePattern(v => v.Part),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;

            foreach (var @this in matches.Cast<PurchaseOrderItem>())
            {
                @this.IsReceivable = @this.ExistPart
                    && @this.ExistInvoiceItemType
                    && (@this.InvoiceItemType.Equals(new InvoiceItemTypes(transaction).PartItem)
                        || @this.InvoiceItemType.Equals(new InvoiceItemTypes(transaction).ProductItem)
                        || @this.InvoiceItemType.Equals(new InvoiceItemTypes(transaction).GseUnmotorized));
            }
        }
    }
}
