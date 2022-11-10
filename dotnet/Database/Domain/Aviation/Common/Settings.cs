using System.Text.RegularExpressions;

namespace Allors.Database.Domain
{
    public partial class Settings
    {
        public static Regex ParameterExtractionRegex = new Regex(@"\[\s*(\d+)\s*]");


        public Expression CleaningExpression => this.ExistCleaningCalculation ? new Expression(this.CleaningCalculation) : null;

        public Expression SundriesExpression => this.ExistSundriesCalculation ? new Expression(this.SundriesCalculation) : null;
    }
}
