namespace ShopErp.Domain
{
    public class DeliveryTemplateArea
    {
        public long Id { get; set; }

        public long DeliveryTemplateId { get; set; }

        public string Areas { get; set; }

        public float StartWeight { get; set; }

        public float StartPrice { get; set; }

        public float StepWeight { get; set; }

        public float StepPrice { get; set; }
    }
}
