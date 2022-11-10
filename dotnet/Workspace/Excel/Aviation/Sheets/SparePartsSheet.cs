using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Allors.Excel;
using Allors.Workspace;
using Allors.Workspace.Data;
using Task = System.Threading.Tasks.Task;
using System.Text;
using Allors.Workspace.Meta;
using DateTime = System.DateTime;
using NonSerialisedInventoryItem = Allors.Workspace.Domain.NonSerialisedInventoryItem;
using NonUnifiedPart = Allors.Workspace.Domain.NonUnifiedPart;
using Part = Allors.Workspace.Domain.Part;
using PartCategory = Allors.Workspace.Domain.PartCategory;
using SupplierOffering = Allors.Workspace.Domain.SupplierOffering;

namespace Application.Sheets
{
    public class SparePartsSheet : ISheet, ISaveable
    {
        public SparePartsSheet(Program program)
        {
            this.Sheet = program.ActiveWorkbook.AddWorksheet();

            this.M = program.M;
            this.MessageService = program.Workspace.Services.Get<IMessageService>();
            this.ErrorService = program.Workspace.Services.Get<IErrorService>();

            this.Session = program.Workspace.CreateSession();
        }

        public M M { get; set; }

        public ISession Session { get; }

        public IWorksheet Sheet { get; }

        public IMessageService MessageService { get; set; }

        public IErrorService ErrorService { get; }

        public async Task Load()
        {
            var pulls = new Pull[]
            {
                new Pull()
                {
                    Extent = new Filter(M.NonUnifiedPart)
                    {
                        Predicate = new ContainedIn(M.NonUnifiedPart.InventoryItemKind)
                        {
                            Extent = new Filter(M.InventoryItemKind)
                            {
                                Predicate = new Equals(M.InventoryItemKind.UniqueId){Value = new Guid("eaa6c331-0dd9-4bb1-8245-12a673304468")}
                            }
                        }
                    },

                    Results = new[]
                    {
                        new Result()
                        {
                            Select = new Select
                            {
                                Include =  new []
                                {
                                    new Node(M.NonUnifiedPart.PartWeightedAverage),
                                    new Node(M.NonUnifiedPart.CurrentSupplierOfferings, new []
                                    {
                                        new Node(M.SupplierOffering.Supplier, new []
                                        {
                                            new Node(M.Party.AsOrganisation.InternalOrganisationsWhereActiveSupplier),
                                        }),
                                    }),
                                }
                            }
                        }
                    }
                },
                new Pull
                {
                    Extent = new Filter(M.NonSerialisedInventoryItem),

                    Results = new[]
                    {
                        new Result
                        {
                            Select = new Select
                            {
                                Include = new []
                                {
                                    new Node(M.NonSerialisedInventoryItem.Part),
                                    new Node(M.NonSerialisedInventoryItem.Facility, new []
                                    {
                                        new Node(M.Facility.Owner),
                                    }),
                                }
                            }
                        }
                    }
                },
            };

            var result = await this.Session.PullAsync(pulls);
            this.Session.Reset();

            var nonUnifiedParts = result.GetCollection<NonUnifiedPart>("NonUnifiedParts") ?? Array.Empty<NonUnifiedPart>();
            var nonSerialisedInventoryItems = result.GetCollection<NonSerialisedInventoryItem>("NonSerialisedInventoryItems") ?? Array.Empty<NonSerialisedInventoryItem>();

            var inventoryItemByPartByInternalOrganisation = new Dictionary<string, Dictionary<Part, ISet<NonSerialisedInventoryItem>>>();
            foreach(NonSerialisedInventoryItem inventoryItem in nonSerialisedInventoryItems)
            {           
                var internalOrganisation = inventoryItem.Facility.Owner.ExternalPrimaryKey;

                if (!inventoryItemByPartByInternalOrganisation.TryGetValue(internalOrganisation, out var partsByInternalOrganisation))
                {
                    partsByInternalOrganisation = new Dictionary<Part, ISet<NonSerialisedInventoryItem>>();
                    inventoryItemByPartByInternalOrganisation.Add(internalOrganisation, partsByInternalOrganisation);
                }

                if (!partsByInternalOrganisation.TryGetValue(inventoryItem.Part, out var inventoryItems))
                {
                    inventoryItems = new HashSet<NonSerialisedInventoryItem>();
                    partsByInternalOrganisation.Add(inventoryItem.Part, inventoryItems);
                }

                inventoryItems.Add(inventoryItem);
            }

            var supplierOfferingsByPartByInternalOrganisation = new Dictionary<string, Dictionary<Part, ISet<SupplierOffering>>>();
            foreach(NonUnifiedPart sparePart in nonUnifiedParts)
            {
                foreach (SupplierOffering supplierOffering in sparePart.CurrentSupplierOfferings)
                {
                    foreach (string internalOrganisation in supplierOffering.Supplier.InternalOrganisationsWhereActiveSupplier.Select(v => v.ExternalPrimaryKey).ToArray())
                    {
                        if (!supplierOfferingsByPartByInternalOrganisation.TryGetValue(internalOrganisation, out var partsByAviaco))
                        {
                            partsByAviaco = new Dictionary<Part, ISet<SupplierOffering>>();
                            supplierOfferingsByPartByInternalOrganisation.Add(internalOrganisation, partsByAviaco);
                        }

                        if (!partsByAviaco.TryGetValue(supplierOffering.Part, out var offerings))
                        {
                            offerings = new HashSet<SupplierOffering>();
                            partsByAviaco.Add(supplierOffering.Part, offerings);
                        }

                        offerings.Add(supplierOffering);
                    }
                }
            }

            var acme = "acme";

            var index = 0;
            var productId = new Column { Header = "Product Id", NumberFormat = "@", Index = index++ };
            var name = new Column { Header = "Name", NumberFormat = "@", Index = index++ };
            var nameNL = new Column { Header = "Dutch Name", NumberFormat = "@", Index = index++ };
            var brand = new Column { Header = "Brand", NumberFormat = "@", Index = index++ };
            var model = new Column { Header = "Model", NumberFormat = "@", Index = index++ };
            var manufacturer = new Column { Header = "Manufacturer", NumberFormat = "@", Index = index++ };
            var inventoryKind = new Column { Header = "Inventory kind", NumberFormat = "@", Index = index++ };
            var categories = new Column { Header = "Categories", NumberFormat = "@", Index = index++ };
            var suppliers = new Column { Header = "Suppliers Aviaco AM", NumberFormat = "@", Index = index++ };
            var quantityOnHand = new Column { Header = "Total Quantity On Hand", NumberFormat = "#.###,00;#.###,00;;@", Index = index++ };
            var uom = new Column { Header = "UOM", NumberFormat = "#.###,00;#.###,00;;@", Index = index++ };
            var weightedAverageCost = new Column { Header = "Weighted Average Cost", NumberFormat = "#.###,00;#.###,00;;@", Index = index++ };
            var totalCost = new Column { Header = "Total Cost", NumberFormat = "#.###,00;#.###,00;;@", Index = index++ };
            var expectedIn = new Column { Header = "Expected in", NumberFormat = "#.###,00;#.###,00;;@", Index = index++ };
            var isSundries = new Column { Header = "Is Sundries", NumberFormat = "@", Index = index++ };

            var columns = new[] {
                productId,
                name,
                nameNL,
                brand,
                model,
                manufacturer,
                inventoryKind,
                categories,
                suppliers,
                quantityOnHand,
                uom,
                weightedAverageCost,
                totalCost,
                expectedIn,
                isSundries
            };

            foreach (var item in columns)
            {
                this.Sheet[0, item.Index].Value = item.Header;
                this.Sheet[0, item.Index].Style = new Style(Color.LightBlue, Color.Black);
            }

            var row = 1;

            // list nonserialised item only
            foreach (var sparePart in nonUnifiedParts)
            {
                foreach (var column in columns)
                {
                    this.Sheet[row, column.Index].NumberFormat = column.NumberFormat;
                }

                this.Sheet[row, productId.Index].Value = sparePart.ProductNumber;
                this.Sheet[row, name.Index].Value = sparePart.Name;
                this.Sheet[row, nameNL.Index].Value = sparePart.DutchName;
                this.Sheet[row, brand.Index].Value = sparePart.BrandName;
                this.Sheet[row, model.Index].Value = sparePart.ModelName;
                this.Sheet[row, manufacturer.Index].Value = sparePart.ManufacturedByDisplayName;
                this.Sheet[row, inventoryKind.Index].Value = sparePart.InventoryItemKindName;
                this.Sheet[row, categories.Index].Value = sparePart.PartCategoriesDisplayName;

                if (supplierOfferingsByPartByInternalOrganisation.ContainsKey(acme) && supplierOfferingsByPartByInternalOrganisation[acme].ContainsKey(sparePart))
                {
                    this.Sheet[row, suppliers.Index].Value = this.BuildString(supplierOfferingsByPartByInternalOrganisation[acme][sparePart]);
                }

                this.Sheet[row, quantityOnHand.Index].Value = sparePart.QuantityOnHand;
                this.Sheet[row, uom.Index].Value = sparePart.UnitOfMeasureAbbreviation;
                this.Sheet[row, weightedAverageCost.Index].Value = decimal.Round(sparePart.PartWeightedAverage.AverageCost, 2);
                this.Sheet[row, totalCost.Index].Value = decimal.Round(sparePart.QuantityOnHand * sparePart.PartWeightedAverage.AverageCost, 2);
                this.Sheet[row, expectedIn.Index].Value = sparePart.QuantityExpectedIn;
                this.Sheet[row, isSundries.Index].Value = sparePart.IsSundries;

                ++row;
            }

            await this.Sheet.Flush();
        }

        public async Task Save()
        {
            var response = await this.Session.PushAsync();
            if (response.HasErrors)
            {
                this.ErrorService.Handle(response, this.Session);
            }
            else
            {
                this.MessageService.Show("Successfully saved", "Info");
            }

            await this.Load();
        }

        public async Task Refresh()
        {
            await this.Load();
        }

        private string BuildString(ISet<SupplierOffering> supplierOfferings)
        {
            var builder = new StringBuilder();
            foreach (SupplierOffering supplierOffering in supplierOfferings)
            {
                if (builder.Length > 0)
                {
                    builder.Append(Environment.NewLine);
                }

                builder.Append($"{supplierOffering.Supplier.DisplayName}, ref.: {supplierOffering.SupplierProductId}, price: {decimal.Round(supplierOffering.Price, 2)}");
            }

            return builder.ToString();
        }
    }
}
