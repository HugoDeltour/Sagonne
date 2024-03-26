using DatabaseFunctions;

namespace Sagonne.DataBase.Table
{
    public class Anniversaire : TableBase<Anniversaire>
    {
        public int ID_ANNIVERSAIRE { get; set; }
        public string NOM_ANNIVERSAIRE { get; set; }
        public DateTime DATE {get; set; }
    }
}
