namespace MiracleLandBE.MinimalModels
{
    public class CsOrdersRequest
    {
        public string token { get; set; }
        public Guid Orderid { get; set; }

        public Guid Uid { get; set; }

        public decimal Total { get; set; }

        public bool IsPayment { get; set; }

        public string? ShipId { get; set; }

        public List<CsOrderDetailRequest> CsOrderDetails { get; set; } = new List<CsOrderDetailRequest>();
    }

    public class CsOrderDetailRequest
    {
        public Guid Odid { get; set; }

        public Guid Orderid { get; set; }

        public Guid Pid { get; set; }
        
        public string Pname { get; set; } 
        public string Pimg { get; set; } 

        public int Quantity { get; set; }
    }
}
