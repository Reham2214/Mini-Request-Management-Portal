using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mini_Request_Management_Portal.Models
{
    public class RequestViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string RequestType { get; set; }
        public string Department { get; set; }
        public string Priority { get; set; }
        public string Status { get; set; }
        public string CreatedByName { get; set; }
        public string CreatedAt { get; set; }
    }
}