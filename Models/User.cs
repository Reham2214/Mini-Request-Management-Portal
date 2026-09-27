using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace Mini_Request_Management_Portal.Models
{
    public class User
    {
        public int Id { get; set; }
        public string FullName { get; set; }
        public string Role { get; set; }
        public bool IsRequester => Role == "Requester";
        public bool IsReviewer => Role == "Reviewer";
    }
}