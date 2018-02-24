package bjc.shoperp;

import android.content.Intent;
import android.support.annotation.NonNull;
import android.support.v7.app.AppCompatActivity;
import android.app.LoaderManager.LoaderCallbacks;

import android.content.Loader;
import android.database.Cursor;
import android.os.AsyncTask;

import android.os.Bundle;
import android.text.TextUtils;
import android.view.KeyEvent;
import android.view.View;
import android.view.View.OnClickListener;
import android.view.inputmethod.EditorInfo;
import android.widget.AdapterView;
import android.widget.ArrayAdapter;
import android.widget.Button;
import android.widget.EditText;
import android.widget.Spinner;
import android.widget.TextView;

import bjc.shoperp.domain.Operator;
import bjc.shoperp.service.LocalConfigService;
import bjc.shoperp.service.restful.OperatorService;
import bjc.shoperp.service.restful.ServiceContainer;

/**
 * A login screen that offers login via email/password.
 */
public class LoginActivity extends AppCompatActivity implements AdapterView.OnItemSelectedListener {

    private static final String DEFAULT_SERVER_ADDRESS = "http://bjcgroup.imwork.net:60014/shoperp,http://bjcgroup.imwork.net:60014/shoperpdebug,http://192.168.31.9/shoperp,http://192.168.31.9/shoperpdebug";

    private UserLoginTask mAuthTask = null;
    private String[] serverAddress = null;
    private EditText mWorkNumber;
    private EditText mPasswordView;
    private EditText mServerAddressView;
    private Spinner mSpinnerView;

    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_login);
        // Set up the login form.
        mWorkNumber = (EditText) findViewById(R.id.worknumber);
        mPasswordView = (EditText) findViewById(R.id.password);
        mServerAddressView = (EditText) findViewById(R.id.login_server_address);

        Button mEmailSignInButton = (Button) findViewById(R.id.email_sign_in_button);
        mEmailSignInButton.setOnClickListener(new OnClickListener() {
            @Override
            public void onClick(View view) {
                attemptLogin();
            }
        });
        String add = LocalConfigService.get(this, LocalConfigService.CONFIG_SERVERADD, DEFAULT_SERVER_ADDRESS);
        serverAddress = parseServerAddress(add);
        mSpinnerView = (Spinner) findViewById(R.id.server_address);
        ArrayAdapter aa = new ArrayAdapter(this, android.R.layout.simple_dropdown_item_1line, serverAddress);
        aa.setDropDownViewResource(android.R.layout.simple_spinner_dropdown_item);
        mSpinnerView.setAdapter(aa);
        mSpinnerView.setOnItemSelectedListener(this);
    }

    /**
     * Callback received when a permissions request has been completed.
     */
    @Override
    public void onRequestPermissionsResult(int requestCode, @NonNull String[] permissions,
                                           @NonNull int[] grantResults) {
    }


    /**
     * Attempts to sign in or register the account specified by the login form.
     * If there are form errors (invalid email, missing fields, etc.), the
     * errors are presented and no actual login attempt is made.
     */
    private void attemptLogin() {
        if (mAuthTask != null) {
            return;
        }

        // Reset errors.
        mWorkNumber.setError(null);
        mPasswordView.setError(null);

        // Store values at the time of the login attempt.
        String worknumber = mWorkNumber.getText().toString().trim();
        String password = mPasswordView.getText().toString().trim();
        String url = mServerAddressView.getText().toString().trim();
        boolean cancel = false;
        View focusView = null;

        // Check for a valid password, if the user entered one.
        if (TextUtils.isEmpty(password)) {
            mPasswordView.setError("密码不能为空");
            focusView = mPasswordView;
            cancel = true;
        }

        // Check for a valid worknumber address.
        if (TextUtils.isEmpty(worknumber)) {
            mWorkNumber.setError("工号不能为空");
            focusView = mWorkNumber;
            cancel = true;
        }

        if (TextUtils.isEmpty(url)) {
            mServerAddressView.setError("服务器地址不能为空");
            focusView = mServerAddressView;
            cancel = true;
        }

        if (cancel) {
            focusView.requestFocus();
            return;
        }
        mAuthTask = new UserLoginTask(url, worknumber, password);
        mAuthTask.execute((Void) null);
    }

    @Override
    public void onItemSelected(AdapterView<?> adapterView, View view, int i, long l) {
        mServerAddressView.setText(serverAddress[i]);
    }

    @Override
    public void onNothingSelected(AdapterView<?> adapterView) {
        this.mServerAddressView.setText("");
    }

    /**
     * Represents an asynchronous login/registration task used to authenticate
     * the user.
     */
    public class UserLoginTask extends AsyncTask<Void, Void, Boolean> {

        private final String mUrl;
        private final String mWorkNumber;
        private final String mPassword;
        private String error;

        UserLoginTask(String url, String worknumber, String password) {
            mUrl = url;
            mWorkNumber = worknumber;
            mPassword = password;
        }

        @Override
        protected Boolean doInBackground(Void... params) {
            try {
                ServiceContainer.ServerAddress = mUrl;
                Operator op = ServiceContainer.GetService(OperatorService.class).Login(mWorkNumber, mPassword);
            } catch (Exception e) {
                this.error = e.getMessage();
                return false;
            }
            return true;
        }

        @Override
        protected void onPostExecute(final Boolean success) {
            mAuthTask = null;
            if (success) {
                setServerAddress(mUrl);
                Intent intent = new Intent();
                intent.setClass(LoginActivity.this, MainActivity.class);
                startActivity(intent);
                LoginActivity.this.finish();
            } else {
                mServerAddressView.setError(this.error);
                mServerAddressView.requestFocus();
            }
        }

        @Override
        protected void onCancelled() {
            mAuthTask = null;
        }
    }

    private String[] getServerAddress() {
        String add = LocalConfigService.get(this, LocalConfigService.CONFIG_SERVERADD, DEFAULT_SERVER_ADDRESS);
        try {
            return parseServerAddress(add);
        } catch (Exception e) {
            return parseServerAddress(DEFAULT_SERVER_ADDRESS);
        }
    }

    private String[] parseServerAddress(String s) {
        String[] urls = s.split(",");
        return urls;

    }

    private void setServerAddress(String serverAddress) {
        String add = LocalConfigService.get(this, LocalConfigService.CONFIG_SERVERADD, DEFAULT_SERVER_ADDRESS);
        String[] urls = add.split(",");
        String newUrls = serverAddress;

        for (int i = 0; i < urls.length && i < 4; i++) {
            if (urls[i].equalsIgnoreCase(serverAddress)) {
                continue;
            }
            newUrls += "," + urls[i];
        }
        LocalConfigService.update(this, LocalConfigService.CONFIG_SERVERADD, newUrls);
    }
}

