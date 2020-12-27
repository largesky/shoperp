using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ShopErp.App.Domain.TaobaoHtml.Goods
{
    class TitleConverter : Newtonsoft.Json.JsonConverter
    {
        public override bool CanConvert(Type objectType)
        {
            return (objectType == typeof(string));
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            Newtonsoft.Json.Linq.JToken tokens = Newtonsoft.Json.Linq.JToken.Load(reader);
            if (tokens.Type == Newtonsoft.Json.Linq.JTokenType.String)
            {
                return tokens.ToString();
            }
            if (tokens.Type == Newtonsoft.Json.Linq.JTokenType.Object)
            {
                return tokens.First.First.First.ToString();
            }

            throw new Exception("TitleConverter:无法转换" + tokens.ToString());
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotImplementedException();
        }
    }
}
