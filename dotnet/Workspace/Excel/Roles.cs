using System;

namespace Application
{
    public class Roles
    {
        public static readonly Guid AdministratorsId = new Guid("CDC04209-683B-429C-BED2-440851F430DF");

        public static readonly Guid SalesAccountManagersId = new Guid("449EA7CE-124B-4E19-AFDF-46CAFB8D7B20");


        public bool IsAdministrator { get; set; }

        public bool IsSalesAccountManager { get; set; }

        public bool IsLocalAdministrator { get; set; }
    }
}