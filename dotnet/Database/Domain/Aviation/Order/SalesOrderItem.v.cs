namespace Allors.Database.Domain
{
    public partial class SalesOrderItem
    {
        private bool IsSubTotalItem => this.AviationIsSubTotalItem;
    }
}