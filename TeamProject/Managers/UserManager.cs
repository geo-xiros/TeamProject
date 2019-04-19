using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using TeamProject.Models;
using Dapper;
using System.Security.Claims;
using Microsoft.Owin.Security;
using Microsoft.AspNet.Identity;
using TeamProject.Dal;

namespace TeamProject.Managers
{
    public class UserManager : TableManager<User>
    {
        public UserManager(ProjectDbContext projectDbContext) 
        {
            AddField(t => t.Firstname);
            AddField(t => t.Lastname);
            AddField(t => t.Email);
            AddField(t => t.Password);
            PrepareQueries();

            _db = projectDbContext;
        }


        public override IEnumerable<User> Get(string queryWhere = null, object parameters = null)
        {
            IEnumerable<User> courts = null;

            _db.UsingConnection((dbCon) =>
            {
                var userDictionary = new Dictionary<int, User>();
                courts = dbCon.Query<User, Role, User>(
                    "SELECT [User].*, Role.* FROM [User] " +
                    "LEFT JOIN UserRoles ON [User].Id = UserRoles.UserId " +
                    "LEFT JOIN Role ON UserRoles.RoleId = Role.Id " +
                    (queryWhere == null ? string.Empty : $" WHERE {queryWhere}"),
                    (user, role) =>
                    {
                        User userEntry;

                        if (!userDictionary.TryGetValue(user.Id, out userEntry))
                        {
                            userEntry = user;
                            userEntry.Roles = new List<Role>();
                            userDictionary.Add(userEntry.Id, userEntry);
                        }
                        if (role != null)
                        {
                            userEntry.Roles.Add(role);
                        }

                        return userEntry;
                    },
                        splitOn: "Id",
                        param: parameters)
                        .Distinct()
                        .ToList();
            });

            return courts;
        }

        public User Login(string email, string password)
        {

            var loggedInUser = Get("Email=@email and Password=@password", new { email, password }).FirstOrDefault();

            if (loggedInUser != null)
            {
                var claims = new List<Claim>(new[]
                {
                    // adding following 2 claim just for supporting default antiforgery provider
                    new Claim(ClaimTypes.NameIdentifier, email),
                    new Claim(
                        "http://schemas.microsoft.com/accesscontrolservice/2010/07/claims/identityprovider",
                        "ASP.NET Identity", "http://www.w3.org/2001/XMLSchema#string"),
                    new Claim(ClaimTypes.Name, email),
                    new Claim(ClaimTypes.UserData, loggedInUser.Id.ToString())
                });

                foreach (var role in loggedInUser.Roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role.Description));
                }

                var identity = new ClaimsIdentity(claims, DefaultAuthenticationTypes.ApplicationCookie);

                HttpContext.Current.GetOwinContext().Authentication.SignIn(
                    new AuthenticationProperties { IsPersistent = false }, identity);
            }

            return loggedInUser;
        }
    }
}
