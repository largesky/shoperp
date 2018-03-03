package bjc.shoperp.utils;

import java.text.ParseException;
import java.text.SimpleDateFormat;
import java.util.Date;

/**
 * Created by hth on 2018/2/27.
 */

public class DateUtil {

    public static final SimpleDateFormat dateTimeFormat = new SimpleDateFormat("yyyy-MM-dd HH:mm:ss");
    public static final SimpleDateFormat dateFormat = new SimpleDateFormat("yyyy-MM-dd");

    public static String format(Date dateTime) {
        return dateTimeFormat.format(dateTime);
    }

    public static Date parse(String dateTime) throws ParseException {
        return dateTimeFormat.parse(dateTime);
    }

    public static String formatDate(Date dateTime) {
        return dateFormat.format(dateTime);
    }

}
