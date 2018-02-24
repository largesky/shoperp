package bjc.shoperp.domain.restfulresponse;

import android.os.Build;
import android.support.annotation.RequiresApi;

/**
 * Created by hcq on 2018/2/15.
 */

public class ResponseBase
{
    public static  ResponseBase SUCCESS = new ResponseBase();

    public String error;

    public ResponseBase()
    {
        this.error = "success";
    }

    public ResponseBase(String error)
    {
        this.error = error;
    }

    @RequiresApi(api = Build.VERSION_CODES.KITKAT)
    public ResponseBase(Exception ex)
    {
        Throwable e = ex;
        Throwable[] ths=e.getSuppressed();
        if(ths!=null&&ths.length>0){
            e=ths[ths.length-1];
        }
        this.error = e.getMessage();
    }
}
