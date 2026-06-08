using System;
using System.Collections.Generic;

namespace ShoesApi.Models;

public partial class Product
{
    public string ProductId { get; set; } = null!;

    public string ProductName { get; set; } = null!;

    public string UnitOfMeasurement { get; set; } = null!;

    public decimal Price { get; set; }

    public int SupplierId { get; set; }

    public int ManufacturerId { get; set; }

    public string Category { get; set; } = null!;

    public int Current { get; set; }

    public int InWarehouse { get; set; }

    public string? Description { get; set; }

    public string? Photo { get; set; }

    public virtual Manufacturer Manufacturer { get; set; } = null!;

    public virtual ICollection<OrdersProduct> OrdersProducts { get; set; } = new List<OrdersProduct>();

    public virtual Supplier Supplier { get; set; } = null!;
}
