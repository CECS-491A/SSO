using System;
using DataAccessLayer.Database;
using DataAccessLayer.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLayer.Services;
using System.Data.Entity;
using System.Security.Cryptography;
using ManagerLayer;

namespace UnitTesting
{
    public class TestingUtils
    {
        public byte[] GetRandomness()
        {
            byte[] salt = new byte[128 / 8];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(salt);
            }
            return salt;
        }

        public User CreateUserInDb()
        {

            User u = new User
            {
                Id = Guid.NewGuid(),
                Email = Guid.NewGuid() + "@" + Guid.NewGuid() + ".com",
                DateOfBirth = DateTime.UtcNow,
                City = "Los Angeles",
                State = "California",
                Country = "United States",
                SecurityQ1 = "MySecurityQ1",
                SecurityQ1Answer = "MySecurityAnswerQ1",
                SecurityQ2 = "MySecurityQ2",
                SecurityQ2Answer = "MySecurityAnswerQ2",
                SecurityQ3 = "MySecurityQ3",
                SecurityQ3Answer = "MySecurityAnswerQ3",
                PasswordHash = (Guid.NewGuid()).ToString(),
                PasswordSalt = GetRandomness()
            };

            return CreateUserInDb(u);
        }

        public User CreateUserInDb(User user)
        {
            using ( var _db = new DatabaseContext() )
            {
                _db.Entry(user).State = System.Data.Entity.EntityState.Added;
                _db.SaveChanges();

                return user;
            }
        }

        public User CreateUserInDbManager()
        {
            using (var _db = new DatabaseContext())
            {
                UserManager um = new UserManager(_db);
                User u = um.CreateUser(Guid.NewGuid() + "@" + Guid.NewGuid() + ".com","qwertyuiop136_!2019",new DateTime(1996,12,15),"Long Beach", "CA",
                    "USA", "securityQ1?", "q1", "securityQ2?", "q2", "securityQ3?", "q3");

                _db.SaveChanges();

                return u;
            }
        }

        public User CreateUserObject()
        {
            User user = new User
            {
                Id = Guid.NewGuid(),
                Email = Guid.NewGuid() + "@" + Guid.NewGuid() + ".com",
                DateOfBirth = DateTime.UtcNow,
                City = "Los Angeles",
                State = "California",
                Country = "United States",
                SecurityQ1 = "MySecurityQ1",
                SecurityQ1Answer = "MySecurityAnswerQ1",
                SecurityQ2 = "MySecurityQ2",
                SecurityQ2Answer = "MySecurityAnswerQ2",
                SecurityQ3 = "MySecurityQ3",
                SecurityQ3Answer = "MySecurityAnswerQ3",
                PasswordHash = (Guid.NewGuid()).ToString(),
                PasswordSalt = GetRandomness()
            };
            return user;
        }

        public Session CreateSessionObject(User user)
        {
            Session session = new Session
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                UserId = user.Id,
                UpdatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(Session.MINUTES_UNTIL_EXPIRATION),
                User = user,
                Token = (Guid.NewGuid()).ToString()
            };
            return session;
        }


        public Session CreateSessionInDb(Session session)
        {
            using(var _db = new DatabaseContext())
            {
                _db.Sessions.Add(session);
                _db.SaveChanges();

                return session;
            }
        }

        public Session CreateSessionInDb(User user)
        {
            Session session = new Session
            {
                Id = Guid.NewGuid(),
                CreatedAt = DateTime.UtcNow,
                UserId = user.Id,
                UpdatedAt = DateTime.UtcNow,
                ExpiresAt = DateTime.UtcNow.AddMinutes(Session.MINUTES_UNTIL_EXPIRATION),
                Token = (Guid.NewGuid()).ToString()
            };

            return CreateSessionInDb(session);
        }
		public Session CreateExpiredSessionInDb(User user)
		{
			Session session = new Session
			{
				Id = Guid.NewGuid(),
				UserId = user.Id,
				CreatedAt = DateTime.UtcNow.AddHours(-2),
				UpdatedAt = DateTime.UtcNow.AddHours(-2),
				ExpiresAt = DateTime.UtcNow.AddHours(-1.5),
			Token = (Guid.NewGuid()).ToString()
			};

			return CreateSessionInDb(session);
		}

		public PasswordReset CreatePasswordResetInDB()
        {
            PasswordReset pr = new PasswordReset
            {
                Id = new Guid(),
                ResetToken = "",
                UserID = new Guid(),
                ExpirationTime = DateTime.Now.AddMinutes(5),
                ResetCount = 0,
                Disabled = false
            };
            return CreatePasswordResetInDB(pr);
        }

        public PasswordReset CreatePasswordResetInDB(PasswordReset resetToken)
        {
            using (var _db = new DatabaseContext())
            {
                _db.Entry(resetToken).State = System.Data.Entity.EntityState.Added;
                _db.SaveChanges();
                return resetToken;
            }
        }

        public PasswordReset CreatePasswordResetObject(User user)
        {
            PasswordReset pr = new PasswordReset
            {
                Id = Guid.NewGuid(),
                ResetToken = Guid.NewGuid().ToString(),
                UserID = user.Id,
                User = user,
                ExpirationTime = DateTime.Now.AddMinutes(5),
                ResetCount = 0,
                Disabled = false
            };
            return pr;
        }


        public Application CreateApplicationInDb()
        {
            var app = CreateApplicationObject();

            return CreateApplicationInDb(app);
        }

        public Application CreateApplicationInDb(Application app)
        {
            using (var _db = new DatabaseContext())
            {
                _db.Entry(app).State = System.Data.Entity.EntityState.Added;
                _db.SaveChanges();

                return app;
            }
        }

        public Application CreateApplicationObject()
        {
            var title = Guid.NewGuid();
            Application app = new Application
            {
                Title = Guid.NewGuid() + " App",
                LaunchUrl = "https://" + title + ".com",
                Email = title + "@email.com",
                UserDeletionUrl = "https://" + title + ".com/delete",
                LogoUrl = "https://kfc.com/logo.png",
                Description = "A KFC app",
                SharedSecretKey = Guid.NewGuid().ToString("N"),
                HealthCheckUrl = "https://kfc.com/health",
                LogoutUrl = "https://" + title + ".com/logout",
            };
            return app;
        }

        public ApiKey CreateApiKeyInDb()
        {
            var apiKey = CreateApiKeyObject();

            return CreateApiKeyInDb(apiKey);
        }

        public ApiKey CreateApiKeyInDb(ApiKey apiKey)
        {
            using (var _db = new DatabaseContext())
            {
                _db.Entry(apiKey).State = System.Data.Entity.EntityState.Added;
                _db.SaveChanges();

                return apiKey;
            }
        }

        public ApiKey CreateApiKeyObject()
        {
            Application app = CreateApplicationInDb();
            ApiKey apiKey = new ApiKey
            {
                Key = Guid.NewGuid().ToString("N"),
                ApplicationId = app.Id
            };
            return apiKey;
        }

        public DatabaseContext CreateDataBaseContext()
        {
            return new DatabaseContext();
        }

        public bool isEqual(string[] arr1, string[] arr2)
        {
            if (arr1.Length != arr2.Length)
                return false;
            for (int i = 0; i < arr1.Length; i++)
            {
                if (arr1[i] != arr2[i])
                    return false;
            }
            return true;
        }

    }
}
