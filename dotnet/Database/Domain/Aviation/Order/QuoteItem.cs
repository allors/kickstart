// <copyright file="QuoteItem.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    public partial class QuoteItem
    {
        public void AviationOnPostDerive(ObjectOnPostDerive method)
        {
            if (this.ExistInvoiceItemType && this.InvoiceItemType.IsProductItem)
            {
                method.Derivation.Validation.AssertExists(this, this.Meta.SaleKind);
            }

            if (this.ExistInvoiceItemType && this.InvoiceItemType.IsProductItem && this.ExistSaleKind && this.SaleKind.IsRental)
            {
                method.Derivation.Validation.AssertExists(this, this.Meta.RentalType);
            }
        }
    }
}
