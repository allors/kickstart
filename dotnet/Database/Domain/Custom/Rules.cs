// <copyright file="ObjectsBase.cs" company="Allors bvba">
// Copyright (c) Allors bvba. All rights reserved.
// Licensed under the LGPL license. See LICENSE file in the project root for full license information.
// </copyright>

namespace Allors.Database.Domain
{
    using Meta;
    using Allors.Database.Domain.Derivations.Rules;

    public static partial class Rules
    {
        public static Rule[] Create(MetaPopulation m) =>
            new Rule[]
            {
                // Core
                new UserNormalizedUserNameRule(m),
                new UserNormalizedUserEmailRule(m),
                new UserInUserPasswordRule(m),
                new GrantEffectiveUsersRule(m),
                new GrantEffectivePermissionsRule(m),
                new SecurityTokenSecurityStampRule(m),

                // Base
                new MediaRule(m),
                new NotificationListRule(m),
                new TransitionalDeniedPermissionRule(m),

                // Apps
                new AccountingPeriodRule(m),
                new AccountingTransactionRule(m),
                new AgreementProductApplicabilityRule(m),
                new AgreementTermRule(m),
                new AutomatedAgentDisplayNameRule(m),
                new AutomatedAgentRule(m),
                new BankAccountIbanRule(m),
                new BankAccountRule(m),
                new BankRule(m),
                new BasePriceOrderQuantityBreakRule(m),
                new BasePriceOrderValueRule(m),
                new BasePriceProductFeatureRule(m),
                new BasePriceProductRule(m),
                new BrandDeniedPermissionRule(m),
                new BudgetDeniedPermissionRule(m),
                new CarrierSearchStringRule(m),
                new CaseDeniedPermissionRule(m),
                new CashRule(m),
                new CatalogueImageRule(m),
                new CatalogueLocalisedDescriptionsRule(m),
                new CatalogueLocalisedNamesRule(m),
                new CatalogueSearchStringRule(m),
                new CommunicationEventDeniedPermissionRule(m),
                new CommunicationEventRule(m),
                new CommunicationTaskParticipantsRule(m),
                new CommunicationTaskRule(m),
                new CountryRule(m),
                new CountryVatRegimesRule(m),
                new CustomerRelationshipRule(m),
                new CustomerReturnDeniedPermissionRule(m),
                new CustomerReturnExistShipmentNumberRule(m),
                new CustomerReturnExistShipToAddressRule(m),
                new CustomerReturnRule(m),
                new CustomerReturnSearchStringRule(m),
                new CustomerShipmentDeniedPermissionRule(m),
                new CustomerShipmentExistShipmentNumberRule(m),
                new CustomerShipmentExistShipToAddressRule(m),
                new CustomerShipmentInvoiceRule(m),
                new CustomerShipmentRule(m),
                new CustomerShipmentSearchStringRule(m),
                new CustomerShipmentShipmentValueRule(m),
                new CustomerShipmentShipRule(m),
                new DeliverableBasedServiceDisplayNameRule(m),
                new DeliverableBasedServiceRule(m),
                new DeliverableBasedServiceSearchStringRule(m),
                new DiscountComponentRule(m),
                new DropShipmentDeniedPermissionRule(m),
                new DropShipmentExistShipmentNumberRule(m),
                new DropShipmentRule(m),
                new DropShipmentSearchStringRule(m),
                new DropShipmentShipToAddressRule(m),
                new EmailAddressDisplayNameRule(m),
                new EmailCommunicationRule(m),
                new EmailCommunicationSearchStringRule(m),
                new EmploymentRule(m),
                new EngagementBillToPartyRule(m),
                new EngagementPlacingPartyRule(m),
                new EngineeringChangeDeniedPermissionRule(m),
                new EquipmentSearchStringRule(m),
                new ExchangeRateRule(m),
                new ExchangeRateSearchStringRule(m),
                new FaceToFaceCommunicationRule(m),
                new FaceToFaceCommunicationSearchStringRule(m),
                new FacilityDeniedPermissionRule(m),
                new FacilityFacilityTypeNameRule(m),
                new FacilityOwnerNameRule(m),
                new FacilityParentNameRule(m),
                new FaxCommunicationRule(m),
                new FaxCommunicationSearchStringRule(m),
                new GeneralLedgerAccountCostCenterAllowedRule(m),
                new GeneralLedgerAccountCostCenterRequiredRule(m),
                new GeneralLedgerAccountCostUnitAllowedRule(m),
                new GeneralLedgerAccountCostUnitRequiredRule(m),
                new GeneralLedgerAccountRule(m),
                new GoodProductIdentificationsRule(m),
                new InternalOrganisationCustomerReturnSequenceRule(m),
                new InternalOrganisationExistDefaultCollectionMethodRule(m),
                new InternalOrganisationInvoiceSequenceRule(m),
                new InternalOrganisationPurchaseShipmentSequenceRule(m),
                new InternalOrganisationQuoteSequenceRule(m),
                new InternalOrganisationRequestSequenceRule(m),
                new InternalOrganisationRequirementSequenceRule(m),
                new InternalOrganisationRule(m),
                new InternalOrganisationWorkEffortSequenceRule(m),
                new InventoryItemRule(m),
                new InventoryItemTransactionFacilityNameRule(m),
                new InventoryItemTransactionInventoryItemStateNameRule(m),
                new InventoryItemTransactionPartDisplayNameRule(m),
                new InventoryItemTransactionPartNumberRule(m),
                new InventoryItemTransactionRule(m),
                new InventoryItemTransactionSerialisedItemItemNumberRule(m),
                new InvoiceItemTotalIncVatRule(m),
                new JournalContraAccountRule(m),
                new JournalTypeRule(m),
                new LetterCorrespondenceRule(m),
                new LetterCorrespondenceSearchStringRule(m),
                new ModelDeniedPermissionRule(m),
                new NonSerialisedInventoryItemDeniedPermissionRule(m),
                new NonSerialisedInventoryItemQuantitiesRule(m),
                new NonSerialisedInventoryItemSearchStringRule(m),
                new NonUnifiedGoodDeniedPermissionRule(m),
                new NonUnifiedGoodDisplayNameRule(m),
                new NonUnifiedGoodProductIdentificationsRule(m),
                new NonUnifiedGoodSearchStringRule(m),
                new NonUnifiedGoodVariantsRule(m),
                new NonUnifiedPartDeniedPermissionRule(m),
                new NonUnifiedPartDisplayNameRule(m),
                new NonUnifiedPartProductIdentificationsRule(m),
                new NonUnifiedPartRule(m),
                new NonUnifiedPartSearchStringRule(m),
                new NonUnifiedPartSupplierReferenceNumbersRule(m),
                new OrderShipmentRule(m),
                new OrganisationContactRelationshipDateRule(m),
                new OrganisationContactRelationshipPartyRule(m),
                new OrganisationDeniedPermissionRule(m),
                new OrganisationDisplayNameRule(m),
                new OrganisationDisplayNameRule(m),
                new OrganisationRelationshipsRule(m),
                new OrganisationRollupRule(m),
                new OrganisationSearchStringRule(m),
                new OrganisationSyncContactRelationshipsRule(m),
                new OwnBankAccountRule(m),
                new OwnCreditCardInternalOrganisationPaymentMethodsRule(m),
                new OwnCreditCardRule(m),
                new PackagingContentRule(m),
                new PartBrandNameRule(m),
                new PartCategoryDisplayNameRule(m),
                new PartCategoryImageRule(m),
                new PartCategoryLocalisedDescriptionsRule(m),
                new PartCategoryNameRule(m),
                new PartCategoryRule(m),
                new PartCategorySearchStringRule(m),
                new PartCurrentSupplierOfferingsRule(m),
                new PartDefaultFacilityNameRule(m),
                new PartInventoryItemKindNameRule(m),
                new PartInventoryItemRule(m),
                new PartManufacturedByDisplayNameRule(m),
                new PartModelNameRule(m),
                new PartPartCategoriesDisplayNameRule(m),
                new PartProductTypeNameRule(m),
                new PartQuantitiesRule(m),
                new PartRule(m),
                new PartSpecificationDeniedPermissionRule(m),
                new PartSuppliedByDisplayNameRule(m),
                new PartSuppliedByRule(m),
                new PartSyncInventoryItemsRule(m),
                new PartyContactMechanismRule(m),
                new PartyFinancialRelationshipAmountDueRule(m),
                new PartyRule(m),
                new PayHistoryRule(m),
                new PaymentApplicationRule(m),
                new PaymentApplicationValidationRule(m),
                new PaymentRule(m),
                new PayrollPreferenceRule(m),
                new PersonDeniedPermissionRule(m),
                new PersonDisplayNameRule(m),
                new PersonDisplayNameRule(m),
                new PersonRule(m),
                new PersonSalutationRule(m),
                new PersonSearchStringRule(m),
                new PersonTimeSheetWorkerRule(m),
                new PhoneCommunicationRule(m),
                new PhoneCommunicationSearchStringRule(m),
                new PickListDeniedPermissionRule(m),
                new PickListItemQuantityPickedRule(m),
                new PickListItemRule(m),
                new PickListRule(m),
                new PickListStateRule(m),
                new PositionTypeRateSearchStringRule(m),
                new PositionTypeSearchStringRule(m),
                new PostalAddressDisplayNameRule(m),
                new PostalAddressRule(m),
                new PriceComponentDerivePricedByRule(m),
                new PriceComponentRule(m),
                new ProductCategoryDisplayNameRule(m),
                new ProductCategoryImageRule(m),
                new ProductCategoryLocalisedDescriptionRule(m),
                new ProductCategoryLocalisedNameRule(m),
                new ProductCategoryRule(m),
                new ProductCategorySearchStringRule(m),
                new ProductProductCategoriesDisplayNameRule(m),
                new ProductQuoteApprovalRule(m),
                new ProductQuoteAwaitingApprovalRule(m),
                new ProductQuoteDeniedPermissionRule(m),
                new ProductQuoteItemByProductRule(m),
                new ProductQuotePrintRule(m),
                new ProductQuoteQuoteNumberRule(m),
                new ProductQuoteRule(m),
                new ProductQuoteSearchStringRule(m),
                new ProfessionalServicesRelationshipRule(m),
                new PropertySearchStringRule(m),
                new ProposalDeniedPermissionRule(m),
                new ProposalPrintRule(m),
                new ProposalQuoteNumberRule(m),
                new ProposalRule(m),
                new ProposalSearchStringRule(m),
                new PurchaseInvoiceAmountPaidRule(m),
                new PurchaseInvoiceApprovalRule(m),
                new PurchaseInvoiceAwaitingApprovalRule(m),
                new PurchaseInvoiceBilledRule(m),
                new PurchaseInvoiceCreatedCurrencyRule(m),
                new PurchaseInvoiceCreatedInvoiceItemRule(m),
                new PurchaseInvoiceCreatedRule(m),
                new PurchaseInvoiceDeniedPermissionRule(m),
                new PurchaseInvoiceItemDeniedPermissionRule(m),
                new PurchaseInvoiceItemRule(m),
                new PurchaseInvoiceItemStateRule(m),
                new PurchaseInvoicePriceRule(m),
                new PurchaseInvoicePrintRule(m),
                new PurchaseInvoiceRule(m),
                new PurchaseInvoiceSearchStringRule(m),
                new PurchaseInvoiceStateRule(m),
                new PurchaseOrderApprovalLevel1Rule(m),
                new PurchaseOrderApprovalLevel1Rule(m),
                new PurchaseOrderApprovalLevel2Rule(m),
                new PurchaseOrderApprovalLevel2Rule(m),
                new PurchaseOrderAwaitingApprovalLevel1Rule(m),
                new PurchaseOrderAwaitingApprovalLevel2Rule(m),
                new PurchaseOrderCreatedBillToContactMechanismRule(m),
                new PurchaseOrderCreatedCurrencyRule(m),
                new PurchaseOrderCreatedIrpfRegimeRule(m),
                new PurchaseOrderCreatedLocaleRule(m),
                new PurchaseOrderCreatedShipToAddressRule(m),
                new PurchaseOrderCreatedTakenViaContactMechanismRule(m),
                new PurchaseOrderCreatedVatRegimeRule(m),
                new PurchaseOrderDeniedPermissionRule(m),
                new PurchaseOrderDisplayNameRule(m),
                new PurchaseOrderItemBillingsWhereOrderItemRule(m),
                new PurchaseOrderItemByProductRule(m),
                new PurchaseOrderItemCreatedDeliveryDateRule(m),
                new PurchaseOrderItemCreatedIrpfRateRule(m),
                new PurchaseOrderItemCreatedVatRegimeRule(m),
                new PurchaseOrderItemDelegatedAccessRule(m),
                new PurchaseOrderItemDeniedPermissionRule(m),
                new PurchaseOrderItemDisplayNameRule(m),
                new PurchaseOrderItemQuantityReturnedRule(m),
                new PurchaseOrderItemRule(m),
                new PurchaseOrderItemStateRule(m),
                new PurchaseOrderPriceRule(m),
                new PurchaseOrderPriceRule(m),
                new PurchaseOrderPrintRule(m),
                new PurchaseOrderRule(m),
                new PurchaseOrderSearchStringRule(m),
                new PurchaseOrderStateRule(m),
                new PurchaseOrderOverdueRule(m),
                new PurchaseReturnDeniedPermissionRule(m),
                new PurchaseReturnExistShipmentNumberRule(m),
                new PurchaseReturnExistShipToAddressRule(m),
                new PurchaseReturnCanShipRule(m),
                new PurchaseReturnRule(m),
                new PurchaseReturnSearchStringRule(m),
                new PurchaseShipmentDeniedPermissionRule(m),
                new PurchaseShipmentSearchStringRule(m),
                new PurchaseShipmentShipFromAddressRule(m),
                new PurchaseReturnShipmentItemQuantityRule(m),
                new PurchaseShipmentShipToPartyRule(m),
                new PurchaseShipmentStateRule(m),
                new QuoteCreatedCurrencyRule(m),
                new QuoteCreatedIrpfRegimeRule(m),
                new QuoteCreatedLocalRule(m),
                new QuoteCreatedVatRegimeRule(m),
                new QuoteItemCreatedIrpfRegimeRule(m),
                new QuoteItemCreatedVatRegimeRule(m),
                new QuoteItemDeniedPermissionRule(m),
                new QuoteItemDetailsRule(m),
                new QuoteItemPriceRule(m),
                new QuotePriceRule(m),
                new QuoteQuoteItemDelegatedAccessRule(m),
                new RepeatingPurchaseInvoiceRule(m),
                new RepeatingSalesInvoiceDeniedPermissionRule(m),
                new RepeatingSalesInvoiceRule(m),
                new RequestAnonymousRule(m),
                new RequestCurrencyRule(m),
                new RequestForInformationDeniedPermissionRule(m),
                new RequestForInformationRule(m),
                new RequestForInformationSearchStringRule(m),
                new RequestForProposalDeniedPermissionRule(m),
                new RequestForProposalRule(m),
                new RequestForProposalSearchStringRule(m),
                new RequestForQuoteDeniedPermissionRule(m),
                new RequestForQuoteRule(m),
                new RequestForQuoteSearchStringRule(m),
                new RequestItemDeniedPermissionRule(m),
                new RequestItemRule(m),
                new RequestItemValidationRule(m),
                new RequestRule(m),
                new RequirementOriginatorNameRule(m),
                new RequirementRequirementPriorityNameRule(m),
                new RequirementRequirementStateNameRule(m),
                new RequirementRequirementTypeNameRule(m),
                new RequirementServicedByNameRule(m),
                new SalesInvoiceBilledFromValidationRule(m),
                new SalesInvoiceBillingOrderItemBillingRule(m),
                new SalesInvoiceBillingShipmentItemBillingRule(m),
                new SalesInvoiceBillingtimeEntryBillingRule(m),
                new SalesInvoiceBillingWorkEffortBillingRule(m),
                new SalesInvoiceCustomerRule(m),
                new SalesInvoiceDelegatedAccessRule(m),
                new SalesInvoiceDeniedPermissionRule(m),
                new SalesInvoiceDueDateRule(m),
                new SalesInvoiceIsRepeatingInvoiceRule(m),
                new SalesInvoiceItemAssignedIrpfRegimeRule(m),
                new SalesInvoiceItemAssignedVatRegimeRule(m),
                new SalesInvoiceItemDeniedPermissionRule(m),
                new SalesInvoiceItemInvoiceItemTypeRule(m),
                new SalesInvoiceItemPaymentApplicationAmountAppliedRule(m),
                new SalesInvoiceItemRule(m),
                new SalesInvoiceItemSalesInvoiceRule(m),
                new SalesInvoiceItemSubTotalItemRule(m),
                new SalesInvoicePreviousBillToCustomerRule(m),
                new SalesInvoicePriceRule(m),
                new SalesInvoicePrintRule(m),
                new SalesInvoiceReadyForPostingDerivedBilledFromContactMechanismRule(m),
                new SalesInvoiceReadyForPostingDerivedBillToContactMechanismRule(m),
                new SalesInvoiceReadyForPostingDerivedBillToEndCustomerContactMechanismRule(m),
                new SalesInvoiceReadyForPostingDerivedCurrencyRule(m),
                new SalesInvoiceReadyForPostingDerivedLocaleRule(m),
                new SalesInvoiceReadyForPostingDerivedShipToAddressRule(m),
                new SalesInvoiceReadyForPostingDerivedShipToEndCustomerAddressRule(m),
                new SalesInvoiceReadyForPostingRule(m),
                new SalesInvoiceSearchStringRule(m),
                new SalesInvoiceStateRule(m),
                new SalesInvoiceStoreRule(m),
                new SalesInvoiceTemporaryInvoiceNumberRule(m),
                new SalesOrderCanInvoiceRule(m),
                new SalesOrderCanShipRule(m),
                new SalesOrderCustomerRule(m),
                new SalesOrderDeniedPermissionRule(m),
                new SalesOrderItemByProductRule(m),
                new SalesOrderItemDelegatedAccessRule(m),
                new SalesOrderItemDeniedPermissionRule(m),
                new SalesOrderItemInventoryAssignmentRule(m),
                new SalesOrderItemInventoryItemRule(m),
                new SalesOrderItemInvoiceItemTypeRule(m),
                new SalesOrderItemPriceRule(m),
                new SalesOrderItemProvisionalDeliveryDateRule(m),
                new SalesOrderItemProvisionalIrpfRegimeRule(m),
                new SalesOrderItemProvisionalShipFromAddressRule(m),
                new SalesOrderItemProvisionalShipToAddressRule(m),
                new SalesOrderItemProvisionalShipToPartyRule(m),
                new SalesOrderItemProvisionalVatRegimeRule(m),
                new SalesOrderItemQuantitiesRule(m),
                new SalesOrderItemRule(m),
                new SalesOrderItemSalesOrderItemsByProductRule(m),
                new SalesOrderItemShipmentRule(m),
                new SalesOrderItemValidationRule(m),
                new SalesOrderOrderNumberRule(m),
                new SalesOrderPriceRule(m),
                new SalesOrderPrintRule(m),
                new SalesOrderProvisionalBillToContactMechanismRule(m),
                new SalesOrderProvisionalBillToEndCustomerContactMechanismRule(m),
                new SalesOrderProvisionalCurrencyRule(m),
                new SalesOrderProvisionalIrpfRegimeRule(m),
                new SalesOrderProvisionalLocaleRule(m),
                new SalesOrderProvisionalPaymentMethodRule(m),
                new SalesOrderProvisionalShipFromAddressRule(m),
                new SalesOrderProvisionalShipmentMethodRule(m),
                new SalesOrderProvisionalShipToAddressRule(m),
                new SalesOrderProvisionalShipToEndCustomerAddressRule(m),
                new SalesOrderProvisionalTakenByContactMechanismRule(m),
                new SalesOrderProvisionalVatRegimeRule(m),
                new SalesOrderRule(m),
                new SalesOrderSearchStringRule(m),
                new SalesOrderShipRule(m),
                new SalesOrderStateRule(m),
                new SalesOrderTransferRule(m),
                new SalesOrderValidationsRule(m),
                new SerialisedInventoryItemDeniedPermissionRule(m),
                new SerialisedInventoryItemQuantitiesRule(m),
                new SerialisedInventoryItemDisplayNameRule(m),
                new SerialisedInventoryItemSearchStringRule(m),
                new SerialisedItemBrandNameRule(m),
                new SerialisedItemBuyerNameRule(m),
                new SerialisedItemCharacteristicRule(m),
                new SerialisedItemCharacteristicSearchStringRule(m),
                new SerialisedItemCharacteristicTypeDisplayNameRule(m),
                new SerialisedItemCharacteristicTypeRule(m),
                new SerialisedItemDeniedPermissionRule(m),
                new SerialisedItemManufacturedByNameRule(m),
                new SerialisedItemModelNameRule(m),
                new SerialisedItemOwnedByPartyNameRule(m),
                new SerialisedItemOwnerRule(m),
                new SerialisedItemOwnershipNameRule(m),
                new SerialisedItemPartNameRule(m),
                new SerialisedItemProductCategoriesDisplayNameRule(m),
                new SerialisedItemProductTypeNameRule(m),
                new SerialisedItemPurchaseInvoiceNumberRule(m),
                new SerialisedItemPurchaseOrderNumberRule(m),
                new SerialisedItemQuoteItemWhereSerialisedItemRule(m),
                new SerialisedItemRentedByPartyNameRule(m),
                new SerialisedItemSalesOrderItemWhereSerialisedItemRule(m),
                new SerialisedItemSearchStringRule(m),
                new SerialisedItemSellerNameRule(m),
                new SerialisedItemSerialisedItemAvailabilityNameRule(m),
                new SerialisedItemSuppliedByPartyNameRule(m),
                new SerialisedItemSuppliedByRule(m),
                new SerialisedItemWorkEffortFixedAssetAssignemtsWhereFixedAssetRule(m),
                new ShipmentItemDeniedPermissionRule(m),
                new ShipmentItemRule(m),
                new ShipmentItemStateRule(m),
                new ShipmentPackageRule(m),
                new ShipmentReceiptRule(m),
                new ShipmentRule(m),
                new SingletonLocalesRule(m),
                new StatementOfWorkDeniedPermissionRule(m),
                new StatementOfWorkQuoteNumberRule(m),
                new StatementOfWorkRule(m),
                new StatementOfWorkSearchStringRule(m),
                new StoreRule(m),
                new SubcontractorRelationshipRule(m),
                new SupplierOfferingExistCurrencyRule(m),
                new SupplierOfferingExistSupplierRule(m),
                new SupplierRelationshipRule(m),
                new SurchargeComponentRule(m),
                new SurchargeComponentRule(m),
                new TelecommunicationsNumberDisplayNameRule(m),
                new TimeAndMaterialsServiceDisplayNameRule(m),
                new TimeAndMaterialsServiceRule(m),
                new TimeAndMaterialsServiceSearchStringRule(m),
                new TimeEntryRule(m),
                new TimeEntrySecurityRule(m),
                new TimeEntryWorkerRule(m),
                new TransferDeniedPermissionRule(m),
                new TransferDeriveShipFromAddressRule(m),
                new TransferDeriveShipToAddressRule(m),
                new TransferSearchStringRule(m),
                new UnifiedGoodDeniedPermissionRule(m),
                new UnifiedGoodDisplayNameRule(m),
                new UnifiedGoodRule(m),
                new UnifiedGoodSearchStringRule(m),
                new UnifiedProductProductIdentificationNamesRule(m),
                new UnifiedProductScopeNameRule(m),
                new UnifiedProductUnitOfMeasureAbbreviationRule(m),
                new UserUserProfileRule(m),
                new WebAddressDisplayNameRule(m),
                new WebSiteCommunicationSearchStringRule(m),
                new WebSiteCommunicationsRule(m),
                new WorkEffortAssignmentRateDelegatedAccessRule(m),
                new WorkEffortAssignmentRateValidationRule(m),
                new WorkEffortAssignmentRateWorkEffortRule(m),
                new WorkEffortDisplayNameRule(m),
                new WorkEffortInventoryAssignmentDelegatedAccessRule(m),
                new WorkEffortInventoryAssignmentSyncInventoryTransactionsRule(m),
                new WorkEffortInventoryAssignmentWorkeffortNumberRule(m),
                new WorkEffortInvoiceItemAssignmentDelegatedAccessRule(m),
                new WorkEffortInvoiceItemDelegatedAccessRule(m),
                new WorkEffortInvoiceItemDeniedPermissionRule(m),
                new WorkEffortPartyAssignmentDelegatedAccessRule(m),
                new WorkEffortPartyAssignmentRule(m),
                new WorkEffortPurchaseOrderItemAssignmentDelegatedAccessRule(m),
                new WorkEffortPurchaseOrderItemAssignmentPurchaseOrderItemRule(m),
                new WorkEffortPurchaseOrderItemAssignmentRule(m),
                new WorkEffortTotalRevenueRule(m),
                new WorkEffortTotalSubContractedRevenueRule(m),
                new WorkRequirementDisplayNameRule(m),
                new WorkRequirementFixedAssetNameRule(m),
                new WorkRequirementFulfillmentDeniedPermissionRule(m),
                new WorkRequirementFulfillmentRule(m),
                new WorkRequirementFulfillmentWorkEffortRule(m),
                new WorkRequirementSearchStringRule(m),
                new WorkRequirementServicedByRule(m),
                new WorkRequirementWorkEffortNumberRule(m),
                new WorkTaskActualHoursRule(m),
                new WorkTaskCanInvoiceRule(m),
                new WorkTaskDeniedPermissionRule(m),
                new WorkTaskExecutedByRule(m),
                new WorkTaskPrintRule(m),
                new WorkTaskRule(m),
                new WorkTaskSearchStringRule(m),
                new WorkTaskStateRule(m),
                new WorkTaskTakenByRule(m),
                new WorkTaskWorkEffortAssignmentRule(m),

                //new CustomerShipmentStateRule(m),
                //new GoodLocalisedDescriptionRule(m),
                //new GoodLocalisedNamesRule(m),
                //new NonSerialisedInventoryItemDisplayNameRule(m),
                //new OrganisationContactUserGroupRule(m),
                //new ProductQuoteApprovalParticipantsRule(m),
                //new PurchaseInvoiceApprovalParticipantsRule(m),
                //new PurchaseInvoiceSerialisedItemRule(m),
                //new PurchaseOrderApprovalLevel1ParticipantsRule(m),
                //new PurchaseOrderApprovalLevel2ParticipantsRule(m),
                //new PurchaseOrderItemIsReceivableRule(m),
                //new SalesInvoiceInvoiceNumberRule(m),
                //new SalesOrderProvisionalVatClauseRule(m),
                //new SerialisedItemDisplayProductCategoriesRule(m),
                //new SerialisedItemDisplayNameRule(m),
                //new SerialisedItemPurchaseInvoiceRule(m),
                //new SerialisedItemPurchaseOrderRule(m),
                //new SerialisedItemRule(m),
                //new SupplierOfferingRule(m),
                //new TimeSheetSecurityTokenRule(m),
                //new VehicleSearchStringRule(m),
                //new WorkEffortGrandTotalRule(m),
                //new WorkEffortInventoryAssignmentCostOfGoodsSoldRule(m),
                //new WorkEffortInventoryAssignmentDerivedBillableQuantityRule(m),
                //new WorkEffortInventoryAssignmentUnitSellingPriceRule(m),
                //new WorkEffortPurchaseOrderItemAssignmentUnitSellingRule(m),
                //new WorkEffortTotalLabourRevenueRule(m),
                //new WorkEffortTotalMaterialRevenueRule(m),
                //new WorkEffortTotalOtherRevenueRule(m),
                //new WorkEffortTypeRule(m),
                //new WorkRequirementDeniedPermissionRule(m),
                //new WorkRequirementStateRule(m),
                //new QuoteItemRule(m),
                //new SalesOrderItemStateRule(m),
                //new SerialisedItemPurchasePriceRule(m),
                //new UserGroupInMembersRule(m),
                //new UserGroupOutMembersRule(m),

                // Aviation
                new CreatorsMemberRule(m),
                new CustomerRelationshipCustomerNameRule(m),
                new CustomerRelationshipSecurityTokenRule(m),
                new AviationCustomerShipmentStateRule(m),
                new AviationNonSerialisedInventoryItemDisplayNameRule(m),
                new AviationProductQuoteApprovalParticipantsRule(m),
                new AviationPurchaseInvoiceApprovalParticipantsRule(m),
                new AviationPurchaseInvoiceSerialisedItemRule(m),
                new AviationPurchaseOrderApprovalLevel1ParticipantsRule(m),
                new AviationPurchaseOrderApprovalLevel2ParticipantsRule(m),
                new AviationPurchaseOrderItemIsReceivableRule(m),
                new AviationQuoteItemDetailsRule(m),
                new AviationQuoteItemRule(m),
                new AviationSalesOrderItemStateRule(m),
                new AviationSalesInvoiceInvoiceNumberRule(m),
                new AviationCustomSerialisedItemRule(m),
                new AviationSerialisedItemDisplayProductCategoriesRule(m),
                new AviationSerialisedItemDisplayNameRule(m),
                new AviationSerialisedItemPurchaseInvoiceRule(m),
                new AviationSerialisedItemPurchaseOrderRule(m),
                new AviationSerialisedItemPurchasePriceRule(m),
                new AviationSerialisedItemRule(m),
                new AviationTimeSheetSecurityTokenRule(m),
                new AviationVehicleSearchStringRule(m),
                new AviationUnifiedGoodRule(m),
                new AviationUserGroupInMembersRule(m),
                new AviationUserGroupOutMembersRule(m),
                new AviationWorkEffortGrandTotalRule(m),
                new AviationWorkEffortInventoryAssignmentCostOfGoodsSoldRule(m),
                new AviationWorkEffortInventoryAssignmentDerivedBillableQuantityRule(m),
                new AviationWorkEffortInventoryAssignmentUnitSellingPriceRule(m),
                new AviationWorkEffortPurchaseOrderItemAssignmentUnitSellingPriceRule(m),
                new AviationWorkEffortTotalRevenueRule(m),
                new AviationWorkEffortTotalMaterialRevenueRule(m),
                new AviationWorkEffortTotalOtherRevenueRule(m),
                new AviationWorkEffortTypeRule(m),
                new AviationWorkRequirementDeniedPermissionRule(m),
                new AviationWorkRequirementStateRule(m),
                new AviationWorkTaskRule(m),
                new EmployeesMemberRule(m),
                new FacilityNonSerialisedInventoryItemRule(m),
                new FacilityRule(m),
                new FacilitySecurityTokenRule(m),
                new FacilitySupplierOfferingRule(m),
                new FacilityWorkshopWarehouseNameRule(m),
                new LocalAdministratorsGlobalMemberRule(m),
                new LocalEmployeesMemberRule(m),
                new MaintenanceAgreementDeniedPermissionRule(m),
                new MaintenanceAgreementRule(m),
                new NonSerialisedInventoryItemSecurityTokenRule(m),
                new NonUnifiedPartBrandRule(m),
                new NonUnifiedPartSecurityTokenRule(m),
                new NonUnifiedPartUomNameRule(m),
                new OperatingHoursTransactionRule(m),
                new OrganisationCalculationRule(m),
                new PartLocalisedNameRule(m),
                new PersonIsUserRule(m),
                new PersonLastTimeEntryRule(m),
                new ProductCategoryCustomRule(m),
                new ProductCategoryProductsRule(m),
                new PurchaseOrderInvoiceSundriesRule(m),
                new QuoteItemPrintDocumentRule(m),
                new SalesAccountManagersGlobalMemberRule(m),
                new SalesInvoiceResetPrintDocumentRule(m),
                new SalesInvoiceItemDescriptionRule(m),
                new SalesOrderDerivedVatClauseDescriptionRule(m),
                new SalesOrderItemDescriptionRule(m),
                new SerialisedInventoryItemSecurityTokenRule(m),
                new SerialisedItemCostRule(m),
                new SerialisedItemDerivedAssumedMonthlyOperatingHoursRule(m),
                new SerialisedItemSalesInvoiceRule(m),
                new SerialisedItemSecurityTokenRule(m),
                new SerialisedItemSerialisedItemCharacteristicRule(m),
                new SerialisedItemSuppliedByCountryCodeRule(m),
                new SerialisedPartPropertiesRule(m),
                new SettingsCalculationRule(m),
                new SubContractorRelationshipSecurityTokenRule(m),
                new SupplierOfferingSecurityTokenRule(m),
                new SupplierOfferingSupplierSecuritytokenRule(m),
                new SupplierRelationshipSecurityTokenRule(m),
                new UnifiedGoodDerivedAssumedMonthlyOperatingHoursRule(m),
                new UnifiedGoodIataGseCodeRule(m),
                new VehicleDeniedPermissionRule(m),
                new VehicleDisplayNameRule(m),
                new VehicleOwnedByPartyNameRule(m),
                new WorkEffortFixedAssetAssignmentMaintenanceAgreementRule(m),
                new WorkEffortFixedAssetRule(m),
                new WorkEffortInvoiceItemAmountRule(m),
                new WorkEffortInvoiceItemAssignmentRule(m),
                new WorkEffortTotalCostRule(m),
                new WorkEffortTypeDeniedPermissionRule(m),
                new WorkEffortTypeDisplayNameRule(m),
                new WorkEffortTypeProductCategoryDisplayNameRule(m),
                new WorkEffortTypeUnifiedGoodDisplayNameRule(m),
                new WorkRequirementFleetCodeRule(m),
                new WorkRequirementIsActiveRule(m),
            };
    }
}
