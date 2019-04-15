﻿using System;
using ServiceLayer.Services;
using DataAccessLayer.Database;
using DataAccessLayer.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Data.Entity.Validation;

namespace UnitTesting
{
    /// <summary>
    /// Tests ApiKey Services
    /// </summary>
    [TestClass]
    public class ApiKeyServiceUT
    {
        DatabaseContext _db;
        TestingUtils tu;
        ApiKey newKey;
        IApiKeyService _apiKeyService;

        public ApiKeyServiceUT()
        {
            _db = new DatabaseContext();
            tu = new TestingUtils();
            _apiKeyService = new ApiKeyService();
        }


        #region Additional test attributes
        //
        // You can use the following additional attributes as you write your tests:
        //
        // Use ClassInitialize to run code before running the first test in the class
        // [ClassInitialize()]
        // public static void MyClassInitialize(TestContext testContext) { }
        //
        // Use ClassCleanup to run code after all tests in a class have run
        // [ClassCleanup()]
        // public static void MyClassCleanup() { }
        //
        // Use TestInitialize to run code before running each test 
        // [TestInitialize()]
        // public void MyTestInitialize() { }
        //
        // Use TestCleanup to run code after each test has run
        // [TestCleanup()]
        // public void MyTestCleanup() { }
        //
        #endregion

        [TestMethod]
        public void CreateKey_Pass_ReturnKey()
        {
            // Arrange
            newKey = tu.CreateApiKeyObject();
            var expected = newKey;

            using (_db = tu.CreateDataBaseContext())
            {
                // Act
                var response = _apiKeyService.CreateKey(_db, newKey);
                _db.SaveChanges();

                // Assert
                Assert.IsNotNull(response);
                Assert.AreEqual(response.Id, expected.Id);

                _apiKeyService.DeleteKey(_db, newKey.Id);
                _db.SaveChanges();
            }
        }

        [TestMethod]
        public void CreateKey_Fail_ExistingKeyShouldReturnNull()
        {
            // Arrange
            newKey = tu.CreateApiKeyObject();
            var expected = newKey;

            using (_db = tu.CreateDataBaseContext())
            {
                // Act
                var response = _apiKeyService.CreateKey(_db, newKey);
                _db.SaveChanges();

                var actual = _apiKeyService.CreateKey(_db, newKey);

                // Assert
                Assert.IsNull(actual);
                Assert.AreNotEqual(expected, actual);

                _apiKeyService.DeleteKey(_db, newKey.Id);
                _db.SaveChanges();
            }
        }

        [TestMethod]
        public void CreateKey_Fail_NullValuesShouldReturnNullReferenceException()
        {
            bool expected = true;

            using (_db = tu.CreateDataBaseContext())
            {
                try
                {
                    // Act
                    var result = _apiKeyService.CreateKey(_db, null);
                }
                catch (NullReferenceException)
                {
                    expected = false;
                }

                // Assert
                Assert.IsFalse(expected);
            }
        }

        [TestMethod]
        public void DeleteKey_Pass_ReturnKey()
        {
            // Arrange
            newKey = tu.CreateApiKeyObject();

            using (_db = tu.CreateDataBaseContext())
            {
                // Act
                newKey = _apiKeyService.CreateKey(_db, newKey);
                var expected = newKey;

                _db.SaveChanges();

                var response = _apiKeyService.DeleteKey(_db, newKey.Id);
                _db.SaveChanges();
                var result = _db.Keys.Find(expected.Id);

                // Assert
                Assert.IsNotNull(response);
                Assert.IsNull(result);
                Assert.AreEqual(response.Id, expected.Id);
            }
        }

        [TestMethod]
        public void DeleteKey_Fail_NonExistingKeyShouldReturnNull()
        {
            // Arrange
            Guid nonExistingId = Guid.NewGuid();

            var expected = nonExistingId;

            using (_db = new DatabaseContext())
            {
                // Act
                var response = _apiKeyService.DeleteKey(_db, nonExistingId);
                _db.SaveChanges();
                var result = _db.Keys.Find(expected);

                // Assert
                Assert.IsNull(response);
                Assert.IsNull(result);
            }
        }

        [TestMethod]
        public void UpdateKey_Pass_ReturnKey()
        {
            // Arrange
            newKey = tu.CreateApiKeyObject();
            var expected = newKey.Key;

            // Act
            using (_db = tu.CreateDataBaseContext())
            {
                newKey = _apiKeyService.CreateKey(_db, newKey);
                _db.SaveChanges();

                newKey.Key = "A new Key";
                var response = _apiKeyService.UpdateKey(_db, newKey);
                _db.SaveChanges();

                var result = _db.Keys.Find(newKey.Id);

                // Assert
                Assert.IsNotNull(response);
                Assert.IsNotNull(result);
                Assert.AreEqual(result.Id, newKey.Id);
                Assert.AreNotEqual(expected, result.Key);

                _apiKeyService.DeleteKey(_db, newKey.Id);
                _db.SaveChanges();
            }
        }

        [TestMethod]
        public void UpdateKey_Fail_NonExistingKeyShouldThrowException()
        {
            // Arrange
            newKey = tu.CreateApiKeyObject();
            var expected = newKey;

            // Act
            using (_db = tu.CreateDataBaseContext())
            {
                var response = _apiKeyService.UpdateKey(_db, newKey);
                try
                {
                    _db.SaveChanges();
                }
                catch (System.Data.Entity.Infrastructure.DbUpdateConcurrencyException)
                {
                    _db.Entry(newKey).State = System.Data.Entity.EntityState.Detached;
                }
                catch (System.Data.Entity.Core.EntityCommandExecutionException)
                {
                    _db.Entry(newKey).State = System.Data.Entity.EntityState.Detached;
                }
                var result = _db.Keys.Find(expected.Id);

                // Assert
                Assert.IsNull(response);
                Assert.IsNull(result);
            }
        }

        [TestMethod]
        public void UpdateKey_Fail_NullValuesShouldReturnNull()
        {
            bool expected = true;

            using (_db = tu.CreateDataBaseContext())
            {
                try
                {
                    // Act
                    var result = _apiKeyService.UpdateKey(_db, null);
                }
                catch (NullReferenceException)
                {
                    expected = false;
                }

                // Assert
                Assert.IsFalse(expected);
            }
        }

        [TestMethod]
        public void GetKeyById_Pass_ReturnKey()
        {
            // Arrange
            newKey = tu.CreateApiKeyObject();
            var expected = newKey;

            // Act
            using (_db = tu.CreateDataBaseContext())
            {
                newKey = _apiKeyService.CreateKey(_db, newKey);
                _db.SaveChanges();
                var result = _apiKeyService.GetKey(_db, newKey.Id);

                // Assert
                Assert.IsNotNull(result);
                Assert.AreEqual(expected.Id, result.Id);

                _apiKeyService.DeleteKey(_db, newKey.Id);
                _db.SaveChanges();
            }
        }

        [TestMethod]
        public void GetApiKeyById_Fail_NonExistingKeyShouldReturnNull()
        {
            // Arrange
            Guid nonExistingKey = Guid.NewGuid();
            ApiKey expected = null;

            // Act
            using (_db = tu.CreateDataBaseContext())
            {
                var result = _apiKeyService.GetKey(_db, nonExistingKey);

                // Assert
                Assert.IsNull(result);
                Assert.AreEqual(expected, result);
            }
        }

        [TestMethod]
        public void GetApiKeyByKey_Pass_ReturnKey()
        {
            // Arrange
            newKey = tu.CreateApiKeyObject();
            var expected = newKey;

            // Act
            using (_db = tu.CreateDataBaseContext())
            {
                newKey = _apiKeyService.CreateKey(_db, newKey);
                _db.SaveChanges();
                var result = _apiKeyService.GetKey(_db, newKey.Key);

                // Assert
                Assert.IsNotNull(result);
                Assert.AreEqual(expected.Key, result.Key);

                _apiKeyService.DeleteKey(_db, newKey.Id);
                _db.SaveChanges();
            }
        }

        [TestMethod]
        public void GetApiKeyByKey_Fail_NonExistingKeyShouldReturnNull()
        {
            // Arrange
            string nonExistingKey = "key";
            ApiKey expected = null;

            // Act
            using (_db = tu.CreateDataBaseContext())
            {
                var result = _apiKeyService.GetKey(_db, nonExistingKey);

                // Assert
                Assert.IsNull(result);
                Assert.AreEqual(expected, result);
            }
        }

        [TestMethod]
        public void GetApiKeyByKey_Fail_NullValuesShouldReturnNull()
        {
            using (_db = tu.CreateDataBaseContext())
            {
                // Act
                var result = _apiKeyService.GetKey(_db, null);

                // Assert
                Assert.IsNull(result);
            }
        }

        [TestMethod]
        public void GetApiKeyByAppIdIsUsed_Pass_ReturnKey()
        {
            // Arrange
            newKey = tu.CreateApiKeyObject();
            var expected = newKey;

            // Act
            using (_db = tu.CreateDataBaseContext())
            {
                newKey = _apiKeyService.CreateKey(_db, newKey);
                _db.SaveChanges();
                var result = _apiKeyService.GetKey(_db, newKey.ApplicationId, false);

                // Assert
                Assert.IsNotNull(result);
                Assert.AreEqual(expected.Key, result.Key);

                _apiKeyService.DeleteKey(_db, newKey.Id);
                _db.SaveChanges();
            }
        }

        [TestMethod]
        public void GetApiKeyByAppIdIsUsed_Fail_NonExistingKeyShouldReturnNull()
        {
            // Arrange
            Guid id = Guid.NewGuid();
            ApiKey expected = null;

            // Act
            using (_db = tu.CreateDataBaseContext())
            {
                var result = _apiKeyService.GetKey(_db, id, false);

                // Assert
                Assert.IsNull(result);
                Assert.AreEqual(expected, result);
            }
        }
    }
}
