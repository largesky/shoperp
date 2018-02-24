package bjc.shoperp.utils;

import java.io.UnsupportedEncodingException;
import java.math.BigInteger;
import java.security.MessageDigest;
import java.security.NoSuchAlgorithmException;

/**
 * Created by hcq on 2018/2/17.
 */

public class Md5Util {
    public static String Md5(String content) throws NoSuchAlgorithmException, UnsupportedEncodingException {
        MessageDigest m = MessageDigest.getInstance( "MD5" );
        byte[] bts = m.digest( content.getBytes("UTF-8") );
        String s = new BigInteger( 1, bts ).toString( 16 );
        while (s.length()< 32) {
            s  = "0" + s ;
        }
        return s;
    }
}
