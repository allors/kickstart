// <copyright file="QuoteExtensions.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

using Allors.Database.Meta;
using System.Linq;

namespace Allors.Database.Domain
{
    public static partial class QuoteExtensions
    {
        private static bool AviationNeedsApproval(this Quote @this) =>
          @this.ValidQuoteItems.Any(v => v.InvoiceItemType.Equals(new InvoiceItemTypes(@this.Transaction()).ProductItem)
                                      && v.ExistSerialisedItem
                                      && (!v.SerialisedItem.ExistExpectedSalesPrice || v.UnitPrice < v.SerialisedItem.ExpectedSalesPrice));

        public static void AviationSetReadyForProcessing(this Quote @this, QuoteSetReadyForProcessing method)
        {
            @this.QuoteState = @this.AviationNeedsApproval()
                ? new QuoteStates(@this.Strategy.Transaction).AwaitingApproval : new QuoteStates(@this.Strategy.Transaction).Accepted;
            AviationSetItemState(@this);

            method.StopPropagation = true;
        }

        public static void AviationApprove(this Quote @this, QuoteApprove method)
        {
            @this.QuoteState = new QuoteStates(@this.Strategy.Transaction).Accepted;
            AviationSetItemState(@this);

            method.StopPropagation = true;
        }

        public static void AviationOrder(this Quote @this, QuoteOrder method)
        {
            @this.QuoteState = new QuoteStates(@this.Strategy.Transaction).Ordered;

            var quoteItemStates = new QuoteItemStates(@this.Transaction());
            foreach (QuoteItem quoteItem in @this.QuoteItems)
            {
                if (Equals(quoteItem.QuoteItemState, quoteItemStates.Accepted))
                {
                    quoteItem.QuoteItemState = quoteItemStates.Ordered;
                }
            }

            var salesOrder = new SalesOrderBuilder(@this.Strategy.Transaction)
                .WithTakenBy(@this.Issuer)
                .WithBillToCustomer(@this.Receiver)
                .WithDescription(@this.Description)
                .WithAssignedVatRegime(@this.DerivedVatRegime)
                .WithAssignedIrpfRegime(@this.DerivedIrpfRegime)
                .WithShipToContactPerson(@this.ContactPerson)
                .WithBillToContactPerson(@this.ContactPerson)
                .WithQuote(@this)
                .WithAssignedCurrency(@this.DerivedCurrency)
                .WithLocale(@this.DerivedLocale)
                .Build();

            var quoteItems = @this.ValidQuoteItems
                .Where(i => i.QuoteItemState.Equals(new QuoteItemStates(@this.Strategy.Transaction).Ordered))
                .ToArray();

            foreach (var quoteItem in quoteItems)
            {
                quoteItem.QuoteItemState = new QuoteItemStates(@this.Strategy.Transaction).Ordered;

                if (quoteItem.ExistSparePartDescription)
                {
                    var product = new NonUnifiedPartBuilder(@this.Transaction())
                        .WithName(quoteItem.SparePartDescription)
                        .WithInventoryItemKind(new InventoryItemKinds(@this.Transaction()).NonSerialised)
                        .WithUnitOfMeasure(quoteItem.UnitOfMeasure)
                        .WithDefaultFacility(@this.Issuer.FacilitiesWhereOwner.First())
                        .Build();

                    quoteItem.Product = product;
                    quoteItem.RemoveSparePartDescription();
                }

                var salesOrderItem = new SalesOrderItemBuilder(@this.Strategy.Transaction)
                        .WithInvoiceItemType(quoteItem.InvoiceItemType)
                        .WithSaleKind(quoteItem.SaleKind)
                        .WithRentalType(quoteItem.RentalType)
                        .WithAssignedDeliveryDate(quoteItem.EstimatedDeliveryDate)
                        .WithAssignedUnitPrice(quoteItem.UnitPrice)
                        .WithAssignedVatRegime(quoteItem.AssignedVatRegime)
                        .WithAssignedIrpfRegime(quoteItem.AssignedIrpfRegime)
                        .WithProduct(quoteItem.Product)
                        .WithSerialisedItem(quoteItem.SerialisedItem)
                        .WithProductFeature(quoteItem.ProductFeature)
                        .WithQuantityOrdered(quoteItem.Quantity)
                        .WithDescription(quoteItem.Details)
                        .WithInternalComment(quoteItem.InternalComment)
                        .Build();

                if (quoteItem.ExistRentalType)
                {
                    salesOrderItem.NextSerialisedItemAvailability = new SerialisedItemAvailabilities(@this.Transaction()).InRent;
                }
                else
                {
                    salesOrderItem.NextSerialisedItemAvailability = new SerialisedItemAvailabilities(@this.Transaction()).Sold;
                }

                salesOrder.AddSalesOrderItem(salesOrderItem);
            }

            foreach (var salesTerm in @this.SalesTerms)
            {
                if (salesTerm.GetType().Name == nameof(IncoTerm))
                {
                    salesOrder.AddSalesTerm(new IncoTermBuilder(@this.Strategy.Transaction)
                        .WithTermType(salesTerm.TermType)
                        .WithTermValue(salesTerm.TermValue)
                        .WithDescription(salesTerm.Description)
                        .Build());
                }

                if (salesTerm.GetType().Name == nameof(InvoiceTerm))
                {
                    salesOrder.AddSalesTerm(new InvoiceTermBuilder(@this.Strategy.Transaction)
                        .WithTermType(salesTerm.TermType)
                        .WithTermValue(salesTerm.TermValue)
                        .WithDescription(salesTerm.Description)
                        .Build());
                }

                if (salesTerm.GetType().Name == nameof(OrderTerm))
                {
                    salesOrder.AddSalesTerm(new OrderTermBuilder(@this.Strategy.Transaction)
                        .WithTermType(salesTerm.TermType)
                        .WithTermValue(salesTerm.TermValue)
                        .WithDescription(salesTerm.Description)
                        .Build());
                }

                if (salesTerm.GetType().Name == nameof(QuoteTerm))
                {
                    salesOrder.AddSalesTerm(new QuoteTermBuilder(@this.Strategy.Transaction)
                        .WithTermType(salesTerm.TermType)
                        .WithTermValue(salesTerm.TermValue)
                        .WithDescription(salesTerm.Description)
                        .Build());
                }
            }

            method.StopPropagation = true;
        }

        public static Quote AviationCopy(this Quote @this, QuoteCopy method)
        {
            Quote copy = null;
            if (@this.GetType().Name.Equals(typeof(ProductQuote).Name))
            {
                copy = new ProductQuoteBuilder(@this.Transaction()).Build();
            }

            if (@this.GetType().Name.Equals(typeof(Proposal).Name))
            {
                copy = new ProposalBuilder(@this.Transaction()).Build();
            }

            if (@this.GetType().Name.Equals(typeof(StatementOfWork).Name))
            {
                copy = new StatementOfWorkBuilder(@this.Transaction()).Build();
            }

            copy.Issuer = @this.Issuer;
            copy.Receiver = @this.Receiver;
            copy.ContactPerson = @this.ContactPerson;
            copy.ValidFromDate = @this.ValidFromDate;
            copy.ValidThroughDate = @this.ValidThroughDate;
            copy.RequiredResponseDate = @this.RequiredResponseDate;
            copy.FullfillContactMechanism = @this.FullfillContactMechanism;
            copy.AssignedCurrency = @this.AssignedCurrency;
            copy.AssignedVatRegime = @this.AssignedVatRegime;
            copy.AssignedIrpfRegime = @this.AssignedIrpfRegime;
            copy.AssignedVatClause = @this.AssignedVatClause;
            copy.Description = @this.Description;
            copy.InternalComment = @this.InternalComment;

            foreach (var localisedComment in @this.LocalisedComments)
            {
                copy.AddLocalisedComment(new LocalisedTextBuilder(@this.Transaction()).WithLocale(localisedComment.Locale).WithText(localisedComment.Text).Build());
            }

            foreach (var salesTerm in @this.SalesTerms)
            {
                if (salesTerm.GetType().Name == nameof(IncoTerm))
                {
                    copy.AddSalesTerm(new IncoTermBuilder(@this.Transaction())
                        .WithTermType(salesTerm.TermType)
                        .WithTermValue(salesTerm.TermValue)
                        .WithDescription(salesTerm.Description)
                        .Build());
                }

                if (salesTerm.GetType().Name == nameof(InvoiceTerm))
                {
                    copy.AddSalesTerm(new InvoiceTermBuilder(@this.Transaction())
                        .WithTermType(salesTerm.TermType)
                        .WithTermValue(salesTerm.TermValue)
                        .WithDescription(salesTerm.Description)
                        .Build());
                }

                if (salesTerm.GetType().Name == nameof(OrderTerm))
                {
                    copy.AddSalesTerm(new OrderTermBuilder(@this.Transaction())
                        .WithTermType(salesTerm.TermType)
                        .WithTermValue(salesTerm.TermValue)
                        .WithDescription(salesTerm.Description)
                        .Build());
                }

                if (salesTerm.GetType().Name == nameof(QuoteTerm))
                {
                    copy.AddSalesTerm(new QuoteTermBuilder(@this.Transaction())
                        .WithTermType(salesTerm.TermType)
                        .WithTermValue(salesTerm.TermValue)
                        .WithDescription(salesTerm.Description)
                        .Build());
                }
            }

            foreach (var orderAdjustment in @this.OrderAdjustments)
            {
                OrderAdjustment newAdjustment = null;
                if (orderAdjustment.GetType().Name.Equals(typeof(DiscountAdjustment).Name))
                {
                    newAdjustment = new DiscountAdjustmentBuilder(@this.Transaction()).Build();
                }

                if (orderAdjustment.GetType().Name.Equals(typeof(SurchargeAdjustment).Name))
                {
                    newAdjustment = new SurchargeAdjustmentBuilder(@this.Transaction()).Build();
                }

                if (orderAdjustment.GetType().Name.Equals(typeof(Fee).Name))
                {
                    newAdjustment = new FeeBuilder(@this.Transaction()).Build();
                }

                if (orderAdjustment.GetType().Name.Equals(typeof(ShippingAndHandlingCharge).Name))
                {
                    newAdjustment = new ShippingAndHandlingChargeBuilder(@this.Transaction()).Build();
                }

                if (orderAdjustment.GetType().Name.Equals(typeof(MiscellaneousCharge).Name))
                {
                    newAdjustment = new MiscellaneousChargeBuilder(@this.Transaction()).Build();
                }

                newAdjustment.Amount ??= orderAdjustment.Amount;
                newAdjustment.Percentage ??= orderAdjustment.Percentage;
                copy.AddOrderAdjustment(newAdjustment);
            }

            foreach (var quoteItem in @this.QuoteItems.Where(v => !v.ExistQuoteItemWhereQuotedWithFeature))
            {
                AviationCopyQuoteItem(@this, copy, quoteItem);
            }

            method.StopPropagation = true;

            return copy;
        }

        private static QuoteItem AviationCopyQuoteItem(Quote @this, Quote copy, QuoteItem quoteItem)
        {
            var itemCopy = new QuoteItemBuilder(@this.Strategy.Transaction)
                .WithInvoiceItemType(quoteItem.InvoiceItemType)
                .WithSaleKind(quoteItem.SaleKind)
                .WithRentalType(quoteItem.RentalType)
                .WithProduct(quoteItem.Product)
                .WithSerialisedItem(quoteItem.SerialisedItem)
                .WithUnitOfMeasure(quoteItem.UnitOfMeasure)
                .WithQuantity(quoteItem.Quantity)
                .WithAssignedUnitPrice(quoteItem.AssignedUnitPrice)
                .WithAssignedVatRegime(quoteItem.AssignedVatRegime)
                .WithAssignedIrpfRegime(quoteItem.AssignedIrpfRegime)
                .WithAuthorizer(quoteItem.Authorizer)
                .WithDetails(quoteItem.Details)
                .WithInternalComment(quoteItem.InternalComment)
                .WithDeliverable(quoteItem.Deliverable)
                .WithSkill(quoteItem.Skill)
                .WithWorkEffort(quoteItem.WorkEffort)
                .Build();

            copy.AddQuoteItem(itemCopy);

            foreach (var featureItem in quoteItem.QuotedWithFeatures)
            {
                CopyQuoteItem(@this, copy, featureItem);
            }

            foreach (var orderAdjustment in quoteItem.DiscountAdjustments)
            {
                itemCopy.AddDiscountAdjustment(new DiscountAdjustmentBuilder(@this.Transaction())
                    .WithAmount(orderAdjustment.Amount)
                    .WithPercentage(orderAdjustment.Percentage)
                    .Build());
            }

            foreach (var orderAdjustment in quoteItem.SurchargeAdjustments)
            {
                itemCopy.AddSurchargeAdjustment(new SurchargeAdjustmentBuilder(@this.Transaction())
                    .WithAmount(orderAdjustment.Amount)
                    .WithPercentage(orderAdjustment.Percentage)
                    .Build());
            }

            foreach (var localisedComment in quoteItem.LocalisedComments)
            {
                itemCopy.AddLocalisedComment(new LocalisedTextBuilder(@this.Transaction()).WithLocale(localisedComment.Locale).WithText(localisedComment.Text).Build());
            }

            foreach (var salesTerm in quoteItem.SalesTerms)
            {
                if (salesTerm.GetType().Name == nameof(IncoTerm))
                {
                    itemCopy.AddSalesTerm(new IncoTermBuilder(@this.Transaction())
                        .WithTermType(salesTerm.TermType)
                        .WithTermValue(salesTerm.TermValue)
                        .WithDescription(salesTerm.Description)
                        .Build());
                }

                if (salesTerm.GetType().Name == nameof(InvoiceTerm))
                {
                    itemCopy.AddSalesTerm(new InvoiceTermBuilder(@this.Transaction())
                        .WithTermType(salesTerm.TermType)
                        .WithTermValue(salesTerm.TermValue)
                        .WithDescription(salesTerm.Description)
                        .Build());
                }

                if (salesTerm.GetType().Name == nameof(OrderTerm))
                {
                    itemCopy.AddSalesTerm(new OrderTermBuilder(@this.Transaction())
                        .WithTermType(salesTerm.TermType)
                        .WithTermValue(salesTerm.TermValue)
                        .WithDescription(salesTerm.Description)
                        .Build());
                }

                if (salesTerm.GetType().Name == nameof(QuoteTerm))
                {
                    itemCopy.AddSalesTerm(new QuoteTermBuilder(@this.Transaction())
                        .WithTermType(salesTerm.TermType)
                        .WithTermValue(salesTerm.TermValue)
                        .WithDescription(salesTerm.Description)
                        .Build());
                }
            }

            return itemCopy;
        }

        public static void AviationSetItemState(this Quote @this)
        {
            var quoteItemStates = new QuoteItemStates(@this.Strategy.Transaction);

            foreach (var quoteItem in @this.QuoteItems)
            {
                if (@this.QuoteState.IsCreated)
                {
                    quoteItem.QuoteItemState = new QuoteItemStates(@this.Strategy.Transaction).Draft;
                }

                if (@this.QuoteState.IsCancelled)
                {
                    if (!Equals(quoteItem.QuoteItemState, quoteItemStates.Rejected))
                    {
                        quoteItem.QuoteItemState = new QuoteItemStates(@this.Strategy.Transaction).Cancelled;
                    }
                }

                if (@this.QuoteState.IsRejected)
                {
                    if (!Equals(quoteItem.QuoteItemState, quoteItemStates.Cancelled))
                    {
                        quoteItem.QuoteItemState = new QuoteItemStates(@this.Strategy.Transaction).Rejected;
                    }
                }

                if (@this.QuoteState.IsAwaitingApproval)
                {
                    if (Equals(quoteItem.QuoteItemState, quoteItemStates.Draft))
                    {
                        quoteItem.QuoteItemState = new QuoteItemStates(@this.Strategy.Transaction).AwaitingApproval;
                    }
                }

                if (@this.QuoteState.IsInProcess)
                {
                    if (!Equals(quoteItem.QuoteItemState, quoteItemStates.Cancelled)
                        && !Equals(quoteItem.QuoteItemState, quoteItemStates.Rejected))
                    {
                        quoteItem.QuoteItemState = new QuoteItemStates(@this.Strategy.Transaction).InProcess;
                    }
                }

                if (@this.QuoteState.IsAccepted)
                {
                    if (Equals(quoteItem.QuoteItemState, quoteItemStates.Draft)
                        || Equals(quoteItem.QuoteItemState, quoteItemStates.AwaitingApproval)
                        || Equals(quoteItem.QuoteItemState, quoteItemStates.AwaitingAcceptance))
                    {
                        quoteItem.QuoteItemState = new QuoteItemStates(@this.Strategy.Transaction).Accepted;
                    }
                }

                if (@this.QuoteState.IsOrdered)
                {
                    if (Equals(quoteItem.QuoteItemState, quoteItemStates.Accepted))
                    {
                        quoteItem.QuoteItemState = new QuoteItemStates(@this.Strategy.Transaction).Ordered;
                    }
                }
            }
        }
    }
}
