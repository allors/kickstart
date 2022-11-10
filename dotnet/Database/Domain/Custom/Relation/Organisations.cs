using System;

namespace Allors.Database.Domain
{
    public partial class Organisations
    {
        protected override void CustomSetup(Setup setup)
        {
            //var logo = setup.DataPath + @"\www\admin\images\logo.png";
            var logo = "";

            var belgium = new Countries(this.Transaction).CountryByIsoCode["BE"];
            var euro = belgium.Currency;

            var serialisedItemSoldOns = new SerialisedItemSoldOn[] { new SerialisedItemSoldOns(this.Transaction).SalesInvoiceSend, new SerialisedItemSoldOns(this.Transaction).PurchaseInvoiceConfirm };

            if (new Organisations(this.Transaction).FindBy(M.Organisation.Name, "acme") == null)
            {
                var singleton = this.Transaction.GetSingleton();
                var acme = singleton.DefaultInternalOrganisation = CreateInternalOrganisation(
                    this.Transaction,
                name: "acme",
                address1: "address",
                postalCode: "code",
                locality: "city",
                country: belgium,
                phone1CountryCode: "+32",
                phone1: "111",
                phone1Purpose: new ContactMechanismPurposes(this.Transaction).GeneralPhoneNumber,
                phone2CountryCode: string.Empty,
                phone2: string.Empty,
                phone2Purpose: null,
                emailAddress: "email@acme.com",
                websiteAddress: "www.acme.com",
                taxNumber: "BE 1234567",
                bankName: "ING",
                facilityName: "Warehouse",
                bic: "BBRUBEBB",
                iban: "BE89 3200 1467 7685",
                currency: euro,
                logo: "allors.png",
                storeName: "Store",
                billingProcess: new BillingProcesses(this.Transaction).BillingForOrderItems,
                customerShipmentNumberPrefix: "i-CS",
                customerReturnNumberPrefix: "i-CR",
                purchaseShipmentNumberPrefix: "i-PS",
                purchaseReturnNumberPrefix: "i-PR",
                salesInvoiceNumberPrefix: "i-SI",
                salesOrderNumberPrefix: "i-SO",
                purchaseOrderNumberPrefix: "purchase orderno: ",
                purchaseInvoiceNumberPrefix: "incoming invoiceno: ",
                requestNumberPrefix: "i-RFQ",
                productQuoteNumberPrefix: "i-PQ",
                statementOfWorkNumberPrefix: "i-WQ",
                productNumberPrefix: "i-",
                workEffortPrefix: "i-WO-",
                requirementPrefix: "i-REQ-",
                creditNoteNumberPrefix: "i-CN-",
                isImmediatelyPicked: true,
                autoGenerateShipmentPackage: true,
                isImmediatelyPacked: true,
                isAutomaticallyShipped: true,
                autoGenerateCustomerShipment: true,
                isAutomaticallyReceived: false,
                shipmentIsAutomaticallyReturned: false,
                autoGeneratePurchaseShipment: false,
                useCreditNoteSequence: true,
                requestCounterValue: 0,
                productQuoteCounterValue: 0,
                statementOfWorkCounterValue: 0,
                orderCounterValue: 0,
                purchaseOrderCounterValue: 0,
                invoiceCounterValue: 0,
                purchaseInvoiceCounterValue: 0,
                purchaseOrderNeedsApproval: false,
                purchaseOrderApprovalThresholdLevel1: null,
                purchaseOrderApprovalThresholdLevel2: null,
                serialisedItemSoldOns: serialisedItemSoldOns,
                collectiveWorkEffortInvoice: true,
                invoiceSequence: new InvoiceSequences(this.Transaction).EnforcedSequence,
                requestSequence: new RequestSequences(this.Transaction).EnforcedSequence,
                quoteSequence: new QuoteSequences(this.Transaction).EnforcedSequence,
                customerShipmentSequence: new CustomerShipmentSequences(this.Transaction).EnforcedSequence,
                purchaseShipmentSequence: new PurchaseShipmentSequences(this.Transaction).EnforcedSequence,
                workEffortSequence: new WorkEffortSequences(this.Transaction).EnforcedSequence,
                requirementSequence: new RequirementSequences(this.Transaction).EnforcedSequence);

                acme.UniqueId = new Guid("ee1a54bf-cae4-4769-ae6f-5bcb507056da");
            }
        }
    }
}
