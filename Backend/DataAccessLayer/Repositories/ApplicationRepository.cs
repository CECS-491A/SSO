using System;
using DataAccessLayer.Database;
using DataAccessLayer.Models;
using System.Linq;
using System.Data.Entity;
using System.Collections;
using System.Collections.Generic;

namespace DataAccessLayer.Repositories
{
    public class ApplicationRepository : IApplicationRepository
    {
        DatabaseContext _db;

        public ApplicationRepository(DatabaseContext _db)
        {
            this._db = _db;
        }

        public Application CreateNewApplication(Application app)
        {
            var result = GetApplication(app.Title, app.Email);
            if (result != null)
            {
                return null;
            }
            _db.Entry(app).State = EntityState.Added;
            return app;

        }

        public Application DeleteApplication(Guid id)
        {
            var app = GetApplication(id);
            if (app == null)
            {
                return null;
            }

            _db.Entry(app).State = EntityState.Deleted;
            return app;

        }

        public Application GetApplication(Guid id)
        {
            var response = _db.Applications.Include(x => x.ApiKeys)
                .Where(a => a.Id == id)
                .FirstOrDefault<Application>();
            return response;
        }

        public Application GetApplication(string title, string email)
        {
            var app = _db.Applications.Include(x => x.ApiKeys)
                .Where(a => a.Title == title && a.Email == email)
                .FirstOrDefault<Application>();

            return app;
        }

        /// <summary>
        /// Gets a certain amount of applications based on the current page and page size.
        /// </summary>
        /// <param name="currentPage">The current page the user is on</param>
        /// <param name="pageSize">The number of applications to display</param>
        /// <param name="totalPages">The total number of pages to display</param>
        /// <returns>A collection of paginated applications along with the total number of pages</returns>
        public IEnumerable GetPaginatedApplications(int currentPage, int pageSize, string sortOrder, out int totalPages)
        {
            // Calculates the row index of where to start the query from.
            // This will enable us to start at a certain row based on what page the user is on.
            // We subtract 1 to the current page because the query starts at index 0
            var startingIndex = pageSize * (currentPage - 1);

            // Perform a query to get all applications from the database
            var applications = _db.Applications.Select(app => new
            {
                app.Id,
                app.Title,
                app.Description,
                app.Email,
                app.LogoUrl,
                app.LaunchUrl,
                app.UnderMaintenance,
                app.ClickCount
            });

            // Sort order cannot be enull or the switch statement breaks
            if (sortOrder == null)
            {
                sortOrder = "id";
            }

            // Order the query based on the sort order parameter
            switch (sortOrder.ToLower())
            {
                // Order alphabetically in ascending order
                case "ascending":
                case "a":
                    applications = applications.OrderBy(app => app.Title);
                    break;
                // Order alphabetically in descending order
                case "descending":
                case "d":
                    applications = applications.OrderByDescending(app => app.Title);
                    break;
                // Order by number of application clicks
                case "count":
                case "c":
                case "popularity":
                case "p":
                    applications = applications.OrderByDescending(app => app.ClickCount);
                    break;
                // Order by application id
                default:
                    applications = applications.OrderBy(app => app.Id);
                    break;
            }

            // Get the total number of rows from the Application table
            int totalApplicationCount = applications.Count();
            // Calculates total number of pages for the pagination
            totalPages = (int)Math.Ceiling((double)totalApplicationCount / pageSize);

            // Paginated the query based on starting index and page size
            // Starting from the starting index row, take the amount of 
            // applications that the user wants to display
            applications = applications.Skip(startingIndex).Take(pageSize);

            // This is an error as the currentPage should not exceed the max number of pages
            if (currentPage > totalPages)
            {
                totalPages = 0;
            }

            return applications.ToList();
        }

        public List<Application> GetAllApplicationsList()
        {
            try
            {
                return _db.Applications.ToList();
            }
            catch (Exception)
            {
                return null;
            }
        }

        public Application UpdateApplication(Application app)
        {
            var result = GetApplication(app.Id);
            if (result == null)
            {
                return null;
            }
            _db.Entry(app).State = EntityState.Modified;
            return result;
        }

    }
}
