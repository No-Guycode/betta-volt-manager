package com.voltbetta.app;

import android.annotation.SuppressLint;
import android.os.Bundle;
import android.webkit.WebSettings;
import android.webkit.WebView;
import android.webkit.WebViewClient;
import androidx.appcompat.app.AppCompatActivity;

public class MainActivity extends AppCompatActivity {
    private WebView webView;

    @SuppressLint("SetJavaScriptEnabled")
    @Override
    protected void onCreate(Bundle savedInstanceState) {
        super.onCreate(savedInstanceState);
        setContentView(R.layout.activity_main);

        webView = findViewById(R.id.webview);
        webView.setWebViewClient(new WebViewClient());

        // Enable JavaScript and DOM storage
        WebSettings webSettings = webView.getSettings();
        webSettings.setJavaScriptEnabled(true);
        webSettings.setDomStorageEnabled(true);
        
        // For local database support
        webSettings.setDatabaseEnabled(true);
        
        // For allowing app to access files
        webSettings.setAllowFileAccess(true);
        
        // Load the app from the embedded asset or hosted URL
        // For demonstration - we'll use a hardcoded URL for now
        // In production, you'd have this configurable or load from embedded HTML
        webView.loadUrl("http://10.0.2.2:5000"); // Special IP that points to host's localhost in Android emulator
    }

    @Override
    public void onBackPressed() {
        if (webView.canGoBack()) {
            webView.goBack();
        } else {
            super.onBackPressed();
        }
    }
}