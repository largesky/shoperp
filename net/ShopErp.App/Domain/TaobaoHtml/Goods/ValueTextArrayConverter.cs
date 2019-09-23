using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.App.Domain.TaobaoHtml.Goods
{
    class ValueTextArrayConverter : JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(ValueTextArray));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            JToken tokens = JToken.Load(reader);
            ValueTextArray[] values = new ValueTextArray[tokens.Count()];
            for (int i = 0; i < values.Length; i++)
            {
                var eleToken = tokens.ElementAt(i);
                ValueTextArray valueTextArray = new ValueTextArray { name = eleToken.Path };
                if (eleToken.First.Type == JTokenType.Array)
                {
                    valueTextArray.values = eleToken.First.ToObject<ValueTextArrayEntry[]>();
                }
                else if (eleToken.First.Type == JTokenType.Object)
                {
                    valueTextArray.values = new ValueTextArrayEntry[1];
                    valueTextArray.values[0] = eleToken.First.ToObject<ValueTextArrayEntry>();
                }
                else
                {
                    valueTextArray.values = new ValueTextArrayEntry[1];
                    valueTextArray.values[0] = new ValueTextArrayEntry { text = eleToken.First.ToString(), value = "" };
                }
                values[i] = valueTextArray;
            }

            return values;
        }

        public override bool CanWrite
        {
            get { return false; }
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
