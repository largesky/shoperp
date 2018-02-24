package bjc.shoperp.domain.restfulresponse.domainresponse;

import java.util.Date;

import bjc.shoperp.domain.Operator;
import bjc.shoperp.domain.restfulresponse.ResponseBase;

/**
 * Created by hcq on 2018/2/15.
 */

public class LoginResponse  extends ResponseBase {

    public String session;

    public Date loginTime;

    public Date lastOperateTime;

    public Operator op;
}