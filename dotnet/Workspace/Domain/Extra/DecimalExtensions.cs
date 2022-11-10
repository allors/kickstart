using System;

namespace Allors.Extra
{
    public static class DecimalExtensions
    {
        public static decimal? Round(this decimal? @this, int decimals)
        {
            if (@this != null)
            {
                return decimal.Round(@this.Value, decimals, MidpointRounding.AwayFromZero);
            }

            return null;
        }

        public static decimal? Round(this double? @this, int decimals)
        {
            if (@this != null)
            {
                var thisDecimal = Convert.ToDecimal(@this);
                return decimal.Round(thisDecimal, decimals, MidpointRounding.AwayFromZero);
            }

            return null;
        }

        public static decimal RoundValue(this decimal @this, int decimals)
        {
            return decimal.Round(@this, decimals, MidpointRounding.AwayFromZero);
        }

        public static decimal ToDecimal(this decimal? @this, decimal defaultValue)
        {
            if (@this == null)
            {
                return defaultValue;
            }

            return (decimal)@this;
        }
    }
}
