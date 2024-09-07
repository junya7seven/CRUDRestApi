namespace CRUDRestApi.DataBase.Models
{
    public class UserHistoryChange
    {
        public int Change_Id {  get; set; }
        public int User_Id { get; set; }
        public string ChangedColumn { get; set; }
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public DateTime DateTime { get; set; }
    }
}
