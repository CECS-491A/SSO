﻿using System;
using DataAccessLayer.Database;
using DataAccessLayer.Models;
using System.Linq;
using System.Data.Entity;
using System.Collections;
using System.Collections.Generic;

namespace DataAccessLayer.Repositories
{
    public static class ApplicationRepository
    {
        /// <summary>
        /// Create a new application record
        /// </summary>
        /// <param name="_db">database</param>
        /// <param name="app">application</param>
        /// <returns>Created application</returns>
        public static Application CreateNewApplication(DatabaseContext _db, Application app)
        {
            try
            {
                var result = GetApplication(_db, app.Title, app.Email);
                if (result != null)
                {
                    return null;
                }
                _db.Entry(app).State = EntityState.Added;
                return app;
            }
            catch(Exception)
            {
                return null;
            }
            
        }

        /// <summary>
        /// Delete an application record
        /// </summary>
        /// <param name="_db">database</param>
        /// <param name="id">application id</param>
        /// <returns>The deleted application record</returns>
        public static Application DeleteApplication(DatabaseContext _db, Guid id)
        {
            try
            {
                var app = GetApplication(_db, id);
                if (app == null)
                {
                    return null;
                }
                _db.Entry(app).State = EntityState.Deleted;
                return app;
            }
            catch (Exception)
            {
                return null;
            }
            
        }

        /// <summary>
        /// Retrieve an application record by id field
        /// </summary>
        /// <param name="_db">database</param>
        /// <param name="id">application id</param>
        /// <returns>The retrieved application</returns>
        public static Application GetApplication(DatabaseContext _db, Guid id)
        {
            try
            {
                var response = _db.Applications.Find(id);
                return response;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Retrieve an application record by title and email
        /// </summary>
        /// <param name="_db">databasee</param>
        /// <param name="title">application title</param>
        /// <param name="email">application email</param>
        /// <returns></returns>
        public static Application GetApplication(DatabaseContext _db, string title, string email)
        {
            try
            {
                var app = _db.Applications
                .Where(a => a.Title == title && a.Email == email)
                .FirstOrDefault<Application>();

                return app;
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Get all applications
        /// </summary>
        /// <param name="_db">database</param>
        /// <returns>All application registered with the SSO</returns>
        public static IEnumerable GetAllApplications(DatabaseContext _db)
        {
            try
            {
                return _db.Applications.Select(app => new
                {
                    Id = app.Id,
                    LaunchUrl = app.LaunchUrl,
                    Title = app.Title,
                    LogoUrl = app.LogoUrl,
                    Description = app.Description,
                    UnderMaintenance = app.UnderMaintenance
                }).ToList();
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Get all applications sorted in alphabetical ascending order
        /// </summary>
        /// <param name="_db">database</param>
        /// <returns>All sorted application registered with the SSO</returns>
        public static IEnumerable SortAllApplicationsAlphaAscending(DatabaseContext _db)
        {
            try
            {
                return _db.Applications.OrderBy(app => app.Title).Select(app => new
                {
                    Id = app.Id,
                    LaunchUrl = app.LaunchUrl,
                    Title = app.Title,
                    LogoUrl = app.LogoUrl,
                    Description = app.Description,
                    UnderMaintenance = app.UnderMaintenance
                }).ToList();
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Get all applications sorted in alphabetical ascending order
        /// </summary>
        /// <param name="_db">database</param>
        /// <returns>All sorted application registered with the SSO</returns>
        public static IEnumerable SortAllApplicationsAlphaDescending(DatabaseContext _db)
        {
            try
            {
                return _db.Applications.OrderByDescending(app => app.Title).Select(app => new
                {
                    Id = app.Id,
                    LaunchUrl = app.LaunchUrl,
                    Title = app.Title,
                    LogoUrl = app.LogoUrl,
                    Description = app.Description,
                    UnderMaintenance = app.UnderMaintenance
                }).ToList();
            }
            catch (Exception)
            {
                return null;
            }
        }

        /// <summary>
        /// Update an application record
        /// </summary>
        /// <param name="_db">database</param>
        /// <param name="app">application</param>
        /// <returns>The updated application</returns>
        public static Application UpdateApplication(DatabaseContext _db, Application app)
        {

            try
            {
                var result = GetApplication(_db, app.Id);
                if (result == null)
                {
                    return null;
                }
                _db.Entry(app).State = EntityState.Modified;
                return result;
            }
            catch (Exception)
            {
                return null;
            }
        }

    }
}
