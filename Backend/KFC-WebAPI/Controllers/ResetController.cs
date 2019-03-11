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

        public class NewPassword
        {
            public string newPassword;
        }
        
        //After the user fills in the field with email, this action gets called
        [HttpPost]
        [Route("api/reset/send")]
        public HttpResponseMessage SendResetEmail([FromBody]string email)
        {
            if (email != null)
            {
                PasswordManager pm = new PasswordManager();
                
                string url = "kfcsso.com/api/reset/";

                pm.SendResetToken(email, url);
                var response = new HttpResponseMessage(HttpStatusCode.OK)
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
        public HttpResponseMessage Get(string resetToken)
        {
            PasswordManager pm = new PasswordManager();
            if (pm.CheckPasswordResetValid(resetToken))
            {
                return Request.CreateResponse(HttpStatusCode.OK, JsonConvert.SerializeObject(pm.GetSecurityQuestions(resetToken));
            }
            return Request.CreateResponse(HttpStatusCode.Unauthorized, "Reset link is no longer valid");
        }

        [HttpPost]
        [Route("api/reset/{resetToken}/checkanswers")]
        public HttpResponseMessage CheckAnswers(string resetToken, [FromBody] SecurityAnswerRequest request)
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
                    return Request.CreateResponse(HttpStatusCode.OK, true);
                }
                else
                {
                    return Request.CreateResponse(HttpStatusCode.Unauthorized, false);
                }
            }
            return Request.CreateResponse(HttpStatusCode.Unauthorized, "Reset link is no longer valid");
        }

        [HttpPost]
        [Route("api/reset/{resetToken}/resetpassword")]
        public HttpResponseMessage ResetPassword(string resetToken, [FromBody] NewPassword submittedPassword)
        {
            PasswordManager pm = new PasswordManager();
            if (pm.CheckPasswordResetValid(resetToken))
            {
                if (pm.CheckIfPasswordResetAllowed(resetToken))
                {
                    string newPassword = submittedPassword.newPassword;
                    if (!pm.CheckIsPasswordPwned(newPassword))
                    {
                        string newPasswordHashed = pm.SaltAndHashPassword(newPassword);
                        return Request.CreateResponse(HttpStatusCode.OK, pm.UpdatePassword(resetToken, newPasswordHashed));
                    }
                    return Request.CreateResponse(HttpStatusCode.BadRequest, "Password has been pwned, please use a different password");
                }
            }
            return Request.CreateResponse(HttpStatusCode.Unauthorized, "Reset link is no longer valid");
        }
        
    }
}
