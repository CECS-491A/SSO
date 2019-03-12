﻿using System;
using ManagerLayer.UserManagement;
using ManagerLayer.Login;
using DataAccessLayer.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace UnitTesting
{

    [TestClass]
    public class LoginManagerUT
    {

        LoginManager lm;
        UserManagementManager um;
        User user;
        LoginRequest request;

        public LoginManagerUT()
        {
            lm = new LoginManager();
            um = new UserManagementManager();
            user = um.CreateUser("rrrcf2080@gmail.com", "qwerty12345", new DateTime(1996, 12, 15));
            request = new LoginRequest();
            request.email = "rrrcf2080@gmail.com";
            request.password = "qwerty12345";

        }

        //LoginCheckUserExists()

        [TestMethod]
        public void LoginCheckUserExists_Success_ReturnTrue()
        {
            bool result = lm.LoginCheckUserExists(request);
            Assert.AreEqual(true, result);
        }
        
        [TestMethod]
        public void LoginCheckUserExists_Fail_ReturnTrue()
        {
            request.email = "doesnotexist@gmail.com";
            bool result = lm.LoginCheckUserExists(request);
            Assert.AreNotEqual(true, result);
        }

        [TestMethod]
        public void LoginCheckUserExists_Success_ReturnFalse()
        {
            request.email = "doesnotexist@gmail.com";
            bool result = lm.LoginCheckUserExists(request);
            Assert.AreEqual(false, result);
        }

        [TestMethod]
        public void LoginCheckUserExists_Fail_ReturnFalse()
        {
            bool result = lm.LoginCheckUserExists(request);
            Assert.AreNotEqual(false, result);
        }

        //LoginCheckUserDisabled()

        [TestMethod]
        public void LoginCheckUserDisabled_Success_ReturnTrue()
        {
            user.Disabled = true;
            bool result = lm.LoginCheckUserDisabled(request);
            Assert.AreEqual(true, result);
        }       
        
        [TestMethod]
        public void LoginCheckUserDisabled_Fail_ReturnTrue()
        {
            um.EnableUser(user);
            bool result = lm.LoginCheckUserDisabled(request);
            Assert.AreNotEqual(true, result);
        }

        [TestMethod]
        public void LoginCheckUserDisabled_Success_ReturnFalse()
        {
            user.Disabled = false;
            bool result = lm.LoginCheckUserDisabled(request);
            Assert.AreEqual(true, result);
        }

        [TestMethod]
        public void LoginCheckUserDisabled_Fail_ReturnFalse()
        {
            user.Disabled = true;
            bool result = lm.LoginCheckUserDisabled(request);
            Assert.AreNotEqual(false, result);
        }
        
        //LoginCheckPassword
        
        [TestMethod]
        public void LoginCheckPassword_Success_ReturnTrue()
        {
            bool result = lm.LoginCheckPassword(request);
            Assert.AreEqual(true, result);
        }
        
        [TestMethod]
        public void LoginCheckPassword_Fail_ReturnTrue()
        {
            request.password = "pass";
            bool result = lm.LoginCheckPassword(request);
            Assert.AreNotEqual(true, result);
        }

        [TestMethod]
        public void LoginCheckPassword_Success_ReturnFalse()
        {
            request.password = "pass";
            bool result = lm.LoginCheckPassword(request);
            Console.WriteLine(user.IncorrectPasswordCount);
            Assert.AreEqual(false, result);
            Assert.AreNotEqual(0, user.IncorrectPasswordCount);
        }
        [TestMethod]
        public void test()
        {
            user = um.CreateUser("rrrcf2080@gmail.com", "qwerty12345", new DateTime(1996, 12, 15));
            Console.WriteLine(user.IncorrectPasswordCount);
            request.password = "pass";
            lm.LoginCheckPassword(request);
            lm.LoginCheckPassword(request);
            lm.LoginCheckPassword(request);
            Console.WriteLine(user.IncorrectPasswordCount);
            Assert.AreEqual(0, user.IncorrectPasswordCount);
        }

        [TestMethod]
        public void LoginCheckPassword_Fail_ReturnFalse()
        {
            bool result = lm.LoginCheckPassword(request);
            Assert.AreNotEqual(false, result);
        }
        
        // LoginAuthorized()

        [TestMethod]
        public void LoginAuthorized_Success_ReturnToken()
        {
            
            lm.LoginAuthorized();
        }

        [TestMethod]
        public void LoginAuthorized_Fail_ReturnNull()
        {

        }
    }
}