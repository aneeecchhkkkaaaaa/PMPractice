using System;
using System.Collections.Generic;

namespace ShoesApp.Models;

public partial class Address
{
    public int AddressId { get; set; }

    public string PostalCode { get; set; } = null!;

    public string CityName { get; set; } = null!;

    public string StreetName { get; set; } = null!;

    public string? BuildingNumber { get; set; }

    public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
}
