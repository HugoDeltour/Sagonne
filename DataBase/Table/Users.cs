using DatabaseFunctions;

namespace Sagonne.DataBase.Table
{
    public class Users : TableBase<Users>
    {
        public int ID_USERS { get; set; }
        public string LOGIN { get; set; }
        public string MDP { get; set; }
        public bool ADMIN { get; set; }
    }
}
