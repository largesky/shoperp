package bjc.shoperp;

import android.app.Activity;
import android.content.Context;
import android.os.AsyncTask;
import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.text.TextUtils;
import android.view.LayoutInflater;
import android.view.View;
import android.view.ViewGroup;
import android.widget.BaseAdapter;
import android.widget.Button;
import android.widget.ListView;
import android.widget.ProgressBar;
import android.widget.TextView;

import java.util.ArrayList;
import java.util.HashMap;
import java.util.LinkedList;
import java.util.List;
import java.util.concurrent.atomic.AtomicBoolean;
import java.util.concurrent.atomic.AtomicInteger;

import bjc.shoperp.domain.Shop;
import bjc.shoperp.domain.pop.OrderDownload;
import bjc.shoperp.domain.restfulresponse.domainresponse.OrderDownloadCollectionResponse;
import bjc.shoperp.domain.restfulresponse.domainresponse.ShopCollectionResponse;
import bjc.shoperp.service.restful.OrderService;
import bjc.shoperp.service.restful.ServiceContainer;
import bjc.shoperp.service.restful.ShopService;

public class DownloadActivity extends AppCompatActivity {

    private List<AsyncTask<Void, Void, Boolean>> tasks = new LinkedList<>();

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate( savedInstanceState );
        setContentView( R.layout.activity_download );
        ((Button) findViewById( R.id.download_start )).setOnClickListener( new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                if (tasks.size() > 0) {
                    cancelAllTask();
                    ((Button) v).setText( "开始下载" );
                } else {
                    ((Button) v).setText( "停止下载" );
                    tasks.clear();
                    AsyncTask<Void, Void, Boolean> task = new DownloadTask( "1", (ListView) findViewById( R.id.download_lst_shop_state ) );
                    tasks.add( task );
                    task.execute( (Void) null );
                }
            }
        } );
    }

    @Override
    protected void onDestroy() {
        super.onDestroy();
        cancelAllTask();
    }

    protected void cancelAllTask() {
        for (AsyncTask<Void, Void, Boolean> task : tasks) {
            try {
                if (task.getStatus() == AsyncTask.Status.RUNNING)
                    task.cancel( true );
            } catch (Exception e) {

            }
        }
        tasks.clear();
    }

    public class DownloadOneShopTask extends AsyncTask<Void, Void, Boolean> {

        private Shop shop;

        private DownloadTask mainTask;

        private DownloadShopDataAdapter ada;

        private ListView lv;

        public DownloadOneShopTask(ListView lv, Shop s, DownloadShopDataAdapter ada, DownloadTask mainTask) {
            this.lv = lv;
            this.shop = s;
            this.ada = ada;
            this.mainTask = mainTask;
        }

        @Override
        protected Boolean doInBackground(Void... voids) {
            int pageIndex = 0, pageSize = 20, download = 0;
            try {
                OrderService os = ServiceContainer.GetService( OrderService.class );
                while (true) {
                    String msg = String.format( "每页 %d 条，正在下载 %d 页，已下载：%d...", pageSize, pageIndex + 1, download );
                    ada.setShopMsg( this.shop, msg );
                    this.publishProgress();
                    OrderDownloadCollectionResponse rsp = os.getPopWaitSendOrders( this.shop, 1, pageIndex, pageSize );
                    if (rsp.Datas == null || rsp.Datas.size() < 1) {
                        break;
                    }
                    pageIndex++;
                    download += rsp.Datas.size();
                    ada.setShopData( this.shop, rsp );
                    ada.setShopDownload( this.shop, download );
                    for (OrderDownload od : rsp.Datas) {
                        if (od.Error != null) {
                            mainTask.hasError = true;
                        }
                    }
                    this.publishProgress();
                }
                if (download < 1) {
                    ada.setShopMsg( this.shop, "没有订单" );
                    this.publishProgress();
                    Thread.sleep( 1000 * 3 );
                }
            } catch (Exception ex) {
                ada.setShopMsg( this.shop, ex.getMessage() );
                mainTask.hasError = true;
            } finally {
                this.publishProgress();
            }
            return true;
        }

        @Override
        protected void onProgressUpdate(Void... values) {
            super.onProgressUpdate( values );
            ada.notifyDataSetChanged();
        }

        @Override
        protected void onPostExecute(Boolean aBoolean) {
            super.onPostExecute( aBoolean );
            int runningCounter = mainTask.taskCounter.decrementAndGet();
        }
    }

    public class DownloadTask extends AsyncTask<Void, Void, Boolean> {
        private String popPayType;
        private ListView lstView;
        private DownloadShopDataAdapter ada;
        private List<Shop> shops;

        public boolean hasError = false;

        public AtomicInteger taskCounter;

        public DownloadTask(String popPayType, ListView lstView) {
            this.popPayType = popPayType;
            this.lstView = lstView;
        }

        @Override
        protected Boolean doInBackground(Void... params) {
            try {
                List<Shop> shopcr = ServiceContainer.GetService( ShopService.class ).getByAll();
                shops = new ArrayList<>();
                for (Shop s : shopcr) {
                    if (s.Enabled && s.AppEnabled && !TextUtils.isEmpty( s.AppKey ) && !TextUtils.isEmpty( s.AppAccessToken )) {
                        shops.add( s );
                    }
                }
                //生成新线程下载数据
                this.taskCounter = new AtomicInteger( shops.size() );
                this.ada = new DownloadShopDataAdapter( DownloadActivity.this, shops );
                this.publishProgress();
                DownloadOneShopTask[] tasks = new DownloadOneShopTask[shops.size()];
                for (int i = 0; i < tasks.length; i++) {
                    tasks[i] = new DownloadOneShopTask( this.lstView, shops.get( i ), this.ada, this );
                    DownloadActivity.this.tasks.add( tasks[i] );
                    tasks[i].executeOnExecutor( AsyncTask.THREAD_POOL_EXECUTOR, (Void) null );
                }
                while (true && this.isCancelled() == false) {
                    Thread.sleep( 300 );
                    if (taskCounter.get() <= 0) {
                        break;
                    }
                }
            } catch (Exception ex) {
                hasError = true;
            } finally {
            }
            return !hasError;
        }

        @Override
        protected void onProgressUpdate(Void... params) {
            lstView.setAdapter( ada );
            ada.notifyDataSetChanged();
        }

        @Override
        protected void onPostExecute(Boolean aBoolean) {
            super.onPostExecute( aBoolean );
            if (aBoolean) {
                setResult( Activity.RESULT_OK, getIntent() );
                DownloadActivity.this.finish();
            }else{
                ((Button) findViewById( R.id.download_start )).setText( "开始下载" );
            }
            tasks.clear();
        }
    }

    public class DownloadShopDataAdapter extends BaseAdapter {

        private List<Shop> shops;

        private HashMap<Shop, OrderDownloadCollectionResponse> shopDatas = new HashMap<>();

        private HashMap<Shop, String> shopMsgs = new HashMap<>();

        private HashMap<Shop, Integer> shopDownloads = new HashMap<>();

        LayoutInflater infl = null;

        @Override
        public int getCount() {
            return this.shops.size();
        }

        @Override
        public Object getItem(int position) {
            return this.shops.get( position );
        }

        @Override
        public long getItemId(int position) {
            return position;
        }

        public void setShopData(Shop s, OrderDownloadCollectionResponse value) {
            this.shopDatas.put( s, value );
        }

        public void setShopMsg(Shop s, String value) {
            this.shopMsgs.put( s, value );
        }

        public void setShopDownload(Shop s, Integer value) {
            this.shopDownloads.put( s, value );
        }

        public DownloadShopDataAdapter(Context context, List<Shop> shops) {
            this.infl = (LayoutInflater) context.getSystemService( Context.LAYOUT_INFLATER_SERVICE );
            this.shops = shops;

        }

        @Override
        public View getView(int position, View convertView, ViewGroup parent) {
            convertView = infl.inflate( R.layout.activity_download_shoplist, null );

            ProgressBar progressBar = (ProgressBar) convertView.findViewById( R.id.download_progress );
            TextView shopMark = (TextView) convertView.findViewById( R.id.download_shopMark );
            TextView msg = (TextView) convertView.findViewById( R.id.download_msg );

            Shop s = this.shops.get( position );

            //设置mark
            shopMark.setText( s.Mark );

            //设置消息
            if (shopMsgs.containsKey( s )) {
                msg.setText( shopMsgs.get( s ) );
            } else {
                msg.setText( "" );
            }

            //设置进度
            if (this.shopDatas.containsKey( s )) {
                OrderDownloadCollectionResponse detail = this.shopDatas.get( s );
                if (detail.IsTotalValid) {
                    detail.Total = detail.Total <= 0 ? 1 : detail.Total;
                    int progress = (int) (100 * shopDownloads.get( s ) / detail.Total);
                    progressBar.setProgress( progress );
                } else {
                    progressBar.setProgress( 0 );
                }
            } else {
                progressBar.setProgress( 0 );
            }
            return convertView;
        }
    }
}
