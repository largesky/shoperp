package bjc.shoperp.domain;

import java.util.Comparator;

/**
 * Created by hcq on 2018/2/15.
 */

public class GoodsCountSort implements Comparator<GoodsCount> {

    private boolean isByStreet=false;

    public GoodsCountSort(boolean isByStreet) {
        this.isByStreet=isByStreet;
    }

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

        if (lhs.Area != rhs.Area) {
            return lhs.Area > rhs.Area ? 1 : -1;
        }

        if(lhs.LianLang!=rhs.LianLang){
            return lhs.LianLang?-1:1;
        }

        if(this.isByStreet==false){
            if (lhs.Door != rhs.Door) {
                return lhs.Door > rhs.Door ? 1 : -1;
            }

            if (lhs.Street != rhs.Street) {
                return lhs.Street > rhs.Street ? 1 : -1;
            }
        }else{
            if (lhs.Street != rhs.Street) {
                return lhs.Street > rhs.Street ? 1 : -1;
            }
            if (lhs.Door != rhs.Door) {
                return lhs.Door > rhs.Door ? 1 : -1;
            }
        }

        if ( lhs.Number.equalsIgnoreCase(rhs.Number)==false) {
            return lhs.Number.compareTo(rhs.Number);
        }

        if (lhs.Edtion.equalsIgnoreCase( rhs.Edtion)==false) {
            return lhs.Edtion.compareTo(rhs.Edtion);
        }

        if (lhs.Color.equalsIgnoreCase( rhs.Color)==false) {
            return lhs.Color.compareTo(rhs.Color);
        }

        return lhs.Size.compareTo(rhs.Size);
    }
}
