package bjc.shoperp.domain;

import java.util.Comparator;

/**
 * Created by hcq on 2018/2/15.
 */

public class GoodsCountSort implements Comparator<GoodsCount> {
    @Override
    public int compare(GoodsCount lhs, GoodsCount rhs) {
        if (lhs == null && rhs == null) {
            return 0;
        }

        if (lhs == null) {
            return 1;
        }

        if (rhs == null) {
            return -1;
        }

        if(lhs.Address.equalsIgnoreCase(rhs.Address)==false){
            return lhs.Address.compareToIgnoreCase(rhs.Address);
        }

        if ( lhs.Number.equalsIgnoreCase(rhs.Number)==false) {
            return lhs.Number.compareToIgnoreCase(rhs.Number);
        }

        if (lhs.Edtion.equalsIgnoreCase( rhs.Edtion)==false) {
            return lhs.Edtion.compareToIgnoreCase(rhs.Edtion);
        }

        if (lhs.Color.equalsIgnoreCase( rhs.Color)==false) {
            return lhs.Color.compareToIgnoreCase(rhs.Color);
        }

        return lhs.Size.compareToIgnoreCase(rhs.Size);
    }
}
