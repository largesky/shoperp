using System;

namespace ShopErp.Domain
{
    public class Vendor : ICloneable
    {
        public virtual long Id { get; set; }
        public virtual string Name { get; set; }
        public virtual string PingyingName { get; set; }
        public virtual string Phone { get; set; }
        public virtual string MarketAddress { get; set; }
        public virtual string HomePage { get; set; }
        public virtual DateTime CreateTime { get; set; }
        public bool Watch { get; set; }
        public int Count { get; set; }
        public int AveragePrice { get; set; }
        public string Comment { get; set; }
        public string Alias { get; set; }

        public override string ToString()
        {
            return this.Name + " " + this.HomePage;
        }

        public object Clone()
        {
            return base.MemberwiseClone();
        }
    }
}
