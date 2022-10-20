using DataBase;
using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;
using MySql.Data.MySqlClient;
using Sagonne.DataBase.Table;
using Sagonne.Models;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Net;
using static System.Net.Mime.MediaTypeNames;
using DatabaseFunctions;

namespace Sagonne.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public async Task<ActionResult> Index()
        {
            int height = 0;
            String[] images = Directory.GetFiles("wwwroot/images/Caroussel/");
            foreach(String file in images)
            {
                FileInfo fileInfo = new FileInfo(file);
                var sizeInBytes = fileInfo.Length;

                Bitmap img = new Bitmap(file);

                if (img.Height<height || height == 0)
                {
                    height = img.Height;
                }
            }

            //suppression du "wwwroot/"
            int i = 0;
            foreach (String image in images)
            {
                images[i] = image.Replace("wwwroot/", "");
                i++;
            }

            IndexModel model = new IndexModel
            {
                min_height = "" + height + "px",
                caroussel = images
            };

            return View(model);
        }

        [HttpPost]
        public async Task<JsonResult> GetEvenement(DateTime dateEvent)
        {
            try
            {
                IEnumerable<Evenement> evenements = await Database.ExecuteReader<Evenement>(
                    "SELECT * FROM EVENT WHERE DATE_DEBUT<=@DATE AND DATE_FIN>=@DATE",
                    new List<MySqlParameter> { new MySqlParameter("@DATE",dateEvent)});

                string Event = "";
                if (evenements.Any())
                {
                    foreach(Evenement ev in evenements)
                    {
                        Event += ev.NOM ;
                    }
                }

                return Json(new { success = true, Event = Event, dateEvent = dateEvent });
            }
            catch
            {
                return Json(new { success = false });
            }
        }

        public IActionResult FileUpload()
        {
            return View();
        }

        [HttpPost]
        public IActionResult FileUpload(IFormFile[] Files)
        {
            if(Files != null && Files.Count() > 0)
            {
                foreach(var File in Files)
                {
                    if (File != null)
                    {
                        string FileName = File.FileName;
                        var path = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/images", FileName);

                        var stream = new FileStream(path, FileMode.Create);
                        File.CopyToAsync(stream);
                    }
                }
            }

            return View();
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}