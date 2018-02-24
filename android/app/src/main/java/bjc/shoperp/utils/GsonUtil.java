package bjc.shoperp.utils;

import com.google.gson.Gson;
import com.google.gson.GsonBuilder;

import bjc.shoperp.service.net.DotNetDateSerializer;

/**
 * Created by hcq on 2018/2/16.
 */

public class GsonUtil {
    static final GsonBuilder builder;

    static {
        builder = new GsonBuilder();
        builder.registerTypeAdapter(java.util.Date.class, new DotNetDateSerializer());
    }

    public static Gson getGson(){
        return builder.create();
    }
}
