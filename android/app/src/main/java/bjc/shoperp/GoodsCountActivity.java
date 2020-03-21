package bjc.shoperp;

import android.app.Activity;
import android.os.AsyncTask;
import android.os.Build;
import android.support.annotation.RequiresApi;
import android.support.v7.app.AppCompatActivity;
import android.os.Bundle;
import android.text.TextUtils;
import android.view.View;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.CheckBox;
import android.widget.ProgressBar;
import android.widget.Spinner;
import android.widget.TextView;
import android.widget.Toast;

import java.text.ParseException;
import java.text.SimpleDateFormat;
import java.util.ArrayList;
import java.util.Collections;
import java.util.Comparator;
import java.util.Date;

import bjc.shoperp.domain.GoodsCount;
import bjc.shoperp.domain.GoodsCountSort;
import bjc.shoperp.domain.restfulresponse.domainresponse.GoodsCountCollectionResponse;
import bjc.shoperp.service.LocalConfigService;
import bjc.shoperp.service.restful.OrderGoodsService;
import bjc.shoperp.service.restful.OrderService;
import bjc.shoperp.service.restful.ServiceContainer;
import bjc.shoperp.service.restful.SystemConfigService;


public class GoodsCountActivity extends AppCompatActivity {

    private SimpleDateFormat dateFormat = new SimpleDateFormat( "yyyy-MM-dd HH:mm:ss" );

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate( savedInstanceState );
        setContentView( R.layout.activity_goods_count );
        ((Button) findViewById( R.id.goodscount_btn_download )).setOnClickListener( new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                try {

                    String strStart = ((TextView) findViewById( R.id.goods_count_starttime )).getText().toString().trim();
                    String strEnd = ((TextView) findViewById( R.id.goods_count_endtime )).getText().toString().trim();
                    if (TextUtils.isEmpty( strStart )) {
                        throw new Exception( "开始时间必须输入" );
                    }
                    if (TextUtils.isEmpty( strEnd )) {
                        strEnd = dateFormat.format( new Date( (new Date().getTime() + 3600 * 1000) ) );
                    }
                    Date start = dateFormat.parse( strStart );
                    Date end = dateFormat.parse( strEnd );
                    ArrayList<Integer> flags = new ArrayList<Integer>();
                    if (((CheckBox) findViewById( R.id.goodscount_flag_red )).isChecked()) {
                        flags.add( 2 );
                    }
                    if (((CheckBox) findViewById( R.id.goodscount_flag_yellow )).isChecked()) {
                        flags.add( 3 );
                    }
                    if (((CheckBox) findViewById( R.id.goodscount_flag_green )).isChecked()) {
                        flags.add( 4 );
                    }
                    if (((CheckBox) findViewById( R.id.goodscount_flag_blue )).isChecked()) {
                        flags.add( 5 );
                    }
                    if (((CheckBox) findViewById( R.id.goodscount_flag_pink )).isChecked()) {
                        flags.add( 6 );
                    }
                    int[] values = new int[flags.size()];
                    for (int i = 0; i < values.length; i++) {
                        values[i] = (int) flags.get( i );
                    }
                    new UserStartDownloadTask( start, end, values, (ProgressBar) findViewById( R.id.download_progress ), (TextView) findViewById( R.id.goodsdownload_msg ) ).execute();
                } catch (Exception ex) {
                    ((TextView) findViewById( R.id.goodsdownload_msg )).setText( ex.getMessage() );
                }
            }
        } );
        ((Button) findViewById( R.id.goodscount_btn_sys )).setOnClickListener( new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                SetDateTime( v );
            }
        } );
        ((Button) findViewById( R.id.goodscount_btn_local )).setOnClickListener( new View.OnClickListener() {
            @Override
            public void onClick(View v) {
                String timeNow = dateFormat.format( new Date( (new Date().getTime() + 3600 * 1000) ) );
                ((TextView) findViewById( R.id.goods_count_endtime )).setText( timeNow );
            }
        } );

        Date t = new Date( (new Date().getTime()) - 20 * 24 * 3600 * 1000 );
        String ts = dateFormat.format( t );
        String tts = dateFormat.format( new Date() );
        ((TextView) findViewById( R.id.goods_count_starttime )).setText( dateFormat.format( t ) );
        new GoodsCountLastOrderInitTask( (Button) findViewById( R.id.goodscount_btn_sys ), (Button) findViewById( R.id.goodscount_btn_local ) ).execute( (Void) null );
    }

    private void SetDateTime(View v) {
        Button btn = (Button) v;
        Object tag = btn.getTag();
        String strTime = "";
        if (tag != null) {
            String dateTime = tag.toString();
            try {
                Date dt = dateFormat.parse( dateTime );
                dt = new Date( dt.getTime() + 1000 );
                strTime = dateFormat.format( dt );
            } catch (ParseException e) {
                Toast.makeText( this, "时间转换错误" + e.getMessage(), Toast.LENGTH_LONG ).show();
            }
        } else {
            strTime = dateFormat.format( new Date( (new Date().getTime() + 3600 * 1000) ) );
        }
        ((TextView) findViewById( R.id.goods_count_starttime )).setText( strTime );
    }

    public class UserStartDownloadTask extends AsyncTask<Void, Void, Boolean> {
        private Date startTime;
        private Date endTime;
        private int[] flags;
        private ProgressBar progressBar;
        private TextView msgTextView;
        private boolean hasError;
        private String error;
        private int total;
        private ArrayList<GoodsCount> goods = new ArrayList<GoodsCount>();

        public UserStartDownloadTask(Date startTime, Date endTime, int[] flags, ProgressBar progressBar, TextView msgTextView) {
            this.startTime = startTime;
            this.endTime = endTime;
            this.flags = flags;
            this.progressBar = progressBar;
            this.msgTextView = msgTextView;
        }

        private void setTextMsg(final String msg) {
            GoodsCountActivity.this.runOnUiThread( new Runnable() {
                @Override
                public void run() {
                    msgTextView.setText( msg );
                }
            } );
        }

        @Override
        protected Boolean doInBackground(Void... params) {
            try {
                do {
                    this.setTextMsg( "正在请求网络:" );
                    GoodsCountCollectionResponse gcs = ServiceContainer.GetService( OrderGoodsService.class ).GetGoodsCount( flags, startTime, endTime, 0, 0 );
                    this.goods.addAll( gcs.Datas );
                    this.total = gcs.Total;
                    break;
                } while (true);
                Comparator<GoodsCount> comparator =  new GoodsCountSort( );
                Collections.sort( goods, comparator );//地址
                Collections.sort( goods, comparator );//货号
                Collections.sort( goods, comparator );//版本
                Collections.sort( goods, comparator );//颜色
                Collections.sort( goods, comparator );//尺码
                SimpleDateFormat dateFormat = new SimpleDateFormat( "MM-dd HH:mm" );
                for (GoodsCount gc : goods) {
                    gc.PayTimeStr = dateFormat.format( gc.FirstPayTime );
                    if (gc.Color.length() > 2) {
                        gc.Color = gc.Color.replace( "色", "" );
                    }
                    if (gc.Color.length() > 2) {
                        gc.Color = gc.Color.substring( 0, 1 );
                    }
                }
                //更新合并数据
                MainActivity.setGoodsCounts( goods );
            } catch (Exception ex) {
                error = ex.getMessage();
                hasError = true;
            } finally {
            }
            return !hasError;
        }


        @RequiresApi(api = Build.VERSION_CODES.O)
        @Override
        protected void onPostExecute(Boolean aBoolean) {
            try {
                if (aBoolean) {
                    if (this.goods.size() > 0) {
                        GoodsCount gc = this.goods.get( 0 );
                        for (GoodsCount g : this.goods) {
                            if (g.LastPayTime.after( gc.LastPayTime )) {
                                gc = g;
                            }
                        }
                        String[] objs=new String[]{ dateFormat.format( gc.LastPayTime ), gc.Vendor, gc.Number, gc.Edtion, gc.Color, gc.Size};
                        String lastInfo = TextUtils.join( " ",objs );
                        LocalConfigService.update( GoodsCountActivity.this, LocalConfigService.CONFIG_GOODSCOUNTLASTINFO, lastInfo );
                    }
                }
            } catch (Exception e) {
                hasError = true;
                error = e.getMessage();
            }

            if (hasError) {
                this.msgTextView.setText( error );
            }else{
                setResult( Activity.RESULT_OK, getIntent() );
                GoodsCountActivity.this.finish();
            }
            super.onPostExecute( aBoolean );
        }
    }

    public class GoodsCountLastOrderInitTask extends AsyncTask<Void, Void, Boolean> {
        private Button btnSys;
        private Button btnLocal;
        private String strSysInfo;
        private String strLocalInfo;

        public GoodsCountLastOrderInitTask(Button btnSys, Button btnLocal) {
            this.btnSys = btnSys;
            this.btnLocal = btnLocal;
        }

        @Override
        protected Boolean doInBackground(Void... params) {
            try {
                strSysInfo = ServiceContainer.GetService( SystemConfigService.class ).get( -1, "GoodsCountLastOrder", "系统从未统计过 点击填充默认时间" );
            } catch (Exception ex) {
                strSysInfo = ex.getMessage();
            }
            try {
                strLocalInfo = LocalConfigService.get( GoodsCountActivity.this, LocalConfigService.CONFIG_GOODSCOUNTLASTINFO, "本地从未统计过 点击填充默认时间" );
            } catch (Exception ex) {
                strLocalInfo = ex.getMessage();
            }
            return true;
        }

        @Override
        protected void onPostExecute(Boolean aBoolean) {
            String[] strs = this.strSysInfo.split( "[,，]" );
            if (strs.length >= 2) {
                btnSys.setTag( strs[0] );
            } else {
                btnSys.setTag( null );
            }
            strs = this.strLocalInfo.split( "[,，]" );
            if (strs.length >= 2) {
                btnLocal.setTag( strs[0] );
            } else {
                btnLocal.setTag( null );
            }
            btnSys.setText( "系统:" + strSysInfo );
            btnLocal.setText( "本地:" + strLocalInfo );
            super.onPostExecute( aBoolean );
        }
    }
}
