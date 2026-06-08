using System;
using System.Collections.Generic;

namespace ShoesApi.Models;

public partial class Role
{
    public int RoleId { get; set; }

    public string RolesName { get; set; } = null!;

    public virtual ICollection<User> Users { get; set; } = new List<User>();
}
