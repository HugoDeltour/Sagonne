using DatabaseFunctions;

namespace Sagonne.DataBase.Table
{
    public class Anniversaire : TableBase<Evenement>
    {
        public int ID { get; set; }
        public string NOM { get; set; }
        public DateTime DATE {get; set; }
    }
}
