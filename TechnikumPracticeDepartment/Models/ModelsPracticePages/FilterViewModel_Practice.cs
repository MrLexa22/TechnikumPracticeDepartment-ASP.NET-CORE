namespace TechnikumPracticeDepartment.Models.ModelsPracticePages
{
    public class FilterViewModel_Practice
    {
        public FilterViewModel_Practice(int? typeSortList, int? filterListBySpecializaion, string search)
        {
            TypeSortList = typeSortList;
            FilterListBySpecializaion = filterListBySpecializaion;
            Search = search;
        }
        public int? TypeSortList { get; private set; }
        public int? FilterListBySpecializaion { get; private set; }
        public string Search { get; private set; }
    }
}
