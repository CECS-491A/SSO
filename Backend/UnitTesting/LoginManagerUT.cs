using System;
using ManagerLayer.UserManagement;
using ManagerLayer.Login;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using DataAccessLayer.Models;

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
            user = um.GetUser("new@csulb.edu");
            //CreateUser("new@csulb.edu", "passwordtest54321", new DateTime(1996, 12, 15));
            request = new LoginRequest();
            request.email = "new@csulb.edu";
            request.password = "passwordtest54321";
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
            Assert.AreEqual(false, result);
        }

        [TestMethod]
        public void LoginCheckUserDisabled_Fail_ReturnFalse()
        {
            user.Disabled = true;
            bool result = lm.LoginCheckUserDisabled(request);
            Assert.AreNotEqual(true, result);
        }

        //LoginCheckPassword

        [TestMethod]
        public void LoginCheckPassword_Success_ReturnTrue()
        {
            bool result = lm.LoginCheckPassword(request);
            Console.WriteLine(user.IncorrectPasswordCount);
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
        }

        [TestMethod]
        public void LoginCheckPassword_Fail_ReturnFalse()
        {
            bool result = lm.LoginCheckPassword(request);
            Assert.AreNotEqual(false, result);
        }
        [TestMethod]
        public void test()
        {
            Console.WriteLine(user.IncorrectPasswordCount);
            request.password = "pass";
            lm.LoginCheckPassword(request);
            Console.WriteLine(user.IncorrectPasswordCount);
            Assert.AreNotEqual(0, user.IncorrectPasswordCount);

            request.password = "passwordtest54321";
            bool r = lm.LoginCheckPassword(request);
            Assert.AreEqual(true, r);
        }

        // LoginAuthorized()

        [TestMethod]
        public void LoginAuthorized_Success_ReturnToken()
        {
            string result = lm.LoginAuthorized(request);
            Assert.IsNotNull(result);
        }
    }
}
