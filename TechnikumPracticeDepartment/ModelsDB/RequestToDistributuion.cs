using System;
using System.Collections.Generic;

namespace TechnikumPracticeDepartment.ModelsDB
{
    public partial class RequestToDistributuion
    {
        public int IdRequest { get; set; }
        public int StudentId { get; set; }
        public string InnOrganization { get; set; } = null!;
        public string NotFullNameOrganization { get; set; } = null!;
        public string FullNameOrganization { get; set; } = null!;
        public string SurnameContactNameOrganization { get; set; } = null!;
        public string NameContactNameOrganization { get; set; } = null!;
        public string? PatronymicNameContactNameOrganization { get; set; }
        public string EmailContactNameOrganization { get; set; } = null!;
        public string PhoneNumberContactNameOrganization { get; set; } = null!;
        public string AddressOrganization { get; set; } = null!;
        public int StatusReuqest { get; set; }
        public int? EmployeeOfTechnikumId { get; set; }

        public virtual User? EmployeeOfTechnikum { get; set; }
        public virtual Student Student { get; set; } = null!;
    }
}
