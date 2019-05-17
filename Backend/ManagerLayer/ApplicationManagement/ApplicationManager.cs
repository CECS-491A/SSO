using DataAccessLayer.Database;
using DataAccessLayer.Models;
using DataAccessLayer.RequestModels;
using MimeKit;
using ServiceLayer.Exceptions;
using ServiceLayer.Services;
using System;
using System.Data.Entity.Validation;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net;

namespace ManagerLayer.ApplicationManagement
{
    public class ApplicationManager : IApplicationManager
    {
        private DatabaseContext _db;

        public ApplicationManager(DatabaseContext _db)
        {
            this._db = _db;
            _applicationService = new ApplicationService(_db);
            _apiKeyService = new ApiKeyService(_db);
            _emailService = new EmailService();
            _tokenService = new TokenService();
        }

        // Business Rules
        private const int titleLength = 100;
        private const int descriptionLength = 2000;
        private const int xDimension = 55;
        private const int yDimension = 55;
        private const string imageType = ".PNG";
        private const int urlLength = 500;

        // Services
        private IApplicationService _applicationService;
        private IApiKeyService _apiKeyService;
        private IEmailService _emailService;
        private ITokenService _tokenService;

        /// <summary>
        /// Validate the app registration request values, and call registration services
        /// </summary>
        /// <param name="request">Values from Register POST request</param>
        /// <returns>Http status code and content</returns>
        public HttpResponseContent ValidateRegistration(ApplicationRequest request)
        {
            HttpResponseContent response; // Body of http response content

            Uri launchUrl = null;
            Uri deleteUrl = null;
            Uri healthCheckUrl = null;
            Uri logoutUrl = null;

            // Validate request values
            if (request.Title == null || !IsValidStringLength(request.Title, titleLength))
            {
                throw new InvalidStringException("Invalid Title: Length cannot be greater than " + titleLength + " characters");
            }
            else if (request.Email == null || !IsValidEmail(request.Email))
            {
                throw new InvalidEmailException("Invalid Email Format");
            }
            else if (request.LaunchUrl == null || !IsValidUrl(request.LaunchUrl, ref launchUrl) || !IsValidStringLength(request.LaunchUrl, urlLength))
            {
                throw new InvalidUrlException("Invalid Launch Url Format");
            }
            else if (request.DeleteUrl == null || !IsValidUrl(request.DeleteUrl, ref deleteUrl) || !IsValidStringLength(request.DeleteUrl, urlLength))
            {
                throw new InvalidUrlException("Invalid User Deletion Url Format");
            }
            else if (request.HealthCheckUrl == null || !IsValidUrl(request.HealthCheckUrl, ref healthCheckUrl) || !IsValidStringLength(request.HealthCheckUrl, urlLength))
            {
                throw new InvalidUrlException("Invalid Health Check Url Format");
            }
            else if (request.LogoutUrl == null || !IsValidUrl(request.LogoutUrl, ref logoutUrl) || !IsValidStringLength(request.LogoutUrl, urlLength))
            {
                throw new InvalidUrlException("Invalid Logout Url Format");
            }

            // Create application from request data
            Application app = new Application
            {
                Title = request.Title,
                LaunchUrl = launchUrl.ToString(),
                Email = request.Email,
                UserDeletionUrl = deleteUrl.ToString(),
                HealthCheckUrl = healthCheckUrl.ToString(),
                SharedSecretKey = _tokenService.GenerateToken(),
                LogoutUrl = logoutUrl.ToString()
            };

            // Create a new api key for application
            ApiKey apiKey = new ApiKey
            {
                // Generate a unique key
                Key = _tokenService.GenerateToken(),
                ApplicationId = app.Id
            };
            
            // Add the api key to the application
            app.ApiKeys.Add(apiKey);

            // Create an Application entry
            var appResponse = _applicationService.CreateApplication(app);
            if (appResponse == null) // Application was not created
            {
                throw new ArgumentException("Application Already Exists.");
            }
            
            // Save data store changes
            SaveChanges(appResponse);

            string message;

            // Email the application ID, api key, and shared secret key
            try
            {
                SendAppRegistrationEmail(app.Email, apiKey.Key, app.SharedSecretKey, app.Id);
                message = "Successful Registration!  An email has also been sent containing the following information.";
            }
            catch
            {
                message = "Successful Registration!  Please save the following information.  An email containing this information was unable to send.";
            }

            // Return success response
            response = new HttpResponseContent(message, apiKey.Key, app.SharedSecretKey, app.Id);
            return response;
        }

        /// <summary>
        /// Validate the app publish request values, and call publish services
        /// </summary>
        /// <param name="request">Values from Publish POST request</param>
        /// <returns>Http status code and content</returns>
        public HttpResponseContent ValidatePublish(ApplicationRequest request)
        {
            HttpResponseContent response; // Body of http response content

            Uri logoUrl = null;

            // Validate publish request values
            if (request.Title == null)
            {
                throw new InvalidStringException("Invalid Title: Null");
            }
            else if (!IsValidStringLength(request.Description, descriptionLength))
            {
                throw new InvalidStringException("Invalid Description: Length cannot be greater than " + descriptionLength + " characters.");
            }
            else if (!IsValidUrl(request.LogoUrl, ref logoUrl) || !IsValidStringLength(request.LogoUrl, urlLength))
            {
                throw new InvalidUrlException("Invalid Logo Url Format");
            }
            else if (!IsValidImageExtension(logoUrl, imageType))
            {
                throw new InvalidImageException("Invalid Logo Image Extension: Can only be " + imageType);
            }
            else if (!IsValidDimensions(logoUrl,xDimension,yDimension))
            {
                throw new InvalidImageException("Invalid Logo Dimensions: Can be no more than " + xDimension + "x" + yDimension + " pixels.");
            }

            // Attempt to find api key
            var apiKey = _apiKeyService.GetKey(request.Key);
            // Key does not exist
            if (apiKey == null)
            {
                throw new InvalidApiKeyException("Invalid API Key: Key Not Found");
            }
            // Key is found, but used
            else if (apiKey.IsUsed)
            {
                throw new InvalidApiKeyException("Invalid API Key: Key Has Already Been Used");
            }

            // Attempt to find application that api key belongs to
            var app = _applicationService.GetApplication(apiKey.ApplicationId);
            // App does not exist, or does not match the api key
            if(app == null || app.Title != request.Title)
            {
                throw new InvalidApiKeyException("Invalid API Key: Key and Application Do Not Match");
            }

            // Update values of application
            app.Description = request.Description;
            app.LogoUrl = request.LogoUrl;
            app.UnderMaintenance = request.UnderMaintenance;
            
            // Iterate through applications's api keys
            for (int i = 0; i < app.ApiKeys.Count; i++)
            {
                // Found api key belongs to application's collection of api keys
                if (app.ApiKeys.ElementAt(i).Id.Equals(apiKey.Id))
                {
                    // Invalidate api key
                    app.ApiKeys.ElementAt(i).IsUsed = true;

                    // Update values of application record
                    var appResponse = _applicationService.UpdateApplication(app);
                    // Application was not found
                    if (appResponse == null)
                    {
                        throw new ArgumentException("Application Could Not Be Updated");
                    }
                    
                    // Save data store changes
                    SaveChanges(appResponse);

                    string message;

                    // Email successful publish confirmation
                    try
                    {
                        SendAppPublishEmail(app.Email, app);
                        message = "Successful Publish to the KFC SSO portal!  An email has been sent confirming your publish.";
                    }
                    catch
                    {
                        message = "Successful Publish to the KFC SSO portal!  A confirmation email was unable to send.";
                    }

                    // Return successful publish response
                    response = new HttpResponseContent(message);
                    return response;
                }
            }

            // Key does not exist
            throw new InvalidApiKeyException("Invalid API Key: Key Not Found");
        }

        /// <summary>
        /// Validate the key generation request values, and call key generation services
        /// </summary>
        /// <param name="request">Values from Key Generation POST request</param>
        /// <returns>Http status code and content</returns>
        public HttpResponseContent ValidateKeyGeneration(ApplicationRequest request)
        {
            HttpResponseContent response; // Body of http response content

            // Validate key generation request values
            if (request.Title == null)
            {
                throw new InvalidStringException("Invalid Title: Null");
            }
            else if (request.Email == null || !IsValidEmail(request.Email))
            {
                throw new InvalidEmailException("Invalid Email Format");
            }

            // Attempt to find application
            var app = _applicationService.GetApplication(request.Title, request.Email);
            if (app == null)
            {
                throw new ArgumentException("Application Does Not Exist");
            }

            // Create a new ApiKey
            ApiKey apiKey = new ApiKey
            {
                Key = _tokenService.GenerateToken(),
                ApplicationId = app.Id
            };
            
            // Invalidate old api keys
            for (int i = 0; i < app.ApiKeys.Count; i++)
            {
                if (!app.ApiKeys.ElementAt(i).IsUsed)
                {
                    app.ApiKeys.ElementAt(i).IsUsed = true;
                }
            }

            // Add new api key to application
            app.ApiKeys.Add(apiKey);

            // Update the application with the changed keys
            var appResponse = _applicationService.UpdateApplication(app);
            // Application was not found
            if (appResponse == null)
            {
                throw new ArgumentException("Application Could Not Be Updated");
            }

            // Save data store changes
            SaveChanges(appResponse);

            string message;

            // Email the new api key
            try
            {
                SendNewApiKeyEmail(app.Email, apiKey.Key);
                message = "Successful Key Generation!  An email has been sent containing your new API Key.";
            }
            catch
            {
                message = "Successful Key Generation!  Please save the following information.  An email containing this information was unable to send.";
            }

            // Return successful key generation response
            response = new HttpResponseContent(message, apiKey.Key);
            return response;

        }

        /// <summary>
        /// Validate the App Deletion request values, and call deletion services
        /// </summary>
        /// <param name="request">Values from App Deletion POST request</param>
        /// <returns>Http status code and content</returns>
        public HttpResponseContent ValidateDeletion(ApplicationRequest request)
        {
            HttpResponseContent response; // Body of http response content

            // Validate deletion request values
            if (request.Title == null)
            {
                throw new InvalidStringException("Invalid Title: Null");
            }
            else if (!IsValidEmail(request.Email))
            {
                throw new InvalidEmailException("Invalid Email Format");
            }

            // Attempt to find application
            var app = _applicationService.GetApplication(request.Title, request.Email);
            if (app == null)
            {
                throw new ArgumentException("Application Does Not Exist");
            }

            // Attempt to delete application
            var appResponse = _applicationService.DeleteApplication(app.Id);
            if (appResponse == null)
            {
                throw new ArgumentException("Application Could Not Be Deleted");
            }
            
            // Save data store changes
            SaveChanges(appResponse);

            string message;

            // Email application deletion confirmation
            try
            {
                SendAppDeleteEmail(app.Email, app.Title);
                message = "Application Deleted.  An email has been sent confirming your deletion.";
            }
            catch
            {
                message = "Application Deleted.  A confirmation email was unable to send.";
            }

            // Return successful deletion response
            response = new HttpResponseContent(message);
            return response;
        }


        public HttpResponseContent ValidateUpdate(ApplicationRequest request)
        {
            // Http status code and message
            HttpResponseContent response;

            // Attempt to find application
            var app = _applicationService.GetApplication(request.Title, request.Email);
            if (app == null)
            {
                response = new HttpResponseContent("Invalid Application");
                response.Code = HttpStatusCode.BadRequest;
                return response;
            }

            // Update click count of application record

            app.ClickCount = request.ClickCount;
            var appResponse = _applicationService.UpdateApplication(app);
            
            // Attempt to save database changes
            try
            {
                SaveChanges(appResponse);
            }
            catch
            {
                // Error response
                response = new HttpResponseContent("Unable to save database changes");
                response.Code = HttpStatusCode.InternalServerError;
                return response;
            }

            // Successful publish
            response = new HttpResponseContent("Updated application from SSO");
            response.Code = HttpStatusCode.OK;
            return response;
        }

        #region InputValidation
        public bool IsValidPaginationParameters(int currentPage, int pageSize)
        {
            if (currentPage <= 0 || pageSize <=0)
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Validates a string length
        /// </summary>
        /// <param name="length">string</param>
        /// <returns></returns>
        public bool IsValidStringLength(string s, int length)
        {
            if (s == null || s.Length > length)
            {
                return false;
            }

            return true;
        }

        /// <summary>
        /// Validates email format
        /// </summary>
        /// <param name="email"></param>
        /// <returns></returns>
        public bool IsValidEmail(string email)
        {
            try
            {
                // Check for a valid email.
                var valid = new System.Net.Mail.MailAddress(email);
                return true;
            }
            catch (FormatException)
            {
                return false;
            }
            catch (ArgumentNullException)
            {
                return false;
            }
        }

        /// <summary>
        /// Validates url format
        /// </summary>
        /// <param name="url"></param>
        /// <param name="uriResult"></param>
        /// <returns></returns>
        public bool IsValidUrl(string url, ref Uri uriResult)
        {
            bool result = Uri.TryCreate(url, UriKind.Absolute, out uriResult)
                && (uriResult.Scheme == Uri.UriSchemeHttp || uriResult.Scheme == Uri.UriSchemeHttps);
            return result;
        }

        /// <summary>
        /// Validates image file type
        /// </summary>
        /// <param name="imageUrl">image url</param>
        /// <param name="ex">file type</param>
        /// <returns></returns>
        public bool IsValidImageExtension(Uri imageUrl, string type)
        {
            if(imageUrl == null)
            {
                return false;
            }
            string extension = Path.GetExtension("@" + imageUrl.ToString());

            // Logo can only be of .PNG image file type.
            if (!extension.ToUpper().Equals(type))
            {
                return false;
            }
            return true;
        }

        /// <summary>
        /// Validates image dimensions
        /// </summary>
        /// <param name="imgUrl">image url</param>
        /// <param name="width"></param>
        /// <param name="height"></param>
        /// <returns></returns>
        public bool IsValidDimensions(Uri imgUrl, int width, int height)
        {
            if(imgUrl == null)
            {
                return false;
            }

            // Download image
            WebClient wc = new WebClient();
            byte[] bytes = wc.DownloadData(imgUrl);
            MemoryStream ms = new MemoryStream(bytes);
            Image img = Image.FromStream(ms);

            // Get dimensions
            int x, y;
            x = img.Width;
            y = img.Height;
            
            if(x > width || y > height)
            {
                return false;
            }
            return true;
        }
        #endregion

        /// <summary>
        /// Saves the changes made to the data store
        /// </summary>
        /// <param name="change"></param>
        public void SaveChanges(object change)
        {
            try
            {
                // Save changes in the database
                _db.SaveChanges();
            }
            catch (DbEntityValidationException)
            {
                // Catch error
                // Detach item attempted to be changed from the db context - rollback
                _db.Entry(change).State = System.Data.Entity.EntityState.Detached;

                // Error
                throw new DbEntityValidationException("Cannot Save Database Changes");
            }
        }

        #region Emails

        /// <summary>
        /// Creates an email to send the application id, api key,
        /// and shared secret key to newly registered application.
        /// </summary>
        /// <param name="receiverEmail"></param>
        /// <param name="apiKey"></param>
        /// <param name="sharedSecretKey"></param>
        /// <param name="appId"></param>
        public void SendAppRegistrationEmail(string receiverEmail, string apiKey, string sharedSecretKey, Guid appId)
        {
            string appRegisterSubjectString = "KFC SSO: Application Registration";
            string userFullName = receiverEmail;
            string template = "Hi, \r\n \r\n" +
                                             "You recently registered your application to the KFC SSO portal.\r\n \r\n" +
                                             "Your Application ID:\r\n {0} \r\n \r\n" +
                                             "A single-use API Key to publish your application into the portal:\r\n {1} \r\n \r\n" +
                                             "The Shared Secret Key between your application and the KFC SSO Portal.\r\n {2} \r\n \r\n" +
                                             "If you did not register your application to KFC, please contact us by responding to this email.\r\n\r\n" +
                                             "Thanks, \r\nKFC Team";

            string appRegisterBodyString = string.Format(template, appId, apiKey, sharedSecretKey);

            //Create the message that will be sent
            MimeMessage emailToSend = _emailService.CreateEmailPlainBody(userFullName, receiverEmail, appRegisterSubjectString, appRegisterBodyString);
            //Send the email with the message
            _emailService.SendEmail(emailToSend);
        }

        /// <summary>
        /// Creates an email to send a confirmation of app publish.
        /// </summary>
        /// <param name="receiverEmail"></param>
        /// <param name="app"></param>
        public void SendAppPublishEmail(string receiverEmail, Application app)
        {
            string appPublishSubjectString = "KFC SSO: Application Publish";
            string userFullName = receiverEmail;
            string template = "Hi, \r\n \r\n" +
                                             "Your application, {0}, was successfully published to the KFC SSO portal with the following details:\r\n \r\n" +
                                             "Description: {1}\r\n" +
                                             "Logo URL: {2}\r\n" +
                                             "Under Maintenance: {3}\r\n \r\n" +
                                             "If you did not make this request, please contact us by responding to this email.\r\n \r\n" +
                                             "Thanks, \r\nKFC Team";
            
            string appPublishBodyString = string.Format(template, app.Title, app.Description, app.LogoUrl, app.UnderMaintenance);

            //Create the message that will be sent
            MimeMessage emailToSend = _emailService.CreateEmailPlainBody(userFullName, receiverEmail, appPublishSubjectString, appPublishBodyString);
            //Send the email with the message
            _emailService.SendEmail(emailToSend);
        }

        /// <summary>
        /// Creates an email to send new api keys to applications.
        /// </summary>
        /// <param name="receiverEmail"></param>
        /// <param name="apiKey"></param>
        public void SendNewApiKeyEmail(string receiverEmail, string apiKey)
        {
            string generateKeySubjectString = "KFC SSO: New API Key";
            string userFullName = receiverEmail;
            string template = "Hi, \r\n \r\n" +
                                             "You recently requested a new API Key for your KFC SSO application.\r\n \r\n" +
                                             "Below is a new single-use API Key to publish your application into the portal.\r\n {0} \r\n \r\n" +
                                             "If you did not make this request, please contact us by responding to this email.\r\n \r\n" +
                                             "Thanks, \r\nKFC Team";
            
            string generateKeyBodyString = string.Format(template, apiKey);

            //Create the message that will be sent
            MimeMessage emailToSend = _emailService.CreateEmailPlainBody(userFullName, receiverEmail, generateKeySubjectString, generateKeyBodyString);
            //Send the email with the message
            _emailService.SendEmail(emailToSend);
        }

        /// <summary>
        /// Creates an email to send a confirmation of app deletion.
        /// </summary>
        /// <param name="receiverEmail"></param>
        /// <param name="appTitle"></param>
        public void SendAppDeleteEmail(string receiverEmail, string appTitle)
        {
            string appDeleteSubjectString = "KFC SSO: Application Deletion";
            string userFullName = receiverEmail;
            string template = "Hi, \r\n \r\n" +
                                             "Your application, {0}, was successfully deleted from the KFC SSO portal.\r\n \r\n" +
                                             "If you did not make this request, please contact us by responding to this email.\r\n \r\n" +
                                             "Thanks, \r\nKFC Team";

            string appDeleteBodyString = string.Format(template, appTitle);

            //Create the message that will be sent
            MimeMessage emailToSend = _emailService.CreateEmailPlainBody(userFullName, receiverEmail, appDeleteSubjectString, appDeleteBodyString);
            //Send the email with the message
            _emailService.SendEmail(emailToSend);
        }

        #endregion
    }
}
