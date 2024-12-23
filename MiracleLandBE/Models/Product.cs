using System;
using System.Collections.Generic;

namespace MiracleLandBE.Models;

public partial class Product
{
    public Guid Pid { get; set; }

    public string Pname { get; set; } = null!;

    public decimal Pprice { get; set; }

    public int Pquantity { get; set; }

    public string Pinfo { get; set; } = null!;

    public string Pimg { get; set; } = null!;

    public virtual ICollection<CsOrderDetail> CsOrderDetails { get; set; } = new List<CsOrderDetail>();

    public virtual ICollection<ShoppingCart> ShoppingCarts { get; set; } = new List<ShoppingCart>();
}
