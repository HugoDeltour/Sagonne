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
using Microsoft.AspNetCore.DataProtection.KeyManagement;
using System.IO;
using System.Text;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;

namespace Sagonne.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private static string _fileName;
        private static string _filePath;
        

        public HomeController(ILogger<HomeController> logger)
        {
            _logger = logger;
        }

        public async Task<ActionResult> LogOut()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            return RedirectToAction("Login","Account");  
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
                    new List<MySqlParameter> { new MySqlParameter("@DATE", dateEvent)});

                Dictionary<string, string> Event = new Dictionary<string, string>();
                if (evenements.Any())
                {
                    foreach(Evenement ev in evenements)
                    {
                        ev.DESCRIPTION = $"De {ev.HEURE_DEBUT} à {ev.HEURE_FIN} : {ev.DESCRIPTION}";
                        Event.Add(ev.NOM,ev.DESCRIPTION??"");
                    }
                }

                DateTime date = new DateTime();
                dateEvent = date.AddYears(1969).AddMonths(dateEvent.Month - 1).AddDays(dateEvent.Day - 1);

                IEnumerable<Anniversaire> Anniversaires = await Database.ExecuteReader<Anniversaire>(
                   "SELECT * FROM ANNIVERSAIRE WHERE DATE=@DATE",
                   new List<MySqlParameter> { new MySqlParameter("@DATE", dateEvent) });
                if (Anniversaires.Any())
                {
                    foreach (Anniversaire anniversaire in Anniversaires)
                    {
                        Event.Add(anniversaire.NOM_ANNIVERSAIRE, "");
                    }
                }

                string eventToHtml="";
                foreach (KeyValuePair<string, string> kvp in Event)
                {
                    eventToHtml += $"<div>{kvp.Key}<br>{(kvp.Value !="" ? $"<small>{kvp.Value}</small>":"")}</div>";
                }

                return Json(new { success = true, Event = eventToHtml, dateEvent = dateEvent });
            }
            catch
            {
                return Json(new { success = false });
            }
        }

        [Authorize(Policy = "IsAdmin")]
        public IActionResult Administration()
        {
            AdministrationModel model = new AdministrationModel()
            {
                Phrase = ""
            };
            return View(model);
        }

        [Authorize(Policy = "IsAdmin")]
        [HttpPost]
        public async Task<IActionResult> FileUploadAsync(IFormFile[] Files)
        {
            AdministrationModel model = new AdministrationModel();

            if(Files != null && Files.Count() > 0)
            {
                foreach(var File in Files)
                {
                    if (File != null)
                    {
                        _fileName = File.FileName;
                        _filePath = "wwwroot/images/" + _fileName;
                        var path = Path.Combine(System.IO.Directory.GetCurrentDirectory(), "wwwroot/images", _fileName);

                        var stream = new FileStream(path, FileMode.Create);
                        await File.CopyToAsync(stream);
                        stream.Close();

                        string destFileName = "wwwroot/images/" + DateTime.Now.Year + "/";

                        IEnumerable<MetadataExtractor.Directory> directories = ImageMetadataReader.ReadMetadata("wwwroot/images/"+ _fileName);
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
                                        destFileName = "wwwroot/images/" + d.Year + "/";
                                    }
                                }
                            }
                        }

                        if (!System.IO.Directory.Exists(destFileName))
                        {
                            System.IO.Directory.CreateDirectory(destFileName);
                        }
                        if (!System.IO.File.Exists(destFileName + _fileName))
                        {
                            System.IO.File.Move(path, destFileName + _fileName);
                            model.Phrase = $"Image {_fileName} ajouté";
                        }
                        else
                        {
                            model.Phrase = $"Image {_fileName} déjà existant";
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

        public async Task<ActionResult> PhotoAnnee(int annee=0)
        {
            PhotoAnneeModel model = new PhotoAnneeModel()
            {
                Annee = annee,
                Photos = new Dictionary<string, string>()
            };
            List<string> images = System.IO.Directory.GetFiles("wwwroot/images/" + annee).ToList();
            int i = 0;
            foreach(string image in images)
            {
                string key = $"{i}";
                string value = image.Replace("wwwroot", "");
                model.Photos.Add(key, value);
                i++;
            }

            return View(model);
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}