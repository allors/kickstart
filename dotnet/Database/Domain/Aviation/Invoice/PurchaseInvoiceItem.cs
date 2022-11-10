// <copyright file="PurchaseInvoiceItem.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Linq;

namespace Allors.Database.Domain
{
    public partial class PurchaseInvoiceItem
    {
        public void AviationOnPostDerive(ObjectOnPostDerive method)
        {
            var derivation = method.Derivation;

            if (this.ExistInvoiceItemType
                && (this.InvoiceItemType.IsPartItem || this.InvoiceItemType.IsProductItem
                // TODO: Martien
                // || this.InvoiceItemType.IsGseUnmotorized || this.InvoiceItemType.IsTransport
                ))
            {
                derivation.Validation.AssertExists(this, this.Meta.Part);
            }
        }
    }
}
