namespace Allors.Repository
{
    using Allors.Repository.Attributes;
    using static Workspaces;

    [Workspace(Default)]
    public partial class SerialisedItem: ExternalWithPrimaryKey
    {
        public string ExternalPrimaryKey { get; set; }

        #region Allors
        [Id("5e4810ec-4136-481f-9d67-907a3eca1ce2")]
        
        
        #endregion
        [Workspace(Default)]
        public decimal AssignedBookValue { get; set; }

        #region Allors
        [Id("57b1bfd6-c5bd-405e-a811-a78796ddca17")]
        
        
        #endregion
        [Derived]
        [Workspace(Default)]
        public decimal DerivedBookValue { get; set; }

        #region Allors
        [Id("6EBA501E-5EB8-4B9C-B0E7-2658562D8F44")]
        
        
        #endregion
        [Workspace(Default)]
        public decimal EstimatedRefurbishCost { get; set; }

        #region Allors
        [Id("C0DD8167-2975-4DDB-9D20-4F82E3457C99")]
        
        
        #endregion
        [Derived]
        [Workspace(Default)]
        public decimal ActualRefurbishCost { get; set; }

        #region Allors
        [Id("901E5D13-4B43-4FE4-9A79-8EFED2CAFE74")]
        
        
        #endregion
        [Workspace(Default)]
        public decimal EstimatedTransportCost { get; set; }

        #region Allors
        [Id("42A6D660-5483-4FCB-940D-F4E27297AB82")]
        
        
        #endregion
        [Derived]
        [Workspace(Default)]
        public decimal ActualTransportCost { get; set; }

        #region Allors
        [Id("1A2285C0-9DE8-4BC4-B5F8-225C357A149C")]
        
        
        #endregion
        [Workspace(Default)]
        public decimal ExpectedRentalPriceFullServiceLongTerm { get; set; }

        #region Allors
        [Id("4f644894-1de7-48c3-a54f-66001340f90c")]
        #endregion
        [Workspace(Default)]
        public decimal ExpectedRentalPriceFullServiceShortTerm { get; set; }

        #region Allors
        [Id("FEC7C97D-1505-48F0-838D-9FFD8B9BB033")]
        
        
        #endregion
        [Workspace(Default)]
        public decimal ExpectedRentalPriceDryLeaseLongTerm { get; set; }

        #region Allors
        [Id("4b9a654e-a65d-4a91-aa0c-bfe9a563040d")]
        #endregion
        [Workspace(Default)]
        public decimal ExpectedRentalPriceDryLeaseShortTerm { get; set; }

        #region Allors
        [Id("173a1a7a-c3fc-4493-a647-e45c2617ca19")]
        #endregion
        [Derived]
        [Workspace(Default)]
        public decimal ActualOtherCost { get; set; }

        #region Allors
        [Id("33C4F6C7-CF4C-4C2B-9F24-C859117228F3")]
        
        
        #endregion
        [Derived]
        [Precision(19)]
        [Scale(2)]
        [Workspace(Default)]
        public decimal SellingPrice { get; set; }

        #region Allors
        [Id("ea9ca48d-43d2-43c3-bd7b-804398e796c4")]
        
        
        #endregion
        [Size(-1)]
        [Workspace(Default)]
        public string FromInitialImport { get; set; }

        #region Allors
        [Id("30da5b7c-daae-4512-819b-120b70aa7958")]
        
        
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public Brand ChassisBrand { get; set; }

        #region Allors
        [Id("5d370e69-4a22-4cd0-ae75-88cdcd4e0c47")]
        
        
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.ManyToOne)]
        [Workspace(Default)]
        public Model ChassisModel { get; set; }

        #region Allors
        [Id("00c535e8-f7bb-43db-b58a-86c7f573051e")]
        
        
        #endregion
        [Derived]
        [Workspace(Default)]
        public string Location { get; set; }

        #region Allors
        [Id("87cec8a4-4a23-4bfb-8c3f-e70badaa5c5e")]
        #endregion
        [Indexed]
        [Derived]
        [Workspace(Default)]
        public string SuppliedByCountryCode { get; set; }

        #region Allors
        [Id("d7acf937-e90c-4d5b-931d-8ca0d54ff315")]
        #endregion
        [Indexed]
        [Derived]
        [Workspace(Default)]
        public string SalesInvoiceNumber { get; set; }

        #region Allors
        [Id("2cb2bfc6-0af2-4685-af90-0f6e3bb5b64d")]
        #endregion
        [Indexed]
        [Derived]
        [Workspace(Default)]
        public string BillToCustomerName { get; set; }

        #region Allors
        [Id("77a71a30-3116-4f76-bfe5-665a4315855c")]
        #endregion
        [Indexed]
        [Derived]
        [Workspace(Default)]
        public string BillToCustomerCountryCode { get; set; }

        #region Allors
        [Id("35f3d2ee-4cc2-428f-a100-1bb781cea842")]
        
        
        #endregion
        [Derived]
        [Workspace(Default)]
        public string IataCode { get; set; }

        #region Allors
        [Id("efd1dfb2-d300-4dc3-9cfd-0c1a7f1fe3c3")]
        #endregion
        [Derived]
        [Workspace(Default)]
        public string ChassisNumber { get; set; }

        #region Allors
        [Id("b8f6226c-a6b1-49ed-a28d-21894a92de75")]
        #endregion
        [Indexed]
        [Derived]
        [Workspace(Default)]
        public string EngineBrand { get; set; }

        #region Allors
        [Id("d15b9cee-efce-40a8-9e71-d1d653307548")]
        #endregion
        [Indexed]
        [Derived]
        [Workspace(Default)]
        public string EngineModel { get; set; }

        #region Allors
        [Id("0decab95-c9ec-48c9-b97c-d8379696d631")]
        #endregion
        [Derived]
        [Workspace(Default)]
        public string EngineSerialNumber { get; set; }

        #region Allors
        [Id("580fdc80-52f7-4a48-8751-46ef755400f7")]
        #endregion
        [Derived]
        [Workspace(Default)]
        public string OperatingHours { get; set; }

        #region Allors
        [Id("5c8ea019-2b1d-4a86-8a15-ac16c3920557")]
        #endregion
        [Derived]
        [Workspace(Default)]
        public decimal ReplacementValue { get; set; }

        #region Allors
        [Id("38a0af9d-a7f9-4bcf-90a6-075b48021975")]
        #endregion
        [Derived]
        [Workspace(Default)]
        public int LifeTime { get; set; }

        #region Allors
        [Id("d5845ba6-3297-471b-9214-e92b43f57ba7")]
        #endregion
        [Derived]
        [Workspace(Default)]
        public int DepreciationYears { get; set; }

        #region Allors
        [Id("3b8627db-f847-4432-8aef-2fcbe7f23988")]
        #endregion
        [Indexed]
        [Derived]
        [Workspace(Default)]
        public string HsCode { get; set; }

        #region Allors
        [Id("9c15180f-83e6-4419-9f82-85f7e06e05d7")]
        #endregion
        [Derived]
        [Workspace(Default)]
        public string Length { get; set; }

        #region Allors
        [Id("31d63089-033b-45fe-9832-97fd58e63f20")]
        #endregion
        [Derived]
        [Workspace(Default)]
        public string Width { get; set; }

        #region Allors
        [Id("0597f661-637c-45ea-8520-a539454c2bcc")]
        #endregion
        [Derived]
        [Workspace(Default)]
        public string Height { get; set; }

        #region Allors
        [Id("2eaaba49-ce39-4f5e-8198-bfd1b64f1abe")]
        #endregion
        [Derived]
        [Workspace(Default)]
        public string Weight { get; set; }

        #region Allors
        [Id("d0228b1e-4e9e-4eef-b7e2-4baa94a9aa6c")]
        [Indexed]
        #endregion
        [Multiplicity(Multiplicity.OneToMany)]
        [Derived]
        [Workspace(Default)]
        public OperatingHoursTransaction[] SyncedOperatingHoursTransactions { get; set; }

        #region Allors
        [Id("e143ee6b-b9c3-4aff-8c62-6b5487ea5b0b")]
        #endregion
        [Workspace(Default)]
        public decimal AssignedAssumedMonthlyOperatingHours { get; set; }

        #region Allors
        [Id("165bf721-33c3-4dac-a282-30642c7fe387")]
        #endregion
        [Derived]
        [Workspace(Default)]
        public decimal DerivedAssumedMonthlyOperatingHours { get; set; }

        #region Allors
        [Id("cac39cfb-c1bd-4c93-8dc0-3c0884eaa328")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        [Derived]
        public decimal GrossBookValue { get; set; }

        #region Allors
        [Id("842c0adb-14dd-47b9-a94e-eafd6411443c")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        [Derived]
        public decimal ActualGrossBookValue { get; set; }

        #region Allors
        [Id("6de65353-0a72-4f4f-a783-005cf6d8f984")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        [Derived]
        public decimal ExpectedPosa { get; set; }

        #region Allors
        [Id("b750d72f-1bb5-4efc-9397-047ee74749fa")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        [Derived]
        public decimal GoingConcern { get; set; }

        #region Allors
        [Id("2e914712-fed6-4a94-b24f-8f8ad6b04b6a")]
        #endregion
        [Workspace(Default)]
        [Origin(Origin.Session)]
        [Derived]
        public decimal MarketValue { get; set; }
    }
}