// --------------------------------------------------------------------------------------------------------------------
// <copyright file="Model.cs" company="Allors bvba">
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

namespace Allors.Database.Domain.Print.PartModel
{
    public class Model
    {
        public Model(Part part, string barcodeDescription, string barcodeLocation)
        {
            this.BarcodeDescription = barcodeDescription.Substring(0, barcodeDescription.LastIndexOf(",")); // remove productnumber
            this.PartLocation = barcodeLocation;
            this.Name = part.Name;
            this.Barcode = BarcodeName(part);
        }

        public string BarcodeDescription { get; }

        public string PartLocation { get; }

        public string Name { get; }

        public static string BarcodeName(Part part) => $"Barcode{part.PartIdentification()}";

        public string Barcode { get; }
    }
}
