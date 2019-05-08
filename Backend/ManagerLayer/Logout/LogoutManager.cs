using System;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using DataAccessLayer.Models;
using DataAccessLayer.Database;
using ServiceLayer.Services;
using ServiceLayer.Exceptions;
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
        LogoutService logservice;

        public LogoutManager(DatabaseContext _db)
        {
            this._db = _db;
            userService = new UserService(_db);
            _applicationService = new ApplicationService(_db);
            sessionServ = new SessionService(_db);
            logservice = new LogoutService();

        }
        IApplicationService _applicationService;
        public async Task<Session> SendLogoutRequest(string token)
        {
            var responseList = new List<HttpResponseMessage>();
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

                //This converts payload to JSON and sends it to each application logout URL.
                var stringPayload = JsonConvert.SerializeObject(logoutPayload);
                var jsonPayload = new StringContent(stringPayload, Encoding.UTF8, "application/json");
                var request = await logservice.LogoutRequest(app.LogoutUrl, logoutPayload);
                responseList.Add(request);
            }
            if (responseList.All(response => response.IsSuccessStatusCode || response.StatusCode == System.Net.HttpStatusCode.NotFound))
            {
                return session;
            }
            else
            {
                return null;
            }
        }

    }
}