using System;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using BloggingAPI.BlogModel;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Linq;
using System.Collections.Generic;

namespace bloggingapi.Hubs
{
    [Authorize]
    public class NotificationHub : Hub
    {
        private BloggingContext _bContext;
        public override async Task OnConnectedAsync()
        {
            _bContext = new BloggingContext();
            var _con = Context.ConnectionId;
            var us_p = new UserParam();
            us_p.ConnectionId = Context.ConnectionId.ToString();
            us_p.UserToken = Context.User.Claims;
            us_p.Username = await GetCrntUser();
            Crntusr.param.Add(us_p);
            await base.OnConnectedAsync();
        }

        private async Task<string> GetCrntUser()
        {
            var name = Context.User.Claims.Where(p => p.Type == "UserID").FirstOrDefault()?.Value;
            _bContext = new BloggingContext();
            var rslt = await _bContext.Users.FirstOrDefaultAsync(x => x.UsId == Convert.ToInt32(name.ToString()));
            if (rslt != null)
            {
                return rslt.EmailId.ToString();
            }
            else
            {
                return null;
            }
        }
        public async Task NewMessage(Message msg)
        {
            _bContext = new BloggingContext();
            var qst = await _bContext.Questions.FirstOrDefaultAsync(x => x.Qid == Convert.ToInt32(msg.Qid));
            var usr = await _bContext.Users.FirstOrDefaultAsync(x => x.UsId == qst.AskedBy);
            var users_ = Crntusr.param.FirstOrDefault(x => x.Username == usr.EmailId);
            //await Clients.All.SendAsync("MessageReceived", msg);
            if (users_ != null)
            {
                await Clients.All.SendAsync("MessageReceived", msg);
            }
            else
            {
                await Clients.All.SendAsync("MessageReceived", msg);
            }
        }
    }
    public class Message
    {
        public string Qid { get; set; }
        public string Body { get; set; }
        public string To { get; set; }
        public DateTime Dtime { get; set; }
        public string Navigation { get; set; }
    }
    public class UserParam
    {
        public string ConnectionId { get; set; }
        public IEnumerable<Claim> UserToken { get; set; }
        public string Username { get; set; }
    }
    public class Crntusr
    {
        public static List<UserParam> param = new List<UserParam>();
    }
}