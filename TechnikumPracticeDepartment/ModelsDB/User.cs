using System;
using System.Collections.Generic;

namespace TechnikumPracticeDepartment.ModelsDB
{
    public partial class User
    {
        public User()
        {
            RequestToDistributuions = new HashSet<RequestToDistributuion>();
            UsersRoles = new HashSet<UsersRole>();
        }

        public int IdUser { get; set; }
        public string Email { get; set; } = null!;
        public string? Password { get; set; }
        public string SurnameUser { get; set; } = null!;
        public string NameUser { get; set; } = null!;
        public string? PatronymicNameUser { get; set; }
        public bool? IsAvaliable { get; set; }
        public bool? Fz152 { get; set; }

        public virtual EmployeeOfOrganization EmployeeOfOrganization { get; set; } = null!;
        public virtual Student Student { get; set; } = null!;
        public virtual ICollection<RequestToDistributuion> RequestToDistributuions { get; set; }
        public virtual ICollection<UsersRole> UsersRoles { get; set; }
    }
}
