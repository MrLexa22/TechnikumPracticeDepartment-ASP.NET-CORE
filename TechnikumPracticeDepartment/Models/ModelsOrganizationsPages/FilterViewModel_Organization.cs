namespace TechnikumPracticeDepartment.Models.ModelsOrganizationsPages
{
    public class FilterViewModel_Organization
    {
        public FilterViewModel_Organization(int? typeSortList, int? filterAvaliable, string search)
        {
            TypeSortList = typeSortList;
            FilterAvaliable = filterAvaliable;
            Search = search;
        }
        public int? TypeSortList { get; private set; }
        public int? FilterAvaliable { get; private set; }
        public string Search { get; private set; }
    }
}
