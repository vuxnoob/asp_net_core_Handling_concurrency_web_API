using System;

namespace asp_net_core_Handling_concurrency_web_API.Model
{
    public class Product
    {
        public Guid ProductId { get; set; }
        public string ProductName { get; set; }
        public decimal UnitPrice { get; set; }
        public int UnitsInStock { get; set; }
        public byte[] Version { get; internal set; }
    }
}
