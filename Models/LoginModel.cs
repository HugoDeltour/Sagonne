using Sagonne.DataBase.Table;

namespace Sagonne.Models
{
    public class LoginModel
    {
        public string Login { get; set; }
        public string Mdp { get; set; }
        public bool KeepLoggedIn { get; set; }
        public string Phrase { get; set; }
    }
}