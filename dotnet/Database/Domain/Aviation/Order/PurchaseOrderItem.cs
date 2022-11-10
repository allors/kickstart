// <copyright file="PurchaseOrderItem.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using System.Linq;

namespace Allors.Database.Domain
{
    public partial class PurchaseOrderItem
    {
        public void AviationOnInit(ObjectOnInit method)
        {
            if (!this.ExistStoredInFacility
                && this.ExistInvoiceItemType
                && (this.InvoiceItemType.IsPartItem || this.InvoiceItemType.IsProductItem || this.InvoiceItemType.IsGseUnmotorized))
            {
                this.StoredInFacility = this.PurchaseOrderWherePurchaseOrderItem?.StoredInFacility;

                if (!this.ExistStoredInFacility && this.PurchaseOrderWherePurchaseOrderItem?.OrderedBy?.StoresWhereInternalOrganisation.Count() == 1)
                {
                    this.StoredInFacility = this.PurchaseOrderWherePurchaseOrderItem.OrderedBy.StoresWhereInternalOrganisation.Single().DefaultFacility;
                }
            }

            method.StopPropagation = true;
        }

        public void AviationOnPostDerive(ObjectOnPostDerive method)
        {
            var derivation = method.Derivation;

            var orderedBy = PurchaseOrderWherePurchaseOrderItem?.OrderedBy;

            if (this.ExistInvoiceItemType
                && (this.InvoiceItemType.IsPartItem || this.InvoiceItemType.IsProductItem
                // TODO: Martien
                // || this.InvoiceItemType.IsGseUnmotorized || this.InvoiceItemType.IsTransport
                ))
            {
                derivation.Validation.AssertExists(this, this.Meta.Part);
                derivation.Validation.AssertExists(this, this.Meta.StoredInFacility);
            }

            this.SecurityTokens = new[]
            {
                new SecurityTokens(this.Transaction()).DefaultSecurityToken,
                    orderedBy?.PurchaseOrderApproverLevel1SecurityToken,
                    orderedBy?.PurchaseOrderApproverLevel2SecurityToken,
                    orderedBy?.LocalAdministratorSecurityToken,
                    orderedBy?.LocalSalesAccountManagerSecurityToken,
            };
        }
    }
}
