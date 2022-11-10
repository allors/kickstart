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
using System.Text.RegularExpressions;
using System.Globalization;

namespace Allors.Database.Domain.Print.ProductQuoteModel
{
    public class QuoteItemGSEModel
    {
        public QuoteItemGSEModel(QuoteItem item, Dictionary<string, byte[]> imageByImageName)
        {
            var transaction = item.Strategy.Transaction;
            var m = item.Strategy.Transaction.Database.Services.Get<MetaPopulation>();

            var product = item.Product;
            var serialisedItem = item.SerialisedItem;

            this.Product = $"{serialisedItem?.ItemNumber} {product?.Name}";
            this.IsRental = item.ExistRentalType;
            this.IsSale = !item.ExistRentalType;
            this.RentalType = $"{item.RentalType?.Name} (monthly)";
            this.Reference = this.IsRental ? "Rental" : item.InvoiceItemType?.Name;

            var description = serialisedItem?.Description ?? product?.Description;
            if (description != null)
            {
                description = Markdown.ToPlainText(description);
            }

            this.Description = description?.Split('\n');

            if (item.ExistDetails && item.Details.Contains("Operating Hours"))
            {
                Regex Pattern = new Regex(@"\w+\s\b\w+\:\s\w+\b\shr,{1}\s", RegexOptions.Compiled);
                this.Details = Pattern.Replace(item.Details, "");
            }
            else
            {
                this.Details = item.Details;
            }

            this.Quantity = item.Quantity.ToString("0");
            // TODO: Where does the currency come from?
            var currency = "€";
            this.Price = item.UnitPrice.ToString("N2", new CultureInfo("nl-BE")) + " " + currency;

            this.UnitAmount = item.UnitPrice.ToString("N2", new CultureInfo("nl-BE")) + " " + currency;
            this.TotalAmount = item.TotalExVat.ToString("N2", new CultureInfo("nl-BE")) + " " + currency;

            this.Comment = item.Comment?.Split('\n');

            if (product != null)
            {
                this.ProductCategory = string.Join(", ", product.ProductCategoriesWhereProduct.Select(v => v.Name));
            }

            var unifiedGood = product as UnifiedGood;
            var nonUnifiedGood = product as NonUnifiedGood;

            if (unifiedGood != null)
            {
                this.BrandName = unifiedGood.Brand?.Name;
                this.ModelName = unifiedGood.Model?.Name;
            }
            else if (nonUnifiedGood != null)
            {
                this.BrandName = nonUnifiedGood.Part?.Brand?.Name;
                this.ModelName = nonUnifiedGood.Part?.Model?.Name;
            }

            if (serialisedItem != null)
            {
                this.IdentificationNumber = serialisedItem.ItemNumber;
                this.Year = serialisedItem.ManufacturingYear.ToString();

                var hoursType = new SerialisedItemCharacteristicTypes(transaction).FindBy(m.SerialisedItemCharacteristicType.Name, "Operating Hours");
                var hoursCharacteristic = serialisedItem.SerialisedItemCharacteristics.FirstOrDefault(v => v.SerialisedItemCharacteristicType.Equals(hoursType));
                this.Hours = $"{hoursCharacteristic?.Value} {hoursType?.UnitOfMeasure?.Abbreviation}";

                var lengthType = new SerialisedItemCharacteristicTypes(transaction).FindBy(m.SerialisedItemCharacteristicType.Name, "Length");
                var lengthCharacteristic = serialisedItem.SerialisedItemCharacteristics.FirstOrDefault(v => v.SerialisedItemCharacteristicType.Equals(lengthType));
                var widthType = new SerialisedItemCharacteristicTypes(transaction).FindBy(m.SerialisedItemCharacteristicType.Name, "Width");
                var widthCharacteristic = serialisedItem.SerialisedItemCharacteristics.FirstOrDefault(v => v.SerialisedItemCharacteristicType.Equals(widthType));
                var heightType = new SerialisedItemCharacteristicTypes(transaction).FindBy(m.SerialisedItemCharacteristicType.Name, "Height");
                var heightCharacteristic = serialisedItem.SerialisedItemCharacteristics.FirstOrDefault(v => v.SerialisedItemCharacteristicType.Equals(heightType));
                this.Dimensions = $"{lengthCharacteristic?.Value} {lengthType.UnitOfMeasure?.Abbreviation} * {widthCharacteristic?.Value} {widthType.UnitOfMeasure?.Abbreviation} * {heightCharacteristic?.Value} {heightType.UnitOfMeasure?.Abbreviation}";

                var weightType = new SerialisedItemCharacteristicTypes(transaction).FindBy(m.SerialisedItemCharacteristicType.Name, "Weight");
                var weightCharacteristic = serialisedItem.SerialisedItemCharacteristics.FirstOrDefault(v => v.SerialisedItemCharacteristicType.Equals(weightType));
                this.Weight = $"{weightCharacteristic?.Value} {weightType?.UnitOfMeasure?.Abbreviation}";

                if (serialisedItem.ExistPrimaryPhoto)
                {
                    this.PrimaryPhotoName = $"{item.Id}_primaryPhoto";
                    imageByImageName.Add(this.PrimaryPhotoName, serialisedItem.PrimaryPhoto.MediaContent.Data);
                }

                if (serialisedItem.SecondaryPhotos.Any())
                {
                    this.SecondaryPhotoName1 = $"{item.Id}_secondaryPhoto1";
                    imageByImageName.Add(this.SecondaryPhotoName1, serialisedItem.SecondaryPhotos.First().MediaContent.Data);
                }

                if (serialisedItem.SecondaryPhotos.Count() > 1)
                {
                    this.SecondaryPhotoName2 = $"{item.Id}_secondaryPhoto2";
                    imageByImageName.Add(this.SecondaryPhotoName2, serialisedItem.SecondaryPhotos.Skip(1).First().MediaContent.Data);
                }
            }
            else if (product != null)
            {
                this.IdentificationNumber = product.ProductIdentifications.FirstOrDefault(v => v.ProductIdentificationType.Equals(new ProductIdentificationTypes(transaction).Good)).Identification;

                var lengthType = new SerialisedItemCharacteristicTypes(transaction).FindBy(m.SerialisedItemCharacteristicType.Name, "Length");
                var lengthCharacteristic = unifiedGood?.SerialisedItemCharacteristics.FirstOrDefault(v => v.SerialisedItemCharacteristicType.Equals(lengthType));
                var widthType = new SerialisedItemCharacteristicTypes(transaction).FindBy(m.SerialisedItemCharacteristicType.Name, "Width");
                var widthCharacteristic = unifiedGood?.SerialisedItemCharacteristics.FirstOrDefault(v => v.SerialisedItemCharacteristicType.Equals(widthType));
                var heightType = new SerialisedItemCharacteristicTypes(transaction).FindBy(m.SerialisedItemCharacteristicType.Name, "Height");
                var heightCharacteristic = unifiedGood?.SerialisedItemCharacteristics.FirstOrDefault(v => v.SerialisedItemCharacteristicType.Equals(heightType));
                this.Dimensions = $"{lengthCharacteristic?.Value} {lengthType.UnitOfMeasure?.Abbreviation} * {widthCharacteristic?.Value} {widthType.UnitOfMeasure?.Abbreviation} * {heightCharacteristic?.Value} {heightType.UnitOfMeasure?.Abbreviation}";

                var weightType = new SerialisedItemCharacteristicTypes(transaction).FindBy(m.SerialisedItemCharacteristicType.Name, "Weight");
                var weightCharacteristic = unifiedGood?.SerialisedItemCharacteristics.FirstOrDefault(v => v.SerialisedItemCharacteristicType.Equals(weightType));
                this.Weight = $"{weightCharacteristic?.Value} {weightType?.UnitOfMeasure?.Abbreviation}";

                if (product.ExistPrimaryPhoto)
                {
                    this.PrimaryPhotoName = $"{item.Id}_primaryPhoto";
                    imageByImageName.Add(this.PrimaryPhotoName, product.PrimaryPhoto.MediaContent.Data);
                }

                if (product.Photos.Any())
                {
                    this.SecondaryPhotoName1 = $"{item.Id}_secondaryPhoto1";
                    imageByImageName.Add(this.SecondaryPhotoName1, product.Photos.First().MediaContent.Data);
                }

                if (product.Photos.Count() > 1)
                {
                    this.SecondaryPhotoName2 = $"{item.Id}_secondaryPhoto2";
                    imageByImageName.Add(this.SecondaryPhotoName2, product.Photos.Skip(1).First().MediaContent.Data);
                }
            }
        }

        public bool IsRental { get; set; }

        public bool IsSale { get; set; }

        public string RentalType { get; set; }

        public string PrimaryPhotoName { get; set; }

        public string SecondaryPhotoName1 { get; set; }

        public string SecondaryPhotoName2 { get; set; }

        public string Reference { get; }

        public string Product { get; }

        public string[] Description { get; }

        public string Details { get; }

        public string Quantity { get; }

        public string Price { get; }

        public string UnitAmount { get; }

        public string TotalAmount { get; }

        public string[] Comment { get; }

        public string IdentificationNumber { get; }

        public string ProductCategory { get; }

        public string BrandName { get; }

        public string ModelName { get; }

        public string Year { get; }

        public string Hours { get; }

        public string Dimensions { get; }

        public string Weight { get; }
    }
}
