using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace KFC_WebAPI.Models
{
    public class ApplicationHealthCheck
    {
        // Stores the date and time of the last health check
        public DateTime LastHealthCheck { get; set; }
        // Dictionary that maps the app id as the key and a boolean stating
        // if the app is healthy or not for the value
        public Dictionary<Guid, bool> HealthStatuses { get; set; }

        // Initialize the time to the current time
        // Initialize the dictionary as empty
        public ApplicationHealthCheck()
        {
            HealthStatuses = new Dictionary<Guid, bool>();
            LastHealthCheck = DateTime.UtcNow;
        }
    }
}