using System;
using System.Collections.Generic;

namespace BloggingAPI.BlogModel
{
    public partial class Questions
    {
        public Questions()
        {
            Answers = new HashSet<Answers>();
        }

        public int Qid { get; set; }
        public string Question { get; set; }
        public int? AskedBy { get; set; }
        public DateTime? AskedOn { get; set; }
        public string Tags { get; set; }
        public string Title { get; set; }

        public virtual Users AskedByNavigation { get; set; }
        public virtual ICollection<Answers> Answers { get; set; }
    }
}
