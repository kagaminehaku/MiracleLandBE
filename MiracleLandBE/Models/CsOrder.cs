using System;
using System.Collections.Generic;

namespace MiracleLandBE;

public partial class CsOrder
{
    public Guid Orderid { get; set; }

    public Guid Uid { get; set; }

    public decimal Total { get; set; }

    public bool IsPayment { get; set; }

    public string? ShipId { get; set; }

    public virtual ICollection<CsOrderDetail> CsOrderDetails { get; set; } = new List<CsOrderDetail>();
}
