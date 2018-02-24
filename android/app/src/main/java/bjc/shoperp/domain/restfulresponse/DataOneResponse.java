package bjc.shoperp.domain.restfulresponse;

/**
 * Created by hcq on 2018/2/19.
 */

public class DataOneResponse<T> extends ResponseBase {

    public T data;

    public DataOneResponse() {
    }

    public DataOneResponse(T data) {
        this.data = data;
    }

}
