﻿using DataAccessLayer.Database;
using DataAccessLayer.Models;
using ManagerLayer.Login;
using ServiceLayer.Services;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;

namespace KFC_WebAPI.Controllers
{
    [EnableCors(origins: "*", headers: "*", methods: "*")]
    public class UsersController : ApiController
    {
        public class RegisterRequest
        {
            public string email;
            public string password;
            public DateTime dob;
        }

        [HttpPost]
        [Route("api/users/register")]
        public string Register([FromBody] RegisterRequest request)
        {
            try
            {
                var valid = new System.Net.Mail.MailAddress(request.email);
            }
            catch (Exception)
            {
                return request.email;
                // TODO: Handle with REST error
            }
            IPasswordService _passwordService = new PasswordService();
            DateTime timestamp = DateTime.UtcNow;
            byte[] salt = _passwordService.GenerateSalt();
            string hash = _passwordService.HashPassword(request.password, salt);
            User user = new User
            {
                Email = request.email,
                PasswordHash = hash,
                PasswordSalt = salt,
                DateOfBirth = request.dob,
                UpdatedAt = timestamp
            };

            using (var _db = new DatabaseContext())
            {
                IUserService _userService = new UserService();
                var response = _userService.CreateUser(_db, user);
                try
                {
                    _db.SaveChanges();
                    return "woo it succeeded!";
                }
                catch (DbEntityValidationException ex)
                {
                    //catch error
                    // detach user attempted to be created from the db context - rollback
                    _db.Entry(response).State = System.Data.Entity.EntityState.Detached;
                }
            }
            return "got to end without stuff";
        }

        [HttpPost]
        [Route("api/users/login")]
        public IHttpActionResult Login([FromBody] LoginRequest request)
        {
            LoginManager loginM = new LoginManager();
            if (loginM.LoginCheckUserExists(request.email) == false)
            {
                //404
                return Content(HttpStatusCode.NotFound, "Invalid Username");
            }
            else
            {
                if (loginM.LoginCheckUserDisabled())
                {
                    //401
                    return Content(HttpStatusCode.Unauthorized, "User is Disabled");
                }
                else
                {
                    if (loginM.LoginCheckPassword(request.password))
                    {
                        return Ok(loginM.LoginAuthorized());
                    }
                    else
                    {
                        //400
                        return Content(HttpStatusCode.BadRequest, "Invalid Password");
                    }
                }
            }
        }
    }
}
