using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mini_Request_Management_Portal.Models
{
    public class RequestStatus
    {
        public const string Draft = "Draft";
        public const string Submitted = "Submitted";
        public const string InReview = "In Review";
        public const string Completed = "Completed";
        public const string Cancelled = "Cancelled";
    }
}