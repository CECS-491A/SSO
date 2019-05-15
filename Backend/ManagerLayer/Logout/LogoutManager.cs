using System;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using DataAccessLayer.Models;
using DataAccessLayer.Database;
using ServiceLayer.Services;
using System.Linq;
using Newtonsoft.Json;
using System.Collections.Generic;


namespace ManagerLayer.Logout
{



    public class LogoutManager
    {
        DatabaseContext _db;
        UserService userService;
        SessionService sessionServ;
        LogoutService logoutServ;

        public LogoutManager(DatabaseContext _db)
        {
            this._db = _db;
            userService = new UserService(_db);
            _applicationService = new ApplicationService(_db);
            sessionServ = new SessionService(_db);
            logoutServ = new LogoutService();

        }
        IApplicationService _applicationService;
        public void SendLogoutRequest(string token)
        {
            
            var applist = _applicationService.GetAllApplicationsList();
            Session session = sessionServ.GetSession(token);
            User user = userService.GetUser(session.UserId);


            foreach (Application app in applist)
            {

                //The logoutpayload will have a dictonary that represents signed body of request.
                var logoutPayload = new Dictionary<string, string>();
                logoutPayload.Add("ssoUserId", session.UserId.ToString());
                logoutPayload.Add("email", user.Email);
                logoutPayload.Add("timestamp", DateTimeOffset.Now.ToUnixTimeMilliseconds().ToString());

                //This signs the dictonary and adds it back to the payload.
                var signatureService = new SignatureService();
                var signature = signatureService.Sign(app.SharedSecretKey, logoutPayload);
                logoutPayload.Add("signature", signature);

                //sends it to each application logout URL
                logoutServ.LogoutRequest(app.LogoutUrl, logoutPayload);
                
            }
            
        }

    }
}