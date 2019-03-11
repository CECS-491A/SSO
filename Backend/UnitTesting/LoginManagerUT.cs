﻿using System;
using ManagerLayer.UserManagement;
using ManagerLayer.Login;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTesting
{

    [TestClass]
    public class LoginManagerUT
    {

        LoginManager lm;
        UserManagementManager um;

        public LoginManagerUT()
        {
            lm = new LoginManager();
            um = new UserManagementManager();
        }

        //LoginCheckUserExists()

        [TestMethod]
        public void LoginCheckUserExists_Success_ReturnTrue()
        {
            var username = Guid.NewGuid() + "@" + Guid.NewGuid() + ".com";
            var password = Guid.NewGuid().ToString();
            um.CreateUser(username, password, new DateTime(1996, 12, 15));
            bool result = lm.LoginCheckUserExists(username);
            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public void LoginCheckUserExists_Fail_ReturnTrue()
        {

        }

        [TestMethod]
        public void LoginCheckUserExists_Success_ReturnFalse()
        {

        }

        [TestMethod]
        public void LoginCheckUserExists_Fail_ReturnFalse()
        {

        }

        //LoginCheckUserDisabled()

        [TestMethod]
        public void LoginCheckUserDisabled_Success_ReturnTrue()
        {

        }

        [TestMethod]
        public void LoginCheckUserDisabled_Fail_ReturnTrue()
        {

        }

        [TestMethod]
        public void LoginCheckUserDisabled_Success_ReturnFalse()
        {

        }

        [TestMethod]
        public void LoginCheckUserDisabled_Fail_ReturnFalse()
        {

        }

        //LoginCheckPassword

        [TestMethod]
        public void LoginCheckPassword_Success_ReturnTrue()
        {

        }

        [TestMethod]
        public void LoginCheckPassword_Fail_ReturnTrue()
        {

        }

        [TestMethod]
        public void LoginCheckPassword_Success_ReturnFalse()
        {

        }

        [TestMethod]
        public void LoginCheckPassword_Fail_ReturnFalse()
        {

        }

        // LoginAuthorized()

        [TestMethod]
        public void LoginAuthorized_Success_ReturnToken()
        {

        }

        [TestMethod]
        public void LoginAuthorized_Fail_ReturnToken()
        {

        }
    }
}