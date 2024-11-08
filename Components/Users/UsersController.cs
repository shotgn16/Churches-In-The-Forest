using ForestChurches.Components;
using ForestChurches.Components.Roles;
using ForestChurches.Data;
using ForestChurches.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Net;
using System.Security.Claims;

namespace ForestChurches.Components.Users
{
    public class UsersController : Controller, UserInterface
    {
        private ForestChurchesContext _context;
        private Configuration.Configuration _configuration;

        public UsersController(ForestChurchesContext context, Configuration.Configuration configuration)
        {
            _context = context;
            _configuration = configuration;
        }

        public async Task SeedChurchUserAsync(UserManager<ChurchAccount> userManager, RoleManager<ChurchRoles> roleManager)
        {
            // Create the 'Demo' User
            var defaultUser = new ChurchAccount
            {
                Email = _configuration.Client.GetSecret("sys-demo-username").Value.Value,
                UserName = _configuration.Client.GetSecret("sys-demo-username").Value.Value,
                ChurchName = "Demo Church",
                ChurchDenomination = "Denomination",
                ChurchWebsite = "https://www.demochurch.org",
                EmailConfirmed = true,
                ImageArray = await ConvertLinkToByteArray("https://i.imgur.com/oLC9RcU.png"),
                Role = Roles.Roles.AuthorizedChurch.ToString()
            };

            if (userManager.Users.All(u => u.Id != defaultUser.Id))
            {
                var user = await userManager.FindByEmailAsync(defaultUser.Email);

                if (user == null)
                {
                    await userManager.CreateAsync(defaultUser, _configuration.Client.GetSecret("sys-demo-password").Value.Value);

                    await userManager.AddToRoleAsync(defaultUser, Roles.Roles.AuthorizedChurch.ToString());

                    // Create a ChurchInformation object
                    var churchInformation1 = new ChurchInformation
                    {
                        ID = Guid.NewGuid().ToString(),
                        ChurchAccountId = defaultUser.Id,
                        Name = defaultUser.ChurchName,
                        Denominaion = defaultUser.ChurchDenomination,
                        Website = defaultUser.ChurchWebsite,
                        Address = "4725 Sed St.",
                        Description = "A local evangelical church in the center of quiet town.",
                        Activities = new List<string> { "Totts - Toddler group", "Worship Evening", "Church Meals" },
                        WheelchairAccess = true,
                        Wifi = true,
                        Parking = true,
                        Refreshments = true,
                        Restrooms = true,
                        ServiceTimes = new List<string> { "10:30AM" },
                        Churchsuite = "https://login.churchsuite.com/?account=demo_church",
                        Congregation = "100-200",
                        Phone = "076 2934 8138"
                    };

                    _context.ChurchInformation.Add(churchInformation1);
                    await _context.SaveChangesAsync();
                }
            }
        }

        // Create the 'SuperAdmin' User
        public async Task SeedSuperAdminAsync(UserManager<ChurchAccount> userManager, RoleManager<ChurchRoles> roleManager)
        {
            // Seed Super Admin User
            var superAdminUser = new ChurchAccount
            {
                Email = _configuration.Client.GetSecret("sys-admin-username").Value.Value,
                UserName = _configuration.Client.GetSecret("sys-admin-username").Value.Value,
                ChurchName = "Demo Church",
                ChurchDenomination = "Denomination",
                ChurchWebsite = "https://www.demochurch.org",
                EmailConfirmed = true,
                ImageArray = await ConvertLinkToByteArray("https://i.imgur.com/oLC9RcU.png"),
                Role = Roles.Roles.SuperAdmin.ToString()
            };

            if (userManager.Users.All(u => u.Id != superAdminUser.Id))
            {
                var user = await userManager.FindByEmailAsync(superAdminUser.Email);

                if (user == null)
                {
                    await userManager.CreateAsync(superAdminUser, _configuration.Client.GetSecret("sys-admin-password").Value.Value);
                    await userManager.AddToRoleAsync(superAdminUser, "SuperAdmin");

                    // Create a ChurchInformation object
                    var churchInformation2 = new ChurchInformation
                    {
                        ID = Guid.NewGuid().ToString(),
                        ChurchAccountId = superAdminUser.Id,
                        Name = superAdminUser.ChurchName,
                        Denominaion = superAdminUser.ChurchDenomination,
                        Website = superAdminUser.ChurchWebsite,
                        Description = "A local anglican church in the center of our busy city.",
                        Activities = new List<string> { "Baby & Toddler Group", "Friday Fitness", "Home groups" },
                        WheelchairAccess = true,
                        Wifi = true,
                        Parking = true,
                        Refreshments = true,
                        Restrooms = true,
                        ServiceTimes = new List<string> { "9:30AM", "11:00AM", "6:00PM" },
                        Address = "7250 Red St.",
                        Churchsuite = "https://login.churchsuite.com/?account=demo_church",
                        Congregation = "50-100",
                        Phone = "077 3454 9108"
                    };

                    _context.ChurchInformation.Add(churchInformation2);
                    await _context.SaveChangesAsync();
                }
            }
        }

        // Private Methods
        private async Task<byte[]> ConvertLinkToByteArray(string url)
        {
            using (WebClient client = new WebClient())
            {
                ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;

                byte[] imagebytes = client.DownloadData(url);

                return imagebytes;
            }
        }

        public async Task AddPermissionClaim(RoleManager<ChurchRoles> roleManager, ChurchRoles role, string module)
        {
            var allPermissions = new List<string>
            {
                $"Permissions.{module}.View" ,
                $"Permissions.{module}.View" ,
                $"Permissions.{module}.Edit",
                $"Permissions.{module}.Delete"
            };

            foreach (var permission in allPermissions)
            {
                if (role.RolePermissions.Any(p => p.Permission.Name == permission))
                {
                    var claimValue = permission;
                    var claim = new Claim("Permission", claimValue);
                    await roleManager.AddClaimAsync(role, claim);
                }
            }
        }
    }
}
