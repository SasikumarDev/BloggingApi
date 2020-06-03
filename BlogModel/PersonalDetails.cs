using System;
using System.Collections.Generic;

namespace BloggingAPI.BlogModel
{
    public partial class PersonalDetails
    {
        public int Pid { get; set; }
        public int? UsId { get; set; }
        public string Job { get; set; }
        public string JobLocation { get; set; }
        public string Address { get; set; }

        public virtual Users Us { get; set; }
    }
}
