package bjc.shoperp;

import android.graphics.Paint;
import android.os.AsyncTask;
import android.os.Bundle;
import android.support.v7.app.AppCompatActivity;
import android.view.View;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.EditText;
import android.widget.LinearLayout;
import android.widget.Spinner;
import android.widget.TextView;
import android.widget.Toast;

import com.infragistics.controls.CellContentHorizontalAlignment;
import com.infragistics.controls.CellContentVerticalAlignment;
import com.infragistics.controls.Column;
import com.infragistics.controls.ColumnWidth;
import com.infragistics.controls.DataGridView;
import com.infragistics.controls.Header;
import com.infragistics.controls.RowSeparator;
import com.infragistics.controls.TextColumn;
import com.infragistics.graphics.SolidColorBrush;

import java.text.ParseException;
import java.util.ArrayList;
import java.util.Collections;
import java.util.Comparator;
import java.util.Date;
import java.util.LinkedList;
import java.util.List;

import bjc.shoperp.domain.DeliveryOut;
import bjc.shoperp.domain.Shop;
import bjc.shoperp.domain.restfulresponse.domainresponse.DeliveryOutCollectionResponse;
import bjc.shoperp.service.restful.DeliveryOutService;
import bjc.shoperp.service.restful.ServiceContainer;
import bjc.shoperp.service.restful.ShopService;
import bjc.shoperp.utils.DateUtil;

public class DeliveryOutHistoryActivity extends AppCompatActivity {
    List<Shop> shops;
    DataGridView grid;
    Spinner spinnerShops;
    TextView sumInfo;
    private EditText etDeliveryNumber;
    private EditText startTime;
    private EditText endTime;
    private Button btnQuery;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_delivery_out_history);
        grid = new DataGridView(this);
        createColumns(grid);
        LinearLayout rl = (LinearLayout) findViewById(R.id.delivery_out_history);
        rl.addView(grid);
        spinnerShops = (Spinner) findViewById(R.id.deliveryouthistory_shops);
        etDeliveryNumber = (EditText) findViewById(R.id.deliveryouthistory_deliverynumber);
        startTime = (EditText) findViewById(R.id.deliveryouthistory_starttime);
        endTime = (EditText) findViewById(R.id.deliveryouthistory_endtime);
        btnQuery = (Button) findViewById(R.id.deliveryouthistory_query);
        sumInfo = (TextView) findViewById(R.id.delivery_out_history_suminfo);
        btnQuery.setOnClickListener(new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                try {
                    long shopId = ((Shop) spinnerShops.getSelectedItem()).Id;
                    String deliveryNumber = etDeliveryNumber.getText().toString().trim();
                    String st = startTime.getText().toString().trim();
                    String et = endTime.getText().toString().trim();
                    DataQueryAsyncTask t = new DataQueryAsyncTask(st, et, shopId, deliveryNumber);
                    t.execute((Void) null);
                } catch (Exception e) {
                    Toast.makeText(DeliveryOutHistoryActivity.this, e.getMessage(), Toast.LENGTH_LONG).show();
                }
            }
        });
        ShopDownloadAsyncTask t = new ShopDownloadAsyncTask();
        t.execute((Void) null);
        startTime.setText(DateUtil.formatDate(new Date()) + " 00:00:01");
        endTime.setText(DateUtil.formatDate(new Date()) + " 23:59:59");
    }

    private int getTextWidth(String text) {
        android.graphics.Rect bounds = new android.graphics.Rect();
        Paint paint = new Paint();
        paint.getTextBounds(text, 0, text.length(), bounds);
        return bounds.width();
    }

    private void createColumns(DataGridView dataGridView) {
        dataGridView.clearColumns();
        dataGridView.setAutoGenerateColumns(false);
        ArrayList<Column> tcs = new ArrayList<Column>();

        int fontSize = 10;
        TextColumn tc = new TextColumn();
        tc.setTitle("店铺");
        tc.setKey("ShopId");
        tcs.add(tc);

        tc = new TextColumn();
        tc.setTitle("物流单号");
        tc.setKey("DeliveryNumber");
        tcs.add(tc);

        tc = new TextColumn();
        tc.setTitle("商品信息");
        tc.setKey("GoodsInfo");
        tcs.add(tc);

        tc = new TextColumn();
        tc.setTitle("发货时间");
        tc.setKey("CreateTimeStr");
        tcs.add(tc);

        tc = new TextColumn();
        tc.setTitle("收货人信息");
        tc.setKey("ReceiverAddress");
        tcs.add(tc);
        ColumnWidth cw = new ColumnWidth();
        cw.setValue(1);
        cw.setIsStarSized(true);
        tc.setWidth(cw);

        for (Column t : tcs) {
            t.setFontSize(8);
            t.setPaddingBottom(0);
            t.setPaddingLeft(0);
            t.setPaddingRight(0);
            t.setPaddingTop(0);
            t.setVerticalAlignment(CellContentVerticalAlignment.CENTER);
            t.setHorizontalAlignment(CellContentHorizontalAlignment.CENTER);
            Header h = t.getHeader();
            h.setPaddingBottom(0);
            h.setPaddingLeft(0);
            h.setPaddingRight(0);
            h.setPaddingTop(0);
            h.setFontSize(8);
            h.setVerticalAlignment(CellContentVerticalAlignment.CENTER);
            h.setHorizontalAlignment(CellContentHorizontalAlignment.CENTER);
            t.setHeader(h);

            dataGridView.addColumn(t);
        }

        dataGridView.setRowSeparatorHeight(1);
        dataGridView.setRowHeight(16);
        dataGridView.setHeaderHeight(22);
        RowSeparator rs = dataGridView.getRowSeparator();
        rs.setBackground(new SolidColorBrush(0x80000000));
        rs.setContentOpacity(1);
        dataGridView.setRowSeparator(rs);
    }

    class ShopDownloadAsyncTask extends AsyncTask<Void, Void, Boolean> {

        private List<Shop> shops = new LinkedList<>();

        @Override
        protected Boolean doInBackground(Void... voids) {
            try {
                Shop s = new Shop();
                s.AppAccessToken = "";
                s.Enabled = true;
                s.Mark = "所有";
                s.Id = 0;
                shops.add(s);
                List<Shop> ss = ServiceContainer.GetService(ShopService.class).getByAll();
                shops.addAll(ss);
                return true;
            } catch (Exception ex) {
                return false;
            }
        }

        @Override
        protected void onPostExecute(Boolean aBoolean) {
            if (aBoolean == false) {
                Toast.makeText(DeliveryOutHistoryActivity.this, "读取店铺出错", Toast.LENGTH_LONG).show();
                return;
            }
            DeliveryOutHistoryActivity.this.shops = this.shops;
            ArrayAdapter<Shop> list = new ArrayAdapter<Shop>(DeliveryOutHistoryActivity.this, R.layout.support_simple_spinner_dropdown_item, shops);
            DeliveryOutHistoryActivity.this.spinnerShops.setAdapter(list);
        }
    }

    class DataQueryAsyncTask extends AsyncTask<Void, Void, Boolean> {

        private Date startTime;
        private Date endTime;
        private long shopId;
        private String deliveryNumber;

        private DeliveryOutCollectionResponse ret;

        public DataQueryAsyncTask(String startTime, String endTime, long shopId, String deliveryNumber) throws ParseException {
            this.startTime = DateUtil.parse(startTime);
            this.endTime = DateUtil.parse(endTime);
            this.shopId = shopId;
            this.deliveryNumber = deliveryNumber;
        }

        @Override
        protected Boolean doInBackground(Void... voids) {
            try {
                this.ret = ServiceContainer.GetService(DeliveryOutService.class).GetByAll(this.shopId, "", this.deliveryNumber, "", "", startTime, endTime, 0, 0);
                return true;
            } catch (Exception e) {
                return false;
            }
        }

        @Override
        protected void onPostExecute(Boolean aBoolean) {
            String msg = "没有数据";
            if (this.ret != null && this.ret.Datas != null && this.ret.Datas.size() > 0) {
                Collections.sort(this.ret.Datas, new Comparator<DeliveryOut>() {
                    @Override
                    public int compare(DeliveryOut o1, DeliveryOut o2) {
                        if (o1 == null && o2 == null) {
                            return 0;
                        }
                        if (o1 == null && o2 != null) {
                            return -1;
                        }
                        if (o1 != null && o2 == null) {
                            return 1;
                        }
                        return o1.GoodsInfo.compareToIgnoreCase(o2.GoodsInfo);
                    }
                });
                DeliveryOutHistoryActivity.this.grid.setDataSource(this.ret.Datas);
                int popGoods = 0, costGoods = 0, delivery = 0;
                for (DeliveryOut out : this.ret.Datas) {
                    popGoods += (int) out.PopGoodsMoney;
                    costGoods += (int) out.ERPGoodsMoney;
                    delivery += (int) out.ERPDeliveryMoney;
                    out.CreateTimeStr=DateUtil.format(out.CreateTime);
                }
                msg = String.format("共：%d 条数据，商品金额：%d,成本金额：%d,快递成本：%d", this.ret.Datas.size(), popGoods, costGoods, delivery);

            } else {
                DeliveryOutHistoryActivity.this.grid.setDataSource(null);
            }
            DeliveryOutHistoryActivity.this.sumInfo.setText(msg);
            DeliveryOutHistoryActivity.this.grid.requestFocus();
        }
    }
}


