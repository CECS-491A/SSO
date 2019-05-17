using System;
using System.Collections;
using System.Collections.Generic;
using System.Data.Entity.Validation;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;
using DataAccessLayer.Database;
using DataAccessLayer.Models;
using DataAccessLayer.RequestModels;
using KFC_WebAPI.Models;
using ManagerLayer.ApplicationManagement;
using ServiceLayer.Exceptions;
using ServiceLayer.Services;

namespace KFC_WebAPI.Controllers
{
    public class ApplicationController : ApiController
    {
        private ApplicationHealthCheck _appHealthStatus = new ApplicationHealthCheck();

        /// <summary>
        /// Performs a health check of all applications every minute
        /// Uses a dictionary to store the health status of all applications
        /// The application only gets added to the dictionary if the health
        /// check url return anything other than 200 Success
        /// </summary>
        /// <returns>A timestamp of the last health check and the health statuses of every app</returns>
        [HttpGet, Route("api/applications/healthcheck")]
        public async Task<IHttpActionResult> CheckApplicationHealthCheckAsync()
        {
            using (var _db = new DatabaseContext())
            {
                // 10 second interval for calling the health check service
                int healthCheckInterval = 10;
                // Stores responses to concurrently wait for them to complete
                var taskList = new List<Task<HttpResponseMessage>>();

                // DateTime when a health check is performed
                DateTime checkTime = (DateTime.UtcNow.Subtract(TimeSpan.FromSeconds(healthCheckInterval)));

                // Check if another health check is allowed. If the last health check was within the same interval, 
                // then no health check will be performed
                if (_appHealthStatus.LastHealthCheck > checkTime)
                {
                    // Get the entire list of applications again each time in case they were updated or new ones were added
                    ApplicationService _applicationService = new ApplicationService(_db);
                    List<Application> applications = _applicationService.GetAllApplicationsList();

                    // For every app in the applications table, perform a health check
                    foreach (Application app in applications)
                    {
                        // Check the health check url of each app
                        var response = _applicationService.GetApplicationHealth(app.HealthCheckUrl);

                        // If the status is not a 200 success, then the the app is down (true)
                        _appHealthStatus.HealthStatuses[app.Id] = !response.Result.IsSuccessStatusCode;

                        // Achieving parallelism by not awaiting each http request
                        taskList.Add(response);
                    }

                    // Asynchronously wait until all tasks are complete
                    await Task.WhenAll(taskList.ToArray());

                    // Updates last health check to current time
                    _appHealthStatus.LastHealthCheck = DateTime.UtcNow;
                }
            }
            // If the the last health check has been one minute then just return the statuses again
            // Or, return the updated statuses if they were updated
            return Content((HttpStatusCode)200, _appHealthStatus);
        }

        /// <summary>
        /// Using the application service, obtain a selection of applications based on the 
        /// pagination parameters.
        /// </summary>
        /// <param name="currentPage">The current page the user is viewing</param>
        /// <param name="pageSize">The amount of applications to display</param>
        /// <returns>A response with an object for the total pages and another for the paginated applications</returns>
        [HttpGet, Route("api/applications")]
        public IHttpActionResult GetPaginatedApplications(int currentPage, int pageSize, string sortOrder)
        {
            using (var _db = new DatabaseContext())
            {
                ApplicationManager manager = new ApplicationManager(_db);

                // Only proceed if both the current page and page size is valid
                if (manager.IsValidPaginationParameters(currentPage, pageSize))
                {
                    IApplicationService applicationService = new ApplicationService(_db);
                    IEnumerable applications = applicationService.GetPaginatedApplications(currentPage, pageSize, sortOrder, out int totalPages);

                    // Total page would only be zero if the current page value exceeded max pages for available applications
                    if (totalPages == 0)
                    {
                        return Content((HttpStatusCode)400, "Current page value exceeded max pages for the total amount of paginated applications.");
                    }

                    // Create application response to hold total pages and paginated applications
                    var applicationResponse = new ApplicationResponse(totalPages, applications);

                    return Content((HttpStatusCode)200, applicationResponse);
                }
                // Current page or page size is less than or equal to 0
                else
                {
                    return Content((HttpStatusCode)400, "Current page and/or page size cannot be zero or negative.");
                }
            }
        }

        /// <summary>
        /// Register application into portal
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost, Route("api/applications/create")]
        public IHttpActionResult Register([FromBody] ApplicationRequest request)
        {
            using (var _db = new DatabaseContext())
            {
                HttpResponseContent responseContent = new HttpResponseContent(); // Body of Response Message

                if (!ModelState.IsValid || request == null)
                {
                    // 412 Response
                    responseContent.Code = HttpStatusCode.PreconditionFailed;
                    responseContent.Message = "Invalid Request";
                    return Content(responseContent.Code, responseContent);
                }

                try
                {
                    IApplicationManager manager = new ApplicationManager(_db);
                    // Validate request and register application
                    responseContent = manager.ValidateRegistration(request);

                    responseContent.Code = HttpStatusCode.OK;
                }
                catch (Exception ex)
                {
                    // Request inputs are invalid format or violate business rules
                    if (ex is InvalidStringException ||
                        ex is InvalidEmailException ||
                        ex is InvalidUrlException ||
                        ex is ArgumentException)
                    {
                        responseContent.Code = HttpStatusCode.BadRequest;
                    }
                    // Error in data store
                    else if (ex is DbEntityValidationException)
                    {
                        responseContent.Code = HttpStatusCode.InternalServerError;
                    }
                    else
                    {
                        responseContent.Code = HttpStatusCode.InternalServerError;
                    }

                    responseContent.Message = ex.Message;
                }
                return Content(responseContent.Code, responseContent);
            }
        }

        /// <summary>
        /// Publish application to portal
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost, Route("api/applications/publish")]
        public IHttpActionResult Publish([FromBody] ApplicationRequest request)
        {
            using (var _db = new DatabaseContext())
            {
                HttpResponseContent responseContent = new HttpResponseContent();  // Body of Response Message

                if (!ModelState.IsValid || request == null)
                {
                    // 412 Response
                    responseContent.Code = HttpStatusCode.PreconditionFailed;
                    responseContent.Message = "Invalid Request";
                    return Content(responseContent.Code, responseContent);
                }

                try
                {
                    IApplicationManager manager = new ApplicationManager(_db);
                    // Validate request inputs and publish application
                    responseContent = manager.ValidatePublish(request);

                    responseContent.Code = HttpStatusCode.OK;
                }
                catch (Exception ex)
                {
                    // User inputs are invalid format or violate business rules
                    if (ex is InvalidStringException ||
                        ex is InvalidUrlException ||
                        ex is InvalidImageException ||
                        ex is InvalidApiKeyException)
                    {
                        responseContent.Code = HttpStatusCode.BadRequest;
                    }
                    // Error in data store
                    else if (ex is DbEntityValidationException)
                    {
                        responseContent.Code = HttpStatusCode.InternalServerError;
                    }
                    else
                    {
                        responseContent.Code = HttpStatusCode.InternalServerError;
                    }

                    responseContent.Message = ex.Message;
                }
                return Content(responseContent.Code, responseContent);
            }
        }

        /// <summary>
        /// Generate a new api key for an application
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost, Route("api/applications/generatekey")]
        public IHttpActionResult GenerateKey([FromBody] ApplicationRequest request)
        {
            using (var _db = new DatabaseContext())
            {
                HttpResponseContent responseContent = new HttpResponseContent();  // Body of Response Message

                if (!ModelState.IsValid || request == null)
                {
                    // 412 Response
                    responseContent.Code = HttpStatusCode.PreconditionFailed;
                    responseContent.Message = "Invalid Request";
                    return Content(responseContent.Code, responseContent);
                }

                try
                {
                    IApplicationManager manager = new ApplicationManager(_db);
                    // Validate request inputs and generate a new api key
                    responseContent = manager.ValidateKeyGeneration(request);

                    responseContent.Code = HttpStatusCode.OK;
                }
                catch (Exception ex)
                {
                    // User inputs are invalid format or violate business rules
                    if (ex is InvalidStringException ||
                        ex is InvalidEmailException ||
                        ex is ArgumentException)
                    {
                        responseContent.Code = HttpStatusCode.BadRequest;
                    }
                    // Error in data store
                    else if (ex is DbEntityValidationException)
                    {
                        responseContent.Code = HttpStatusCode.InternalServerError;
                    }
                    else
                    {
                        responseContent.Code = HttpStatusCode.InternalServerError;
                    }

                    responseContent.Message = ex.Message;
                }
                return Content(responseContent.Code, responseContent);
            }
        }

        /// <summary>
        /// Delete application from portal
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost, Route("api/applications/delete")]
        public IHttpActionResult DeleteApplication([FromBody] ApplicationRequest request)
        {
            using (var _db = new DatabaseContext())
            {
                HttpResponseContent responseContent = new HttpResponseContent();  // Body of Response Message

                if (!ModelState.IsValid || request == null)
                {
                    // 412 Response
                    responseContent.Code = HttpStatusCode.PreconditionFailed;
                    responseContent.Message = "Invalid Request";
                    return Content(responseContent.Code, responseContent);
                }

                try
                {
                    IApplicationManager manager = new ApplicationManager(_db);
                    // Validate request inputs and delete application from portal
                    responseContent = manager.ValidateDeletion(request);

                    responseContent.Code = HttpStatusCode.OK;
                }
                catch (Exception ex)
                {
                    // User inputs are invalid format or violate business rules
                    if (ex is InvalidStringException ||
                        ex is InvalidEmailException ||
                        ex is ArgumentException)
                    {
                        responseContent.Code = HttpStatusCode.BadRequest;
                    }
                    // Error in data store
                    else if (ex is DbEntityValidationException)
                    {
                        responseContent.Code = HttpStatusCode.InternalServerError;
                    }
                    else
                    {
                        responseContent.Code = HttpStatusCode.InternalServerError;
                    }

                    responseContent.Message = ex.Message;
                }
                return Content(responseContent.Code, responseContent);
            }
        }

        /// <summary>
        /// Update application from portal
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        [HttpPost, Route("api/applications/update")]
        public HttpResponseMessage UpdateApplication([FromBody] ApplicationRequest request)
        {
            using (var _db = new DatabaseContext())
            {
                IApplicationManager manager = new ApplicationManager(_db);
                HttpResponseContent responseContent = manager.ValidateUpdate(request);
                HttpResponseMessage response = Request.CreateResponse(responseContent.Code, responseContent.Message);

                return response;
            }
        }
    }
}