package bjc.shoperp.service.net;

import com.google.gson.JsonDeserializationContext;
import com.google.gson.JsonDeserializer;
import com.google.gson.JsonElement;
import com.google.gson.JsonPrimitive;
import com.google.gson.JsonSerializationContext;
import com.google.gson.JsonSerializer;

import java.lang.reflect.Type;
import java.util.Date;

/**
 * Created by hcq on 2018/2/15.
 */


public class DotNetDateSerializer implements JsonSerializer<Date>, JsonDeserializer<Date> {
    @Override
    public JsonElement serialize(java.util.Date date, Type typfOfT, JsonSerializationContext context) {
        if (date == null)
            return null;
        String dateStr = "/Date(" + date.getTime() + "+0800)/";
        return new JsonPrimitive( dateStr );
    }

    @Override
    public java.util.Date deserialize(JsonElement json, Type typfOfT, JsonDeserializationContext context) {
        try {
            String str = json.getAsString();
            String dateStr = str.replace( "/Date(", "" ).replace( ")/", "" );
            String[] ss = dateStr.split( "[+-]" );

            if (ss.length == 0) {
                throw new Exception( "时间数据错误，无法解析：" + str );
            }
            if (ss.length == 1) {
                long time = Long.parseLong( ss[0] );
                Date t = new Date( time );
                return t;
            }
            long time = Long.parseLong( ss[0] );
            Date t=new Date( time );
            return t;
        } catch (Exception ex) {
            ex.printStackTrace();
            return null;
        }
    }
}
