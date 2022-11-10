using System;
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;
using System.Linq;
using Allors.Excel;
using Allors.Extra;
using Allors.Workspace;
using Allors.Workspace.Data;
using Allors.Workspace.Meta;
using Application.Excel;
using Organisation = Allors.Workspace.Domain.Organisation;
using Ownership = Allors.Workspace.Domain.Ownership;
using SerialisedItem = Allors.Workspace.Domain.SerialisedItem;
using SerialisedItemAvailability = Allors.Workspace.Domain.SerialisedItemAvailability;
using SerialisedItemCharacteristic = Allors.Workspace.Domain.SerialisedItemCharacteristic;
using SerialisedItemCharacteristicType = Allors.Workspace.Domain.SerialisedItemCharacteristicType;
using SerialisedItemState = Allors.Workspace.Domain.SerialisedItemState;
using Task = System.Threading.Tasks.Task;
using UnifiedGood = Allors.Workspace.Domain.UnifiedGood;

namespace Application.Sheets
{
    public class SerialisedItemsSheet : ISheet, ISaveable
    {
        private int StartRowNewItems;

        public SerialisedItemsSheet(Program program)
        {
            this.NamedRangesSheet = program.ActiveWorkbook.AddWorksheet();
            this.NamedRangesSheet.Name = "NamedRanges";
            this.NamedRangesControls = new Controls(this.NamedRangesSheet);

            this.Sheet = program.ActiveWorkbook.AddWorksheet();
            this.Binder = new Binder(this.Sheet, Constants.ChangedStyle);
            this.Binder.ToDomained += this.Binder_ToDomained;

            this.Controls = new Controls(this.Sheet);

            this.NamedRangesSheet.SheetActivated += this.Sheet_SheetActivated;
            this.Sheet.SheetActivated += this.Sheet_SheetActivated;
            this.Sheet.CellsChanged += Sheet_CellsChanged;

            this.partSelectCellByNewSerialisedItem = new Dictionary<SerialisedItem, ICell>();
            this.ownerSelectCellByNewSerialisedItem = new Dictionary<SerialisedItem, ICell>();

            this.M = program.M;
            this.MessageService = program.Workspace.Services.Get<IMessageService>();
            this.ErrorService = program.Workspace.Services.Get<IErrorService>();

            this.Session = program.Workspace.CreateSession();
        }

        public M M { get; set; }

        public ISession Session { get; private set; }

        public IWorksheet Sheet { get; }

        public IWorksheet NamedRangesSheet { get; }

        public Binder Binder { get; set; }

        public Controls Controls { get; }

        public Controls NamedRangesControls { get; }

        public IMessageService MessageService { get; set; }

        public IErrorService ErrorService { get; }

        private bool IsWorksheetUpToDate { get; set; }

        private Dictionary<SerialisedItem, ICell> partSelectCellByNewSerialisedItem;

        private Dictionary<SerialisedItem, ICell> ownerSelectCellByNewSerialisedItem;

        private UnifiedGood[] parts;

        private Organisation[] organisations;

        public async Task Init()
        {
            var now = System.DateTime.Now;

            var pulls = new List<Pull>()
            {
                new Pull
                {
                    Extent = new Filter(M.SerialisedItem),

                    Results = new[]
                    {
                        new Result
                        {
                            Select = new Select
                            {
                                Include = new []
                                {
                                    new Node(M.SerialisedItem.OwnedBy),
                                    new Node(M.SerialisedItem.Ownership),
                                    new Node(M.SerialisedItem.SerialisedItemState),
                                    new Node(M.SerialisedItem.SerialisedItemAvailability),
                                    new Node(M.SerialisedItem.SerialisedItemCharacteristics, new []
                                    {
                                        new Node(M.SerialisedItemCharacteristic.SerialisedItemCharacteristicType),
                                    }),
                                }
                            }
                        }
                    }
                },
                new Pull
                {
                    Extent = new Filter(M.SerialisedItemCharacteristicType),
                },
            };

            this.AddLookupPulls(pulls);

            var result = await this.Session.PullAsync(pulls.ToArray());
            this.Session.Reset();

            ProcessLookupResults(
                result,
                out var serialisedItemAvailabilities,
                out var serialisedItemStates,
                out var ownerships,
                out var yesNo,
                out var availabilityOptions,
                out var ownershipOptions,
                out var partOptions,
                out var stateOptions,
                out var organisationOptions,
                out var yesNoOptions
                );

            var serialisedItems = result.GetCollection<SerialisedItem>("SerialisedItems") ?? Array.Empty<SerialisedItem>();
            var serialisedItemCharacteristicTypes = result.GetCollection<SerialisedItemCharacteristicType>("SerialisedItemCharacteristicTypes") ?? Array.Empty<SerialisedItemCharacteristicType>();

            var index = 0;
            var aviationRef = new Column { Header = "Aviation Ref.", Index = index++, NumberFormat = "@" };
            var serialNumber = new Column { Header = "Serial Number", Index = index++, NumberFormat = "@" };
            var product = new Column { Header = "Product", Index = index++, NumberFormat = "@" };
            var chassisNumber = new Column { Header = "Chassis Number", Index = index++, NumberFormat = "@" };
            var fleetCode = new Column { Header = "Fleet Code", Index = index++, NumberFormat = "@" };
            var engineBrand = new Column { Header = "Engine Brand", Index = index++, NumberFormat = "@" };
            var engineModel = new Column { Header = "Engine Model", Index = index++, NumberFormat = "@" };
            var engineSerialNumber = new Column { Header = "Engine S/N", Index = index++, NumberFormat = "@" };
            var ownerStatus = new Column { Header = "Owner Status", Index = index++, NumberFormat = "@" };
            var availability = new Column { Header = "Availability", Index = index++, NumberFormat = "@" };
            var availableForSale = new Column { Header = "Availabl for sale", Index = index++, NumberFormat = "@" };
            var state = new Column { Header = "Status", Index = index++, NumberFormat = "@" };
            var ownerName = new Column { Header = "Owner", Index = index++, NumberFormat = "@" };
            var acquisitionDate = new Column { Header = "Acquisition Date", Index = index++, NumberFormat = "dd/MM/yyyy;@" };
            var acquisitionYear = new Column { Header = "Acquisition Year", Index = index++, NumberFormat = "@" };
            var manufacturingYear = new Column { Header = "Manufacturing Year", Index = index++, NumberFormat = "#" };
            var estimatedRefurbishCost = new Column { Header = "Est. Refurbish Cost", Index = index++, NumberFormat = "@" };
            var estimatedTransportCost = new Column { Header = "Est. Transport Cost", Index = index++, NumberFormat = "@" };
            var expectedSalesPrice = new Column { Header = "Expected Sales Price", Index = index++, NumberFormat = "@" };
            var rentalPriceFullServiceLongTerm = new Column { Header = "Est. Rental Price Full Service Long Term", Index = index++, NumberFormat = "@" };
            var rentalPriceFullServiceShortTerm = new Column { Header = "Est. Rental Price Full Service short Term", Index = index++, NumberFormat = "@" };
            var rentalPriceDryLeaseLongTerm = new Column { Header = "Est. Rental Price Dry Lease Long Term", Index = index++, NumberFormat = "@" };
            var rentalPriceDryLeaseShortTerm = new Column { Header = "Est. Rental Price Dry Lease Short Term", Index = index++, NumberFormat = "@" };
            var financialBookValue = new Column { Header = "Fin. Book Value", Index = index++, NumberFormat = "@" };
            var operationHoursAct = new Column { Header = "Operation Hours Act", Index = index++, NumberFormat = "#" };
            var family = new Column { Header = "Family", Index = index++, NumberFormat = "@" };
            var category = new Column { Header = "Category", Index = index++, NumberFormat = "@" };
            var brand = new Column { Header = "Brand", Index = index++, NumberFormat = "@" };
            var model = new Column { Header = "Model", Index = index++, NumberFormat = "@" };
            var onQuote = new Column { Header = "On Quote", Index = index++, NumberFormat = "@" };
            var onSalesOrder = new Column { Header = "On Sales Order", Index = index++, NumberFormat = "@" };
            var onWorkeffort = new Column { Header = "In Workshop", Index = index++, NumberFormat = "@" };
            var supplierName = new Column { Header = "Supplier", Index = index++, NumberFormat = "@" };
            var supplierInvoiceNr = new Column { Header = "Supplier Invoice Nr.", Index = index++, NumberFormat = "@" };
            var country = new Column { Header = "Country", Index = index++, NumberFormat = "@" };
            var location = new Column { Header = "Location", Index = index++, NumberFormat = "@" };
            var replacementValue = new Column { Header = "Replacement Value", Index = index++, NumberFormat = "@" };
            var lifeTime = new Column { Header = "Life Time", Index = index++, NumberFormat = "@" };
            var depreciationYearsWhenNew = new Column { Header = "Depreciation Years when new", Index = index++, NumberFormat = "@" };
            var purchasePrice = new Column { Header = "Purchase Price", Index = index++, NumberFormat = "@" };
            var actualRefurbishCost = new Column { Header = "Actual Refurbish Cost", Index = index++, NumberFormat = "@" };
            var actualTransportCost = new Column { Header = "Actual Transport Cost", Index = index++, NumberFormat = "@" };
            var actualOtherCost = new Column { Header = "Actual Other Cost", Index = index++, NumberFormat = "@" };
            var finalSellingPrice = new Column { Header = "Final Selling Price", Index = index++, NumberFormat = "@" };
            var buyerName = new Column { Header = "Buyer", Index = index++, NumberFormat = "@" };
            var sellerName = new Column { Header = "Seller", Index = index++, NumberFormat = "@" };
            var hsCode = new Column { Header = "HS Code", Index = index++, NumberFormat = "@" };
            var lengthMM = new Column { Header = "Length mm", Index = index++, NumberFormat = "@" };
            var widthMM = new Column { Header = "Width mm", Index = index++, NumberFormat = "@" };
            var heightMM = new Column { Header = "Height mm", Index = index++, NumberFormat = "@" };
            var weightMM = new Column { Header = "Weight mm", Index = index++, NumberFormat = "@" };
            var invoiceNr = new Column { Header = "Invoice Nr", Index = index++, NumberFormat = "@" };
            var customerName = new Column { Header = "Customer Name", Index = index++, NumberFormat = "@" };
            var customerCountry = new Column { Header = "Customer Country", Index = index++, NumberFormat = "@" };
            var fromImport = new Column { Header = "From excel import", Index = index++, NumberFormat = "@" };

            var columns = new[]
            {
                aviationRef,
                serialNumber ,
                product,
                chassisNumber,
                fleetCode,
                engineBrand,
                engineModel,
                engineSerialNumber,
                ownerStatus ,
                availability,
                availableForSale,
                state,
                ownerName ,
                acquisitionDate,
                acquisitionYear,
                manufacturingYear ,
                estimatedRefurbishCost,
                estimatedTransportCost,
                expectedSalesPrice,
                rentalPriceFullServiceLongTerm,
                rentalPriceFullServiceShortTerm,
                rentalPriceDryLeaseLongTerm,
                rentalPriceDryLeaseShortTerm,
                financialBookValue,
                operationHoursAct,
                family,
                category,
                brand ,
                model ,
                onQuote,
                onSalesOrder,
                onWorkeffort,
                supplierName,
                supplierInvoiceNr ,
                country,
                location,
                replacementValue,
                lifeTime,
                depreciationYearsWhenNew,
                purchasePrice,
                actualRefurbishCost,
                actualTransportCost,
                actualOtherCost,
                finalSellingPrice,
                buyerName,
                sellerName,
                hsCode,
                lengthMM,
                widthMM,
                heightMM,
                weightMM ,
                invoiceNr,
                customerName,
                customerCountry ,
                fromImport,
            };

            foreach (var column in columns)
            {
                this.Sheet[0, column.Index].Value = column.Header;
                this.Sheet[0, column.Index].Style = new Style(Color.LightBlue, Color.Black);
            }

            var row = 1;
            foreach (var serialisedItem in serialisedItems.OrderBy(v => v.ItemNumber))
            {
                foreach (var column in columns)
                {
                    this.Sheet[row, column.Index].NumberFormat = column.NumberFormat;
                }

                this.Controls.TextBox(row, serialNumber.Index, serialisedItem, M.SerialisedItem.SerialNumber);
                this.Controls.Static(row, product.Index, serialisedItem.PartName);

                this.Controls.TextBox(row, chassisNumber.Index, serialisedItem?.SerialisedItemCharacteristics
                    .FirstOrDefault(v => string.Equals(v?.SerialisedItemCharacteristicType?.Name, "Chassis Number", StringComparison.OrdinalIgnoreCase)),
                    M.SerialisedItemCharacteristic.Value);

                this.Controls.TextBox(row, fleetCode.Index, serialisedItem, M.SerialisedItem.CustomerReferenceNumber);

                this.Controls.TextBox(row, engineBrand.Index, serialisedItem?.SerialisedItemCharacteristics
                    .FirstOrDefault(v => string.Equals(v?.SerialisedItemCharacteristicType?.Name, "Engine Brand", StringComparison.OrdinalIgnoreCase)),
                    M.SerialisedItemCharacteristic.Value);

                this.Controls.TextBox(row, engineModel.Index, serialisedItem?.SerialisedItemCharacteristics
                    .FirstOrDefault(v => string.Equals(v?.SerialisedItemCharacteristicType?.Name, "Engine Model", StringComparison.OrdinalIgnoreCase)),
                    M.SerialisedItemCharacteristic.Value);

                this.Controls.TextBox(row, engineSerialNumber.Index, serialisedItem?.SerialisedItemCharacteristics
                    .FirstOrDefault(v => string.Equals(v?.SerialisedItemCharacteristicType?.Name, "Engine Serial Number", StringComparison.OrdinalIgnoreCase)),
                    M.SerialisedItemCharacteristic.Value);

                this.Controls.Select(
                    row, ownerStatus.Index,
                    ownershipOptions,
                    serialisedItem,
                    M.SerialisedItem.Ownership,
                    M.Ownership.Name,
                    getRelation: (object key) =>
                    {
                        return ownerships.FirstOrDefault(v => string.Equals(v.Name, Convert.ToString(key, CultureInfo.CurrentCulture), StringComparison.OrdinalIgnoreCase));
                    });

                this.Controls.Select(
                    row, availability.Index,
                    availabilityOptions,
                    serialisedItem,
                    M.SerialisedItem.SerialisedItemAvailability,
                    M.SerialisedItemAvailability.Name,
                    getRelation: (object key) =>
                    {
                        return serialisedItemAvailabilities.FirstOrDefault(v => string.Equals(v.Name, Convert.ToString(key, CultureInfo.CurrentCulture), StringComparison.OrdinalIgnoreCase));
                    });

                this.Controls.Select(
                    row, availableForSale.Index,
                    yesNoOptions,
                    serialisedItem,
                    M.SerialisedItem.AvailableForSale
                );

                this.Controls.Select(
                    row, state.Index,
                    stateOptions,
                    serialisedItem,
                    M.SerialisedItem.SerialisedItemState,
                    M.SerialisedItemState.Name,
                    getRelation: (object key) =>
                    {
                        return serialisedItemStates.FirstOrDefault(v => string.Equals(v.Name, Convert.ToString(key, CultureInfo.CurrentCulture), StringComparison.OrdinalIgnoreCase));
                    });

                this.Controls.Select(
                    row, ownerName.Index,
                    organisationOptions,
                    serialisedItem,
                    M.SerialisedItem.OwnedBy,
                    M.Organisation.DisplayName,
                    getRelation: (object key) =>
                    {
                        return organisations.FirstOrDefault(v => string.Equals(v.DisplayName, Convert.ToString(key, CultureInfo.CurrentCulture), StringComparison.OrdinalIgnoreCase));
                    });

                this.Controls.TextBox(row, acquisitionDate.Index, serialisedItem, M.SerialisedItem.AcquiredDate);
                this.Controls.TextBox(row, acquisitionYear.Index, serialisedItem, M.SerialisedItem.AcquisitionYear);
                this.Controls.TextBox(row, manufacturingYear.Index, serialisedItem, M.SerialisedItem.ManufacturingYear);
                this.Controls.TextBox(row, estimatedRefurbishCost.Index, serialisedItem, M.SerialisedItem.EstimatedRefurbishCost);
                this.Controls.TextBox(row, estimatedTransportCost.Index, serialisedItem, M.SerialisedItem.EstimatedTransportCost);
                this.Controls.TextBox(row, expectedSalesPrice.Index, serialisedItem, M.SerialisedItem.ExpectedSalesPrice);
                this.Controls.TextBox(row, rentalPriceFullServiceLongTerm.Index, serialisedItem, M.SerialisedItem.ExpectedRentalPriceFullServiceLongTerm);
                this.Controls.TextBox(row, rentalPriceFullServiceShortTerm.Index, serialisedItem, M.SerialisedItem.ExpectedRentalPriceFullServiceShortTerm);
                this.Controls.TextBox(row, rentalPriceDryLeaseLongTerm.Index, serialisedItem, M.SerialisedItem.ExpectedRentalPriceDryLeaseLongTerm);
                this.Controls.TextBox(row, rentalPriceDryLeaseShortTerm.Index, serialisedItem, M.SerialisedItem.ExpectedRentalPriceDryLeaseShortTerm);
                this.Controls.TextBox(row, financialBookValue.Index, serialisedItem, roleType: M.SerialisedItem.AssignedBookValue, displayRoleType: M.SerialisedItem.DerivedBookValue);

                this.Controls.Label(row, aviationRef.Index, serialisedItem, M.SerialisedItem.ItemNumber);
                this.Controls.Static(row, family.Index, string.Join(",", serialisedItem.ProductCategoriesDisplayName?.Split('/')[0]));

                if (serialisedItem.ProductCategoriesDisplayName.Contains("/"))
                {
                    this.Controls.Static(row, category.Index, string.Join(",", serialisedItem.ProductCategoriesDisplayName?.Substring(serialisedItem.ProductCategoriesDisplayName.Split('/')[0].Length + 1)));
                }

                this.Controls.Static(row, operationHoursAct.Index, serialisedItem?.OperatingHours);
                this.Controls.Static(row, brand.Index, serialisedItem.BrandName);
                this.Controls.Static(row, model.Index, serialisedItem.ModelName);
                this.Controls.Static(row, onQuote.Index, serialisedItem.OnQuote ? "Yes" : "No");
                this.Controls.Static(row, onSalesOrder.Index, serialisedItem.OnSalesOrder ? "Yes" : "No");
                this.Controls.Static(row, onWorkeffort.Index, serialisedItem.OnWorkEffort ? "Yes" : "No");
                this.Controls.Static(row, supplierName.Index, serialisedItem.SuppliedByPartyName);
                this.Controls.Static(row, country.Index, serialisedItem.SuppliedByCountryCode);

                this.Controls.Static(row, supplierInvoiceNr.Index, string.Join(",", serialisedItem.PurchaseInvoiceNumber));
                this.Controls.Static(row, location.Index, serialisedItem.Location);
                this.Controls.Static(row, replacementValue.Index, serialisedItem.ReplacementValue);
                this.Controls.Static(row, lifeTime.Index, serialisedItem.LifeTime);
                this.Controls.Static(row, depreciationYearsWhenNew.Index, serialisedItem.DepreciationYears);
                this.Controls.Static(row, purchasePrice.Index, (object)serialisedItem.AssignedPurchasePrice ?? (object)serialisedItem.PurchasePrice ?? string.Empty);
                this.Controls.Static(row, actualRefurbishCost.Index, (object)serialisedItem.ActualRefurbishCost ?? string.Empty);
                this.Controls.Static(row, actualTransportCost.Index, (object)serialisedItem.ActualTransportCost ?? string.Empty);
                this.Controls.Static(row, actualOtherCost.Index, (object)serialisedItem.ActualOtherCost ?? string.Empty);
                this.Controls.Static(row, finalSellingPrice.Index, (object)serialisedItem.SellingPrice ?? string.Empty);
                this.Controls.Static(row, buyerName.Index, serialisedItem.BuyerName);
                this.Controls.Static(row, sellerName.Index, serialisedItem.SellerName);
                this.Controls.Static(row, hsCode.Index, serialisedItem.HsCode);
                this.Controls.Static(row, lengthMM.Index, serialisedItem?.Length);
                this.Controls.Static(row, widthMM.Index,serialisedItem?.Width);
                this.Controls.Static(row, heightMM.Index, serialisedItem?.Height);
                this.Controls.Static(row, weightMM.Index,serialisedItem?.Weight);

                this.Controls.Static(row, invoiceNr.Index, serialisedItem.SalesInvoiceNumber);
                this.Controls.Static(row, customerName.Index, serialisedItem.BillToCustomerName);
                this.Controls.Static(row, customerCountry.Index, serialisedItem.BillToCustomerCountryCode);

                this.Controls.Static(row, fromImport.Index, serialisedItem.FromInitialImport);

                row++;
            }

            this.StartRowNewItems = row;

            for (var i = row; i < row + 100; i++)
            {
                this.Controls.TextBox(i, serialNumber.Index, null, M.SerialisedItem.SerialNumber,
                    factory: (cell) =>
                    {
                        var serialisedItem = this.Session.Create<SerialisedItem>() as SerialisedItem;

                        var chassisNumberType = serialisedItemCharacteristicTypes.First(v => v.UniqueId == Guid.Parse("3417E14D-746E-408A-8697-3B1142CA26CB"));
                        var chassisNumberCharc = this.Session.Create<SerialisedItemCharacteristic>() as SerialisedItemCharacteristic;
                        chassisNumberCharc.SerialisedItemCharacteristicType = chassisNumberType;
                        serialisedItem.AddSerialisedItemCharacteristic(chassisNumberCharc);

                        var engineBrandType = serialisedItemCharacteristicTypes.First(v => v.UniqueId == Guid.Parse("9A90CBD7-3AF8-41E8-ACD8-4D8D62554E18"));
                        var engineBrandCharc = this.Session.Create<SerialisedItemCharacteristic>() as SerialisedItemCharacteristic;
                        engineBrandCharc.SerialisedItemCharacteristicType = engineBrandType;
                        serialisedItem.AddSerialisedItemCharacteristic(engineBrandCharc);

                        var engineModelType = serialisedItemCharacteristicTypes.First(v => v.UniqueId == Guid.Parse("A4DCE977-349F-4D1B-900A-3A7462074482"));
                        var engineModelCharc = this.Session.Create<SerialisedItemCharacteristic>() as SerialisedItemCharacteristic;
                        engineModelCharc.SerialisedItemCharacteristicType = engineModelType;
                        serialisedItem.AddSerialisedItemCharacteristic(engineModelCharc);

                        var engineSerialNumberType = serialisedItemCharacteristicTypes.First(v => v.UniqueId == Guid.Parse("A2F4796B-5D38-478B-977A-2042A4AA5906"));
                        var engineSerialNumberCharc = this.Session.Create<SerialisedItemCharacteristic>() as SerialisedItemCharacteristic;
                        engineSerialNumberCharc.SerialisedItemCharacteristicType = engineSerialNumberType;
                        serialisedItem.AddSerialisedItemCharacteristic(engineSerialNumberCharc);

                        var partSelectCell = this.Controls.Select(
                             cell.Row.Index, product.Index,
                             partOptions,
                             isRequired: true,
                             displayRoleType: M.Part.Name,
                             getRelation: (object key) =>
                             {
                                 return parts.FirstOrDefault(v => string.Equals(v.Name, Convert.ToString(key, CultureInfo.CurrentCulture), StringComparison.OrdinalIgnoreCase));
                             });

                        this.partSelectCellByNewSerialisedItem.Add(serialisedItem, partSelectCell);

                        var ownerSelectCell = this.Controls.Select(
                            cell.Row.Index, ownerName.Index,
                            organisationOptions,
                            serialisedItem,
                            M.SerialisedItem.OwnedBy,
                            M.Organisation.DisplayName,
                            getRelation: (object key) =>
                            {
                                return organisations.FirstOrDefault(v => string.Equals(v.DisplayName, Convert.ToString(key, CultureInfo.CurrentCulture), StringComparison.OrdinalIgnoreCase));
                            });

                        this.ownerSelectCellByNewSerialisedItem.Add(serialisedItem, ownerSelectCell);
                        this.Controls.TextBox(cell.Row.Index, chassisNumber.Index, chassisNumberCharc, M.SerialisedItemCharacteristic.Value);
                        this.Controls.TextBox(cell.Row.Index, fleetCode.Index, serialisedItem, M.SerialisedItem.CustomerReferenceNumber);
                        this.Controls.TextBox(cell.Row.Index, engineBrand.Index, engineBrandCharc, M.SerialisedItemCharacteristic.Value);
                        this.Controls.TextBox(cell.Row.Index, engineModel.Index, engineModelCharc, M.SerialisedItemCharacteristic.Value);
                        this.Controls.TextBox(cell.Row.Index, engineSerialNumber.Index, engineSerialNumberCharc, M.SerialisedItemCharacteristic.Value);

                        this.Controls.Select(
                            cell.Row.Index, ownerStatus.Index,
                            ownershipOptions,
                            serialisedItem,
                            M.SerialisedItem.Ownership,
                            M.Ownership.Name,
                            getRelation: (object key) =>
                            {
                                return ownerships.FirstOrDefault(v => string.Equals(v.Name, Convert.ToString(key, CultureInfo.CurrentCulture), StringComparison.OrdinalIgnoreCase));
                            });

                        this.Controls.Select(
                            cell.Row.Index, availability.Index,
                            availabilityOptions,
                            serialisedItem,
                            M.SerialisedItem.SerialisedItemAvailability,
                            M.SerialisedItemAvailability.Name,
                            getRelation: (object key) =>
                            {
                                return serialisedItemAvailabilities.FirstOrDefault(v => string.Equals(v.Name, Convert.ToString(key, CultureInfo.CurrentCulture), StringComparison.OrdinalIgnoreCase));
                            });

                        this.Controls.Select(
                            cell.Row.Index, availableForSale.Index,
                            yesNoOptions,
                            serialisedItem,
                            M.SerialisedItem.AvailableForSale
                        );

                        this.Controls.Select(
                            cell.Row.Index, state.Index,
                            stateOptions,
                            serialisedItem,
                            M.SerialisedItem.SerialisedItemState,
                            M.SerialisedItemState.Name,
                            getRelation: (object key) =>
                            {
                                return serialisedItemStates.FirstOrDefault(v => string.Equals(v.Name, Convert.ToString(key, CultureInfo.CurrentCulture), StringComparison.OrdinalIgnoreCase));
                            });

                        this.Controls.TextBox(cell.Row.Index, acquisitionDate.Index, serialisedItem, M.SerialisedItem.AcquiredDate);
                        this.Controls.TextBox(cell.Row.Index, acquisitionYear.Index, serialisedItem, M.SerialisedItem.AcquisitionYear);
                        this.Controls.TextBox(cell.Row.Index, manufacturingYear.Index, serialisedItem, M.SerialisedItem.ManufacturingYear);
                        this.Controls.TextBox(cell.Row.Index, estimatedRefurbishCost.Index, serialisedItem, M.SerialisedItem.EstimatedRefurbishCost);
                        this.Controls.TextBox(cell.Row.Index, estimatedTransportCost.Index, serialisedItem, M.SerialisedItem.EstimatedTransportCost);
                        this.Controls.TextBox(cell.Row.Index, expectedSalesPrice.Index, serialisedItem, M.SerialisedItem.ExpectedSalesPrice);
                        this.Controls.TextBox(cell.Row.Index, rentalPriceFullServiceLongTerm.Index, serialisedItem, M.SerialisedItem.ExpectedRentalPriceFullServiceLongTerm);
                        this.Controls.TextBox(cell.Row.Index, rentalPriceFullServiceShortTerm.Index, serialisedItem, M.SerialisedItem.ExpectedRentalPriceFullServiceShortTerm);
                        this.Controls.TextBox(cell.Row.Index, rentalPriceDryLeaseLongTerm.Index, serialisedItem, M.SerialisedItem.ExpectedRentalPriceDryLeaseLongTerm);
                        this.Controls.TextBox(cell.Row.Index, rentalPriceDryLeaseShortTerm.Index, serialisedItem, M.SerialisedItem.ExpectedRentalPriceDryLeaseShortTerm);

                        return serialisedItem;
                    });
            }

            this.NamedRangesControls.Bind();
            this.Controls.Bind();

            await this.NamedRangesSheet.Flush().ConfigureAwait(false);
            await this.Sheet.Flush().ConfigureAwait(false);
        }

        public async Task Update()
        {
            var pulls = new List<Pull>();
            this.AddLookupPulls(pulls);

            var result = await this.Session.PullAsync(pulls.ToArray());

            // TODO:
            //this.Session.Refresh();

            ProcessLookupResults(
             result,
             out var serialisedItemAvailabilities,
             out var serialisedItemStates,
             out var ownerships,
             out var yesNo,
             out var availabilityOptions,
             out var ownershipOptions,
             out var partOptions,
             out var stateOptions,
             out var organisationOptions,
             out var booleanOptions
             );

            this.NamedRangesControls.Bind();
            this.Controls.Bind();

            await this.NamedRangesSheet.Flush().ConfigureAwait(false);
            await this.Sheet.Flush().ConfigureAwait(false);
        }

        public async Task Save()
        {
            var missingParts = new HashSet<string>();
            var missingOwners = new HashSet<string>();

            foreach (var kvp in this.partSelectCellByNewSerialisedItem)
            {
                var newSerialisedItem = kvp.Key;
                var partSelectCell = kvp.Value;
                string partName = (string)partSelectCell.Value;

                var part = this.parts.FirstOrDefault(v =>
                {
                    return v.Name == partName;
                });

                if (part != null)
                {
                    part.AddSerialisedItem(newSerialisedItem);
                }
                else
                {
                    missingParts.Add(partName);
                }
            }

            foreach (var kvp in this.ownerSelectCellByNewSerialisedItem)
            {
                var newSerialisedItem = kvp.Key;
                var ownerSelectCell = kvp.Value;
                string ownerName = (string)ownerSelectCell.Value;

                var owner = this.organisations.FirstOrDefault(v =>
                {
                    return v.DisplayName == ownerName;
                });

                if (owner == null)
                {
                    missingOwners.Add(ownerName);
                }
            }

            if (missingParts.Any())
            {
                this.MessageService.Show("Following products do not exist:\n\n" + string.Join(",", missingParts), "Error");
            }

            if (missingOwners.Any())
            {
                this.MessageService.Show("Following organisations do not exist:\n\n" + string.Join(",", missingOwners), "Error");
            }

            if (!missingParts.Any() && !missingOwners.Any())
            {
                var newItems = this.partSelectCellByNewSerialisedItem.Keys.ToArray();

                this.partSelectCellByNewSerialisedItem = new Dictionary<SerialisedItem, ICell>();

                var response = await this.Session.PushAsync();
                if (response.HasErrors)
                {
                    if (response.AccessErrors?.Any() == true || response.MissingErrors?.Any() == true || response.VersionErrors?.Any() == true)
                    {
                        this.ErrorService.Handle(response, this.Session);
                        this.MessageService.Show("Error was irrecoverable, sheet has been reset", "Info");
                        await this.Init();
                    }
                    else
                    {
                        this.ErrorService.Handle(response, this.Session);
                    }
                }
                else
                {
                    this.MessageService.Show("Successfully saved", "Info");
                    this.Sheet.DeleteRows(this.StartRowNewItems, 100);
                    await this.Init();
                }
            }
        }

        public async Task Refresh()
        {
            if (!this.Session.HasChanges)
            {
                await this.Init();
            }
            else
            {
                switch (this.MessageService.ShowDialog("Do you want to keep your changes", "Info"))
                {
                    case true:
                        await this.Update();
                        break;
                    case false:
                        await this.Init();
                        break;
                }
            }
        }

        private async void Sheet_SheetActivated(object sender, string e)
        {
            if (!this.IsWorksheetUpToDate)
            {
                await this.Refresh().ConfigureAwait(false);

                this.IsWorksheetUpToDate = true;
            }
        }

        private async void Binder_ToDomained(object sender, EventArgs e)
        {
            await this.Sheet.Flush().ConfigureAwait(false);
        }

        private async void Sheet_CellsChanged(object sender, CellChangedEvent e)
        {
            foreach (ICell cell in e.Cells)
            {
                var partCell = this.partSelectCellByNewSerialisedItem.FirstOrDefault(kvp => kvp.Value == cell).Value;
                if (partCell != null)
                {
                    var part = this.parts.FirstOrDefault(v => v.Name == (string)partCell.Value);
                    partCell.Style = part != null ? Constants.ChangedStyle : Constants.ReadOnlyStyle;
                }

                var ownerCell = this.ownerSelectCellByNewSerialisedItem.FirstOrDefault(kvp => kvp.Value == cell).Value;
                if (ownerCell != null)
                {
                    var owner = this.organisations.FirstOrDefault(v => v.DisplayName == (string)ownerCell.Value);
                    ownerCell.Style = owner != null ? Constants.ChangedStyle : Constants.ReadOnlyStyle;
                }
            }

            await this.Sheet.Flush().ConfigureAwait(false);
        }

        private void AddLookupPulls(List<Pull> pulls)
        {
            var updatePulls = new[]
            {
                new Pull
                {
                    Extent = new Filter(M.SerialisedItemAvailability),
                },
                new Pull
                {
                    Extent = new Filter(M.SerialisedItemState),
                },
                new Pull
                {
                    Extent = new Filter(M.Ownership),
                },
                new Pull
                {
                    Extent = new Filter(M.Organisation),
                },
                new Pull
                {
                    Extent = new Filter(M.UnifiedGood)
                    {
                        Predicate = new ContainedIn(M.UnifiedGood.InventoryItemKind)
                        {
                            Extent = new Filter(M.InventoryItemKind)
                            {
                                Predicate = new Equals(M.InventoryItemKind.UniqueId){Value = new Guid("2596e2dd-3f5d-4588-a4a2-167d6fbe3fae")}
                            }
                        }
                    },
                    Results = new[]
                    {
                        new Result
                        {
                            Select = new Select
                            {
                                Include = new []
                                {
                                    new Node(M.UnifiedGood.InventoryItemKind),
                                }
                            }
                        }
                    }
                }
            };

            pulls.AddRange(updatePulls);
        }

        private void ProcessLookupResults(
            IPullResult result,
            out SerialisedItemAvailability[] serialisedItemAvailabilities,
            out SerialisedItemState[] serialisedItemStates,
            out Ownership[] ownerships,
            out string[] yesNo,
            out Range availabilityOptions,
            out Range ownershipOptions,
            out Range partOptions,
            out Range stateOptions,
            out Range organisationOptions,
            out Range yesNoOptions
            )
        {
            serialisedItemAvailabilities = result.GetCollection<SerialisedItemAvailability>("SerialisedItemAvailabilities");
            serialisedItemStates = result.GetCollection<SerialisedItemState>("SerialisedItemStates");
            ownerships = result.GetCollection<Ownership>("Ownerships");
            this.parts = result.GetCollection<UnifiedGood>("UnifiedGoods");
            this.organisations = result.GetCollection<Organisation>("Organisations");

            var availabilityOptionsRange = new Range(1, 1, serialisedItemAvailabilities.Length, 1, this.NamedRangesSheet, name: "availability");
            serialisedItemAvailabilities.OrderBy(v => v.Name).Each((item, index) =>
            {
                if (index < availabilityOptionsRange.Rows)
                {
                    this.NamedRangesControls.Label
                    (
                        availabilityOptionsRange.Row + index, availabilityOptionsRange.Column,
                        item,
                        M.Enumeration.Name
                    );
                }
            });

            availabilityOptions = availabilityOptionsRange;

            var ownershipOptionsRange = new Range(1, 2, ownerships.Length, 1, this.NamedRangesSheet, name: "ownership");
            ownerships.OrderBy(v => v.Name).Each((item, index) =>
            {
                if (index < ownershipOptionsRange.Rows)
                {
                    this.NamedRangesControls.Label
                    (
                        ownershipOptionsRange.Row + index, ownershipOptionsRange.Column,
                        item,
                        M.Enumeration.Name
                    );
                }
            });

            ownershipOptions = ownershipOptionsRange;

            var partOptionsRange = new Range(1, 3, parts.Length, 1, this.NamedRangesSheet, name: "part");
            parts.OrderBy(v => v.Name).Each((item, index) =>
            {
                if (index < partOptionsRange.Rows)
                {
                    this.NamedRangesControls.Label
                    (
                        partOptionsRange.Row + index, partOptionsRange.Column,
                        item,
                        M.Part.Name
                    );
                }
            });

            partOptions = partOptionsRange;

            var stateOptionsRange = new Range(1, 4, serialisedItemStates.Length, 1, this.NamedRangesSheet, name: "state");
            serialisedItemStates.OrderBy(v => v.Name).Each((item, index) =>
            {
                if (index < stateOptionsRange.Rows)
                {
                    this.NamedRangesControls.Label
                    (
                        stateOptionsRange.Row + index, stateOptionsRange.Column,
                        item,
                        M.Enumeration.Name
                    );
                }
            });

            stateOptions = stateOptionsRange;

            var organisationOptionsRange = new Range(1, 5, organisations.Length, 1, this.NamedRangesSheet, name: "organisation");
            organisations.OrderBy(v => v.Name).Each((item, index) =>
            {
                if (index < organisationOptionsRange.Rows)
                {
                    this.NamedRangesControls.Label
                    (
                        organisationOptionsRange.Row + index, organisationOptionsRange.Column,
                        item,
                        M.Organisation.DisplayName
                    );
                }
            });

            organisationOptions = organisationOptionsRange;

            yesNo = new string[] { "Yes", "No" };

            var yesNoOptionsRange = new Range(1, 6, 2, 1, this.NamedRangesSheet, name: "boolean");
            yesNo.Each((item, index) =>
            {
                if (index < yesNoOptionsRange.Rows)
                {
                    this.NamedRangesControls.Static
                    (
                        yesNoOptionsRange.Row + index, yesNoOptionsRange.Column,
                        item
                    );
                }
            });

            yesNoOptions = yesNoOptionsRange;

            this.NamedRangesSheet.Workbook.SetNamedRange(availabilityOptionsRange.Name, availabilityOptionsRange);
            this.NamedRangesSheet.Workbook.SetNamedRange(ownershipOptionsRange.Name, ownershipOptionsRange);
            this.NamedRangesSheet.Workbook.SetNamedRange(partOptionsRange.Name, partOptionsRange);
            this.NamedRangesSheet.Workbook.SetNamedRange(stateOptionsRange.Name, stateOptionsRange);
            this.NamedRangesSheet.Workbook.SetNamedRange(organisationOptionsRange.Name, organisationOptionsRange);
            this.NamedRangesSheet.Workbook.SetNamedRange(yesNoOptionsRange.Name, yesNoOptionsRange);
        }
    }
}
