using Sagonne.DataBase.Table;

namespace Sagonne.Models
{
    public class IndexModel
    {
        public string min_height { get; set; }
        public string[] caroussel { get; set; }
        public IEnumerable<Evenement> Evenements { get; set; }
    }
}