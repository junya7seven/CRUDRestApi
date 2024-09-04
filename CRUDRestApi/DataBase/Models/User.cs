using System.ComponentModel.DataAnnotations;

namespace CRUDRestApi.DataBase.Models
{
    public class User
    {
        public string User_Id { get; set; }
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public DateTime date { get; set; }
    }
}
