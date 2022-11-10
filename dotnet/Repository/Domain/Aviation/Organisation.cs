namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;

    [Workspace(Default)]
    public partial class Organisation
    {
        #region inherited properties

        public string ExternalPrimaryKey { get; set; }
        public Template WorkOrderSalesInvoiceTemplate { get; set; }

        public Person[] ProductQuoteApprovers { get; set; }
        
        public SecurityToken ProductQuoteApproverSecurityToken { get; set; }

        public Grant ProductQuoteApproverGrant { get; set; }

        public UserGroup ProductQuoteApproverUserGroup { get; set; }

        public Person[] PurchaseOrderApproversLevel1 { get; set; }

        public SecurityToken PurchaseOrderApproverLevel1SecurityToken { get; set; }

        public Grant PurchaseOrderApproverLevel1Grant { get; set; }

        public UserGroup PurchaseOrderApproverLevel1UserGroup { get; set; }

        public Person[] PurchaseOrderApproversLevel2 { get; set; }

        public SecurityToken PurchaseOrderApproverLevel2SecurityToken { get; set; }
        
        public Grant PurchaseOrderApproverLevel2Grant { get; set; }

        public UserGroup PurchaseOrderApproverLevel2UserGroup { get; set; }

        public Person[] PurchaseInvoiceApprovers { get; set; }

        public SecurityToken PurchaseInvoiceApproverSecurityToken { get; set; }

        public Grant PurchaseInvoiceApproverGrant { get; set; }

        public UserGroup PurchaseInvoiceApproverUserGroup { get; set; }

        public Person[] BlueCollarWorkers { get; set; }

        public SecurityToken BlueCollarWorkerSecurityToken { get; set; }

        public Grant BlueCollarWorkerGrant { get; set; }

        public UserGroup BlueCollarWorkerUserGroup { get; set; }

        public Person[] StockManagers { get; set; }

        public SecurityToken StockManagerSecurityToken { get; set; }
        
        public Grant StockManagerGrant { get; set; }

        public UserGroup StockManagerUserGroup { get; set; }

        public Person[] LocalAdministrators { get; set; }

        public SecurityToken LocalAdministratorSecurityToken { get; set; }

        public Grant LocalAdministratorGrant { get; set; }

        public UserGroup LocalAdministratorUserGroup { get; set; }

        public Person[] LocalEmployees { get; set; }

        public SecurityToken LocalEmployeeSecurityToken { get; set; }

        public Grant LocalEmployeeGrant { get; set; }

        public UserGroup LocalEmployeeUserGroup { get; set; }

        public Person[] LocalSalesAccountManagers { get; set; }

        public SecurityToken LocalSalesAccountManagerSecurityToken { get; set; }

        public Grant LocalSalesAccountManagerGrant { get; set; }

        public UserGroup LocalSalesAccountManagerUserGroup { get; set; }

        public NonUnifiedPart[] SpareParts { get; set; }

        public SerialisedItem[] SerialisedItems { get; set; }

        public Template PartQuoteTemplate { get; set; }

        #endregion

        #region Allors
        [Id("D1EE212E-25CD-4255-B1FA-C63BCE1E6F0E")]
        
        
        [Indexed]
        [Size(256)]
        #endregion
        public string ExternalPersonKey { get; set; }

        #region Allors
        [Id("190d9d13-7c6e-4bd7-ba0a-9eef755e2cc6")]
        #endregion
        [Workspace(Default)]
        public string CleaningCalculation { get; set; }

        #region Allors
        [Id("441e93dc-351f-48e8-b6d4-3c2badf80b79")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        public decimal CleaningMinimum { get; set; }

        #region Allors
        [Id("c8e53d85-7c7b-494c-9594-ed43395df084")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        public decimal CleaningMaximum { get; set; }

        #region Allors
        [Id("5587df5d-d1fc-4d53-b622-f25150512d14")]
        #endregion
        [Required]
        [Workspace(Default)]
        public bool ExcludeCleaning { get; set; }

        #region Allors
        [Id("240c2064-943e-4751-a32f-19e82b6c8dbb")]
        #endregion
        [Workspace(Default)]
        public string SundriesCalculation { get; set; }

        #region Allors
        [Id("eeeaaba7-b578-4595-8b15-f285993efb6a")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        public decimal SundriesMinimum { get; set; }

        #region Allors
        [Id("8d7d2ad6-bd4b-4836-8076-f72c7f486eb7")]
        #endregion
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        public decimal SundriesMaximum { get; set; }

        #region Allors
        [Id("ab593daf-980a-4cb9-ae79-ad3f2e7f0421")]
        #endregion
        [Required]
        [Workspace(Default)]
        public bool ExcludeSundries { get; set; }
    }
}