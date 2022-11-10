using System.Linq;
using Application;
using Allors.Excel.Headless;

namespace Tests.Domain
{
    using Allors;
    using Xunit;

    public class CustomerSheetTest : ExcelTest
    {
        [Fact]
        public async void Load()
        {
            var addIn = new AddIn();
            var workbook = addIn.AddWorkbook();

            await this.Program.OnStart(addIn);
            await this.Program.OnHandle(Actions.Customers);

            var worksheet = workbook.Worksheets.FirstOrDefault(v=>v.IsActive);

            Assert.Equal("Company Name", worksheet[0, 0].Value);

            Assert.Equal("AVIACO ASSET MANAGEMENT B.V.", worksheet[1, 0].Value);
        }
    }
}
