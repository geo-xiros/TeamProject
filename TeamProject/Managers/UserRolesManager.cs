﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using Dapper;
using TeamProject.Dal;
using TeamProject.Models;

namespace TeamProject.Managers
{
    public class UserRolesManager : TableManager<UserRoles>
    {
        public UserRolesManager(ProjectDbContext projectDbContext) 
        {
            _db = projectDbContext;
        }

        public override IEnumerable<UserRoles> Get(string queryWhere = null, object parameters = null)
        {
            IEnumerable<UserRoles> userRoles = null;

            _db.UsingConnection((dbCon) =>
            {
                userRoles = dbCon.Query<UserRoles, User, Role, UserRoles>(
                    "SELECT UserRoles.*, [User].*, [Role].* " +
                    "FROM UserRoles " +
                    "INNER JOIN [User] ON UserRoles.UserId = [User].Id " +
                    "INNER JOIN [Role] ON UserRoles.RoleId = [Role].Id " +
                    (queryWhere == null ? string.Empty : $" WHERE {queryWhere}"),
                    (userRole, user, role) =>
                    {
                        userRole.User = user;
                        userRole.Role = role;
                        return userRole;
                    },
                    splitOn: "id",
                    param: parameters)
                    .Distinct();
            });

            return userRoles;
        }
        
    }
}
