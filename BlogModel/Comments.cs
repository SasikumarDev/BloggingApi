using System;
using System.Collections.Generic;

namespace BloggingAPI.BlogModel
{
    public partial class Comments
    {
        public int ComId { get; set; }
        public string ComText { get; set; }
        public int? ComFrom { get; set; }
        public int? ComFor { get; set; }
        public string ComType { get; set; }
    }
}
