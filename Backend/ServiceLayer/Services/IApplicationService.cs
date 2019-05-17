using DataAccessLayer.Database;
using DataAccessLayer.Models;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace ServiceLayer.Services
{
    public interface IApplicationService
    {
        Application CreateApplication(Application app);
        Application DeleteApplication(Guid id);
        Application GetApplication(Guid id);
        Application GetApplication(string title, string email);
        List<Application> GetAllApplicationsList();
        IEnumerable GetPaginatedApplications(int currentPage, int pageSize, string sortOrder, out int totalPages);
        Application UpdateApplication(Application app);
        Task<HttpResponseMessage> GetApplicationHealth(string healthCheckUrl);
    }
}
