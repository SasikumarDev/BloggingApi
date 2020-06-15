using System;
using System.Collections.Generic;

namespace BloggingAPI.BlogModel
{
    public partial class PersonalDetails
    {
        public int PId { get; set; }
        public int? UsId { get; set; }
        public string Job { get; set; }
        public string Company { get; set; }
        public string FaceBook { get; set; }
        public string Github { get; set; }
        public string Address { get; set; }
    }
}
