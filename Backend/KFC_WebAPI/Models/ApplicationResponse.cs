using System;
using System.Collections;
using System.Linq;
using System.Web;

namespace KFC_WebAPI.Models
{
    public class ApplicationResponse
    {
        // Total number of pages for paginated apps
        public int TotalPages { get; private set; }
        // The applications to display per page
        public IEnumerable PaginatedApplications { get; private set; }

        // Model to hold total pages and paginated apps in one object
        public ApplicationResponse(int totalPages, IEnumerable paginatedApplications)
        {
            TotalPages = totalPages;
            PaginatedApplications = paginatedApplications;
        }
    }
}