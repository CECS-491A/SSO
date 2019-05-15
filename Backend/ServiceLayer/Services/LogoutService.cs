using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Net.Http;
using Newtonsoft.Json;
using System.Runtime.Serialization;
using DataAccessLayer.Database;
using DataAccessLayer.Models;
using DataAccessLayer.Repositories;
using System.Security.Cryptography;

namespace ServiceLayer.Services
{



    public class LogoutService
    {
        public async void LogoutRequest(string logoutUrl, Dictionary<string, string> payload)
        {
            HttpClient client = new HttpClient();

            //This converts payload to JSON and sends the request   
            var stringPayload = JsonConvert.SerializeObject(payload);
            var jsonPayload = new StringContent(stringPayload, Encoding.UTF8, "application/json");
            var request = await client.PostAsync(logoutUrl, jsonPayload);
            
        }

    }
}