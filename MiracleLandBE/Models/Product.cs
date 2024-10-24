using System;
using System.Collections.Generic;

namespace MiracleLandBE.Models;

public partial class Product
{
    public Guid Pid { get; set; }

    public string Pname { get; set; } = null!;

    public int Pprice { get; set; }

    public int Pquantity { get; set; }

    public string Pinfo { get; set; } = null!;

    public string Pimg { get; set; } = null!;

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual ICollection<ShoppingCart> ShoppingCarts { get; set; } = new List<ShoppingCart>();
}
