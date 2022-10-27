using Sagonne.DataBase.Table;

namespace Sagonne.Models
{
    public class IndexModel
    {
        public string MIN_HEIGHT { get; set; }
        public string[] Caroussel { get; set; }
        public IEnumerable<Evenement> Evenements { get; set; }
    }
}