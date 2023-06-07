namespace TechnikumPracticeDepartment.Models
{
    public class ModelErrorWindow
    {
        public bool IsError { get; set; }
        public string ErrorMessage { get; set; }
        public string ErrorTitle { get; set; }
        public string Login { get; set; }
        public string Password { get; set; }
        public bool? IsRecovery { get; set; }
    }
}
