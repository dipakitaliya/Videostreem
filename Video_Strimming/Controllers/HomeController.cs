using NReco.VideoConverter;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using Video_Strimming.Models;

namespace Video_Strimming.Controllers
{
    public class HomeController : Controller
    {
        private video_streamingEntities db = new video_streamingEntities();
        [HttpGet]
        public ActionResult Index()
        {
            List<Videos> list = new List<Videos>();
            using (db = new video_streamingEntities())
            {
                list = db.VideoSteams.Select(m => new Videos
                {
                    ext = m.FileType,
                    Name = m.FileName,
                    Id = m.SrNo
                }).ToList();
            }
            return View(list);
        }

        [HttpPost]
        public ActionResult Index(HttpPostedFileBase file,FormCollection formCollection)
        {
            VideoSteam video = new VideoSteam();
            if (file != null)
            {
                string path = Server.MapPath("~/Upload/TempUpload/" + file.FileName);
                file.SaveAs(path);

                var ffmpag = new FFMpegConverter();
                var tname = Guid.NewGuid();
                string output = Server.MapPath("~/Upload/TempUpload/" + tname + ".mp4");
                ffmpag.ConvertMedia(path, output, Format.mp4);

                byte[] buffer = new byte[file.ContentLength];
                using (MemoryStream ms = new MemoryStream())
                {
                    using (FileStream tempfile = new FileStream(output, FileMode.Open, FileAccess.Read))
                    {
                        buffer = new byte[tempfile.Length];
                        tempfile.Read(buffer, 0, (int)tempfile.Length);
                        ms.Write(buffer, 0, (int)tempfile.Length);
                    }
                }
                string fname = Guid.NewGuid().ToString();
                string ftype = "mp4";
                using (db = new video_streamingEntities())
                {
                    video.FileName = formCollection["DisplayName"];
                   // video.FileName = fname;
                    video.FileType = ftype;
                    video.Video = buffer;
                    db.VideoSteams.Add(video);
                    db.SaveChanges();
                }

            }
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public EmptyResult VideoStream(long id = 0)
        {
            using (db = new video_streamingEntities())
            {
                Videos vm = new Videos();
                vm = db.VideoSteams.Where(m => m.SrNo == id).Select(m => new Videos
                {
                    Name = m.FileName,
                    ext = m.FileType,
                    Id = m.SrNo,
                    video = m.Video
                }).FirstOrDefault();
               
                HttpContext.Response.AddHeader("Content-Disposition", "attachment; filename=" + vm.Name + "." + vm.ext);
                HttpContext.Response.BinaryWrite(vm.video);

                return new EmptyResult();
            }
        }

        public ActionResult DeleteVideo(long id = 0)
        {
            if (id > 0)
            {
                using (db = new video_streamingEntities())
                {
                    VideoSteam vs = new VideoSteam();
                    vs = db.VideoSteams.Where(m => m.SrNo == id).FirstOrDefault();
                    db.VideoSteams.Remove(vs);
                    db.SaveChanges();
                }
            }
            return RedirectToAction("Index", "Home");
        }

        public JsonResult GetbyId(int id)
        {
            //Videos data = new Videos();
            using (db = new video_streamingEntities())
            {
            var  data = db.VideoSteams.Where(a=>a.SrNo == id).Select(m => new Videos
                {
                    ext = m.FileType,
                    Name = m.FileName,
                    Id = m.SrNo
                }).FirstOrDefault();
                return Json(data, JsonRequestBehavior.AllowGet);
            }
          
        }

        
    }
}