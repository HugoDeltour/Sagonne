using DatabaseFunctions;

namespace Sagonne.DataBase.Table
{
    public class Evenement : TableBase<Evenement>
    {
        public int ID { get; set; }
        public string NOM { get; set; }
        public string? DESCRIPTION { get; set; }
        public DateTime DATE_DEBUT { get; set; }
        public DateTime DATE_FIN { get; set; }
        public string? HEURE_DEBUT { get; set; }
        public string? HEURE_FIN { get; set; }

    }
}
