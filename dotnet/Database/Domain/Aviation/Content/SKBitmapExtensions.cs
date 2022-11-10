// --------------------------------------------------------------------------------------------------------------------
// <copyright file="MediaContents.cs" company="Allors bvba">
//   Copyright 2002-2016 Allors bvba.
//
// Dual Licensed under
//   a) the General Public Licence v3 (GPL)
//   b) the Allors License
//
// The GPL License is included in the file gpl.txt.
// The Allors License is an addendum to your contract.
//
// Allors Applications is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
// GNU General Public License for more details.
//
// For more information visit http://www.allors.com/legal
// </copyright>
// --------------------------------------------------------------------------------------------------------------------

using SkiaSharp;

namespace Allors.Database.Domain
{
    public static class SKBitmapExtensions
    {
        private const float RATIO = 4f / 3;
        private const int WIDTH = 800;

        public static SKBitmap Process(this SKBitmap @this, SKBitmap watermark)
        {
            var scaled = @this.Scale(out var width, out var height);

            using var canvas = new SKCanvas(scaled);
            var xScaleWatermark = (float)width / watermark.Width;
            var yScaleWatermark = (float)height / watermark.Height;
            var scaleWatermark = SKMatrix.MakeScale(xScaleWatermark, yScaleWatermark);
            canvas.SetMatrix(scaleWatermark);
            canvas.DrawBitmap(watermark, 0, 0);

            return scaled;
        }

        public static SKBitmap Scale(this SKBitmap @this, out int width, out int height)
        {
            width = @this.Width < WIDTH ? @this.Width : WIDTH;
            height = (int)(width / RATIO);

            var scaled = new SKBitmap(width, height, @this.ColorType, @this.AlphaType);
            using var canvas = new SKCanvas(scaled);

            var xScaleOriginal = (float)width / @this.Width;
            var yScaleOriginal = (float)height / @this.Height;
            var scaleOriginal = SKMatrix.MakeScale(xScaleOriginal, yScaleOriginal);
            canvas.SetMatrix(scaleOriginal);
            canvas.DrawBitmap(@this, 0, 0);

            return scaled;
        }
    }
}