package bjc.shoperp.service.net;

import java.util.HashMap;

import bjc.shoperp.utils.GsonUtil;

/**
 * Created by hcq on 2018/2/15.
 */

public class HttpFunctionException extends Exception {

    private int httpResponseCode;

    private String url;

    private HashMap<String,Object> params;

    public int getHttpResponseCode() {
        return httpResponseCode;
    }

    public void setHttpResponseCode(int httpResponseCode) {
        this.httpResponseCode = httpResponseCode;
    }

    public String getUrl() {
        return url;
    }

    public void setUrl(String url) {
        this.url = url;
    }

    public HashMap<String, Object> getParams() {
        return params;
    }

    public void setParams(HashMap<String, Object> params) {
        this.params = params;
    }

    public HttpFunctionException(String url, int httpResponseCode, HashMap<String,Object> params){
        super("服务端返回错误:"+httpResponseCode);
        this.httpResponseCode=httpResponseCode;
        this.url=url;
        this.params=params;
    }

    @Override
    public String toString(){
        StringBuilder sb=new StringBuilder("请求网址:"+url+"\r\n");
        sb.append("返回状态:"+httpResponseCode+"\r\n");
        sb.append("请求参数:");
        if(params!=null && params.size()>0){
            sb.append( GsonUtil.getGson().toJson(params));
        }
        return  sb.toString();
    }
}