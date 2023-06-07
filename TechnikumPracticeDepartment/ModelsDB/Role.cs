using System;
using System.Collections.Generic;

namespace TechnikumPracticeDepartment.ModelsDB
{
    public partial class Role
    {
        public Role()
        {
            UsersRoles = new HashSet<UsersRole>();
        }

        public int IdRole { get; set; }
        public string NameRole { get; set; } = null!;

        public virtual ICollection<UsersRole> UsersRoles { get; set; }
    }
}
