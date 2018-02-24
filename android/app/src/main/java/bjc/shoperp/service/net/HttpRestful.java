package bjc.shoperp.service.net;

import com.google.gson.Gson;
import com.google.gson.GsonBuilder;
import java.net.HttpURLConnection;
import java.net.URL;
import java.nio.charset.Charset;
import java.util.HashMap;
import java.io.*;
import bjc.shoperp.domain.restfulresponse.ResponseBase;
import bjc.shoperp.utils.GsonUtil;

/**
 * Created by hcq on 2018/2/15.
 */

public class HttpRestful {

    public  static byte[] PostJsonBodyAndReturnBytes(String url,HashMap<String,Object> param,HashMap<String,String> headers) throws  Exception{
        URL _url=new URL(url);//这里出错，则网址不对，不用管它
        try
        {
            HttpURLConnection connection=(HttpURLConnection)_url.openConnection();
            connection.setDoInput(true);
            connection.setDoOutput(true);
            connection.setRequestMethod("POST");
            connection.setConnectTimeout(5*1000);
            connection.setReadTimeout(15*1000);
            connection.setRequestProperty("Content-Type", "application/json");
            connection.setRequestProperty("Accept", "*/*");
            if(headers!=null && headers.size()>0){
                for (String key : headers.keySet()){
                    connection.setRequestProperty(key,headers.get(key));
                }
            }
            connection.getOutputStream().write(GsonUtil.getGson().toJson(param).getBytes( "UTF-8"));
            connection.getOutputStream().close();
            int responseCode=connection.getResponseCode();
            byte[] content=null;
            if (responseCode == HttpURLConnection.HTTP_OK) {
                String line;
                ByteArrayOutputStream s=new ByteArrayOutputStream();
                byte[] tmp=new byte[1024*5];
                int readed=-1;
                while ((readed=connection.getInputStream().read(tmp))>0){
                    s.write(tmp,0,readed);
                }
                content=s.toByteArray();
                s.close();
                connection.getInputStream().close();
                connection.disconnect();
            }else{
                throw  new HttpFunctionException(url,responseCode,param);
            }
            return content;
        }
        catch (Exception ex){
            throw  new HttpIOException(url,param,ex.getMessage());
        }
    }

    public  static String PostJsonBodyAndReturnString(String url,HashMap<String,Object> param,HashMap<String,String> header) throws  Exception {
        byte[] bytes=PostJsonBodyAndReturnBytes(url,param,header);
        return new String(bytes,Charset.forName("UTF-8"));
    }
}
