package bjc.shoperp.domain.restfulresponse;

import java.util.ArrayList;
import java.util.List;

import bjc.shoperp.domain.DeliveryOut;

/**
 * Created by hcq on 2018/2/15.
 */

public class DataCollectionResponse<T> extends ResponseBase {

    public int Total;

    public ArrayList<T> Datas = new ArrayList<T>();

    public DataCollectionResponse() {
    }

    /// <summary>
/// 参数可为空
/// </summary>
/// <param name="t"></param>
    public DataCollectionResponse(T t) {
        if (t == null) {
            this.Total = 0;
        } else {
            this.Total = 1;
            this.Datas.add(t);
        }
    }
}

