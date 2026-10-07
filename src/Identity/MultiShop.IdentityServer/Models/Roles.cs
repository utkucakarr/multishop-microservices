namespace MultiShop.IdentityServer.Models
{
    public static class Roles
    {
        public const string Admin = "Admin";
        public const string Customer = "Customer";

        public static readonly string[] All = { Admin, Customer };
    }
}
