using System;
using System.Collections.Generic;

namespace MiracleLandBE.Models;

public partial class Order
{
    public Guid Orderid { get; set; }

    public Guid Uid { get; set; }

    public long Total { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();
}
