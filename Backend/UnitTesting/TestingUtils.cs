using DataAccessLayer.Database;
using DataAccessLayer.Models;
using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLayer.Services;

using System.Security.Cryptography;

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
                Email = Guid.NewGuid() + "@" + Guid.NewGuid() + ".com",
                DateOfBirth = DateTime.UtcNow,
                City = "Los Angeles",
                State = "California",
                Country = "United States",
                PasswordHash = (Guid.NewGuid()).ToString(),
                PasswordSalt = GetRandomness()
            };

            return CreateUserInDb(u);
        }

        public User CreateUserInDb(User user)
        {
            using (var _db = new DatabaseContext())
            {
                _db.Users.Add(user);
                _db.SaveChanges();

                return user;
            }
        }

        public User CreateUserObject()
        {
            User user = new User
            {
                Email = Guid.NewGuid() + "@" + Guid.NewGuid() + ".com",
                DateOfBirth = DateTime.UtcNow,
                City = "Los Angeles",
                State = "California",
                Country = "United States",
                PasswordHash = (Guid.NewGuid()).ToString(),
                PasswordSalt = GetRandomness()
            };
            return user;
        }

        public Session CreateSession(User user)
        {
           using (var _db = new DatabaseContext())
            {
                Session s = new Session
                {
                    UserId = user.Id,
                    ExpiresAt = DateTime.UtcNow,
                    Token = "token"
                };
                _db.Sessions.Add(s);
                _db.SaveChanges();

                return s;
            }
        }

        public Service CreateServiceInDb(bool enabled)
        {
            using (var _db = new DatabaseContext())
            {
                Service s = new Service
                {
                    ServiceName = (Guid.NewGuid()).ToString(),
                    Disabled = !enabled,
                    UpdatedAt = DateTime.UtcNow
                };
                _db.Services.Add(s);
                _db.SaveChanges();

                return s;
            }
        }

        public Service CreateServiceObject(bool enabled)
        {
            Service s = new Service
            {
                ServiceName = (Guid.NewGuid()).ToString(),
                Disabled = !enabled
            };

            return s;
        }

        public Claim CreateClaim(User user, Service service, User subjectUser)
        {
            using (var _db = new DatabaseContext())
            {
                Claim c = new Claim
                {
                    ServiceId = service.Id,
                    UserId = user.Id
                };
                _db.Claims.Add(c);
                _db.SaveChanges();

                return c;
            }
        }

        public Client CreateClientObject() {
            Client client = new Client
            {
                Id = Guid.NewGuid(),
                Disabled = false,
                Name = Guid.NewGuid().ToString(),
                Address = Guid.NewGuid().ToString(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow

            };
            return client;
        }

        public Client CreateClientInDb()
        {

            Client client = new Client
            {
                Id = Guid.NewGuid(),
                Disabled = false,
                Name = Guid.NewGuid().ToString(),
                Address = Guid.NewGuid().ToString(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow

            };

            return CreateUserInDb(client);
        }

        public Client CreateUserInDb(Client client)
        {
            using (var _db = new DatabaseContext())
            {
                _db.Clients.Add(client);
                _db.SaveChanges();

                return client;
            }
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
