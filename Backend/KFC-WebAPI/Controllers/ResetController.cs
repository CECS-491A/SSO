using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Web.Http;
using ServiceLayer.Services;
using DataAccessLayer.Models;
using ManagerLayer.PasswordManagement;
using ManagerLayer.UserManagement;
using Newtonsoft.Json;

namespace WebAPI.Controllers
{
    public class ResetController : ApiController
    {
        public class SecurityAnswerRequest
        {
            public string securityA1;
            public string securityA2;
            public string securityA3;
        }

        //IHttpActionResult
        //After the user fills in the field with email, this action gets called
        [HttpPost]
        [Route("api/reset/send")]
        public HttpResponseMessage SendResetEmail([FromBody]string email)
        {
            if (email != null)
            {
                PasswordManager pm = new PasswordManager();

                string url = Request.RequestUri.ToString();

                pm.SendResetToken(email, url);
                var response = new HttpResponseMessage(HttpStatusCode.Created)
                {
                    Content = new StringContent("Email received")
                };
                return response;
            }
            else
            {
                return Request.CreateResponse(HttpStatusCode.BadRequest);
            }

        }

        //After the user clicks the link in the email, this action gets called and takes the resetToken that's appended to the URL that was sent to the user
        [HttpGet]
        [Route("api/reset/{resetToken}")]
        public string Get(string resetToken)
        {
            PasswordManager pm = new PasswordManager();
            if (pm.CheckPasswordResetValid(resetToken))
            {
                return JsonConvert.SerializeObject(pm.GetSecurityQuestions(resetToken));
            }else
            {
                return null;
            }
        }

        [HttpPost]
        [Route("api/reset/{resetToken}/checkanswers")]
        public bool CheckAnswers(string resetToken, [FromBody] SecurityAnswerRequest request)
        {
            PasswordManager pm = new PasswordManager();
            if (pm.CheckPasswordResetValid(resetToken))
            {
                List<string> userSubmittedSecurityAnswer = new List<string>
                {
                    request.securityA1,
                    request.securityA2,
                    request.securityA3
                };
                if (pm.CheckSecurityAnswers(resetToken, userSubmittedSecurityAnswer))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            return false;
        }

        [HttpPost]
        [Route("api/reset/{resetToken}/resetpassword")]
        public bool ResetPassword(string resetToken, [FromBody] string newPassword)
        {
            PasswordManager pm = new PasswordManager();
            if (pm.CheckPasswordResetValid(resetToken))
            {
                if (pm.CheckIfPasswordResetAllowed(resetToken))
                {
                    string newPasswordHashed = pm.SaltAndHashPassword(newPassword);
                    return pm.UpdatePassword(resetToken, newPasswordHashed);
                }
            }
            return false;
        }
        
    }
}
