using System;
using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace ShoesApi.Models;

public partial class Supplier
{
    public int SupplierId { get; set; }

    public string SupplierName { get; set; } = null!;
    [JsonIgnore]

    public virtual ICollection<Product> Products { get; set; } = new List<Product>();
}
