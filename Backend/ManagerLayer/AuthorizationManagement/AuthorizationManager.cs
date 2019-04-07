﻿using DataAccessLayer.Database;
using DataAccessLayer.Models;
using ServiceLayer.Services;
using System;
using System.Data.Entity.Infrastructure;
using System.Security.Cryptography;

namespace ManagerLayer
{
    public class AuthorizationManager : IAuthorizationManager
    {
        private ISessionService _sessionService;

        public AuthorizationManager()
        {
             _sessionService = new SessionService();
        }

        public string GenerateSessionToken()
        {
            RNGCryptoServiceProvider provider = new RNGCryptoServiceProvider();
            Byte[] b = new byte[64 / 2];
            provider.GetBytes(b);
            return BitConverter.ToString(b).Replace("-", "").ToLower();
        }

        public Session CreateSession(DatabaseContext _db, User user)
        {
            Session session = new Session();
            session.Token = GenerateSessionToken();
            session.UserId = user.Id;

            var response = _sessionService.CreateSession(_db, session);

            return session;
        }

        public Session ValidateAndUpdateSession(DatabaseContext _db, string token)
        {
            Session response = _sessionService.GetSession(_db, token);

			if (response.ExpiresAt > DateTime.UtcNow)
			{
				return _sessionService.UpdateSession(_db, response);
			}
			else 
			{
				try
				{
					_sessionService.DeleteSession(_db, token);
					_db.SaveChanges();
					return null;
				}
				catch (DbUpdateException ex)
				{
					if (ex.InnerException == null)
					{
						throw;
					}
					else
					{
						//Log?
						var decodeErrors = ex.Entries;
						return null;
					}

				}
			
			}
		}

		public Session DeleteSession(DatabaseContext _db, string token)
        {
            return _sessionService.DeleteSession(_db, token);
        }
    }
}
