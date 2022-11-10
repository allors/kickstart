// --------------------------------------------------------------------------------------------------------------------
// <copyright file="NonUnifiedPartBarcodePrint.cs" company="Allors bvba">
//   Copyright 2002-2012 Allors bvba.
// Dual Licensed under
//   a) the General Public Licence v3 (GPL)
//   b) the Allors License
// The GPL License is included in the file gpl.txt.
// The Allors License is an addendum to your contract.
// Allors Applications is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
// For more information visit http://www.allors.com/legal
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using System.Collections.Generic;
using System.Linq;
using Allors.Database.Meta;
using Microsoft.Extensions.DependencyInjection;

namespace Allors.Database.Domain
{
    using System;

    public partial class NonUnifiedPartBarcodePrint
    {
        public void BasePrint(PrintablePrint method)
        {
            var singleton = this.Strategy.Transaction.GetSingleton();
            var dutchLocale = new Locales(this.Transaction()).DutchNetherlands;
            var spanishLocale = new Locales(this.Transaction()).Spanish;

            if (this.ExistLocale)
            {
                Dictionary<string, NonSerialisedInventoryItem> partByName;
                if (this.Locale.Equals(spanishLocale))
                {
                    partByName = new NonSerialisedInventoryItems(this.Transaction()).Extent()
                        .Where(v => v.Facility.Equals(this.Facility) && this.Parts.Contains(v.Part))
                        .Select(v => v)
                        .ToDictionary(v =>
                        {
                            var part = (NonUnifiedPart)v.Part;
                            return $"{part?.SpanishName}, {part?.SpanishUOM}, {part?.ProductNumber}";
                        });
                }
                else if (this.Locale.Equals(dutchLocale))
                {
                    partByName = new NonSerialisedInventoryItems(this.Transaction()).Extent()
                        .Where(v => v.Facility.Equals(this.Facility) && this.Parts.Contains(v.Part))
                        .Select(v => v)
                        .ToDictionary(v =>
                        {
                            var part = (NonUnifiedPart)v.Part;
                            return $"{part?.DutchName}, {part?.DutchUOM}, {part?.ProductNumber}";
                        });
                }
                else
                {
                    partByName = new NonSerialisedInventoryItems(this.Transaction()).Extent()
                        .Where(v => v.Facility.Equals(this.Facility) && this.Parts.Contains(v.Part))
                        .Select(v => v)
                        .ToDictionary(v =>
                        {
                            var part = (NonUnifiedPart)v.Part;
                            return $"{part?.Name}, {part?.UnitOfMeasure.Name}, {part?.ProductNumber}";
                        });
                }

                var partByNameSorted = new SortedDictionary<string, NonSerialisedInventoryItem>(partByName);

                var images = new Dictionary<string, byte[]>();
                foreach (var keyValuePair in partByNameSorted)
                {
                    var barcodeGenerator = this.Strategy.Transaction.Database.Services.Get<IBarcodeGenerator>();
                    var barcode = barcodeGenerator.Generate(keyValuePair.Value.Part.PartIdentification(), BarcodeType.CODE_128, 320, 80);
                    var barcodeName = Allors.Database.Domain.Print.PartModel.Model.BarcodeName(keyValuePair.Value.Part);
                    if (!images.ContainsKey(barcodeName))
                    {
                        images.Add(barcodeName, barcode);
                    }
                }

                var printModel = new Print.NonUnifiedPartBarcodePrint.Model(partByNameSorted);
                this.RenderPrintDocument(singleton.NonUnifiedPartBarcodePrintTemplate, printModel, images);

                this.PrintDocument.Media.InFileName = $"SparePartsBarcode_{(int)this.strategy.ObjectVersion}.odt";

            }
        }

        // TODO: Martien (was BaseOnDerive)
        public void BaseOnPostDerive(ObjectOnPostDerive method)
        {
            this.ResetPrintDocument();
            this.Print();
        }
    }
}