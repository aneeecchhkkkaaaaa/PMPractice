using System;
using System.Collections.Generic;

namespace ShoesApp.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public DateOnly OrderDate { get; set; }

    public DateOnly DeliveryDate { get; set; }

    public int AddressId { get; set; }

    public int UserId { get; set; }

    public int СodeForReceipt { get; set; }

    public int StatusId { get; set; }

    public virtual Address Address { get; set; } = null!;

    public virtual ICollection<OrdersProduct> OrdersProducts { get; set; } = new List<OrdersProduct>();

    public virtual OrderStatus Status { get; set; } = null!;

    public virtual User User { get; set; } = null!;
}
