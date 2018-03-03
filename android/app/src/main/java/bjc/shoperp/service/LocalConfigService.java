package bjc.shoperp.service;

import android.content.Context;
import android.content.SharedPreferences;

/**
 * Created by hcq on 2018/2/15.
 */

public class LocalConfigService {
    public static final String CONFIG_SERVERADD="SERVERADD";
    public static final String CONFIG_GOODSCOUNTSORTTYPE="GOODSCOUNTSORTTYPE";
    public static final String CONFIG_GOODSCOUNTLASTINFO="GOODSCOUNTLASTINFO";

    public static void update(Context ctx, String name, String value){
        SharedPreferences sp= ctx.getSharedPreferences("ShopErp", Context.MODE_PRIVATE);
        SharedPreferences.Editor et=sp.edit();
        et.remove(name);
        et.putString(name,value);
        et.commit();
    }

    public static String get(Context ctx,String name,String defaultValue){
        SharedPreferences sp= ctx.getSharedPreferences("ShopErp", Context.MODE_PRIVATE);
        if(sp.contains(name)==false){
            return defaultValue;
        }
        return sp.getString(name,defaultValue);
    }
}
