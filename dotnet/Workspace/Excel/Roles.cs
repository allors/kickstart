using System;

namespace Application
{
    public class Roles
    {
        public static readonly Guid AdministratorsId = new Guid("CDC04209-683B-429C-BED2-440851F430DF");

        public bool IsAdministrator { get; set; }
    }
}