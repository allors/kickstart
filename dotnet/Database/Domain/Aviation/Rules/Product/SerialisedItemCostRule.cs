// <copyright file="SerialisedItemCostDerivation.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using System;
using System.Collections.Generic;
using System.Linq;
using Allors.Database.Meta;
using Allors.Database.Derivations;
using Resources;
using Allors.Database.Domain.Derivations.Rules;

namespace Allors.Database.Domain
{
    public class SerialisedItemCostRule : Rule
    {
        public SerialisedItemCostRule(MetaPopulation m) : base(m, new Guid("fc0b7fb8-d5a2-43b4-811f-28b26eb7b7b1")) =>
            this.Patterns = new Pattern[]
            {
                m.SerialisedItem.RolePattern(v => v.Ownership),
                m.SerialisedItem.RolePattern(v => v.Buyer),
                m.SerialisedItem.RolePattern(v => v.Seller),
                m.WorkEffort.RolePattern(v => v.WorkEffortState, v => v.WorkEffortFixedAssetAssignmentsWhereAssignment.WorkEffortFixedAssetAssignment.FixedAsset, m.SerialisedItem),
                m.WorkEffort.RolePattern(v => v.Customer, v => v.WorkEffortFixedAssetAssignmentsWhereAssignment.WorkEffortFixedAssetAssignment.FixedAsset, m.SerialisedItem),
                m.WorkEffort.RolePattern(v => v.ExecutedBy, v => v.WorkEffortFixedAssetAssignmentsWhereAssignment.WorkEffortFixedAssetAssignment.FixedAsset, m.SerialisedItem),
                m.WorkEffort.RolePattern(v => v.TotalCost, v => v.WorkEffortFixedAssetAssignmentsWhereAssignment.WorkEffortFixedAssetAssignment.FixedAsset, m.SerialisedItem),
                m.WorkEffort.RolePattern(v => v.TotalRevenue, v => v.WorkEffortFixedAssetAssignmentsWhereAssignment.WorkEffortFixedAssetAssignment.FixedAsset, m.SerialisedItem),
                m.PurchaseInvoiceItem.RolePattern(v => v.InvoiceItemType, v => v.SerialisedItem),
                m.PurchaseInvoiceItem.RolePattern(v => v.TotalExVat, v => v.SerialisedItem),
                m.PurchaseInvoice.RolePattern(v => v.BilledTo, v => v.PurchaseInvoiceItems.PurchaseInvoiceItem.SerialisedItem),
                m.PurchaseInvoice.RolePattern(v => v.DerivedCurrency, v => v.PurchaseInvoiceItems.PurchaseInvoiceItem.SerialisedItem),
                m.PurchaseInvoice.RolePattern(v => v.InvoiceDate, v => v.PurchaseInvoiceItems.PurchaseInvoiceItem.SerialisedItem),
                m.SerialisedItem.AssociationPattern(v => v.WorkEffortFixedAssetAssignmentsWhereFixedAsset),
                m.SerialisedItem.AssociationPattern(v => v.PurchaseInvoiceItemsWhereSerialisedItem),
            };

        public override void Derive(ICycle cycle, IEnumerable<IObject> matches)
        {
            var transaction = cycle.Transaction;
            var validation = cycle.Validation;

            foreach (var @this in matches.Cast<SerialisedItem>())
            {
                @this.DeriveSerialisedItemCost(validation);
            }
        }
    }

    public static class SerialisedItemCostRuleExtensions
    {
        public static void DeriveSerialisedItemCost(this SerialisedItem @this, IValidation validation)
        {
            var internalWorkOwn = @this.WorkEffortFixedAssetAssignmentsWhereFixedAsset
                .Where(v => (v.ExistAssignment
                            && (v.Assignment.WorkEffortState.Equals(new WorkEffortStates(@this.Transaction()).Completed)
                                || v.Assignment.WorkEffortState.Equals(new WorkEffortStates(@this.Transaction()).Finished)))
                            && v.Assignment.Customer?.GetType().Name == typeof(Organisation).Name
                            && ((Organisation)v.Assignment.Customer).IsInternalOrganisation
                            && (v.Assignment.Customer).Equals(v.Assignment.ExecutedBy)
                            && ((SerialisedItem)v.FixedAsset).ExistOwnership
                            && ((SerialisedItem)v.FixedAsset).Ownership.Equals(new Ownerships(@this.Transaction()).Own))
                .Sum(v => v.Assignment.TotalCost);

            var internalWorkOthers = @this.WorkEffortFixedAssetAssignmentsWhereFixedAsset
                .Where(v => (v.ExistAssignment
                            && (v.Assignment.WorkEffortState.Equals(new WorkEffortStates(@this.Transaction()).Completed)
                                || v.Assignment.WorkEffortState.Equals(new WorkEffortStates(@this.Transaction()).Finished)))
                            && v.Assignment.Customer?.GetType().Name == typeof(Organisation).Name
                            && ((Organisation)v.Assignment.Customer).IsInternalOrganisation
                            && !(v.Assignment.Customer).Equals(v.Assignment.ExecutedBy))
                .Sum(v => v.Assignment.TotalRevenue);

            var externalWorkToAdd = @this.PurchaseInvoiceItemsWhereSerialisedItem
                .Where(v => v.InvoiceItemType.IsRepairAndMaintenance
                            && v.PurchaseInvoiceWherePurchaseInvoiceItem.ExistPurchaseInvoiceType
                            && v.PurchaseInvoiceWherePurchaseInvoiceItem.PurchaseInvoiceType.IsPurchaseInvoice
                            && v.PurchaseInvoiceWherePurchaseInvoiceItem.ExistDerivedCurrency
                            && v.PurchaseInvoiceWherePurchaseInvoiceItem.ExistInvoiceDate
                            && v.PurchaseInvoiceWherePurchaseInvoiceItem.ExistBilledTo)
                .Sum(v => Currencies.ConvertCurrency(v.TotalExVat, v.PurchaseInvoiceWherePurchaseInvoiceItem.InvoiceDate, v.PurchaseInvoiceWherePurchaseInvoiceItem.DerivedCurrency, v.PurchaseInvoiceWherePurchaseInvoiceItem.BilledTo.PreferredCurrency));

            var externalWorkToSubtract = @this.PurchaseInvoiceItemsWhereSerialisedItem
                .Where(v => v.InvoiceItemType.IsRepairAndMaintenance
                            && v.PurchaseInvoiceWherePurchaseInvoiceItem.ExistPurchaseInvoiceType
                            && v.PurchaseInvoiceWherePurchaseInvoiceItem.PurchaseInvoiceType.IsPurchaseReturn
                            && v.PurchaseInvoiceWherePurchaseInvoiceItem.ExistDerivedCurrency
                            && v.PurchaseInvoiceWherePurchaseInvoiceItem.ExistInvoiceDate
                            && v.PurchaseInvoiceWherePurchaseInvoiceItem.ExistBilledTo)
                .Sum(v => Currencies.ConvertCurrency(v.TotalExVat, v.PurchaseInvoiceWherePurchaseInvoiceItem.InvoiceDate, v.PurchaseInvoiceWherePurchaseInvoiceItem.DerivedCurrency, v.PurchaseInvoiceWherePurchaseInvoiceItem.BilledTo.PreferredCurrency));

            @this.ActualRefurbishCost = internalWorkOwn + internalWorkOthers + externalWorkToAdd - externalWorkToSubtract;

            var actualTransportCostToAdd = @this.PurchaseInvoiceItemsWhereSerialisedItem
                .Where(v => v.InvoiceItemType.IsTransport
                            && v.PurchaseInvoiceWherePurchaseInvoiceItem.ExistPurchaseInvoiceType
                            && v.PurchaseInvoiceWherePurchaseInvoiceItem.PurchaseInvoiceType.IsPurchaseInvoice
                            && v.PurchaseInvoiceWherePurchaseInvoiceItem.ExistDerivedCurrency
                            && v.PurchaseInvoiceWherePurchaseInvoiceItem.ExistInvoiceDate
                            && v.PurchaseInvoiceWherePurchaseInvoiceItem.ExistBilledTo)
                .Sum(v => Currencies.ConvertCurrency(v.TotalExVat, v.PurchaseInvoiceWherePurchaseInvoiceItem.InvoiceDate, v.PurchaseInvoiceWherePurchaseInvoiceItem.DerivedCurrency, v.PurchaseInvoiceWherePurchaseInvoiceItem.BilledTo.PreferredCurrency));

            var actualTransportCostToSubtract = @this.PurchaseInvoiceItemsWhereSerialisedItem
                .Where(v => v.InvoiceItemType.IsTransport
                            && v.PurchaseInvoiceWherePurchaseInvoiceItem.ExistPurchaseInvoiceType
                            && v.PurchaseInvoiceWherePurchaseInvoiceItem.PurchaseInvoiceType.IsPurchaseReturn
                            && v.PurchaseInvoiceWherePurchaseInvoiceItem.ExistDerivedCurrency
                            && v.PurchaseInvoiceWherePurchaseInvoiceItem.ExistInvoiceDate
                            && v.PurchaseInvoiceWherePurchaseInvoiceItem.ExistBilledTo)
                .Sum(v => Currencies.ConvertCurrency(v.TotalExVat, v.PurchaseInvoiceWherePurchaseInvoiceItem.InvoiceDate, v.PurchaseInvoiceWherePurchaseInvoiceItem.DerivedCurrency, v.PurchaseInvoiceWherePurchaseInvoiceItem.BilledTo.PreferredCurrency));

            @this.ActualTransportCost = actualTransportCostToAdd - actualTransportCostToSubtract;

            var actualOtherCostToAdd = @this.PurchaseInvoiceItemsWhereSerialisedItem
                .Where(v => v.InvoiceItemType.IsOther
                            && v.PurchaseInvoiceWherePurchaseInvoiceItem.ExistPurchaseInvoiceType
                            && v.PurchaseInvoiceWherePurchaseInvoiceItem.PurchaseInvoiceType.IsPurchaseInvoice
                            && v.PurchaseInvoiceWherePurchaseInvoiceItem.ExistDerivedCurrency
                            && v.PurchaseInvoiceWherePurchaseInvoiceItem.ExistInvoiceDate
                            && v.PurchaseInvoiceWherePurchaseInvoiceItem.ExistBilledTo)
                .Sum(v => Currencies.ConvertCurrency(v.TotalExVat, v.PurchaseInvoiceWherePurchaseInvoiceItem.InvoiceDate, v.PurchaseInvoiceWherePurchaseInvoiceItem.DerivedCurrency, v.PurchaseInvoiceWherePurchaseInvoiceItem.BilledTo.PreferredCurrency));

            var actualOtherCostToSbutract = @this.PurchaseInvoiceItemsWhereSerialisedItem
                .Where(v => v.InvoiceItemType.IsOther
                            && v.PurchaseInvoiceWherePurchaseInvoiceItem.ExistPurchaseInvoiceType
                            && v.PurchaseInvoiceWherePurchaseInvoiceItem.PurchaseInvoiceType.IsPurchaseReturn
                            && v.PurchaseInvoiceWherePurchaseInvoiceItem.ExistDerivedCurrency
                            && v.PurchaseInvoiceWherePurchaseInvoiceItem.ExistInvoiceDate
                            && v.PurchaseInvoiceWherePurchaseInvoiceItem.ExistBilledTo)
                .Sum(v => Currencies.ConvertCurrency(v.TotalExVat, v.PurchaseInvoiceWherePurchaseInvoiceItem.InvoiceDate, v.PurchaseInvoiceWherePurchaseInvoiceItem.DerivedCurrency, v.PurchaseInvoiceWherePurchaseInvoiceItem.BilledTo.PreferredCurrency));

            @this.ActualOtherCost = actualOtherCostToAdd - actualOtherCostToSbutract;
        }
    }
}
