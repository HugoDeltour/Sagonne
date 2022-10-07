using Microsoft.AspNetCore.Hosting.Server;
using Microsoft.AspNetCore.Mvc;
using Sagonne.Models;
using System;
using System.Diagnostics;
using System.Net;
using static System.Net.Mime.MediaTypeNames;

namespace Sagonne.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public IActionResult Index()
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

                        //string url = "/images/" + FileName;
                    }

                }
            }

            return RedirectToAction("Index");
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