// --------------------------------------------------------------------------------------------------------------------
// <copyright file="OrderItemModel.cs" company="Allors bvba">
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

using Markdig;
using System.Collections.Generic;
using System.Globalization;
using System.Text.RegularExpressions;

namespace Allors.Database.Domain.Print.StatementOfWorkModel
{
    public class QuoteItemModel
    {
        public QuoteItemModel(QuoteItem item, Dictionary<string, byte[]> imageByImageName)
        {
            var transaction = item.Strategy.Transaction;
            var product = item.Product;
            var serialisedItem = item.SerialisedItem;

            this.Reference = item.InvoiceItemType?.Name;
            this.Quantity = item.Quantity.ToString("0");
            this.Comment = item.Comment?.Split('\n');

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

            // TODO: Where does the currency come from?
            var currency = "€";
            this.Price = item.UnitPrice.ToString("N2", new CultureInfo("nl-BE")) + " " + currency;
            this.UnitAmount = item.UnitPrice.ToString("N2", new CultureInfo("nl-BE")) + " " + currency;
            this.TotalAmount = item.TotalExVat.ToString("N2", new CultureInfo("nl-BE")) + " " + currency;
            this.Comment = item.Comment?.Split('\n');
        }

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
