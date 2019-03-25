﻿using DataAccessLayer.Database;
using ServiceLayer.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using DataAccessLayer.Models;
using MimeKit;
using System.Data.Entity.Validation;

namespace ManagerLayer.PasswordManagement
{
    public class PasswordManager
    {
        //Variable for how long the token is supposed to be live, in minutes
        private const double TimeToExpire = 5;

        private IResetService _resetService;
        private IUserService _userService;
        private IPasswordService _passwordService;
        private IEmailService _emailService;
        private ITokenService _tokenService;

        public PasswordManager()
        {
            _resetService = new ResetService();
            _userService = new UserService();
            _tokenService = new TokenService();
            _passwordService = new PasswordService();
        }

        private DatabaseContext CreateDbContext()
        {
            return new DatabaseContext();
        }

        public PasswordReset CreatePasswordReset(Guid userID)
        {
            string generatedResetToken = _tokenService.GenerateToken();

            DateTime newExpirationTime = DateTime.Now.AddMinutes(TimeToExpire);

            using (var _db = CreateDbContext())
            {
                PasswordReset passwordReset = new PasswordReset
                {
                    ResetToken = generatedResetToken,
                    UserID = userID
                };
                var response = _resetService.CreatePasswordReset(_db, passwordReset);
                try
                {
                    _db.SaveChanges();
                    return response;
                }
                catch (DbEntityValidationException ex)
                {
                    //catch error
                    //detach PasswordReset attempted to be created from the db context - rollback
                    _db.Entry(response).State = System.Data.Entity.EntityState.Detached;
                }
                return null;
            }
        }

        public int DeletePasswordReset(string resetToken)
        {
            using (var _db = CreateDbContext())
            {
                _resetService.DeletePasswordReset(_db, resetToken);
                return _db.SaveChanges();
            }
        }

        public PasswordReset GetPasswordReset(string resetToken)
        {
            using (var _db = CreateDbContext())
            {
                return _resetService.GetPasswordReset(_db, resetToken);
            }
        }

        public int UpdatePasswordReset(PasswordReset updatedPasswordReset)
        {
            using (var _db = CreateDbContext())
            {
                var response = _resetService.UpdatePasswordReset(_db, updatedPasswordReset);
                try
                {
                    return _db.SaveChanges();
                }
                catch (DbEntityValidationException ex)
                {
                    // catch error
                    // rollback changes
                    _db.Entry(response).CurrentValues.SetValues(_db.Entry(response).OriginalValues);
                    _db.Entry(response).State = System.Data.Entity.EntityState.Unchanged;
                    return 0;
                }
            }
        }

        public DateTime GetPasswordResetExpiration(string resetToken)
        {
            var PasswordResetRetrieved = GetPasswordReset(resetToken);
            if (PasswordResetRetrieved != null)
            {
                return PasswordResetRetrieved.ExpirationTime;
            }
            return DateTime.MinValue;
        }

        public bool ExistingResetToken(string resetToken)
        {
            using (var _db = CreateDbContext())
            {
                return _resetService.ExistingReset(_db, resetToken);
            }
        }

        public int GetAttemptsPerID(string resetToken)
        {
            var PasswordResetRetrieved = GetPasswordReset(resetToken);
            if (PasswordResetRetrieved != null)
            {
                return PasswordResetRetrieved.ResetCount;
            }
            return -1;
        }

        public bool GetPasswordResetStatus(string resetToken)
        {
            var PasswordResetRetrieved = GetPasswordReset(resetToken);
            if (PasswordResetRetrieved != null)
            {
                return PasswordResetRetrieved.Disabled;
            }
            return false;
        }

        public string CreateResetURL(string baseURL, string resetToken)
        {
            string resetControllerURL = baseURL;
            string resetURL = resetControllerURL + resetToken;
            return resetURL;
        }

        //Completely disables the PasswordReset from resetting password
        public void LockPasswordReset(string resetToken)
        {
            var PasswordResetRetrieved = GetPasswordReset(resetToken);
            PasswordResetRetrieved.Disabled = true;
            PasswordResetRetrieved.AllowPasswordReset = false;
            UpdatePasswordReset(PasswordResetRetrieved);
        }

        public bool CheckPasswordResetValid(string resetToken)
        {
            //See if ResetID exists 
            if (ExistingResetToken(resetToken))
            {
                if (GetPasswordResetExpiration(resetToken) > DateTime.Now)
                {
                    if (!GetPasswordResetStatus(resetToken))
                    {
                        if (GetAttemptsPerID(resetToken) < 4)
                        {
                            return true;
                        }
                    }
                }
                else
                {
                    LockPasswordReset(resetToken);
                }
            }
            return false;
        }

        public int PasswordResetsMadeInPast24HoursByUser(Guid UserID)
        {
            int NumOfResetLinks = 3;
            DateTime past24Hours = DateTime.Now.AddDays(-1);
            DateTime currentTime = DateTime.Now.AddMinutes(5);
            using (var _db = CreateDbContext())
            {
                var listOfTokensFrom24Hours = from r in _db.PasswordResets
                                              where r.ExpirationTime <= currentTime & r.ExpirationTime >= past24Hours & r.UserID == UserID
                                              select r;
                NumOfResetLinks = listOfTokensFrom24Hours.Count();
                return NumOfResetLinks;
            }
        }

        public void SendResetToken(string email, string url)
        {
            using (var _db = CreateDbContext())
            {
                if (_userService.ExistingUser(_db, email))
                {
                    Guid userID = _userService.GetUser(_db, email).Id;

                    if (PasswordResetsMadeInPast24HoursByUser(userID) < 3)
                    {
                        PasswordReset newlyCreatedPasswordReset = CreatePasswordReset(userID);
                        string resetToken = newlyCreatedPasswordReset.ResetToken;
                        string resetLink = CreateResetURL(url, resetToken);
                        SendResetEmailUserExists(email, resetLink);
                    }
                    else
                    {
                        SendResetEmailUserExistsTooManyResets(email);
                    }
                }
                else
                {
                    SendResetEmailUserDoesNotExist(email);
                }
            }
        }

        public bool CheckIsPasswordPwned(string newPasswordToCheck)
        {
            return (_passwordService.CheckPasswordPwned(newPasswordToCheck) > 3);
        }

        public string SaltAndHashPassword(string resetToken, string password)
        {
            var retrievedPasswordReset = GetPasswordReset(resetToken);
            var userIDAssociatedWithPasswordReset = retrievedPasswordReset.UserID;
            byte[] salt = _passwordService.GenerateSalt();

            using (var _db = CreateDbContext())
            {
                var userToUpdate = _db.Users.Find(userIDAssociatedWithPasswordReset);
                if (userToUpdate != null)
                {
                    userToUpdate.PasswordSalt = salt;
                    _db.SaveChanges();
                    string hashedPassword = _passwordService.HashPassword(password, salt);
                    return hashedPassword;
                }
            }
            return null;
        }

        public string HashPassword(string password, byte[] salt)
        {
            return _passwordService.HashPassword(password, salt);
        }

        //This password update function is for when the user is not logged in and has answered the security questions
        public bool UpdatePassword(string resetToken, string newPasswordHash)
        {
            var retrievedPasswordReset = GetPasswordReset(resetToken);
            var userIDAssociatedWithPasswordReset = retrievedPasswordReset.UserID;

            using (var _db = CreateDbContext())
            {
                var userToUpdate = _db.Users.Find(userIDAssociatedWithPasswordReset);
                if (userToUpdate != null)
                {
                    userToUpdate.PasswordHash = newPasswordHash;
                    _db.SaveChanges();
                    LockPasswordReset(resetToken);
                    _db.SaveChanges();
                    SendPasswordChange(userToUpdate.Email);
                    return true;
                }
                return false;
            }
        }

        //This password update function is for when the user is already logged in and wants to update their password
        public bool UpdatePassword(User userToUpdate, string newPasswordHash)
        {
            using (var _db = CreateDbContext())
            {
                var userRetrieved = _db.Users.Find(userToUpdate.Id);
                var storedHash = userToUpdate.PasswordHash;
                if (storedHash == newPasswordHash)
                {
                    return false;
                }
                else
                {
                    userRetrieved.PasswordHash = newPasswordHash;
                    _db.SaveChanges();
                    return true;
                }
            }
        }

        //Function to get security questions from the DB
        public List<string> GetSecurityQuestions(string resetToken)
        {
            List<string> listOfSecurityQuestions = new List<string>();
            var retrievedPasswordReset = GetPasswordReset(resetToken);
            var userID = retrievedPasswordReset.UserID;

            using (var _db = CreateDbContext())
            {
                User retrievedUser = _userService.GetUser(_db, userID);
                var securityQ1 = retrievedUser.SecurityQ1;
                var securityQ2 = retrievedUser.SecurityQ2;
                var securityQ3 = retrievedUser.SecurityQ3;
                listOfSecurityQuestions.Add(securityQ1);
                listOfSecurityQuestions.Add(securityQ2);
                listOfSecurityQuestions.Add(securityQ3);
                return listOfSecurityQuestions;
            }
        }

        //Function to check security answers against the DB
        public bool CheckSecurityAnswers(string resetToken, List<string> userSubmittedSecurityAnswers)
        {
            List<string> listOfSecurityAnswers = new List<string>();
            var retrievedPasswordReset = GetPasswordReset(resetToken);
            var userID = retrievedPasswordReset.UserID;
            using (var _db = CreateDbContext())
            {
                User retrievedUser = _userService.GetUser(_db, userID);
                var securityA1 = retrievedUser.SecurityQ1Answer;
                var securityA2 = retrievedUser.SecurityQ2Answer;
                var securityA3 = retrievedUser.SecurityQ3Answer;
                listOfSecurityAnswers.Add(securityA1);
                listOfSecurityAnswers.Add(securityA2);
                listOfSecurityAnswers.Add(securityA3);
                for (int i = 0; i < listOfSecurityAnswers.Count; i++)
                {
                    //If the answers provided don't match the answers in the DB, the number of attempts to reset the password with that resetID is incremented
                    if (listOfSecurityAnswers[i] != userSubmittedSecurityAnswers[i])
                    {
                        //Needs to be updated to get the most recent PasswordReset
                        retrievedPasswordReset.ResetCount = retrievedPasswordReset.ResetCount + 1;
                        UpdatePasswordReset(retrievedPasswordReset);
                        if (GetPasswordReset(retrievedPasswordReset.ResetToken).ResetCount > 4)
                        {
                            retrievedPasswordReset.Disabled = true;
                            UpdatePasswordReset(retrievedPasswordReset);
                        }
                        return false;
                    }
                }
                retrievedPasswordReset.AllowPasswordReset = true;
                UpdatePasswordReset(retrievedPasswordReset);
                return true;
            }
        }

        public bool CheckIfPasswordResetAllowed(string resetToken)
        {
            using (var _db = CreateDbContext())
            {
                var resetTokenRetrieved = _resetService.GetPasswordReset(_db, resetToken);
                return resetTokenRetrieved.AllowPasswordReset;
            }
        }

        #region Email Functions
        //Function to create the email is user exists 
        public void SendResetEmailUserExists(string receiverEmail, string resetURL)
        {
            try
            {
                _emailService = new EmailService();
            }
            catch (Exception)
            {
                throw new Exception("Email service has encountered a problem.");
            }
            string resetPasswordSubjectString = "KFC SSO Reset Password";
            string userFullName = receiverEmail;
            string template = "Hi, \r\n" +
                                             "You recently requested to reset your password for your KFC account, click the link below to reset it.\r\n" +
                                             "The URL is only valid for the next 5 minutes\r\n {0}\r\n\r\n" +
                                             "If you did not request to reset your password, please contact us by responding to this email.\r\n\r\n" +
                                             "Thanks, KFC Team";
            string data = resetURL;
            string resetPasswordBodyString = string.Format(template, data);

            //Create the message that will be sent
            MimeMessage emailToSend = _emailService.createEmailPlainBody(userFullName, receiverEmail, resetPasswordSubjectString, resetPasswordBodyString);
            //Send the email with the message
            _emailService.sendEmail(emailToSend);
        }

        //Function to create the email is user exists, but has too many reset links
        public void SendResetEmailUserExistsTooManyResets(string receiverEmail)
        {
            _emailService = new EmailService();
            string resetPasswordSubjectString = "KFC SSO Reset Password";
            string userFullName = receiverEmail;
            string resetPasswordBodyString = "Hi, \r\n" +
                                             "You recently requested to reset your password for your KFC account, however 3 resets have been attempted within the past 24 hours.\r\n" +
                                             "Please wait 24 hours until you attempt to reset your password\r\n\r\n" +
                                             "If you did not request to reset your password, please contact us by responding to this email.\r\n\r\n" +
                                             "Thanks, KFC Team";

            //Create the message that will be sent
            MimeMessage emailToSend = _emailService.createEmailPlainBody(userFullName, receiverEmail, resetPasswordSubjectString, resetPasswordBodyString);
            //Send the email with the message
            _emailService.sendEmail(emailToSend);
        }

        //Function to create the email is user doesn't exist
        public void SendResetEmailUserDoesNotExist(string receiverEmail)
        {
            _emailService = new EmailService();
            string resetPasswordSubjectString = "KFC SSO Reset Password";
            string userFullName = "Unknown";
            string resetPasswordUserDoesNotExistEmailBody = "Hello, \r\n" +
                              "You (or someone else) entered this email address when trying to reset the password of a KFC account.\r\n" +
                              "However, this email address is not on our database of registered users and therefore the attempt to reset the password has failed.\r\n" +
                              "If you have a KFC account and were expecting this email, please try again using the email address you gave when opening your account." +
                              "If you do not have a KFC account, please ignore this email.\r\n" +
                              "For more information about KFC, please visit www.kfc.com/faq \r\n\r\n" +
                              "Best Regards, KFC Team";

            //Create the message that will be sent
            MimeMessage emailToSend = _emailService.createEmailPlainBody(userFullName, receiverEmail, resetPasswordSubjectString, resetPasswordUserDoesNotExistEmailBody);
            //Send the email with the message
            _emailService.sendEmail(emailToSend);
        }

        //Function to create the email if the password was changed
        public void SendPasswordChange(string receiverEmail)
        {
            _emailService = new EmailService();

            //Need SQL Query to get info about user from DB
            string resetPasswordSubjectString = "KFC SSO Reset Password";
            string userFullName = receiverEmail;
            string changedPasswordBody = "Hi, \r\n" +
                                             "You have changed your password on KFC SSO.\r\n" +
                                             "If you did not change your password, please contact us by responding to this email.\r\n\r\n" +
                                             "Thanks, KFC Team";
            //Create the email service object
            EmailService es = new EmailService();
            //Create the message that will be sent
            MimeMessage emailToSend = _emailService.createEmailPlainBody(userFullName, receiverEmail, resetPasswordSubjectString, changedPasswordBody);
            //Send the email with the message
            _emailService.sendEmail(emailToSend);
        }
        #endregion

    }
}
