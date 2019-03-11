using DataAccessLayer.Database;
using DataAccessLayer.Models;
using ServiceLayer.Services;
using System;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using Newtonsoft.Json;
using ManagerLayer.PasswordManagement;
using ManagerLayer.UserManagement;

namespace KFC_WebAPI.Controllers
{
    public class UsersController : ApiController
    {
        public class RegisterRequest
        {
            public string email;
            public string password;
            public DateTime dob;
        }

        public class postedPasswords
        {
            [JsonProperty("username")]
            public string Username { get; set; }
            [JsonProperty("sessionToken")]
            public string SessionToken { get; set; }
            [JsonProperty("oldPassword")]
            public string OldPassword { get; set; }
            [JsonProperty("newPassword")]
            public string NewPassword { get; set; }
            [JsonProperty("confirmNewPassword")]
            public string ConfirmNewPassword { get; set; }
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
        [Route("api/user/updatepassword")]
        public HttpResponseMessage updatePassword([FromBody] postedPasswords passwords)
        {
            //if ()
            //{
                string oldPassword = passwords.OldPassword;
                string newPassword = passwords.NewPassword;
                string confirmNewPassword = passwords.ConfirmNewPassword;

                if (newPassword.Length < 2001 && newPassword.Length > 11 && newPassword.Equals(confirmNewPassword))
                {
                    PasswordManager pm = new PasswordManager();
                    UserManagementManager umm = new UserManagementManager();
                    if (!pm.CheckIsPasswordPwned(newPassword))
                    {
                        string newPasswordHashed = pm.SaltAndHashPassword(newPassword);
                        User userToUpdate = umm.GetUser(passwords.Username);
                        pm.UpdatePassword(userToUpdate, newPasswordHashed);
                        return Request.CreateResponse(HttpStatusCode.OK, "Password Updated");
                    }
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Password has been pwned, please use a different password");
                }
                return Request.CreateResponse(HttpStatusCode.BadRequest, "Password submitted does not fit password requirements");
            //}
            //return Request.CreateResponse(HttpStatusCode.Unauthorized, "User is not logged in");
        }
    }
}
