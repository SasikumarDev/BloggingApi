using System;
using System.Collections.Generic;

namespace BloggingAPI.BlogModel
{
    public partial class Users
    {
        public Users()
        {
            Answers = new HashSet<Answers>();
            PersonalDetails = new HashSet<PersonalDetails>();
            Questions = new HashSet<Questions>();
        }

        public int UsId { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string EmailId { get; set; }
        public string Password { get; set; }
        public DateTime? Dob { get; set; }
        public string ImagePath { get; set; }
        public DateTime? CreateDate { get; set; }
        public DateTime? Updatedate { get; set; }

        public virtual ICollection<Answers> Answers { get; set; }
        public virtual ICollection<PersonalDetails> PersonalDetails { get; set; }
        public virtual ICollection<Questions> Questions { get; set; }
    }
}
