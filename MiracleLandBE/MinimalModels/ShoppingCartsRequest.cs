namespace MiracleLandBE.MinimalModels
{
    public class ShoppingCartsRequest
    {
        public Guid Cartitemid { get; set; }

        public Guid Uid { get; set; }

        public Guid Pid { get; set; }

        public int Pquantity { get; set; }
    }

    public class ShoppingCartsGet
    {
        public Guid Cartitemid { get; set;}

        public Guid Pid { get; set;}

        public decimal Pprice { get; set; }

        public int Pquantity { get; set; }
    }
}
