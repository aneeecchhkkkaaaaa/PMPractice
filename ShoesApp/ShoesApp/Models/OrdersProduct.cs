using System;
using System.Collections.Generic;

namespace ShoesApp.Models;

public partial class OrdersProduct
{
    public int OrdersProductsId { get; set; }

    public int OrdersId { get; set; }

    public string ProductsId { get; set; } = null!;

    public int Quantity { get; set; }

    public virtual Order Orders { get; set; } = null!;

    public virtual Product Products { get; set; } = null!;
}
