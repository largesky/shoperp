package bjc.shoperp.service.restful;

import java.util.HashMap;

import bjc.shoperp.domain.Operator;
import bjc.shoperp.domain.restfulresponse.domainresponse.LoginResponse;
import bjc.shoperp.utils.Md5Util;

/**
 * Created by hcq on 2018/2/15.
 */

public class OperatorService extends bjc.shoperp.service.restful.ServiceBase<Operator> {


    private static LoginResponse loginVirtual() {
        LoginResponse lr = new LoginResponse();
        lr.op = new Operator();
        lr.op.Number = "1001";

        lr.session = "abcdefg";
        return lr;
    }

    public Operator Login(String number, String password) throws Exception {
        HashMap<String, Object> para = new HashMap<>();
        para.put("number", number);
        para.put("password", Md5Util.Md5(password));
        LoginResponse lr = DoPost(LoginResponse.class, para, null);
        //LoginResponse lr = loginVirtual();
        ServiceContainer.AccessToken = lr.session;
        return lr.op;
    }

}
