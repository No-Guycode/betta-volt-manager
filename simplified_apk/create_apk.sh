#!/bin/bash
# Create a simplified package for VoltBetta app

echo "===== Creating simplified Volt Betta Manager package ====="

# Create directory structure
mkdir -p VoltBettaManager/webapp
mkdir -p VoltBettaManager/META-INF

# Copy web app files
echo "Capturing web app for packaging..."
curl -s http://localhost:5000 > VoltBettaManager/webapp/index.html
cp -r ../VoltBettaApp/database VoltBettaManager/webapp/
cp ../VoltBettaApp/public/js/androidBridge.js VoltBettaManager/webapp/

# Create manifest
echo "Creating manifest..."
cat > VoltBettaManager/META-INF/MANIFEST.MF << EOF
Manifest-Version: 1.0
Main-Class: com.voltbetta.app.VoltBettaManager
Created-By: Replit
EOF

# Create a readme with installation instructions
echo "Creating installation instructions..."
cat > VoltBettaManager/README.txt << EOF
Volt Betta Manager - Installation Instructions
=============================================

This is a simplified package of the Volt Betta Manager app.
To properly install this on an Android device, you'll need to:

1. Unzip this package on your computer
2. Transfer the entire VoltBettaManager folder to your Android device
3. Install a WebView app from the Google Play Store if not already installed
4. Use a file manager on your Android device to browse to the VoltBettaManager folder
5. Open the webapp/index.html file with your WebView app
6. For the best experience, create a shortcut to this file on your home screen

Note: This is not a standard APK installation. For a full Android app experience, 
the project should be built using Android Studio following the instructions in the
build_instructions.md file.
EOF

# Create an archive
echo "Creating final package..."
cd VoltBettaManager
zip -r ../VoltBettaManager.zip .
cd ..

echo "✅ Package created successfully: VoltBettaManager.zip"
echo "Download this file and follow the instructions in README.txt to use the app."