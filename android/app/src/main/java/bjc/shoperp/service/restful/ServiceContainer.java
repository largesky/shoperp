package bjc.shoperp.service.restful;

import java.lang.reflect.Type;
import java.util.ArrayList;

import static java.lang.System.in;

/**
 * Created by hcq on 2018/2/15.
 */
public class ServiceContainer {

    public static String ServerAddress = null;

    public static String AccessToken ="";

    static ArrayList<Object> services = new ArrayList<Object>();

    public static <T > T  GetService(Class<T> c) throws Exception {
        String cname=c.getName();
        if(cname.endsWith("Service")==false ){
            throw new Exception("类型："+cname+"名称结尾不为Service无法创建");
        }
        for (Object obj : services) {
            if(obj.getClass()==c){
                return (T)obj;
            }
        }
        T t =c.newInstance();
        services.add(t);
        return  t;
    }
}
