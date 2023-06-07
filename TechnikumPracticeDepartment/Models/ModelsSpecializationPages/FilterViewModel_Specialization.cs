using Microsoft.AspNetCore.Mvc.Rendering;

namespace TechnikumPracticeDepartment.Models.ModelsSpecializationPages
{
    public class FilterViewModel_Specialization
    {
        public FilterViewModel_Specialization(int? typeSortListSpecialization, string search)
        {
            TypeSortListSpecialization = typeSortListSpecialization;
            Search = search;
        }
        public int? TypeSortListSpecialization { get; private set; }   
        public string Search { get; private set; }    
    }
}
