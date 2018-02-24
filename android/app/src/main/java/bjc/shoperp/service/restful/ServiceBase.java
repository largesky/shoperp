package bjc.shoperp.service.restful;

import java.util.Date;
import java.util.HashMap;

import bjc.shoperp.domain.restfulresponse.DataCollectionResponse;
import bjc.shoperp.domain.restfulresponse.LongResponse;
import bjc.shoperp.domain.restfulresponse.ResponseBase;
import bjc.shoperp.service.net.HttpRestful;
import bjc.shoperp.utils.GsonUtil;

/**
 * Created by hcq on 2018/2/15.
 */
public abstract class ServiceBase<E> {

    static Date dbMinTime = new Date( 1970, 01, 01, 0, 0, 0 );

    static HashMap<String, String> default_headers = new HashMap<>();

    public Class<E> c;

    private static <T extends ResponseBase> T DeserializeObject(Class<T> tClass, String json) throws Exception {

        if (json == null || json.isEmpty()) {
            throw new Exception( "服务端返回空JSON数据" );
        }
        T ret = null;
        try {
            ret = GsonUtil.getGson().fromJson( json, tClass );
        } catch (Exception ex) {
            throw ex;
        }
        if (ResponseBase.SUCCESS.error.equalsIgnoreCase(  ret.error)==false) {
            throw new Exception( ret.error );
        }
        return ret;
    }

    public static <T extends ResponseBase> T DoPost(Class<T> tClass, HashMap<String, Object> para, HashMap<String, String> headers) throws Exception {
        StackTraceElement ste=(new Throwable()).getStackTrace()[1];
        Class c=Class.forName( ste.getClassName() );
        String apiUrl = c.getSimpleName().toLowerCase().replace( "service","" ) +"/" +ste.getMethodName().toLowerCase() + ".html";
        return DoPostWithUrl( tClass, apiUrl, para, headers );
    }

    public static <T extends ResponseBase> T DoPostWithUrl(Class<T> tClass, String url, HashMap<String, Object> para, HashMap<String, String> headers) throws Exception {
        if (headers == null) {
            headers = default_headers;
        }
        headers.put( "session", ServiceContainer.AccessToken );
        String json = HttpRestful.PostJsonBodyAndReturnString( ServiceContainer.ServerAddress + "/" + url, para, headers );
        return DeserializeObject( tClass, json );
    }

    public <T extends DataCollectionResponse> E GetById(Class<T> tClass, Object id) throws Exception {
        HashMap<String, Object> para = new HashMap<String, Object>();
        para.put( "id", id );
        DataCollectionResponse<E> ret = DoPost( tClass, para, null );
        if (ret.Datas == null || ret.Datas.size() < 1) {
            return null;
        }
        return ret.Datas.get( 0 );
    }

    public long Save(E e) throws Exception {
        HashMap<String, Object> para = new HashMap<>();
        para.put( "value", e );
        LongResponse ret = DoPost( LongResponse.class, para, null );
        return ret.data;
    }

    public long Update(E e) throws Exception {
        HashMap<String, Object> para = new HashMap<>();
        para.put( "value", e );
        LongResponse ret = DoPost( LongResponse.class, para, null );
        return ret.data;
    }

    public void Delete(long id) throws Exception {
        HashMap<String, Object> para = new HashMap<>();
        para.put( "value", id );
        DoPost( ResponseBase.class, para, null );
    }

    public Date GetDBMinTime() {
        return dbMinTime;
    }

    public boolean IsDBMinTime(Date date) {
        return date.after( dbMinTime );
    }
}