package com.voltbetta.app;

import androidx.appcompat.app.AppCompatActivity;
import android.annotation.SuppressLint;
import android.content.Intent;
import android.net.Uri;
import android.os.Bundle;
import android.os.Handler;
import android.webkit.JavascriptInterface;
import android.webkit.WebResourceError;
import android.webkit.WebResourceRequest;
import android.webkit.WebSettings;
import android.webkit.WebView;
import android.webkit.WebViewClient;
import android.widget.Toast;
import android.net.ConnectivityManager;
import android.net.NetworkInfo;
import android.content.Context;
import android.util.Log;
import java.io.File;
import java.io.FileOutputStream;
import java.io.IOException;
import java.io.InputStream;
import java.net.HttpURLConnection;
import java.net.URL;
import java.util.concurrent.Executor;
import java.util.concurrent.Executors;

public class MainActivity extends AppCompatActivity {

    private WebView webView;
    private final String LOCAL_SERVER_URL = "http://10.0.2.2:5000"; // For emulator
    private final String OFFLINE_ASSET = "offline.html";
    private final String WEBAPP_PATH = "webapp/index.html";
    private final String TAG = "VoltBettaManager";
    private boolean hasTriedLocalServer = false;
    private final Executor executor = Executors.newSingleThreadExecutor();
    private final Handler handler = new Handler();

    @SuppressLint("SetJavaScriptEnabled")
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        webView = findViewById(R.id.webview);
        configureWebView();
        testServerConnection();
    }
    
    @SuppressLint("SetJavaScriptEnabled")
    private void configureWebView() {
        WebSettings webSettings = webView.getSettings();
        webSettings.setJavaScriptEnabled(true);
        webSettings.setDomStorageEnabled(true);
        webSettings.setDatabaseEnabled(true);
        webSettings.setAllowFileAccess(true);
        webSettings.setAllowContentAccess(true);
        webSettings.setCacheMode(WebSettings.LOAD_DEFAULT);
        
        // Enable zoom controls for better UX
        webSettings.setBuiltInZoomControls(true);
        webSettings.setDisplayZoomControls(false);

        // Add webapp support with proper scaling
        webSettings.setUseWideViewPort(true);
        webSettings.setLoadWithOverviewMode(true);
        webSettings.setSupportZoom(true);
        
        // Setup cache directory
        File dir = new File(getCacheDir(), "webviewcache");
        if (!dir.exists()) {
            dir.mkdirs();
        }
        
        // Add JavaScript interface for deeper integration
        webView.addJavascriptInterface(new WebAppInterface(), "AndroidInterface");
        
        // Configure WebView client for error handling
        webView.setWebViewClient(new WebViewClient() {
            @Override
            public void onReceivedError(WebView view, WebResourceRequest request, WebResourceError error) {
                super.onReceivedError(view, request, error);
                
                // Only handle main page errors, not resource errors
                if (request.isForMainFrame()) {
                    if (!hasTriedLocalServer) {
                        hasTriedLocalServer = true;
                        loadOfflinePage();
                        Toast.makeText(MainActivity.this, "Connection issue. Using offline mode.", Toast.LENGTH_LONG).show();
                    }
                }
            }
            
            @Override
            public void onPageFinished(WebView view, String url) {
                super.onPageFinished(view, url);
                Log.d(TAG, "Page loaded: " + url);
            }
        });
    }
    
    private void testServerConnection() {
        executor.execute(() -> {
            if (isServerReachable()) {
                handler.post(() -> {
                    Log.d(TAG, "Server is reachable, loading from network");
                    webView.loadUrl(LOCAL_SERVER_URL);
                });
            } else if (isNetworkAvailable()) {
                handler.post(() -> {
                    Log.d(TAG, "Network available but server unreachable, trying bundled webapp");
                    loadWebApp();
                });
            } else {
                handler.post(() -> {
                    Log.d(TAG, "No network available, loading offline page");
                    loadOfflinePage();
                    Toast.makeText(MainActivity.this, "No network connection. Using offline mode.", Toast.LENGTH_LONG).show();
                });
            }
        });
    }

    private boolean isServerReachable() {
        try {
            URL url = new URL(LOCAL_SERVER_URL);
            HttpURLConnection connection = (HttpURLConnection) url.openConnection();
            connection.setConnectTimeout(3000);
            connection.setReadTimeout(3000);
            connection.setRequestMethod("GET");
            int responseCode = connection.getResponseCode();
            return (200 <= responseCode && responseCode <= 299);
        } catch (IOException e) {
            Log.e(TAG, "Server connection test failed", e);
            return false;
        }
    }

    private void loadOfflinePage() {
        webView.loadUrl("file:///android_asset/" + OFFLINE_ASSET);
    }
    
    private void loadWebApp() {
        // Try to load the bundled webapp if available
        try {
            File webApp = new File(getFilesDir(), WEBAPP_PATH);
            if (webApp.exists()) {
                webView.loadUrl("file://" + webApp.getAbsolutePath());
                Log.d(TAG, "Loaded bundled webapp");
            } else {
                loadOfflinePage();
                Log.d(TAG, "Bundled webapp not found, loading offline page");
            }
        } catch (Exception e) {
            Log.e(TAG, "Error loading bundled webapp", e);
            loadOfflinePage();
        }
    }

    private boolean isNetworkAvailable() {
        ConnectivityManager connectivityManager = (ConnectivityManager) getSystemService(Context.CONNECTIVITY_SERVICE);
        NetworkInfo activeNetworkInfo = connectivityManager.getActiveNetworkInfo();
        return activeNetworkInfo != null && activeNetworkInfo.isConnected();
    }

    @Override
    public void onBackPressed() {
        if (webView.canGoBack()) {
            webView.goBack();
        } else {
            super.onBackPressed();
        }
    }
    
    // JavaScript interface to allow the web app to interact with Android
    private class WebAppInterface {
        @JavascriptInterface
        public void showToast(String message) {
            Toast.makeText(MainActivity.this, message, Toast.LENGTH_SHORT).show();
        }
        
        @JavascriptInterface
        public void openExternalLink(String url) {
            Intent intent = new Intent(Intent.ACTION_VIEW, Uri.parse(url));
            startActivity(intent);
        }
        
        @JavascriptInterface
        public String getDeviceInfo() {
            return "Android " + android.os.Build.VERSION.RELEASE;
        }
        
        @JavascriptInterface
        public void checkServerConnection() {
            testServerConnection();
        }
    }
}