using System;

namespace ShopErp.Domain
{
    public class Operator
    {
        public long Id { get; set; }
        public string Number { get; set; }
        public string Name { get; set; }
        public string Password { get; set; }
        public string Phone { get; set; }
        public string Rights { get; set; }
        public DateTime CreateTime { get; set; }
        public DateTime UpdateTime { get; set; }
        public string CreateOperator { get; set; }
        public bool Enabled { get; set; }
    }
}
