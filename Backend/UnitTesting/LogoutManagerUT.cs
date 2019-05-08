using System;
using DataAccessLayer.Database;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ManagerLayer.Logout;
using System;

namespace UnitTesting
{
    [TestClass]
    public class LogoutManagerUT
    {
        TestingUtils _tu;

        public LogoutManagerUT()
        {
            _tu = new TestingUtils();
        }
        

        [TestMethod]
        public void Logout_Application_Success()
        {
            var user = _tu.CreateUserInDb();
            var session = _tu.CreateSessionInDb(user);
            var token = session.Token;

            //Act
            using (var _db = _tu.CreateDataBaseContext())
            {
                var application = _tu.CreateApplicationInDb();
                var _logoutM = new LogoutManager(_db);
                var applicationLogout = _logoutM.SendLogoutRequest(token);

                _db.SaveChanges();

                //Assert
                Assert.IsNotNull(applicationLogout);
                

                
            }
        }

        [TestMethod]
        public void Logout_Application_Failure()
        {
            var user = _tu.CreateUserInDb();
            var session = _tu.CreateSessionInDb(user);
            var token = session.Token;
            using (var _db = _tu.CreateDataBaseContext())
            {
                var _logoutM = new LogoutManager(_db);
                var applicationLogout = _logoutM.SendLogoutRequest(token);
                _db.SaveChanges();


            }
        }
    }
}
