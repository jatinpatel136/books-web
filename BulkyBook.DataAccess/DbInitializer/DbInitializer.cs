using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBook.Models;
using BulkyBook.Utility;
using BulkyBookWeb.DataAccess;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.DbInitializer
{
    public class DbInitializer : IDbInitializer
    {

        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly ApplicationDbContext _db;

        public DbInitializer(
            UserManager<IdentityUser> userManager,

            RoleManager<IdentityRole> roleManager,
            ApplicationDbContext db)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _db = db;
        }

        public void Initialize() 
        {
            // migration if they are not applied
            try
            {
                if(_db.Database.GetPendingMigrations().Count() > 0)
                {
                    _db.Database.Migrate();
                }
            }
            catch
            {

            }

            // create roles if they are not created
            //Create all the roles in DB if it not exists
            //Checking existence of only role is enough for creation of roles 

            if (!_roleManager.RoleExistsAsync(SD.ROLE_ADMIN).GetAwaiter().GetResult())
            {
                _roleManager.CreateAsync(new IdentityRole(SD.ROLE_ADMIN)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.ROLE_EMPLOYEE)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.ROLE_USER_INDIVIDUAL)).GetAwaiter().GetResult();
                _roleManager.CreateAsync(new IdentityRole(SD.ROLE_USER_COMPANY)).GetAwaiter().GetResult();

                // if roles are not created then we will create admin user
                _userManager.CreateAsync(new ApplicationUser()
                {
                    UserName = "admin@gmail.com",
                    Email = "admin@gmail.com",
                    Name = "Justin",
                    PhoneNumber = "987654321",
                    StreetAddress = "123 Main st",
                    State = "IL",
                    PostalCode = "654213",
                    City = "Winnipeg",

                }, "Admin123*").GetAwaiter().GetResult();


                ApplicationUser user = _db.ApplicationUsers.FirstOrDefault(u=> u.Email == "admin@gmail.com");

                _userManager.AddToRoleAsync(user!, SD.ROLE_ADMIN).GetAwaiter().GetResult();

                return;
            }


        }
    }
}
