﻿using DataAccessLayer.Database;
using DataAccessLayer.Models;
using ServiceLayer.Services;
using System;
using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;
using System.Text;

namespace ManagerLayer.LaunchManagement
{
    public class LaunchPayload
    {
        // Included in signature
        public Guid ssoUserId { get; set; }
        public string email { get; set; }
        public long timestamp { get; set; }

        // Excluded from signature
        public string signature { get; set; }
        public string url { get; set; }

        // Generate string to be signed
        public string PreSignatureString()
        {
            string acc = "";
            acc += "ssoUserId=" + ssoUserId + ";";
            acc += "email=" + email + ";";
            acc += "timestamp=" + timestamp + ";";
            return acc;
        }
    }

    public class LaunchManager : ILaunchManager
    {
        public LaunchPayload SignLaunch(DatabaseContext _db, Session session, Guid appId)
        {
            Application app = ApplicationService.GetApplication(_db, appId);

            if (app == null)
            {
                throw new ArgumentException();
            }

            IUserService userService = new UserService();
            User user = userService.GetUser(_db, session.UserId);

            LaunchPayload launchPayload = new LaunchPayload
            {
                ssoUserId = session.UserId,
                email = user.Email,
                timestamp = DateTimeOffset.Now.ToUnixTimeMilliseconds(),
                url = app.LaunchUrl
            };

            HMACSHA256 hmacsha1 = new HMACSHA256(Encoding.ASCII.GetBytes(app.SharedSecretKey));

            byte[] launchPayloadBuffer = Encoding.ASCII.GetBytes(launchPayload.PreSignatureString());

            byte[] signatureBytes = hmacsha1.ComputeHash(launchPayloadBuffer);

            launchPayload.signature = Convert.ToBase64String(signatureBytes);

            return launchPayload;
        }
    }
}
