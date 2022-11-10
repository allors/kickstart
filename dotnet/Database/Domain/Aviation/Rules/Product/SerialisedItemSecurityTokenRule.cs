// <copyright file="OrganisationCalculationRule.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using Allors.Database.Meta;
using Allors.Database.Derivations;
using Allors.Database.Domain.Derivations.Rules;

namespace Allors.Database.Domain
{
    public class SerialisedItemSecurityTokenRule : Rule
    {
        public SerialisedItemSecurityTokenRule(MetaPopulation m) : base(m, new Guid("40288d0f-47e0-42fc-afbd-bc2d59431609")) =>
            this.Patterns = new Pattern[]
            {
                m.SerialisedItem.RolePattern(v => v.DerivationTrigger),
                m.SerialisedItem.RolePattern(v => v.Ownership),
                m.SerialisedItem.RolePattern(v => v.OwnedBy),
                m.SerialisedItem.RolePattern(v => v.RentedBy),
                m.SerialisedItem.RolePattern(v => v.Buyer),
                m.SerialisedItem.RolePattern(v => v.Seller),
                m.RequestItem.RolePattern(v => v.SerialisedItem, v => v.SerialisedItem),
                m.QuoteItem.RolePattern(v => v.SerialisedItem, v => v.SerialisedItem),
                m.PurchaseOrderItem.RolePattern(v => v.SerialisedItem, v => v.SerialisedItem),
                m.PurchaseInvoiceItem.RolePattern(v => v.SerialisedItem, v => v.SerialisedItem),
                m.SalesOrderItem.RolePattern(v => v.SerialisedItem, v => v.SerialisedItem),
                m.SalesInvoiceItem.RolePattern(v => v.SerialisedItem, v => v.SerialisedItem),
                m.ShipmentItem.RolePattern(v => v.SerialisedItem, v => v.SerialisedItem),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var validation = cycle.Validation;
            var m = cycle.Transaction.Database.Services.Get<MetaPopulation>();

            foreach (var @this in matches.Cast<SerialisedItem>())
            {
                @this.DeriveSerialisedItemSecurityToken(validation);
            }
        }
    }

    public static class SerialisedItemSecurityTokenRuleExtensions
    {
        public static void DeriveSerialisedItemSecurityToken(this SerialisedItem @this, IValidation validation)
        {
            var m = @this.Strategy.Transaction.Database.Services.Get<MetaPopulation>();
            var transaction = @this.Strategy.Transaction;

            var internalOrganisations = transaction.Extent<Organisation>().Where(v => v.IsInternalOrganisation).ToArray();

            foreach (InternalOrganisation internalOrganisation in internalOrganisations)
            {
                internalOrganisation.RemoveSerialisedItem(@this);
            }

            @this.SecurityTokens = new []
            {
                new SecurityTokens(@this.Strategy.Transaction).DefaultSecurityToken,
            };

            foreach (InternalOrganisation internalOrganisation in internalOrganisations)
            {
                @this.AddSecurityToken(internalOrganisation.LocalEmployeeSecurityToken);
                @this.AddSecurityToken(internalOrganisation.LocalAdministratorSecurityToken);
            }

            foreach (RequestItem item in @this.RequestItemsWhereSerialisedItem)
            {
                item.RequestWhereRequestItem?.Recipient?.AddSerialisedItem(@this);
            }

            foreach (QuoteItem item in @this.QuoteItemsWhereSerialisedItem)
            {
                item.QuoteWhereQuoteItem?.Issuer?.AddSerialisedItem(@this);
            }

            foreach (PurchaseOrderItem item in @this.PurchaseOrderItemsWhereSerialisedItem)
            {
                item.PurchaseOrderWherePurchaseOrderItem?.OrderedBy?.AddSerialisedItem(@this);
            }

            foreach (PurchaseInvoiceItem item in @this.PurchaseInvoiceItemsWhereSerialisedItem)
            {
                item.PurchaseInvoiceWherePurchaseInvoiceItem?.BilledTo?.AddSerialisedItem(@this);
            }

            foreach (SalesOrderItem item in @this.SalesOrderItemsWhereSerialisedItem)
            {
                item.SalesOrderWhereSalesOrderItem?.TakenBy?.AddSerialisedItem(@this);
            }

            foreach (SalesInvoiceItem item in @this.SalesInvoiceItemsWhereSerialisedItem)
            {
                item.SalesInvoiceWhereSalesInvoiceItem?.BilledFrom?.AddSerialisedItem(@this);
            }

            foreach (WorkEffortFixedAssetAssignment item in @this.WorkEffortFixedAssetAssignmentsWhereFixedAsset)
            {
                item.Assignment?.ExecutedBy?.AddSerialisedItem(@this);
            }

            foreach (ShipmentItem item in @this.ShipmentItemsWhereSerialisedItem)
            {
                var shipment = item.ShipmentWhereShipmentItem;

                if (shipment.ShipFromParty is Organisation from && from.IsInternalOrganisation)
                {
                    from?.AddSerialisedItem(@this);
                }

                if (shipment.ShipToParty is Organisation to && to.IsInternalOrganisation)
                {
                    to?.AddSerialisedItem(@this);
                }
            }

            @this.Buyer?.AddSerialisedItem(@this);
            @this.Seller?.AddSerialisedItem(@this);

            if (@this.ExistOwnedBy && @this.OwnedBy is Organisation owner)
            {
                if (owner.IsInternalOrganisation)
                {
                    owner.AddSerialisedItem(@this);
                }
                else
                {
                    @this.AddSecurityToken(owner.ContactsSecurityToken);
                }
            }

            if (@this.ExistRentedBy && @this.RentedBy is Organisation lessee)
            {
                if (lessee.IsInternalOrganisation)
                {
                    lessee.AddSerialisedItem(@this);
                }
                else
                {
                    @this.AddSecurityToken(lessee.ContactsSecurityToken);
                }
            }
        }
    }
}
