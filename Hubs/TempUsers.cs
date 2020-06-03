using System.Collections.Generic;
using System.Linq;

namespace bloggingapi.Hubs
{
    public static class TempUsers
    {
        public static List<SignalRUsers> Usrs { get; set; }
    }
    public class SignalRUsers
    {
        public string emailid { get; set; }
        public string connetionid { get; set; }
        public string id { get; set; }
    }
}