namespace TechnikumPracticeDepartment.Models.ModelsEmployeesPages
{
    public class FilterViewModel_Employees
    {
        public FilterViewModel_Employees(int? typeSortListEmployees, int? role, int? activate, string search)
        {
            TypeSortListEmployees = typeSortListEmployees;
            Role = role;
            Activate = activate;
            Search = search;
        }
        public int? TypeSortListEmployees { get; private set; }
        public int? Role { get; private set; }
        public int? Activate { get; private set; }
        public string Search { get; private set; }
    }
}
