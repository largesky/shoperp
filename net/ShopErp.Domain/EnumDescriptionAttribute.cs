using System;

namespace ShopErp.Domain
{
    [AttributeUsage(AttributeTargets.Field)]
    public class EnumDescriptionAttribute : Attribute
    {
        public string Description { get; set; }

        public EnumDescriptionAttribute(string description)
        {
            this.Description = description;
        }
    }
}
