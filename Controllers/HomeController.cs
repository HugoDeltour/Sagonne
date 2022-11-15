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
using System.Drawing.Imaging;
using MetadataExtractor;
using MetadataExtractor.Formats.FileSystem;
using MetadataExtractor.Formats.Exif;
using System.Globalization;
using Extensions;

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
            String[] images = System.IO.Directory.GetFiles("wwwroot/images/Caroussel/");
            foreach(String file in images)
            {
                FileInfo fileInfo = new FileInfo(file);
                var sizeInBytes = fileInfo.Length;

                Bitmap img = new(file);

                if (img.Height < height || height == 0)
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
                MIN_HEIGHT = "" + height + "px",
                Caroussel = images
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
            FileUploadModel model = new FileUploadModel()
            {
                Phrase = ""
            };
            return View(model);
        }

        [HttpPost]
        public IActionResult FileUpload(IFormFile[] Files)
        {
            FileUploadModel model = new FileUploadModel();

            if(Files != null && Files.Count() > 0)
            {
                foreach(var File in Files)
                {
                    if (File != null)
                    {
                        string FileName = File.FileName;                        
                        var path = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot/images", FileName);

                        var stream = new FileStream(path, FileMode.Create);
                        File.CopyToAsync(stream);
                        stream.Close();

                        IEnumerable<MetadataExtractor.Directory> directories = ImageMetadataReader.ReadMetadata("wwwroot/images/"+FileName);
                        foreach (var directory in directories)
                        {
                            foreach (var tag in directory.Tags)
                            {
                                if (tag.Name.Contains("Date/Time"))
                                {
                                    //string pattern = "ddd MMM dd H:mm:ss K yyyy";
                                    string pattern = "yyyy:MM:dd H:mm:ss";
                                    DateTime d;

                                    if (DateTime.TryParseExact(tag.Description, pattern, null, DateTimeStyles.None, out d))
                                    {
                                        string destFileName = "wwwroot/images/" + d.Year + "/";
                                        if (!System.IO.Directory.Exists(destFileName))
                                        {
                                            System.IO.Directory.CreateDirectory(destFileName);
                                        }
                                        if(!System.IO.File.Exists(destFileName + FileName))
                                        {
                                            System.IO.File.Move(path, destFileName + FileName);
                                            model.Phrase = "Fichier ajouté";
                                        }
                                        else
                                        {
                                            model.Phrase = "Fichier déjà existant";
                                        }
                                    }
                                }
                            }
                        }                        
                    }
                }
            }

            return View(model);
        }
        public async Task<ActionResult> Portofolio()
        {
            PortofolioModel model = new PortofolioModel()
            {
                Annees = new Dictionary<string, string>()
            };

            string[] dossiers = System.IO.Directory.GetDirectories(Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot/images"));

            foreach (string dossier in dossiers)
            {
                string[] year = dossier.Split("\\");
                int annee;
                string Key = year[year.Length - 1];
                if (Int32.TryParse(Key, out annee))
                {
                    List<string> images = System.IO.Directory.GetFiles("wwwroot/images/" + Key).ToList();
                    if(images.Count() > 0)
                    {
                        images.Shuffle();
                        model.Annees.Add(Key, images.First().Replace("wwwroot", ""));
                    }
                }
            }

            return View(model);
        }

        public async Task<ActionResult> PhotoAnnee()
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