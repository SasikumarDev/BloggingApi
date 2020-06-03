using System;
using System.Collections.Generic;

namespace BloggingAPI.BlogModel
{
    public partial class Answers
    {
        public int Aid { get; set; }
        public int? Qid { get; set; }
        public string Answer { get; set; }
        public int? AnswredBy { get; set; }
        public DateTime? AnswredOn { get; set; }

        public virtual Users AnswredByNavigation { get; set; }
        public virtual Questions Q { get; set; }
    }
}
