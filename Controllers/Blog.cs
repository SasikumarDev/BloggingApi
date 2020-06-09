using System.Diagnostics;
using System.IO;
using System.Net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using BloggingAPI.BlogModel;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using bloggingapi.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace BloggingAPI.Controllers
{
    [ApiController]
    public class Blog : ControllerBase
    {
        private BloggingContext _bContext;
        private IHubContext<NotificationHub> Notification;
        public Blog(IHubContext<NotificationHub> _Hub)
        {
            this.Notification = _Hub;
        }
        [AllowAnonymous]
        [HttpGet]
        [Route("[controller]/GetQuetions")]
        public IActionResult GetQuetions()
        {
            var Rslt = GetCrntUserNonayc(HttpContext);
            _bContext = new BloggingContext();
            var reslt = (from qst in _bContext.Questions.ToList()
                         join Usr in _bContext.Users.ToList() on qst.AskedBy equals Usr.UsId
                         let Tgs = new SpltTgs { id = qst.Qid, TagsID = qst.Tags.Split(',').ToList() }
                         select new
                         {
                             QstId = qst.Qid,
                             Question = qst.Question,
                             Askedby = Rslt != null ? (Rslt.UsId == Usr.UsId ? "You" : Usr.FirstName + " " + Usr.LastName.ToString()) : Usr.FirstName + " " + Usr.LastName.ToString(),
                             AskedOn = qst.AskedOn,
                             Tags = _bContext.Tags.ToList().Where(zx => Tgs.TagsID.Any(x => x.ToString() == zx.TgId.ToString())).Select(x => x.Tagname).ToList(),
                             AUID = qst.AskedBy
                         }).OrderByDescending(x => x.AskedOn).Take(10).ToList();
            return Ok(reslt);
        }
        [Authorize]
        [HttpGet]
        [Route("[controller]/GetNotification")]
        public async Task<IActionResult> GetNotification()
        {
            var Rslt = GetCrntUserNonayc(HttpContext);
            _bContext = new BloggingContext();
            var Notications = await (from noti in _bContext.Notification
                                     join usr in _bContext.Users on noti.NotFrom equals usr.UsId
                                     where noti.NotTo == Rslt.UsId
                                     select new
                                     {
                                         fromid = usr.FirstName + usr.LastName,
                                         prof = Request.Scheme + "://" + Request.Host.Value + (usr.ImagePath == null ? Url.Content($"/Contents/Images/Defaultprof.png") : Url.Content($"/Contents/Images/{usr.ImagePath}")),
                                         body = noti.NotBody,
                                         datetime = noti.NotDateTime,
                                         nav = noti.NotRoute
                                     }).OrderByDescending(x => x.datetime).ToListAsync();
            return Ok(Notications);
        }

        [Authorize]
        [HttpPost]
        [Route("[controller]/AddQst")]
        public async Task<IActionResult> AddQst([FromBody] Questions Qst)
        {
            _bContext = new BloggingContext();
            var Rslt = await GetCrntUser(HttpContext);
            Qst.AskedBy = Rslt.UsId;
            Qst.AskedOn = DateTime.Now;
            _bContext.Questions.Add(Qst);
            await _bContext.SaveChangesAsync();
            return Ok(new { Status = HttpStatusCode.OK, Message = "Posted" });
        }
        [AllowAnonymous]
        [HttpPost]
        [Route("[controller]/GetParticularusr")]
        public IActionResult GetParticularusr([FromBody] DParameter Dp)
        {
            _bContext = new BloggingContext();
            var result = (from usr in _bContext.Users
                          where usr.UsId == Convert.ToInt32(Dp.Id)
                          select new
                          {
                              Fullname = usr.FirstName + "" + usr.LastName,
                              DOB = usr.Dob,
                              profile = Request.Scheme + "://" + Request.Host.Value + (usr.ImagePath == null ? Url.Content($"/Contents/Images/Defaultprof.png") : Url.Content($"/Contents/Images/{usr.ImagePath}")),
                              contact = usr.EmailId,
                              Questions = _bContext.Questions.Where(x => x.AskedBy == usr.UsId).Count(),
                              Answers = _bContext.Answers.Where(x => x.AnswredBy == usr.UsId).Count()
                          }).ToList();
            return Ok(result);
        }
        [Authorize]
        [Route("[controller]/CurrentUser")]
        [HttpGet]
        public IActionResult CurrentUser()
        {
            try
            {
                _bContext = new BloggingContext();
                var identity = User.Identity as ClaimsIdentity;
                var name = identity.Claims.Cast<Claim>().Where(p => p.Type == "UserID").FirstOrDefault()?.Value;
                var rslt = (from usr in _bContext.Users
                            where usr.UsId == Convert.ToInt32(name.ToString())
                            select new
                            {
                                Fullname = usr.FirstName + " " + usr.LastName,
                                DOB = usr.Dob,
                                pic = Request.Scheme + "://" + Request.Host.Value + (usr.ImagePath == null ? Url.Content($"/Contents/Images/Defaultprof.png") : Url.Content($"/Contents/Images/{usr.ImagePath}")),
                                email = usr.EmailId
                            }).ToList();
                return Ok(rslt);
            }
            catch (Exception ex)
            {
                return Ok(ex.Message.ToString());
            }
        }
        [Authorize]
        [HttpGet]
        [Route("[controller]/ValidateUser")]
        public bool ValidateUser()
        {
            var identity = User.Identity as ClaimsIdentity;
            if (identity == null)
            {
                return false;
            }
            else
            {
                var name = identity.Claims.Cast<Claim>().Where(p => p.Type == "UserID").FirstOrDefault()?.Value;
                if (name == null)
                {
                    return false;
                }
                else
                {
                    return true;
                }
            }
        }
        [AllowAnonymous]
        [HttpPost]
        [Route("[controller]/GetPartQstAndAns")]
        public IActionResult GetPartQstAndAns([FromBody] DParameter Dp)
        {
            try
            {
                _bContext = new BloggingContext();
                var Rslt = GetCrntUserNonayc(HttpContext);
                if (int.TryParse(Dp.Id.ToString(), out int n))
                {
                    int Qid = Convert.ToInt32(Dp.Id.ToString());
                    var chk = _bContext.Questions.FirstOrDefault(qs => qs.Qid == Qid);
                    if (chk != null)
                    {
                        var Qstion = (from qst in _bContext.Questions.ToList()
                                      join Usr in _bContext.Users.ToList() on qst.AskedBy equals Usr.UsId
                                      let Tgs = new SpltTgs { id = qst.Qid, TagsID = qst.Tags.Split(',').ToList() }
                                      where qst.Qid == Qid
                                      select new
                                      {
                                          QstId = qst.Qid,
                                          Question = qst.Question,
                                          Askedby = Rslt != null ? (Rslt.UsId == Usr.UsId ? "You" : Usr.FirstName + " " + Usr.LastName.ToString()) : Usr.FirstName + " " + Usr.LastName.ToString(),
                                          AskedOn = qst.AskedOn,
                                          Tags = _bContext.Tags.ToList().Where(zx => Tgs.TagsID.Any(x => x.ToString() == zx.TgId.ToString())).Select(x => x.Tagname).ToList(),
                                          AUID = qst.AskedBy
                                      }).Take(1).ToList();
                        var Answers = (from Ans in _bContext.Answers.ToList()
                                       join Usr in _bContext.Users on Ans.AnswredBy equals Usr.UsId
                                       where Ans.Qid == Qid
                                       select new
                                       {
                                           AnsID = Ans.Aid,
                                           Answer = Ans.Answer,
                                           Answeredby = Rslt != null ? (Rslt.UsId == Usr.UsId ? "You" : Usr.FirstName + " " + Usr.LastName.ToString()) : Usr.FirstName + " " + Usr.LastName.ToString(),
                                           Answeredon = Ans.AnswredOn,
                                           aid = Ans.AnswredBy,
                                           TLikes = _bContext.Likes.Where(x => x.AnsId == Ans.Aid).Count(),
                                           ULikes = Rslt != null ? (_bContext.Likes.ToList().Where(x => x.Likeby == Rslt.UsId && x.AnsId == Ans.Aid).ToList().Count()) > 0 : true ? false : false
                                       }).OrderBy(x => x.AnsID).ToList();
                        return new OkObjectResult(new { Question = Qstion[0], Answers = Answers, Message = "", Status = HttpStatusCode.OK });
                    }
                    else
                    {
                        return new OkObjectResult(new { Message = "Invalid", Status = HttpStatusCode.NotFound });
                    }
                }
                else
                {
                    return new OkObjectResult(new { Message = "Invalid", Status = HttpStatusCode.NotFound });
                }
            }
            catch (Exception Ex)
            {
                return new OkObjectResult(new { Message = Ex.Message.ToString(), Status = HttpStatusCode.InternalServerError });
            }
        }
        [Authorize]
        [HttpPost]
        [Route("[controller]/SaveAns")]
        public async Task<IActionResult> SaveAnsAsync([FromBody] Answers Ans)
        {
            _bContext = new BloggingContext();
            Ans.AnswredOn = DateTime.Now;
            var Rslt = await GetCrntUser(HttpContext);
            Ans.AnswredBy = Rslt.UsId;
            _bContext.Answers.Add(Ans);
            await _bContext.SaveChangesAsync();
            DParameter db_pam = new DParameter();
            db_pam.Id = Ans.Qid.ToString();
            var qst = await _bContext.Questions.FirstOrDefaultAsync(x => x.Qid == Convert.ToInt32(Ans.Qid));
            Notification ntf = new Notification();
            ntf.NotFrom = Rslt.UsId;
            ntf.NotDateTime = DateTime.Now;
            ntf.NotIsread = false;
            ntf.NotRoute = "/PartIss/" + Ans.Qid.ToString();
            ntf.NotBody = "Your Question has been answered by " + Rslt.FirstName + Rslt.LastName + "";
            ntf.NotTo = qst.AskedBy;
            await _bContext.Notification.AddAsync(ntf);
            await _bContext.SaveChangesAsync();
            var msg = await (from noti in _bContext.Notification
                             join usr in _bContext.Users on noti.NotFrom equals usr.UsId
                             where noti.NotId == ntf.NotId
                             select new
                             {
                                 fromid = usr.FirstName + usr.LastName,
                                 prof = Request.Scheme + "://" + Request.Host.Value + (usr.ImagePath == null ? Url.Content($"/Contents/Images/Defaultprof.png") : Url.Content($"/Contents/Images/{usr.ImagePath}")),
                                 body = noti.NotBody,
                                 datetime = noti.NotDateTime,
                                 nav = noti.NotRoute
                             }).OrderByDescending(x => x.datetime).ToListAsync();
            var partusr = await GetPartUsr(Convert.ToInt32(Ans.Qid.ToString()));
            if (partusr.Count > 0)
            {
                foreach (var x in partusr)
                {
                    await Notification.Clients.Client(x.ConnectionId.ToString()).SendAsync("MessageReceived", msg[0]);
                }
            }
            return GetPartQstAndAns(db_pam);
        }
        [Authorize]
        [HttpPost]
        [Route("[controller]/LikeAns")]
        public async Task<IActionResult> LikeAns([FromBody] DParameter Dp)
        {
            try
            {
                _bContext = new BloggingContext();
                var Rslt = await GetCrntUser(HttpContext);
                if (Rslt != null)
                {
                    var lks = await _bContext.Likes.FirstOrDefaultAsync(x => x.AnsId == Convert.ToInt32(Dp.Id.ToString()) && x.Likeby == Rslt.UsId);
                    if (lks == null)
                    {
                        Likes likes = new Likes();
                        likes.AnsId = Convert.ToInt32(Dp.Id.ToString());
                        likes.LkCnt = 1;
                        likes.Likeby = Rslt.UsId;
                        await _bContext.Likes.AddAsync(likes);
                        await _bContext.SaveChangesAsync();
                        var Ans = await _bContext.Answers.FirstOrDefaultAsync(x => x.Aid == likes.AnsId);
                        var qst = await _bContext.Questions.FirstOrDefaultAsync(x => x.Qid == Ans.Qid);
                        Notification ntf = new Notification();
                        ntf.NotFrom = Rslt.UsId;
                        ntf.NotDateTime = DateTime.Now;
                        ntf.NotIsread = false;
                        ntf.NotRoute = "/PartIss/" + qst.Qid.ToString();
                        ntf.NotBody = "Your Answer Liked by " + Rslt.FirstName + Rslt.LastName + "";
                        ntf.NotTo = qst.AskedBy;
                        await _bContext.Notification.AddAsync(ntf);
                        await _bContext.SaveChangesAsync();
                        var msg = await (from noti in _bContext.Notification
                                         join usr in _bContext.Users on noti.NotFrom equals usr.UsId
                                         where noti.NotId == ntf.NotId
                                         select new
                                         {
                                             fromid = usr.FirstName + usr.LastName,
                                             prof = Request.Scheme + "://" + Request.Host.Value + (usr.ImagePath == null ? Url.Content($"/Contents/Images/Defaultprof.png") : Url.Content($"/Contents/Images/{usr.ImagePath}")),
                                             body = noti.NotBody,
                                             datetime = noti.NotDateTime,
                                             nav = noti.NotRoute
                                         }).OrderByDescending(x => x.datetime).ToListAsync();
                        var partusr = await GetPartUsrByAns(Convert.ToInt32(Ans.Aid.ToString()));
                        if (partusr.Count > 0)
                        {
                            foreach (var x in partusr)
                            {
                                await Notification.Clients.Client(x.ConnectionId.ToString()).SendAsync("MessageReceived", msg[0]);
                            }
                        }
                        return new OkObjectResult(new { Message = "You Liked", Status = HttpStatusCode.OK });
                    }
                    else
                    {
                        _bContext.Likes.Attach(lks);
                        _bContext.Likes.Remove(lks);
                        await _bContext.SaveChangesAsync();
                        return new OkObjectResult(new { Message = "You Remove Liked", Status = HttpStatusCode.OK });
                    }
                }
                else
                {
                    return new OkObjectResult(new { Message = "", Status = HttpStatusCode.BadRequest });
                }
            }
            catch (Exception Ex)
            {
                return new OkObjectResult(new { Message = "", Error = Ex.Message.ToString(), Status = HttpStatusCode.InternalServerError });
            }
        }
        [AllowAnonymous]
        [HttpGet]
        [Route("[controller]/GetTags")]
        public async Task<IActionResult> GetTags()
        {
            _bContext = new BloggingContext();
            return Ok(await _bContext.Tags.ToListAsync());
        }
        [NonAction]
        public async Task<Users> GetCrntUser(HttpContext httpContext)
        {
            var identity = User.Identity as ClaimsIdentity;
            var name = identity.Claims.Cast<Claim>().Where(p => p.Type == "UserID").FirstOrDefault()?.Value;
            _bContext = new BloggingContext();
            return await _bContext.Users.FirstOrDefaultAsync(x => x.UsId == Convert.ToInt32(name.ToString()));
        }
        [NonAction]
        public Users GetCrntUserNonayc(HttpContext httpContext)
        {
            var identity = User.Identity as ClaimsIdentity;
            var name = identity.Claims.Cast<Claim>().Where(p => p.Type == "UserID").FirstOrDefault()?.Value;
            if (name != null)
            {
                _bContext = new BloggingContext();
                return _bContext.Users.FirstOrDefault(x => x.UsId == Convert.ToInt32(name.ToString()));
            }
            else
            {
                return null;
            }
        }
        [NonAction]
        public async Task<List<UserParam>> GetPartUsr(int id)
        {
            _bContext = new BloggingContext();
            var qst = await _bContext.Questions.FirstOrDefaultAsync(x => x.Qid == Convert.ToInt32(id));
            var usr = await _bContext.Users.FirstOrDefaultAsync(x => x.UsId == qst.AskedBy);
            var users_ = Crntusr.param.Where(x => x.Username == usr.EmailId).ToList();
            return users_;
        }
        [NonAction]
        public async Task<List<UserParam>> GetPartUsrByAns(int id)
        {
            _bContext = new BloggingContext();
            var qst = await _bContext.Answers.FirstOrDefaultAsync(x => x.Aid == Convert.ToInt32(id));
            var usr = await _bContext.Users.FirstOrDefaultAsync(x => x.UsId == qst.AnswredBy);
            var users_ = Crntusr.param.Where(x => x.Username == usr.EmailId).ToList();
            return users_;
        }
    }
    public class DParameter
    {
        public string Id { get; set; }
        public string Name { get; set; }
    }
    public class SpltTgs
    {
        public int id { get; set; }
        public List<string> TagsID { get; set; }
    }
}