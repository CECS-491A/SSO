using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using System.Web.Http.Cors;
using DataAccessLayer.Database;
using DataAccessLayer.Models;
using ServiceLayer.Services;

namespace WebAPI.Controllers
{
    [EnableCors(origins: "http://localhost:8081", headers: "*", methods: "*")]
    public class UsersController : ApiController
    {
        public class RegisterRequest
        {
            public string email;
            public string password;

            public DateTime dob;
            public string city;
            public string state;
            public string country;

            public string securityQ1;
            public string securityQ1Answer;
            public string securityQ2;
            public string securityQ2Answer;
            public string securityQ3;
            public string securityQ3Answer;
        }

        [HttpPost]
        [Route("api/users/register")]
        public IHttpActionResult Register([FromBody] RegisterRequest request)
        {
            try
            {
                var valid = new System.Net.Mail.MailAddress(request.email);
            }
            catch (Exception)
            {
                return BadRequest("Email is not valid");
            }
            IPasswordService _passwordService = new PasswordService();
            ISessionService _sessionService = new SessionService();

            byte[] salt = _passwordService.GenerateSalt();
            string hash = _passwordService.HashPassword(request.password, salt);
            User user = new User
            {
                Email = request.email,
                PasswordHash = hash,
                PasswordSalt = salt,

                DateOfBirth = request.dob,
                City = request.city,
                State = request.state,
                Country = request.country,

                SecurityQ1 = request.securityQ1,
                SecurityQ1Answer = request.securityQ1Answer,
                SecurityQ2 = request.securityQ2,
                SecurityQ2Answer = request.securityQ2Answer,
                SecurityQ3 = request.securityQ3,
                SecurityQ3Answer = request.securityQ3Answer,
                UpdatedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            using (var _db = new DatabaseContext())
            {
                IUserService _userService = new UserService();
                var dbUser = _userService.CreateUser(_db, user);

                try
                {
                    _db.SaveChanges();
                    return Ok(new { data = new { token = "faketoken" } });
                }
                catch (DbEntityValidationException ex)
                {
                    _db.Entry(dbUser).State = System.Data.Entity.EntityState.Detached;
                    return InternalServerError();
                }
            }
        }
    }
}