using System;
using System.Collections.Generic;

namespace BloggingAPI.BlogModel
{
    public partial class Notification
    {
        public int NotId { get; set; }
        public string NotBody { get; set; }
        public int? NotTo { get; set; }
        public int? NotFrom { get; set; }
        public DateTime? NotDateTime { get; set; }
        public bool? NotIsread { get; set; }
        public string NotRoute { get; set; }
    }
}
