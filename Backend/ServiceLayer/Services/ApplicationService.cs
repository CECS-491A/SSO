using System;
using System.Collections;
using System.Collections.Generic;
using System.Net;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using DataAccessLayer.Database;
using DataAccessLayer.Models;
using DataAccessLayer.Repositories;


namespace ServiceLayer.Services
{
    public class ApplicationService: IApplicationService
    {
        private IApplicationRepository _applicationRepository;
        private IApiKeyRepository _apiKeyRepository;
        private readonly DatabaseContext _db;

        public ApplicationService(DatabaseContext _db)
        {
            this._db = _db;
            _applicationRepository = new ApplicationRepository(_db);
            _apiKeyRepository = new ApiKeyRepository(_db);
        }

        public Application CreateApplication(Application app)
        {
            return _applicationRepository.CreateNewApplication(app);
        }

        public Application DeleteApplication(Guid id)
        {
            // Delete children api keys first
            List<ApiKey> keys = _apiKeyRepository.GetAllKeys(id);
            foreach(ApiKey key in keys)
            {
                _apiKeyRepository.DeleteKey(key.Id);
            }

            return _applicationRepository.DeleteApplication(id);
        }

        public Application GetApplication(Guid id)
        {
            return _applicationRepository.GetApplication(id);
        }

        public Application GetApplication(string title, string email)
        {
            return _applicationRepository.GetApplication(title, email);
        }

        public List<Application> GetAllApplicationsList()
        {
            return _applicationRepository.GetAllApplicationsList();
        }
       
        /// <summary>
        /// Uses the application repository to get paginated applications
        /// along with the total number of pages to display
        /// </summary>
        /// <param name="currentPage">The current page the user is on</param>
        /// <param name="pageSize">The number of applications to display</param>
        /// <param name="totalPages">The total number of pages to display</param>
        /// <returns>A collection of paginated applications and the total pages</returns>
        public IEnumerable GetPaginatedApplications(int currentPage, int pageSize, string sortOrder, out int totalPages)
        {
            return _applicationRepository.GetPaginatedApplications(currentPage, pageSize, sortOrder, out totalPages);
        }
      

        public Application UpdateApplication(Application app)
        {
            return _applicationRepository.UpdateApplication(app);
        }

        /// <summary>
        /// Makes a get request using the health check url to retrieve a response
        /// </summary>
        /// <param name="healthCheckUrl">The domain to perform a get request on</param>
        /// <returns>A response containing anything the url returns but primarily the status code</returns>
        public async Task<HttpResponseMessage> GetApplicationHealth(string healthCheckUrl)
        {
            using (var client = new HttpClient()) { 
                try
                {
                    // Makes an asyncronous HTTP GET request to the health check url provided
                    // Using ConfigureAwait(false) to prevent deadlocks on the calling thread
                    return await client.GetAsync(healthCheckUrl).ConfigureAwait(false);
                }
                catch (Exception e)
                {
                    // Sends back a response with the error message
                    HttpResponseMessage response = new HttpResponseMessage
                    {
                        Content = new StringContent(e.InnerException.Message)
                    };
                    return response;
                }
            }
        }
    }
}
