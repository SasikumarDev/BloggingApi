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
        [HttpPost]
        [Route("[controller]/AddQst")]
        public async Task<IActionResult> AddQst([FromBody] Questions Qst)
        {
            _bContext = new BloggingContext();
            var Rslt = await GetCrntUser(HttpContext);
            Qst.AskedBy = Rslt.UsId;
            Qst.AskedOn = DateTime.Now;
            Qst.Tags = string.Join(",", _bContext.Tags.Select(x => x.TgId.ToString()).ToList().Take(2).ToList().ToArray());
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
                              profile = getimagebasestring(usr.ImagePath),
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
                                pic = getimagebasestring(usr.ImagePath),
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
            Message msg = new Message();
            msg.Dtime = DateTime.Now;
            msg.Body = "Your Question has been answered by" + Rslt.FirstName + Rslt.LastName + "";
            msg.Qid = Ans.Qid.ToString();
            msg.To = "";
            msg.Navigation = "/partIss/" + Ans.Qid.ToString();
            var partusr = await GetPartUsr(Convert.ToInt32(Ans.Qid.ToString()));
            if (partusr != null)
            {
                await Notification.Clients.Client(partusr.ConnectionId.ToString()).SendAsync("MessageReceived", msg);
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
        [NonAction]
        public static string getimagebasestring(string imagepath)
        {
            if (imagepath == null)
            {
                imagepath = Directory.GetCurrentDirectory() + @"\Contents\Images\Profile\Defaultprof.png";
            }
            else
            {
                imagepath = Directory.GetCurrentDirectory() + @"\Contents\Images\Profile\" + imagepath;
            }
            byte[] bts = System.IO.File.ReadAllBytes(imagepath);
            return "data:image/png;base64," + Convert.ToBase64String(bts, 0, bts.Length).ToString();
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
        public async Task<UserParam> GetPartUsr(int id)
        {
            _bContext = new BloggingContext();
            var qst = await _bContext.Questions.FirstOrDefaultAsync(x => x.Qid == Convert.ToInt32(id));
            var usr = await _bContext.Users.FirstOrDefaultAsync(x => x.UsId == qst.AskedBy);
            var users_ = Crntusr.param.FirstOrDefault(x => x.Username == usr.EmailId);
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