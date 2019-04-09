﻿using System;
using System.Data.Entity.Validation;
using DataAccessLayer.Database;
using DataAccessLayer.Models;
using ManagerLayer.UserManagement;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ServiceLayer.Services;

namespace UnitTesting
{
    [TestClass]
    public class UserServiceUT
    {
        TestingUtils tu;

        public UserServiceUT()
        {
            tu = new TestingUtils();
        }

        [TestMethod]
        public void Create_User_Success()
        {
            // Arrange
            User newUser = tu.CreateUserObject();
            User expected = newUser;
            using (var _db = tu.CreateDataBaseContext())
            {
                // Act
                UserService _us = new UserService(_db);
                User response = _us.CreateUser(newUser);
                _db.SaveChanges();

                //Assert
                Assert.IsNotNull(response);
                Assert.AreSame(response, expected);
            }
        }

        [TestMethod]
        public void Create_User_RetrieveNew_Success()
        {
            // Arrange
            User newUser = tu.CreateUserObject();
            User expected = newUser;

            using (var _db = tu.CreateDataBaseContext())
            {
                // Act
                UserService _us = new UserService(_db);
                User response = _us.CreateUser(newUser);
                _db.SaveChanges();

                //Assert
                var result = _db.Users.Find(newUser.Id);
                Assert.IsNotNull(response);
                Assert.IsNotNull(result);
                Assert.AreSame(result, expected);
            }
        }

        [TestMethod]
        public void Create_User_Fail_ExceptionThrown()
        {
            // Arrange
            User newUser = new User
            {
                Email = Guid.NewGuid() + "@" + Guid.NewGuid() + ".com",
                DateOfBirth = DateTime.UtcNow,
                City = "Los Angeles",
                State = "California",
                Country = "United States",

                // missing required fields
            };
            var expected = newUser;

            using (var _db = tu.CreateDataBaseContext())
            {
                // ACT
                UserService _us = new UserService(_db);
                var response = _us.CreateUser(newUser);
                try
                {
                    _db.SaveChanges();
                }
                catch (DbEntityValidationException ex)
                {
                    //catch error
                    // detach user attempted to be created from the db context - rollback
                    _db.Entry(response).State = System.Data.Entity.EntityState.Detached;
                }
                var result = _db.Users.Find(newUser.Id);

                // Assert
                Assert.IsNull(result);
                Assert.IsNotNull(response);
                Assert.AreEqual(expected, response);
                Assert.AreNotEqual(expected, result);
            }
        }

        [TestMethod]
        public void Delete_User_Success()
        {
            // Arrange
            User newUser = tu.CreateUserInDb();

            var expectedResponse = newUser;

            using (var _db = tu.CreateDataBaseContext())
            {
                // Act
                UserService _us = new UserService(_db);
                var response = _us.DeleteUser(newUser.Id);
                _db.SaveChanges();
                var result = _db.Users.Find(expectedResponse.Id);

                // Assert
                Assert.IsNotNull(response);
                Assert.IsNull(result);
                Assert.AreEqual(response.Id, expectedResponse.Id);
            }
        }

        [TestMethod]
        public void Delete_User_NonExisting()
        {
            // Arrange
            Guid nonExistingId = Guid.NewGuid();

            var expectedResponse = nonExistingId;

            using (var _db = new DatabaseContext())
            {
                // Act
                UserService _us = new UserService(_db);
                var response = _us.DeleteUser(nonExistingId);
                // will return null if user does not exist
                _db.SaveChanges();
                var result = _db.Users.Find(expectedResponse);

                // Assert
                Assert.IsNull(response);
                Assert.IsNull(result);
            }
        }

        [TestMethod]
        public void Update_User_Success()
        {
            // Arrange
            User newUser = tu.CreateUserInDb();
            newUser.City = "Long Beach";
            var expectedResponse = newUser;
            var expectedResult = newUser;

            // ACT
            using (var _db = tu.CreateDataBaseContext())
            {
                UserService _us = new UserService(_db);
                var response = _us.UpdateUser(newUser);
                _db.SaveChanges();
                var result = _db.Users.Find(expectedResult.Id);

                // Assert
                Assert.IsNotNull(response);
                Assert.IsNotNull(result);
                Assert.AreEqual(result.Id, expectedResult.Id);
                Assert.AreEqual(result.City, expectedResult.City);
            }

        }

        [TestMethod]
        public void Update_User_NonExisting_why()
        {
            // Arrange
            User newUser = tu.CreateUserObject();
            newUser.City = "Long Beach";
            var expectedResponse = newUser;
            var expectedResult = newUser;

            // ACT
            using (var _db = tu.CreateDataBaseContext())
            {
                UserService _us = new UserService(_db);
                var response = _us.UpdateUser(newUser);
                try
                {
                    _db.SaveChanges();
                }
                catch (Exception)
                {
                    // catch error
                    // rollback changes
                    _db.Entry(newUser).State = System.Data.Entity.EntityState.Detached;
                }
                var result = _db.Users.Find(expectedResult.Id);

                // Assert
                Assert.IsNotNull(response);
                Assert.IsNull(result);
            }
        }

        [TestMethod]
        public void Update_User_OnRequiredValue()
        {
            // Arrange
            User newUser = tu.CreateUserInDb();
            var expectedResult = newUser;
            newUser.PasswordHash = null;
            var expectedResponse = newUser;

            // ACT
            using (var _db = tu.CreateDataBaseContext())
            {
                UserService _us = new UserService(_db);
                var response = _us.UpdateUser(newUser);
                try
                {
                    _db.SaveChanges();
                }
                catch (DbEntityValidationException ex)
                {
                    // catch error
                    // rollback changes
                    _db.Entry(response).CurrentValues.SetValues(_db.Entry(response).OriginalValues);
                    _db.Entry(response).State = System.Data.Entity.EntityState.Unchanged;
                }
                var result = _db.Users.Find(expectedResult.Id);

                // Assert
                Assert.IsNotNull(response);
                Assert.AreEqual(expectedResponse, response);
                Assert.IsNotNull(result);
                Assert.AreEqual(expectedResult, result);
            }
        }

        [TestMethod]
        public void Get_User_Success()
        {
            // Arrange 
            User newUser = tu.CreateUserInDb();
            var expectedResult = newUser;

            // ACT
            using (var _db = tu.CreateDataBaseContext())
            {
                UserService _us = new UserService(_db);
                var result = _us.GetUser(expectedResult.Id);

                // Assert
                Assert.IsNotNull(result);
                Assert.AreEqual(expectedResult.Id, result.Id);
            }
        }

        [TestMethod]
        public void Get_User_NonExisting()
        {
            // Arrange
            Guid nonExistingUser = Guid.NewGuid();
            User expectedResult = null;

            // Act
            using (var _db = tu.CreateDataBaseContext())
            {
                UserService _us = new UserService(_db);
                var result = _us.GetUser(nonExistingUser);

                // Assert
                Assert.IsNull(result);
                Assert.AreEqual(expectedResult, result);
            }
        }
    }
}
