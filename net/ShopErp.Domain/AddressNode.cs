using System.Collections.Generic;

namespace ShopErp.Domain
{
    public class AddressNode
    {

        public string Name { get; set; }

        public string ShortName { get; set; }

        public List<AddressNode> SubNodes { get; set; }

        public AddressNode()
        {
            this.SubNodes = new List<AddressNode>();
        }

        public override string ToString()
        {
            return string.Format("{0} {1} {2}", this.GetType().Name, Name, ShortName);
        }
    }
}
