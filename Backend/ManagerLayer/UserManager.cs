﻿using DataAccessLayer.Database;
using DataAccessLayer.Models;
using ServiceLayer.Exceptions;
using ServiceLayer.Services;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Net.Http;
using System.Linq;
using System.Collections.Generic;
using System.Text;

namespace ManagerLayer
{
    public class UserManager
    {
        public User CreateUser(
            DatabaseContext _db,
            string email,
            string password,
            DateTime dob,
            string city,
            string state,
            string country,
            string securityQ1,
            string securityQ1Answer,
            string securityQ2,
            string securityQ2Answer,
            string securityQ3,
            string securityQ3Answer)
        {
            new System.Net.Mail.MailAddress(email);

            DateTime today18YearsAgo = DateTime.Now.AddYears(-18);
            if (dob > today18YearsAgo)
            {
                throw new InvalidDobException("Date of birth less than 18 years ago");
            }

            IPasswordService _passwordService = new PasswordService();

            if (!_passwordService.CheckPasswordLength(password))
            {
                throw new PasswordInvalidException("Password is too short");
            }

            int pwnedCount = _passwordService.CheckPasswordPwned(password);

            if (pwnedCount > 0)
            {
                throw new PasswordPwnedException("Password has been pwned");
            }

            byte[] salt = _passwordService.GenerateSalt();
            string hash = _passwordService.HashPassword(password, salt);

            User user = new User
            {
                Email = email,
                PasswordHash = hash,
                PasswordSalt = salt,

                DateOfBirth = dob,
                City = city,
                State = state,
                Country = country,

                SecurityQ1 = securityQ1,
                SecurityQ1Answer = securityQ1Answer,
                SecurityQ2 = securityQ2,
                SecurityQ2Answer = securityQ2Answer,
                SecurityQ3 = securityQ3,
                SecurityQ3Answer = securityQ3Answer,
                UpdatedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow
            };

            IUserService _userService = new UserService();
            return _userService.CreateUser(_db, user);
        }


        /// <summary>
        ///     Not part of spring 3
        ///     - in development
        /// </summary>
        /// <param name="email"></param>
        /// <param name="password"></param>
        public void Login(string email, string password)
        {
            //UserService _userService = new UserService();
            //PasswordService _passwordService = new PasswordService();
            //var user = _userService.Login(email, password);
        }

        public User GetUser(DatabaseContext _db, Guid userId)
        {
            IUserService _userService = new UserService();
            return _userService.GetUser(_db, userId);
        }


        public async Task<User> DeleteUser(DatabaseContext _db, Guid userId)
        {
            UserDeleteService uds = new UserDeleteService();
            IUserService _userService = new UserService();
            ISessionService _sessionService = new SessionService();
            var sessions = _sessionService.GetSessions(_db, userId);
            var applications = ApplicationService.GetAllApplicationsList(_db);
            //var appList = applications.OfType<Application>().ToList();
            var responseList = new List<HttpResponseMessage>();
            foreach(Application app in applications)
            {
                var request = await uds.SendDeleteRequest(app,userId.ToString());
                responseList.Add(request);
            }
            if (responseList.All(response => response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.NotFound))
            {
                User deletedUser = _userService.DeleteUser(_db, userId);
                if(deletedUser != null){
                    foreach(Session sess in sessions)
                    {
                        _sessionService.DeleteSession(_db, sess.Token);
                    }
                }
                _db.SaveChanges(); //foreign key error
                return deletedUser;
            }
            return null;
        }
    }
}
