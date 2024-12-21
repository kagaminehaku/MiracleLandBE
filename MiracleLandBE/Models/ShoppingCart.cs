using System;
using System.Collections.Generic;

namespace MiracleLandBE;

public partial class ShoppingCart
{
    public Guid Cartitemid { get; set; }

    public Guid Uid { get; set; }

    public Guid Pid { get; set; }

    public int Pquantity { get; set; }

    public virtual UserAccount Pid1 { get; set; } = null!;

    public virtual Product PidNavigation { get; set; } = null!;
}
