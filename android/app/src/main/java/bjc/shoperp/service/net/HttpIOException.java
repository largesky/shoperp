package bjc.shoperp.service.net;

import java.util.HashMap;

import bjc.shoperp.utils.GsonUtil;

/**
 * Created by hcq on 2018/2/15.
 */

public class HttpIOException extends Exception{
    private String url;

    private HashMap<String,Object> params;

    private String innerMessage;

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

    public String getInnerMessage() {
        return innerMessage;
    }

    public void setInnerMessage(String innerMessage) {
        this.innerMessage = innerMessage;
    }

    public HttpIOException(String url,HashMap<String,Object> params,String innerMessage){
        super(innerMessage);
        this.url=url;
        this.params=params;
        this.innerMessage=innerMessage;
    }

    @Override
    public String toString(){
        StringBuilder sb=new StringBuilder("错误信息:"+this.getMessage()+"\r\n");
        sb.append("请求网址:"+url+"\r\n");
        sb.append("请求参数:");
        if(params!=null && params.size()>0){
            sb.append( GsonUtil.getGson().toJson(params));
        }
        return  sb.toString();
    }


}