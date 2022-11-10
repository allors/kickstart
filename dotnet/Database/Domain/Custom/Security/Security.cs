
// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Security.cs" company="Allors bvba">
//   Copyright 2002-2016 Allors bvba.
// 
// Dual Licensed under
//   a) the General Public Licence v3 (GPL)
//   b) the Allors License
// 
// The GPL License is included in the file gpl.txt.
// The Allors License is an addendum to your contract.
// 
// Allors Applications is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// 
// For more information visit http://www.allors.com/legal
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;

namespace Allors.Database.Domain
{
    using Allors.Database.Meta;

    public partial class Security
    {
        private void CustomOnPreSetup()
        {
            var m = this.transaction.Database.Services.Get<MetaPopulation>();

            var security = new Security(this.transaction);

            var full = new[] { Operations.Read, Operations.Write, Operations.Execute };
            var read = new[] { Operations.Read };

            foreach (ObjectType @class in this.transaction.Database.MetaPopulation.DatabaseClasses)
            {
                // global
                security.GrantAdministrator(@class, full);
                security.GrantCreator(@class, full);

                if (@class.Equals(m.Singleton))
                {
                    security.GrantAdministrator(@class, full);
                    security.GrantEmployee(@class, full);
                    security.Grant(Roles.GuestId, @class, m.Singleton.DefaultInternalOrganisation, Operations.Read);
                }
                if (@class.Equals(m.Organisation))
                {
                    security.GrantLocalAdministrator(@class, Operations.Read, Operations.Write);
                    security.GrantLocalEmployee(@class, Operations.Read);

                    // Used for internalOrganisations
                    security.Grant(Roles.CustomerContactCreatorId, @class, m.Organisation.Name, Operations.Read);
                    security.Grant(Roles.CustomerContactCreatorId, @class, m.Organisation.DisplayName, Operations.Read);
                    security.Grant(Roles.CustomerContactCreatorId, @class, m.Organisation.DisplayName, Operations.Read);
                    security.Grant(Roles.CustomerContactCreatorId, @class, m.Organisation.CurrentContacts, Operations.Read);
                    security.Grant(Roles.CustomerContactCreatorId, @class, m.Organisation.CurrentOrganisationContactRelationships, Operations.Read);
                    security.Grant(Roles.CustomerContactCreatorId, @class, m.Organisation.CurrentPartyContactMechanisms, Operations.Read);
                    security.Grant(Roles.CustomerContactCreatorId, @class, m.Organisation.CurrentPartyRelationships, Operations.Read);
                    security.Grant(Roles.CustomerContactCreatorId, @class, m.Organisation.IsInternalOrganisation, Operations.Read);

                    // Used for own organisation
                    security.Grant(Roles.SpecificCustomerContactId, @class, m.Organisation.Name, Operations.Read);
                    security.Grant(Roles.SpecificCustomerContactId, @class, m.Organisation.DisplayName, Operations.Read);
                    security.Grant(Roles.SpecificCustomerContactId, @class, m.Organisation.DisplayName, Operations.Read);
                    security.Grant(Roles.SpecificCustomerContactId, @class, m.Organisation.CurrentContacts, Operations.Read);
                    security.Grant(Roles.SpecificCustomerContactId, @class, m.Organisation.CurrentOrganisationContactRelationships, Operations.Read);
                    security.Grant(Roles.SpecificCustomerContactId, @class, m.Organisation.CurrentPartyContactMechanisms, Operations.Read);
                    security.Grant(Roles.SpecificCustomerContactId, @class, m.Organisation.CurrentPartyRelationships, Operations.Read);

                    security.Grant(Roles.LocalAdministratorId, @class, m.Organisation.CreatePurchaseInvoice, Operations.Execute);
                    security.Grant(Roles.LocalAdministratorId, @class, m.Organisation.CreatePurchaseOrder, Operations.Execute);
                    security.Grant(Roles.LocalAdministratorId, @class, m.Organisation.CreateQuote, Operations.Execute);
                    security.Grant(Roles.LocalAdministratorId, @class, m.Organisation.CreateRequest, Operations.Execute);
                    security.Grant(Roles.LocalAdministratorId, @class, m.Organisation.CreateSalesInvoice, Operations.Execute);
                    security.Grant(Roles.LocalAdministratorId, @class, m.Organisation.CreateSalesOrder, Operations.Execute);
                    security.Grant(Roles.LocalAdministratorId, @class, m.Organisation.ShowInMenu, Operations.Execute);
                    security.Grant(Roles.LocalEmployeeId, @class, m.Organisation.ShowInMenu, Operations.Execute);

                    var accountManagerExcepts = new HashSet<IOperandType>
                    {
                        m.Organisation.Delete,
                    };

                    security.GrantExceptSalesAccountManagerGlobal(@class, accountManagerExcepts, full);
                }
                else if (@class.Equals(m.SalesInvoice) ||
                    @class.Equals(m.SalesInvoiceItem))
                {
                    security.GrantLocalSalesAccountManager(@class, full);
                    security.GrantLocalAdministrator(@class, full);
                    security.GrantLocalEmployee(@class, Operations.Read);
                }
                else if (@class.Equals(m.SalesOrder) ||
                    @class.Equals(m.SalesOrderItem))
                {
                    var excepts = new HashSet<IOperandType>
                    {
                        m.SalesOrderItem.CostOfGoodsSold,
                    };

                    var excepts2 = new HashSet<IOperandType>
                    {
                        m.SalesOrderItem.CostOfGoodsSold,
                        m.SalesOrder.Invoice,
                    };

                    security.GrantExceptLocalSalesAccountManager(@class, excepts, full);
                    security.GrantExceptLocalAdministrator(@class, excepts2, full);
                    security.GrantExceptLocalEmployee(@class, excepts, Operations.Read);
                }
                else if (@class.Equals(m.CustomerReturn) ||
                    @class.Equals(m.CustomerShipment) ||
                    @class.Equals(m.DropShipment) ||
                    @class.Equals(m.PurchaseReturn) ||
                    @class.Equals(m.PurchaseShipment) ||
                    @class.Equals(m.Transfer))
                {
                    security.GrantLocalAdministrator(@class, full);
                    security.GrantLocalEmployee(@class, Operations.Read);
                }
                else if (@class.Equals(m.PurchaseInvoice)
                        || @class.Equals(m.PurchaseInvoiceItem))
                {
                    security.GrantLocalSalesAccountManager(@class, full);
                    security.GrantLocalAdministrator(@class, full);
                    security.GrantLocalEmployee(@class, Operations.Read);
                    security.Grant(Roles.PurchaseInvoiceApproverId, @class, m.PurchaseInvoice.Approve, Operations.Execute);
                    security.Grant(Roles.PurchaseInvoiceApproverId, @class, m.PurchaseInvoice.Reject, Operations.Execute);
                }
                else if (@class.Equals(m.PurchaseOrder)
                        || @class.Equals(m.PurchaseOrderItem))
                {
                    security.GrantStockManager(@class, full);
                    security.GrantLocalSalesAccountManager(@class, full);
                    security.GrantLocalAdministrator(@class, full);
                    security.GrantLocalEmployee(@class, Operations.Read);
                    security.Grant(Roles.PurchaseOrderApproverLevel1Id, @class, m.PurchaseOrder.Approve, Operations.Execute);
                    security.Grant(Roles.PurchaseOrderApproverLevel1Id, @class, m.PurchaseOrder.Reject, Operations.Execute);
                    security.Grant(Roles.PurchaseOrderApproverLevel2Id, @class, m.PurchaseOrder.Approve, Operations.Execute);
                    security.Grant(Roles.PurchaseOrderApproverLevel2Id, @class, m.PurchaseOrder.Reject, Operations.Execute);
                }
                else if (@class.Equals(m.RequestForInformation) ||
                         @class.Equals(m.RequestForProposal) ||
                         @class.Equals(m.RequestForQuote) ||
                         @class.Equals(m.RequestItem))
                {
                    security.GrantLocalAdministrator(@class, full);
                    security.GrantLocalSalesAccountManager(@class, full);
                    security.GrantGuestCreator(@class, full);
                    security.GrantLocalEmployee(@class, Operations.Read);
                }
                else if (@class.Equals(m.NonSerialisedInventoryItem))
                {
                    security.GrantLocalAdministrator(@class, full);
                    security.GrantStockManager(@class, full);
                    security.GrantLocalEmployee(@class, Operations.Read);
                }
                else if (@class.Equals(m.SerialisedInventoryItem))
                {
                    security.GrantLocalAdministrator(@class, full);
                    security.GrantStockManager(@class, full);
                    security.GrantLocalEmployee(@class, Operations.Read);
                }
                else if (@class.Equals(m.ProductQuote)
                        || @class.Equals(m.QuoteItem))
                {
                    security.GrantLocalAdministrator(@class, full);
                    security.GrantLocalEmployee(@class, Operations.Read);
                    security.Grant(Roles.LocalAdministratorId, @class, m.ProductQuote.Delete, Operations.Execute);
                    security.Grant(Roles.LocalAdministratorId, @class, m.Quote.Create, Operations.Execute);
                    security.Grant(Roles.LocalAdministratorId, @class, m.Quote.Cancel, Operations.Execute);
                    security.Grant(Roles.LocalAdministratorId, @class, m.Quote.Reopen, Operations.Execute);
                    security.Grant(Roles.LocalAdministratorId, @class, m.ProductQuote.Print, Operations.Execute);
                    security.Grant(Roles.LocalAdministratorId, @class, m.ProductQuote.Order, Operations.Execute);
                    security.GrantLocalSalesAccountManager(@class, Operations.Read, Operations.Write);
                    security.Grant(Roles.LocalSalesAccountManagerId, @class, m.ProductQuote.Delete, Operations.Execute);
                    security.Grant(Roles.LocalSalesAccountManagerId, @class, m.Quote.Create, Operations.Execute);
                    security.Grant(Roles.LocalSalesAccountManagerId, @class, m.Quote.Cancel, Operations.Execute);
                    security.Grant(Roles.LocalSalesAccountManagerId, @class, m.Quote.Reopen, Operations.Execute);
                    security.Grant(Roles.LocalSalesAccountManagerId, @class, m.ProductQuote.Print, Operations.Execute);
                    security.Grant(Roles.LocalSalesAccountManagerId, @class, m.ProductQuote.Order, Operations.Execute);
                    security.Grant(Roles.ProductQuoteApproverId, @class, m.Quote.Reopen, Operations.Execute);
                    security.Grant(Roles.ProductQuoteApproverId, @class, m.ProductQuote.Print, Operations.Execute);
                    security.Grant(Roles.ProductQuoteApproverId, @class, m.ProductQuote.Order, Operations.Execute);
                    security.Grant(Roles.ProductQuoteApproverId, @class, m.ProductQuote.Approve, Operations.Execute);
                    security.Grant(Roles.ProductQuoteApproverId, @class, m.ProductQuote.Reject, Operations.Execute);
                }
                else if (@class.Equals(m.InventoryItemTransaction))
                {
                    var excepts = new HashSet<IOperandType>
                    {
                        m.InventoryItemTransaction.Cost,
                    };

                    security.GrantExceptSalesAccountManagerGlobal(@class, excepts, full);
                    security.GrantLocalAdministrator(@class, full);
                    security.GrantExceptStockManager(@class, excepts, full);
                    security.GrantExceptLocalEmployee(@class, excepts, Operations.Read);
                }
                else if (@class.Equals(m.PartWeightedAverage))
                {
                    var excepts = new HashSet<IOperandType>
                    {
                        m.PartWeightedAverage.AverageCost,
                    };

                    security.GrantLocalAdministrator(@class, Operations.Read);
                    security.GrantLocalAdministratorGlobal(@class, Operations.Read);
                    security.GrantExceptStockManager(@class, excepts, Operations.Read);
                    security.GrantExceptEmployee(@class, excepts, Operations.Read);
                }
                else if (@class.Equals(m.WorkTask))
                {
                    security.GrantLocalAdministrator(@class, full);
                    security.GrantBlueCollarWorker(@class, full);
                    security.GrantStockManager(@class, full);
                    security.GrantLocalSalesAccountManager(@class, full);
                    security.GrantLocalEmployee(@class, Operations.Read);

                    security.Grant(Roles.LocalSalesAccountManagerId, @class, m.WorkTask.Invoice, Operations.Execute);
                    security.Grant(Roles.LocalSalesAccountManagerId, @class, m.WorkTask.Print, Operations.Execute);
                    security.Grant(Roles.LocalSalesAccountManagerId, @class, m.WorkTask.Revise, Operations.Execute);
                    security.Grant(Roles.LocalSalesAccountManagerId, @class, m.WorkTask.Cancel, Operations.Execute);
                    security.Grant(Roles.LocalSalesAccountManagerId, @class, m.WorkTask.Reopen, Operations.Execute);
                    security.Grant(Roles.LocalSalesAccountManagerId, @class, m.WorkTask.Complete, Operations.Execute);
                }
                else if (@class.Equals(m.WorkRequirement))
                {
                    security.GrantLocalAdministrator(@class, full);
                    security.GrantLocalEmployee(@class, Operations.Read);
                    security.GrantCustomerContactCreator(@class, full);
                    security.GrantSpecificCustomerContact(@class, full);
                }
                else if (@class.Equals(m.WorkRequirementFulfillment))
                {
                    security.GrantLocalAdministrator(@class, full);
                    security.GrantLocalEmployee(@class, Operations.Read);
                    security.GrantSpecificCustomerContact(@class, Operations.Read);
                }
                else if (@class.Equals(m.TimeSheet) ||
                         @class.Equals(m.TimeEntry))
                {
                    security.GrantOwner(@class, full);
                    security.GrantLocalAdministrator(@class, full);
                    security.GrantBlueCollarWorker(@class, full);
                    //security.GrantLocalEmployee(@class, Operations.Read);
                    // TODO: Martien
                    security.GrantLocalEmployee(@class, full);
                    security.GrantEmployee(@class, full);
                }
                else if (@class.Equals(m.WorkEffortInventoryAssignment))
                {
                    var exceptsLocalAdministrator = new HashSet<IOperandType>
                    {
                        m.WorkEffortInventoryAssignment.UnitPurchasePrice,
                        m.WorkEffortInventoryAssignment.CostOfGoodsSold,
                    };

                    var excepts = new HashSet<IOperandType>
                    {
                        m.WorkEffortInventoryAssignment.AssignedBillableQuantity,
                        m.WorkEffortInventoryAssignment.UnitSellingPrice,
                        m.WorkEffortInventoryAssignment.AssignedUnitSellingPrice,
                        m.WorkEffortInventoryAssignment.UnitPurchasePrice,
                        m.WorkEffortInventoryAssignment.CostOfGoodsSold,
                    };

                    security.GrantLocalAdministrator(@class, full);
                    security.GrantExceptLocalSalesAccountManager(@class, exceptsLocalAdministrator, full);
                    security.GrantExceptStockManager(@class, excepts, full);
                    security.GrantExceptLocalEmployee(@class, excepts, Operations.Read);
                }
                else if (@class.Equals(m.SerialisedItem))
                {
                    var excepts = new HashSet<IOperandType>
                    {
                        m.SerialisedItem.PurchasePrice,
                        m.SerialisedItem.AssignedPurchasePrice,
                        m.SerialisedItem.EstimatedRefurbishCost,
                        m.SerialisedItem.ActualRefurbishCost,
                        m.SerialisedItem.EstimatedTransportCost,
                        m.SerialisedItem.ActualTransportCost,
                        m.SerialisedItem.ExpectedRentalPriceDryLeaseLongTerm,
                        m.SerialisedItem.ExpectedRentalPriceDryLeaseShortTerm,
                        m.SerialisedItem.ExpectedRentalPriceFullServiceLongTerm,
                        m.SerialisedItem.ExpectedRentalPriceFullServiceShortTerm,
                        m.SerialisedItem.FromInitialImport,
                        m.SerialisedItem.Delete
                    };

                    security.GrantProductManager(@class, full);
                    security.GrantExceptSalesAccountManagerGlobal(@class, excepts, read);
                    security.GrantExceptLocalEmployee(@class, excepts, read);
                    security.GrantExceptGuest(@class, excepts, read);

                    security.Grant(Roles.SpecificCustomerContactId, @class, m.SerialisedItem.DisplayName, Operations.Read);
                    security.Grant(Roles.SpecificCustomerContactId, @class, m.SerialisedItem.Description, Operations.Read);
                    security.Grant(Roles.SpecificCustomerContactId, @class, m.SerialisedItem.LocalisedDescriptions, Operations.Read);
                    security.Grant(Roles.SpecificCustomerContactId, @class, m.SerialisedItem.LastServiceDate, Operations.Read);
                    security.Grant(Roles.SpecificCustomerContactId, @class, m.SerialisedItem.NextServiceDate, Operations.Read);
                    security.Grant(Roles.SpecificCustomerContactId, @class, m.SerialisedItem.PublicElectronicDocuments, Operations.Read);
                    security.Grant(Roles.SpecificCustomerContactId, @class, m.SerialisedItem.PublicLocalisedElectronicDocuments, Operations.Read);
                    security.Grant(Roles.SpecificCustomerContactId, @class, m.SerialisedItem.ChassisBrand, Operations.Read);
                    security.Grant(Roles.SpecificCustomerContactId, @class, m.SerialisedItem.ChassisModel, Operations.Read);
                    security.Grant(Roles.SpecificCustomerContactId, @class, m.SerialisedItem.IataCode, Operations.Read);
                    security.Grant(Roles.SpecificCustomerContactId, @class, m.SerialisedItem.ItemNumber, Operations.Read);
                    security.Grant(Roles.SpecificCustomerContactId, @class, m.SerialisedItem.SerialNumber, Operations.Read);
                    security.Grant(Roles.SpecificCustomerContactId, @class, m.SerialisedItem.SerialisedItemCharacteristics, Operations.Read);
                    security.Grant(Roles.SpecificCustomerContactId, @class, m.SerialisedItem.ManufacturingYear, Operations.Read);
                    security.Grant(Roles.SpecificCustomerContactId, @class, m.SerialisedItem.PrimaryPhoto, Operations.Read);
                    security.Grant(Roles.SpecificCustomerContactId, @class, m.SerialisedItem.SecondaryPhotos, Operations.Read);
                    security.Grant(Roles.SpecificCustomerContactId, @class, m.SerialisedItem.AdditionalPhotos, Operations.Read);
                    security.Grant(Roles.SpecificCustomerContactId, @class, m.SerialisedItem.RentedBy, Operations.Read);
                    security.Grant(Roles.SpecificCustomerContactId, @class, m.SerialisedItem.OwnedBy, Operations.Read);
                    security.Grant(Roles.SpecificCustomerContactId, @class, m.SerialisedItem.RentalFromDate, Operations.Read);
                    security.Grant(Roles.SpecificCustomerContactId, @class, m.SerialisedItem.RentalThroughDate, Operations.Read);
                    security.Grant(Roles.SpecificCustomerContactId, @class, m.SerialisedItem.ExpectedReturnDate, Operations.Read);
                    security.Grant(Roles.SpecificCustomerContactId, @class, m.SerialisedItem.ProductCategoriesDisplayName, Operations.Read);
                    security.Grant(Roles.SpecificCustomerContactId, @class, m.SerialisedItem.SyncedOperatingHoursTransactions, Operations.Read);

                    security.Grant(Roles.GenericCustomerContactId, @class, m.SerialisedItem.CustomerReferenceNumber, Operations.Read, Operations.Write);

                    security.Grant(Roles.LocalEmployeeId, @class, m.SerialisedItem.PrimaryPhoto, full);
                    security.Grant(Roles.LocalEmployeeId, @class, m.SerialisedItem.SecondaryPhotos, full);
                    security.Grant(Roles.LocalEmployeeId, @class, m.SerialisedItem.AdditionalPhotos, full);
                    security.Grant(Roles.LocalEmployeeId, @class, m.SerialisedItem.PrivatePhotos, full);
                    security.Grant(Roles.LocalEmployeeId, @class, m.SerialisedItem.PublicElectronicDocuments, full);
                    security.Grant(Roles.LocalEmployeeId, @class, m.SerialisedItem.PublicLocalisedElectronicDocuments, full);
                    security.Grant(Roles.LocalEmployeeId, @class, m.SerialisedItem.PrivateElectronicDocuments, full);
                    security.Grant(Roles.LocalEmployeeId, @class, m.SerialisedItem.PrivateLocalisedElectronicDocuments, full);
                    security.Grant(Roles.LocalEmployeeId, @class, m.SerialisedItem.SerialisedItemCharacteristics, Operations.Read, Operations.Write);
                }
                else if (@class.Equals(m.SerialisedItemCharacteristic))
                {
                    security.GrantProductManager(@class, full);
                    security.GrantEmployee(@class, Operations.Read);
                    security.GrantGuest(@class, Operations.Read);
                    security.GrantGenericCustomerContact(@class, Operations.Read, Operations.Write);
                }
                else if (@class.Equals(m.SupplierOffering))
                {
                    var excepts = new HashSet<IOperandType>
                    {
                        m.SupplierOffering.Price,
                    };

                    security.GrantLocalAdministrator(@class, full);
                    security.GrantExceptSalesAccountManagerGlobal(@class, excepts, full);
                    security.GrantExceptLocalEmployee(@class, excepts, read);
                }
                else if (@class.Equals(m.Locale) ||
                         @class.Equals(m.Currency) ||
                         @class.Equals(m.Catalogue) ||
                         @class.Equals(m.Scope) ||
                         @class.Equals(m.ProductCategory) ||
                         @class.Equals(m.SerialisedItemCharacteristicType) ||
                         @class.Equals(m.UnitOfMeasure) ||
                         @class.Equals(m.Priority) ||
                         @class.Equals(m.PartCategory) ||
                         @class.Equals(m.Brand) ||
                         @class.Equals(m.Model) ||
                         @class.Equals(m.WorkEffortType) ||
                         @class.Equals(m.WorkEffortState) ||
                         @class.Equals(m.WorkEffortPurpose) ||
                         @class.Equals(m.RequirementState) ||
                         @class.Equals(m.RequirementType) ||
                         @class.Equals(m.IataGseCode))
                {
                    security.GrantLocalAdministratorGlobal(@class, full);
                    security.GrantEmployee(@class, Operations.Read);
                    security.GrantGuest(@class, Operations.Read);
                    security.GrantGenericCustomerContact(@class, Operations.Read);
                }
                else if (@class.Equals(m.LocalisedMedia)
                         || @class.Equals(m.Media)
                         || @class.Equals(m.MediaContent)
                         || @class.Equals(m.LocalisedText))
                {
                    security.GrantEmployee(@class, full);
                    security.GrantGuest(@class, Operations.Read);
                    security.GrantCustomerContactCreator(@class, full);
                }
                else if (@class.Equals(m.OperatingHoursTransaction))
                {
                    security.GrantEmployee(@class, full);
                    security.GrantGenericCustomerContact(@class, full);
                    security.GrantCustomerContactCreator(@class, full);
                }
                else if (@class.Equals(m.UnifiedGood))
                {
                    var excepts = new HashSet<IOperandType>
                    {
                        m.UnifiedGood.Delete,
                    };

                    security.GrantExceptLocalAdministratorGlobal(@class, excepts, full);
                    security.GrantEmployee(@class, Operations.Read);
                    security.GrantGuest(@class, Operations.Read);
                }
                else if (@class.Equals(m.NonUnifiedPart))
                {
                    var excepts = new HashSet<IOperandType>
                    {
                        m.NonUnifiedPart.Delete,
                    };

                    security.GrantExceptProductManager(@class, excepts, full);
                    security.GrantLocalAdministrator(@class, full);
                    security.GrantLocalEmployee(@class, Operations.Read);
                    security.GrantGuest(@class, Operations.Read);
                }
                else if (@class.Equals(m.Facility))
                {
                    security.GrantLocalAdministrator(@class, full);
                    security.GrantEmployee(@class, Operations.Read);
                }
                else if (@class.Equals(m.PartyRate))
                {
                    security.GrantEmployee(@class, Operations.Read);
                    security.GrantGuest(@class, Operations.Read);
                }
                else if (@class.Equals(m.Bank) ||
                         @class.Equals(m.BankAccount) ||
                         @class.Equals(m.Benefit) ||
                         @class.Equals(m.BillingAccount) ||
                         @class.Equals(m.ClientAgreement) ||
                         @class.Equals(m.CommunicationEventPurpose) ||
                         @class.Equals(m.ContactMechanismPurpose) ||
                         @class.Equals(m.ContactMechanismType) ||
                         @class.Equals(m.CreditCard) ||
                         @class.Equals(m.CreditCardCompany) ||
                         @class.Equals(m.CustomOrganisationClassification) ||
                         @class.Equals(m.Document) ||
                         @class.Equals(m.EmailAddress) ||
                         @class.Equals(m.EmailCommunication) ||
                         @class.Equals(m.Employment) ||
                         @class.Equals(m.EmploymentAgreement) ||
                         @class.Equals(m.EuSalesListType) ||
                         @class.Equals(m.Event) ||
                         @class.Equals(m.EventRegistration) ||
                         @class.Equals(m.FaceToFaceCommunication) ||
                         @class.Equals(m.FaxCommunication) ||
                         @class.Equals(m.FinancialTerm) ||
                         @class.Equals(m.Incentive) ||
                         @class.Equals(m.IndustryClassification) ||
                         @class.Equals(m.IncoTerm) ||
                         @class.Equals(m.LegalForm) ||
                         @class.Equals(m.LegalTerm) ||
                         @class.Equals(m.LetterCorrespondence) ||
                         @class.Equals(m.OrganisationClassification) ||
                         @class.Equals(m.OrganisationContactKind) ||
                         @class.Equals(m.OrganisationRole) ||
                         @class.Equals(m.OrganisationRollUp) ||
                         @class.Equals(m.OrganisationUnit) ||
                         @class.Equals(m.OwnBankAccount) ||
                         @class.Equals(m.OwnCreditCard) ||
                         @class.Equals(m.PartyBenefit) ||
                         @class.Equals(m.Passport) ||
                         @class.Equals(m.PersonClassification) ||
                         @class.Equals(m.PhoneCommunication) ||
                         @class.Equals(m.PostalCode) ||
                         @class.Equals(m.ProfessionalAssignment) ||
                         @class.Equals(m.ProfessionalPlacement) ||
                         @class.Equals(m.PurchaseAgreement) ||
                         @class.Equals(m.QuoteTerm) ||
                         @class.Equals(m.SalesAgreement) ||
                         @class.Equals(m.SalesChannel) ||
                         @class.Equals(m.SalesTerritory) ||
                         @class.Equals(m.ServiceTerritory) ||
                         @class.Equals(m.SubContractorAgreement) ||
                         @class.Equals(m.Threshold) ||
                         @class.Equals(m.WebSiteCommunication))
                {
                    security.GrantLocalAdministratorGlobal(@class, full);
                    security.GrantSalesAccountManagerGlobal(@class, full);
                    security.GrantEmployee(@class, Operations.Read);
                }
                else if (@class.Equals(m.Person))
                {
                    var excepts = new HashSet<IOperandType>
                    {
                        m.Person.UserPasswordHash,
                        m.Person.InUserPassword,
                        m.Person.InExistingUserPassword,
                    };

                    security.GrantExceptSalesAccountManagerGlobal(@class, excepts, full);
                    security.GrantExceptLocalAdministrator(@class, excepts, full);
                    security.GrantExceptLocalEmployee(@class, excepts, Operations.Read);

                    security.Grant(Roles.GenericCustomerContactId, @class, m.Person.FirstName, Operations.Read);
                    security.Grant(Roles.GenericCustomerContactId, @class, m.Person.MiddleName, Operations.Read);
                    security.Grant(Roles.GenericCustomerContactId, @class, m.Person.LastName, Operations.Read);

                    security.Grant(Roles.OwnerId, @class, m.Person.FirstName, Operations.Read, Operations.Write);
                    security.Grant(Roles.OwnerId, @class, m.Person.MiddleName, Operations.Read, Operations.Write);
                    security.Grant(Roles.OwnerId, @class, m.Person.LastName, Operations.Read, Operations.Write);
                    security.Grant(Roles.OwnerId, @class, m.Person.Salutation, Operations.Read, Operations.Write);
                    security.Grant(Roles.OwnerId, @class, m.Person.Titles, Operations.Read, Operations.Write);
                    security.Grant(Roles.OwnerId, @class, m.Person.BirthDate, Operations.Read, Operations.Write);
                    security.Grant(Roles.OwnerId, @class, m.Person.MothersMaidenName, Operations.Read, Operations.Write);
                    security.Grant(Roles.OwnerId, @class, m.Person.Height, Operations.Read, Operations.Write);
                    security.Grant(Roles.OwnerId, @class, m.Person.Gender, Operations.Read, Operations.Write);
                    security.Grant(Roles.OwnerId, @class, m.Person.Weight, Operations.Read, Operations.Write);
                    security.Grant(Roles.OwnerId, @class, m.Person.Hobbies, Operations.Read, Operations.Write);
                    security.Grant(Roles.OwnerId, @class, m.Person.MaritalStatus, Operations.Read, Operations.Write);
                    security.Grant(Roles.OwnerId, @class, m.Person.Picture, Operations.Read, Operations.Write);
                    security.Grant(Roles.OwnerId, @class, m.Person.EmailFrequency, Operations.Read, Operations.Write);
                    security.Grant(Roles.OwnerId, @class, m.Person.SocialSecurityNumber, Operations.Read, Operations.Write);
                    security.Grant(Roles.OwnerId, @class, m.Person.Citizenship, Operations.Read, Operations.Write);
                    security.Grant(Roles.OwnerId, @class, m.Person.UserPasswordHash, Operations.Read, Operations.Write);
                    security.Grant(Roles.OwnerId, @class, m.Person.InUserPassword, Operations.Read, Operations.Write);
                    security.Grant(Roles.OwnerId, @class, m.Person.InExistingUserPassword, Operations.Read, Operations.Write);
                    security.Grant(Roles.OwnerId, @class, m.Person.NotificationList, Operations.Read, Operations.Write);
                }
                else if (@class.Equals(m.Country) ||
                         @class.Equals(m.County) ||
                         @class.Equals(m.GenderType) ||
                         @class.Equals(m.Hobby) ||
                         @class.Equals(m.MaritalStatus) ||
                         @class.Equals(m.PersonalTitle) ||
                         @class.Equals(m.Province) ||
                         @class.Equals(m.Region) ||
                         @class.Equals(m.Salutation) ||
                         @class.Equals(m.State) ||
                         @class.Equals(m.Territory))
                {
                    security.GrantLocalAdministratorGlobal(@class, full);
                    security.GrantSalesAccountManagerGlobal(@class, full);
                    security.GrantEmployee(@class, Operations.Read);
                    security.GrantGenericCustomerContact(@class, Operations.Read);
                }
                else if (@class.Equals(m.OrganisationContactRelationship) ||
                         @class.Equals(m.PartyContactMechanism))
                {
                    security.GrantLocalAdministratorGlobal(@class, full);
                    security.GrantSalesAccountManagerGlobal(@class, full);
                    security.GrantEmployee(@class, Operations.Read);
                    security.GrantGenericCustomerContact(@class, Operations.Read, Operations.Write);
                }
                else if (@class.Equals(m.EmailAddress) ||
                         @class.Equals(m.PostalAddress) ||
                         @class.Equals(m.TelecommunicationsNumber) ||
                         @class.Equals(m.WebAddress))
                {
                    security.GrantLocalAdministratorGlobal(@class, full);
                    security.GrantSalesAccountManagerGlobal(@class, full);
                    security.GrantEmployee(@class, Operations.Read);
                    security.GrantGenericCustomerContact(@class, Operations.Read, Operations.Write);
                }
                else if (@class.Equals(m.WorkEffortFixedAssetAssignment))
                {
                    security.GrantLocalAdministrator(@class, full);
                    security.GrantLocalEmployee(@class, Operations.Read);
                    security.GrantCustomerContactCreator(@class, full);
                }
                else if (@class.Equals(m.PartyFinancialRelationship)
                        || @class.Equals(m.SupplierRelationship)
                        || @class.Equals(m.SubContractorRelationship)
                        || @class.Equals(m.ProfessionalServicesRelationship))
                {
                    security.GrantLocalAdministrator(@class, full);
                    security.GrantSalesAccountManagerGlobal(@class, full);
                    security.GrantLocalEmployee(@class, Operations.Read);
                }
                else if (@class.Equals(m.CustomerRelationship))
                {
                    security.GrantLocalAdministrator(@class, full);
                    security.GrantSalesAccountManagerGlobal(@class, full);
                    security.GrantLocalEmployee(@class, Operations.Read);
                    security.GrantGenericCustomerContact(@class, Operations.Read);
                }
                else if (@class.Equals(m.Notification) ||
                        @class.Equals(m.NotificationList) ||
                         @class.Equals(m.UserProfile))
                {
                    security.GrantEmployee(@class, full);
                    security.GrantOwner(@class, full);
                }
                else if (@class.Equals(m.ExchangeRate) ||
                         @class.Equals(m.Settings))
                {
                    security.GrantEmployee(@class, Operations.Read);
                }
                else if (@class.Equals(m.NonUnifiedPartBarcodePrint))
                {
                    security.GrantEmployee(@class, full);
                }
                else if (@class.Equals(m.UserGroup))
                {
                    security.GrantLocalAdministratorGlobal(@class, full);
                    security.GrantEmployee(@class, Operations.Read);
                }
                else if (@class.Equals(m.RepeatingSalesInvoice))
                {
                    security.GrantLocalSalesAccountManager(@class, full);
                    security.GrantLocalAdministrator(@class, full);
                    security.GrantEmployee(@class, Operations.Read);
                }
                else if (@class.Equals(m.Receipt) ||
                         @class.Equals(m.Disbursement))
                {
                    security.GrantLocalSalesAccountManager(@class, full);
                    security.GrantLocalAdministrator(@class, full);
                    security.GrantEmployee(@class, Operations.Read);
                }
                else if (@class.Equals(m.WorkEffortPartStandard)
                        || @class.Equals(m.WorkEffortSkillStandard)
                        || @class.Equals(m.WorkEffortFixedAssetStandard))
                {
                    security.GrantLocalAdministratorGlobal(@class, full);
                    security.GrantEmployee(@class, Operations.Read);
                }
                else if (@class.Equals(m.Vehicle))
                {
                    security.GrantEmployee(@class, full);
                }
                else if (@class.Equals(m.ProductQuoteApproval)
                        || @class.Equals(m.PurchaseOrderApprovalLevel1)
                        || @class.Equals(m.PurchaseOrderApprovalLevel2))
                {
                    security.GrantOwner(@class, full);
                }
                else if (@class.Equals(m.AccountAdjustment) ||
                         @class.Equals(m.AccountingPeriod) ||
                         @class.Equals(m.AccountingTransactionNumber) ||
                         @class.Equals(m.CapitalBudget) ||
                         @class.Equals(m.ChartOfAccounts) ||
                         @class.Equals(m.Journal) ||
                         @class.Equals(m.AccountingTransaction) ||
                         @class.Equals(m.AccountingTransactionDetail) ||
                         @class.Equals(m.OperatingBudget))
                {
                    // Do not grant read permission to employee
                }
                else
                {
                    security.GrantLocalAdministrator(@class, full);
                    security.GrantEmployee(@class, Operations.Read);
                }
            }
        }

        private void CustomOnPostSetup()
        {
        }
    }
}