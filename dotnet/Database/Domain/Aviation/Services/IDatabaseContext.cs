using Bogus;
using SkiaSharp;

namespace Allors.Database.Domain
{
    public partial interface IDatabaseContext
    {
        SKBitmap Watermark { get; set; }

        Faker Faker { get; set; }
    }
}