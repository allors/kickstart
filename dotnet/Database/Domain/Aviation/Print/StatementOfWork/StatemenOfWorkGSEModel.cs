// --------------------------------------------------------------------------------------------------------------------
// <copyright file="QuoteItemGSEModel.cs" company="Allors bvba">
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
using Markdig;

namespace Allors.Database.Domain.Print.StatementOfWorkModel
{
    public class StatemenOfWorkGSEModel
    {
        public StatemenOfWorkGSEModel(SerialisedItem gse)
        {
            var transaction = gse.Strategy.Transaction;
            var m = gse.Strategy.Transaction.Database.Services.Get<MetaPopulation>();

            var description = gse.Description;
            if (description != null)
            {
                description = Markdown.ToPlainText(description);
            }

            this.Description = description?.Split('\n');

            this.Comment = gse.Comment?.Split('\n');

            this.BrandName = gse.PartWhereSerialisedItem?.Brand?.Name;
            this.ModelName = gse.PartWhereSerialisedItem?.Model?.Name;

            this.IdentificationNumber = gse.ItemNumber;
            this.Year = gse.ManufacturingYear.ToString();

            var hoursType = new SerialisedItemCharacteristicTypes(transaction).FindBy(m.SerialisedItemCharacteristicType.Name, "Operating Hours");
            var hoursCharacteristic = gse.SerialisedItemCharacteristics.FirstOrDefault(v => v.SerialisedItemCharacteristicType.Equals(hoursType));
            this.Hours = $"{hoursCharacteristic?.Value} {hoursType?.UnitOfMeasure?.Abbreviation}";

            var lengthType = new SerialisedItemCharacteristicTypes(transaction).FindBy(m.SerialisedItemCharacteristicType.Name, "Length");
            var lengthCharacteristic = gse.SerialisedItemCharacteristics.FirstOrDefault(v => v.SerialisedItemCharacteristicType.Equals(lengthType));
            var widthType = new SerialisedItemCharacteristicTypes(transaction).FindBy(m.SerialisedItemCharacteristicType.Name, "Width");
            var widthCharacteristic = gse.SerialisedItemCharacteristics.FirstOrDefault(v => v.SerialisedItemCharacteristicType.Equals(widthType));
            var heightType = new SerialisedItemCharacteristicTypes(transaction).FindBy(m.SerialisedItemCharacteristicType.Name, "Height");
            var heightCharacteristic = gse.SerialisedItemCharacteristics.FirstOrDefault(v => v.SerialisedItemCharacteristicType.Equals(heightType));
            this.Dimensions = $"{lengthCharacteristic?.Value} {lengthType.UnitOfMeasure?.Abbreviation} * {widthCharacteristic?.Value} {widthType.UnitOfMeasure?.Abbreviation} * {heightCharacteristic?.Value} {heightType.UnitOfMeasure?.Abbreviation}";

            var weightType = new SerialisedItemCharacteristicTypes(transaction).FindBy(m.SerialisedItemCharacteristicType.Name, "Weight");
            var weightCharacteristic = gse.SerialisedItemCharacteristics.FirstOrDefault(v => v.SerialisedItemCharacteristicType.Equals(weightType));
            this.Weight = $"{weightCharacteristic?.Value} {weightType?.UnitOfMeasure?.Abbreviation}";
        }

        public string[] Description { get; }

        public string[] Comment { get; }

        public string IdentificationNumber { get; }

        public string BrandName { get; }

        public string ModelName { get; }

        public string Year { get; }

        public string Hours { get; }

        public string Dimensions { get; }

        public string Weight { get; }
    }
}
