namespace Allors.Repository
{
    using Attributes;
    using static Workspaces;

    public partial interface InternalOrganisation: ExternalWithPrimaryKey
    {
        #region Allors
        [Id("3570A628-EB0A-4F77-B6A8-431CBB1A2049")]
        
        
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        Template WorkOrderSalesInvoiceTemplate { get; set; }

        #region Allors
        [Id("c39ec979-1570-4b2a-bfe7-69577bc4dea6")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        Template PartQuoteTemplate { get; set; }

        #region Allors
        [Id("0A1163E6-FBAA-4A67-B988-40E0E3548CCE")]
        
        
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        Person[] ProductQuoteApprovers { get; set; }

        #region Allors	
        [Id("556CBF9E-919A-4908-9115-161570B1D045")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Derived]
        SecurityToken ProductQuoteApproverSecurityToken { get; set; }
        
        #region Allors
        [Id("0C8C8316-4DA3-48B4-B758-198ABF06BB5E")]
        
        
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Derived]
        Grant ProductQuoteApproverGrant{ get; set; }

        #region Allors
        [Id("8DCFB787-1C1B-4B15-937A-6E97A38179BA")]
        
        
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Derived]
        [Indexed]
        [Workspace(Default)]
        UserGroup ProductQuoteApproverUserGroup { get; set; }

        #region Allors
        [Id("F003F1F4-5CF9-49AF-B7CE-F4034A79FE7B")]
        
        
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        Person[] PurchaseOrderApproversLevel1 { get; set; }

        #region Allors	
        [Id("E0C5B17D-9DC0-4DDB-A984-8D009F726F1A")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Derived]
        SecurityToken PurchaseOrderApproverLevel1SecurityToken { get; set; }

        #region Allors
        [Id("3907FA80-4FDA-4039-851A-283951A89CAD")]
        
        
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Derived]
        Grant PurchaseOrderApproverLevel1Grant { get; set; }

        #region Allors
        [Id("F594E493-5590-48E4-9C84-E7C0358EEE5E")]
        
        
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Derived]
        [Indexed]
        [Workspace(Default)]
        UserGroup PurchaseOrderApproverLevel1UserGroup { get; set; }

        #region Allors
        [Id("63FAAFDA-7474-4DA2-AA90-C186D8953687")]
        
        
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        Person[] PurchaseOrderApproversLevel2 { get; set; }

        #region Allors	
        [Id("D4A55ED7-EF6C-4454-A1C9-3A80AE452F6C")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Derived]
        SecurityToken PurchaseOrderApproverLevel2SecurityToken { get; set; }

        #region Allors
        [Id("B91DB2D1-F3ED-4B0F-BFA4-D921C43484D2")]
        
        
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Derived]
        Grant PurchaseOrderApproverLevel2Grant { get; set; }

        #region Allors
        [Id("E29EC5C6-EAC5-4974-85F1-4ADF6C5EC743")]
        
        
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Derived]
        [Indexed]
        [Workspace(Default)]
        UserGroup PurchaseOrderApproverLevel2UserGroup { get; set; }

        #region Allors
        [Id("2BC6D816-9E25-4C77-8EAA-1BF06344120A")]
        
        
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        Person[] PurchaseInvoiceApprovers { get; set; }

        #region Allors	
        [Id("2A6EE4F7-3050-4A16-A79B-6E2DA53F8DD9")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Derived]
        SecurityToken PurchaseInvoiceApproverSecurityToken { get; set; }

        #region Allors
        [Id("7AF73021-5B87-46F4-B27E-F846273873E4")]
        
        
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Derived]
        Grant PurchaseInvoiceApproverGrant { get; set; }

        #region Allors
        [Id("A86793A1-4607-451F-B9D3-61287A7341CB")]
        
        
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Derived]
        [Indexed]
        [Workspace(Default)]
        UserGroup PurchaseInvoiceApproverUserGroup { get; set; }

        #region Allors
        [Id("7886C66E-CC9F-44A5-BABC-3E0B987F25E2")]
        
        
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        Person[] BlueCollarWorkers { get; set; }

        #region Allors	
        [Id("1BB408C3-06B6-4F88-AA4F-089DB58676CA")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Derived]
        SecurityToken BlueCollarWorkerSecurityToken { get; set; }

        #region Allors
        [Id("B727ED6B-A18B-4BA3-8CE6-33A164898484")]
        
        
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Derived]
        Grant BlueCollarWorkerGrant { get; set; }

        #region Allors
        [Id("3C6FD866-64AA-481A-9AAC-4A538C643129")]
        
        
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Derived]
        [Indexed]
        [Workspace(Default)]
        UserGroup BlueCollarWorkerUserGroup { get; set; }

        #region Allors
        [Id("B4D37277-2779-40D0-9BBF-754299D8902B")]
        
        
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        Person[] StockManagers { get; set; }

        #region Allors	
        [Id("A23549C3-C8D6-4CE5-83CB-1A309E4AB3D3")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Derived]
        SecurityToken StockManagerSecurityToken { get; set; }

        #region Allors
        [Id("5DD78ECC-8FCD-4F71-A1F8-C1068606A9F4")]
        
        
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Derived]
        Grant StockManagerGrant { get; set; }

        #region Allors
        [Id("9C71DBC1-42DE-4253-A514-8289536B2503")]
        
        
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Derived]
        [Indexed]
        [Workspace(Default)]
        UserGroup StockManagerUserGroup { get; set; }

        #region Allors
        [Id("C0484680-5F19-418E-A9D3-A12A94D47B87")]
        
        
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        Person[] LocalAdministrators { get; set; }

        #region Allors	
        [Id("8C1AF4A4-19EC-441F-A28B-33C301173C87")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Derived]
        SecurityToken LocalAdministratorSecurityToken { get; set; }

        #region Allors
        [Id("FF4343D5-1D2F-4FD0-8A98-50A433013A25")]
        
        
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Derived]
        Grant LocalAdministratorGrant { get; set; }

        #region Allors
        [Id("3EA459D1-54D0-49E4-B4FF-E3AB6C2A0A3C")]
        
        
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Derived]
        [Indexed]
        [Workspace(Default)]
        UserGroup LocalAdministratorUserGroup { get; set; }

        #region Allors
        [Id("3be0fd7a-0924-4dc3-b7df-84677c1b87ce")]
        #endregion
        [Indexed]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        Person[] LocalEmployees { get; set; }

        #region Allors	
        [Id("38d6ea10-e81e-416a-9af4-eb885f3a5563")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Derived]
        SecurityToken LocalEmployeeSecurityToken { get; set; }

        #region Allors
        [Id("4f00f3e1-73dc-4a15-abc0-9696fb02d8aa")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Derived]
        Grant LocalEmployeeGrant { get; set; }

        #region Allors
        [Id("4ccc4e50-98be-48f2-88a7-c6ac0703fba8")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Derived]
        [Indexed]
        [Workspace(Default)]
        UserGroup LocalEmployeeUserGroup { get; set; }

        #region Allors
        [Id("9095fd84-5b93-45c3-994e-ddf4cc628aaf")]
        #endregion
        [Indexed]
        [Multiplicity(Multiplicity.ManyToMany)]
        [Workspace(Default)]
        Person[] LocalSalesAccountManagers { get; set; }

        #region Allors	
        [Id("1eeeb36c-f24a-419a-95a8-3161983b097e")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Derived]
        SecurityToken LocalSalesAccountManagerSecurityToken { get; set; }

        #region Allors
        [Id("5060f289-9bd6-40dc-93a8-7089f9d4f002")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Indexed]
        [Derived]
        Grant LocalSalesAccountManagerGrant { get; set; }

        #region Allors
        [Id("56b847a5-6258-496d-9bb3-9d6871fb8941")]
        #endregion
        [Multiplicity(Multiplicity.OneToOne)]
        [Derived]
        [Indexed]
        [Workspace(Default)]
        UserGroup LocalSalesAccountManagerUserGroup { get; set; }

        #region Allors
        [Id("24ebd659-be19-4434-b040-d940d437f979")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Derived]
        [Indexed]
        [Workspace(Default)]
        NonUnifiedPart[] SpareParts{ get; set; }

        #region Allors
        [Id("e0c288a5-9d3b-4e9d-8157-6a0b6789b040")]
        #endregion
        [Multiplicity(Multiplicity.ManyToMany)]
        [Derived]
        [Indexed]
        [Workspace(Default)]
        SerialisedItem[] SerialisedItems { get; set; }
    }
}