using System;
using System.Collections.Generic;

namespace MiracleLandBE.Models;

public partial class OrderDetail
{
    public Guid Odid { get; set; }

    public Guid Orderid { get; set; }

    public int Pid { get; set; }

    public int Quantity { get; set; }

    public virtual Order Order { get; set; } = null!;

    public virtual Product OrderNavigation { get; set; } = null!;
}
