package bjc.shoperp;

import android.app.Activity;
import android.content.ClipData;
import android.content.ClipboardManager;
import android.content.Context;
import android.os.AsyncTask;
import android.os.Bundle;
import android.support.design.widget.FloatingActionButton;
import android.support.design.widget.Snackbar;
import android.support.v7.app.AppCompatActivity;
import android.support.v7.widget.Toolbar;
import android.text.TextUtils;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.BaseAdapter;
import android.widget.Button;
import android.widget.ListView;
import android.widget.ProgressBar;
import android.widget.TextView;
import android.widget.Toast;

import java.util.ArrayList;
import java.util.List;
import java.util.concurrent.atomic.AtomicInteger;

import bjc.shoperp.domain.Order;
import bjc.shoperp.domain.OrderGoods;
import bjc.shoperp.domain.Shop;
import bjc.shoperp.domain.restfulresponse.domainresponse.OrderCollectionResponse;
import bjc.shoperp.service.restful.OrderService;
import bjc.shoperp.service.restful.ServiceContainer;
import bjc.shoperp.service.restful.ShopService;
import bjc.shoperp.utils.DateUtil;
import bjc.shoperp.utils.StringUtils;

public class OrderActivity extends AppCompatActivity {
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_order);
        Toolbar toolbar = findViewById(R.id.toolbar);
        setSupportActionBar(toolbar);
        ((Button)findViewById(R.id.order_btn_query)).setOnClickListener(new View.OnClickListener(){
            @Override
            public void onClick(View view) {
                query();
            }
        });

    }

    private void query(){
        String phone=((TextView)findViewById(R.id.order_txt_phone)).getText().toString();
        new DownloadTask(phone,(ListView)findViewById(R.id.order_lst_orders)).execute((Void) null);
    }

    public class DownloadTask extends AsyncTask<Void, Void, Boolean> {
        private String phone;
        private ListView lv;
        OrderCollectionResponse ret=null;
        String err=null;

        public DownloadTask(String phone,ListView lv) {
            this.phone=phone;
            this.lv=lv;
        }

        @Override
        protected Boolean doInBackground(Void... params) {
            try {
                 ret=ServiceContainer.GetService(OrderService.class).getByAll(this.phone);
            } catch (Exception ex) {
                err=ex.getLocalizedMessage();
            } finally {
            }
            return true;
        }

        @Override
        protected void onProgressUpdate(Void... params) {

        }

        @Override
        protected void onPostExecute(Boolean aBoolean) {
            if(!bjc.shoperp.utils.StringUtils.isNullOrEmpty(err)){
                Toast.makeText(OrderActivity.this,err,Toast.LENGTH_LONG ).show();
                lv.setAdapter(null);
                return;
            }

            if (ret.Total<1){
                Toast.makeText(OrderActivity.this,"未找到订单",Toast.LENGTH_LONG ).show();
                lv.setAdapter(null);
                return;
            }

            OrderDataAdapter oda=new OrderDataAdapter(OrderActivity.this, ret.Datas);
            lv.setAdapter( oda);
        }
    }

   class OrderDataAdapter extends BaseAdapter {
        private  ArrayList<Order> orders;
       LayoutInflater infl = null;
       @Override
       public int getCount() {
          return this.orders.size();
       }

       @Override
       public Object getItem(int position) {
         return this.orders.get(position);
       }

       @Override
       public long getItemId(int position) {
           return position;
       }

       @Override
       public View getView(int position, View convertView, ViewGroup parent) {
           convertView = infl.inflate( R.layout.activity_order_orderdetail, null );
           final Order order=this.orders.get(position);
           TextView tv = (TextView) convertView.findViewById( R.id.order_txt_id );
           tv.setText(StringUtils.isNullOrEmpty(order.PopOrderId)?order.Id+"":order.PopOrderId);

           tv = (TextView) convertView.findViewById( R.id.order_txt_paytime );
           tv.setText(DateUtil.format(order.PopPayTime));

           tv = (TextView) convertView.findViewById( R.id.order_txt_deliverycompany );
           tv.setText(order.DeliveryCompany);

           tv = (TextView) convertView.findViewById( R.id.order_txt_deliverynumber );
           tv.setText(order.DeliveryNumber);

           tv.setOnClickListener(new View.OnClickListener() {
               @Override
               public void onClick(View view) {
                   ((ClipboardManager)getSystemService(Context.CLIPBOARD_SERVICE)).setPrimaryClip(ClipData.newPlainText("text",((TextView)view).getText()));
                   Toast.makeText(OrderActivity.this,"已复制快递单号:"+((TextView)view).getText(),Toast.LENGTH_LONG).show();
               }
           });

           tv = (TextView) convertView.findViewById( R.id.order_txt_goodsinfo );
           StringBuilder sb=new StringBuilder();
           if(order.OrderGoodss!=null&&order.OrderGoodss.size()>0){
               for (OrderGoods og : order.OrderGoodss) {
                   sb.append(og.Vendor+" "+og.Number+" "+(og.Edition==null?"":og.Edition)+" "+og.Color+" "+og.Size+" "+og.Count);
               }
           }
           tv.setText(sb.toString());
           tv = (TextView) convertView.findViewById( R.id.order_txt_reciverinfo);
           String phone=(StringUtils.isNullOrEmpty(order.ReceiverMobile)?"":order.ReceiverMobile+",")+(StringUtils.isNullOrEmpty(order.ReceiverPhone)?"":order.ReceiverPhone+",");
           tv.setText(order.ReceiverName+","+phone+order.ReceiverAddress);
           return convertView;
       }

       public OrderDataAdapter(Context context, ArrayList<Order> orders){
           this.infl = (LayoutInflater) context.getSystemService( Context.LAYOUT_INFLATER_SERVICE );
           this.orders=orders;
       }
   }
}


