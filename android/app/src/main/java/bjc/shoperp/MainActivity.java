package bjc.shoperp;

import android.app.Activity;
import android.content.Intent;
import android.graphics.Paint;
import android.os.Bundle;
import android.support.v7.app.AppCompatActivity;
import android.support.v7.widget.Toolbar;
import android.view.Menu;
import android.view.MenuItem;
import android.widget.RelativeLayout;
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

import java.util.ArrayList;
import java.util.LinkedList;
import java.util.List;

import bjc.shoperp.activities.deliveryoutscan.DeliveryOutScanActivity;
import bjc.shoperp.domain.GoodsCount;

public class MainActivity extends AppCompatActivity {

    private static ArrayList<GoodsCount> goodsCounts;
    private static int notCreateNewReturnCount;
    DataGridView grid;
    private TextView sumInfo = null;

    public static ArrayList<GoodsCount> getGoodsCounts() {
        return goodsCounts;
    }

    public static void setGoodsCounts(ArrayList<GoodsCount> goodsCounts) {
        MainActivity.goodsCounts = goodsCounts;
    }

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate( savedInstanceState );
        setContentView( R.layout.activity_main );
        Toolbar toolbar = (Toolbar) findViewById( R.id.toolbar );
        setSupportActionBar( toolbar );
        sumInfo = (TextView) findViewById( R.id.goodscount_suminfo );
        grid = new DataGridView( this );
        grid.setAutoGenerateColumns( false );
        createColumns( grid );
        RelativeLayout rl = (RelativeLayout) findViewById( R.id.content_main );
        rl.addView( grid );
    }

    @Override
    public boolean onCreateOptionsMenu(Menu menu) {
        // Inflate the menu; this adds items to the action bar if it is present.
        getMenuInflater().inflate( R.menu.menu_main, menu );
        return true;
    }

    @Override
    public boolean onOptionsItemSelected(MenuItem item) {
        int id = item.getItemId();
        Intent intent = new Intent();
        if (id == R.id.main_menu_count) {
            intent.setClass( MainActivity.this, GoodsCountActivity.class );
            startActivityForResult( intent, 1 );
        }
        if (id == R.id.main_menu_download) {
            intent.setClass( MainActivity.this, DownloadActivity.class );
            startActivityForResult( intent, 2 );
        }

        if (id == R.id.main_menu_scan) {
            intent.setClass( MainActivity.this, DeliveryOutScanActivity.class );
            startActivity( intent );
        }

        if (id == R.id.main_menu_history) {
            intent.setClass( MainActivity.this, DeliveryOutHistoryActivity.class );
            startActivity( intent );
        }
        return super.onOptionsItemSelected( item );
    }

    @Override
    public void onActivityResult(int requestCode, int resultCode, Intent data) {
        switch (requestCode) {
            case 1:
                if (resultCode == Activity.RESULT_OK) {
                    //更新显示 数据
                    this.grid.setDataSource( MainActivity.getGoodsCounts() );
                    if (MainActivity.getGoodsCounts().size() < 1) {
                        Toast.makeText( MainActivity.this, "未找到任何订单商品", Toast.LENGTH_LONG ).show();
                    }
                    float total = 0;
                    int goodsTotal = 0;
                    List<String> vendors = new LinkedList<>();
                    for (GoodsCount gc : MainActivity.getGoodsCounts()) {
                        total += gc.Money * gc.Count;
                        goodsTotal += gc.Count;
                        if (vendors.contains( gc.Vendor ) == false) {
                            vendors.add( gc.Vendor );
                        }
                    }
                    String sum = String.format( "厂家数：%d,商品件数：%d,商品金额：%d", vendors.size(), goodsTotal, (int) total );
                    sumInfo.setText( sum );
                }
                break;
            case 2:
                if (resultCode == Activity.RESULT_OK)
                    Toast.makeText( MainActivity.this, "所有订单下载成功进入统计进行刷新", Toast.LENGTH_LONG ).show();
                break;
        }
    }

    private int getTextWidth(String text) {
        android.graphics.Rect bounds = new android.graphics.Rect();
        Paint paint = new Paint();
        paint.getTextBounds( text, 0, text.length(), bounds );
        return bounds.width();
    }

    private void createColumns(DataGridView dataGridView) {
        dataGridView.clearColumns();
        dataGridView.setAutoGenerateColumns( false );
        ArrayList<Column> tcs = new ArrayList<Column>();

        int fontSize = 10;
        TextColumn tc = new TextColumn();
        tc.setTitle( "门牌" );
        tc.setKey( "Address" );
        tcs.add( tc );

        tc = new TextColumn();
        tc.setTitle( "日期" );
        tc.setKey( "PayTimeStr" );
        //((DateTimeColumn)tc).setDateTimeFormat(DateTimeFormats.DATE_TIME_LONG);

        tcs.add( tc );

        tc = new TextColumn();
        tc.setTitle( "厂家" );
        tc.setKey( "Vendor" );
        tcs.add( tc );

        tc = new TextColumn();
        tc.setTitle( "货号" );
        tc.setKey( "Number" );
        tcs.add( tc );

        tc = new TextColumn();
        tc.setTitle( "版本" );
        tc.setKey( "Edtion" );
        tcs.add( tc );

        ColumnWidth cw = new ColumnWidth();
        tc = new TextColumn();
        tc.setTitle( "颜色" );
        tc.setKey( "Color" );
        tc.setWidth( cw );
        tcs.add( tc );

        cw.setValue( this.getTextWidth( "尺码" ) );
        tc = new TextColumn();
        tc.setTitle( "尺码" );
        tc.setKey( "Size" );
        tc.setWidth( cw );
        tcs.add( tc );

        tc = new TextColumn();
        tc.setTitle( "价格" );
        tc.setKey( "Money" );
        tc.setWidth( cw );
        tcs.add( tc );

        tc = new TextColumn();
        tc.setTitle( "数量" );
        tc.setKey( "Count" );
        tc.setWidth( cw );
        tcs.add( tc );

        for (Column t : tcs) {
            t.setFontSize( 8 );
            t.setPaddingBottom( 0 );
            t.setPaddingLeft( 0 );
            t.setPaddingRight( 0 );
            t.setPaddingTop( 0 );
            t.setVerticalAlignment( CellContentVerticalAlignment.CENTER );
            t.setHorizontalAlignment( CellContentHorizontalAlignment.CENTER );
            Header h = t.getHeader();
            h.setPaddingBottom( 0 );
            h.setPaddingLeft( 0 );
            h.setPaddingRight( 0 );
            h.setPaddingTop( 0 );
            h.setFontSize( 8 );
            h.setVerticalAlignment( CellContentVerticalAlignment.CENTER );
            h.setHorizontalAlignment( CellContentHorizontalAlignment.CENTER );
            t.setHeader( h );

            dataGridView.addColumn( t );
        }

        dataGridView.setRowSeparatorHeight( 1 );
        dataGridView.setRowHeight( 16 );
        dataGridView.setHeaderHeight( 22 );
        RowSeparator rs = dataGridView.getRowSeparator();
        rs.setBackground( new SolidColorBrush( 0x80000000 ) );
        rs.setContentOpacity( 1 );
        dataGridView.setRowSeparator( rs );
    }
}
