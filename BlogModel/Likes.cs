using System;
using System.Collections.Generic;

namespace BloggingAPI.BlogModel
{
    public partial class Likes
    {
        public int Lkid { get; set; }
        public int? AnsId { get; set; }
        public long? LkCnt { get; set; }
        public int? Likeby { get; set; }
    }
}
