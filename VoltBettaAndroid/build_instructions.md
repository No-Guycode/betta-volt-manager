# Volt Betta Manager for Android - Build Instructions

This document provides instructions for building the Android APK for the Volt Betta Manager app.

## Prerequisites

- Android Studio (or Android SDK with Gradle)
- JDK 11 or higher
- The Volt Betta Manager web app running locally on port 5000

## Quick Build (Using the Build Script)

The easiest way to build the APK is to use the included build script:

1. Start the Volt Betta Manager web app:
   ```
   cd VoltBettaApp && node run.js
   ```

2. In a separate terminal, run the build script:
   ```
   ./build_apk.sh
   ```

3. The script will prepare all necessary files and attempt to build the APK using Gradle

4. If successful, the APK will be available at:
   ```
   VoltBettaManager.apk (in the root directory)
   ```
   or
   ```
   VoltBettaAndroid/app/build/outputs/apk/debug/app-debug.apk
   ```

## Manual Build with Android Studio

If you prefer to build manually with Android Studio:

1. Start the Volt Betta Manager web app:
   ```
   cd VoltBettaApp && node run.js
   ```

2. Run the preparation script to package the web app for Android:
   ```
   cd VoltBettaAndroid && ./prepare_apk.sh
   ```

3. Open the VoltBettaAndroid project in Android Studio:
   - Start Android Studio
   - Choose "Open an existing project"
   - Select the VoltBettaAndroid directory

4. Wait for the project to sync and index

5. Build the APK:
   - Click on Build > Build Bundle(s) / APK(s) > Build APK(s)
   - Wait for the build to complete

6. The APK will be available at:
   ```
   VoltBettaAndroid/app/build/outputs/apk/debug/app-debug.apk
   ```

7. Click on the "locate" link in the notification to find the APK

## Installing the APK on Android

Once you have the APK file, you can install it on an Android device:

1. Enable "Install from Unknown Sources" in your device settings (if not already enabled)
2. Transfer the APK to your device via USB, email, or cloud storage
3. Tap on the APK file on your device to install
4. Follow the on-screen instructions to complete installation

## Troubleshooting

If you encounter build issues:

1. Make sure the web app is running on port 5000
2. Check that all directories exist in the Android project
3. Ensure you have JDK 11+ and Android SDK properly configured
4. Review the Gradle build logs for specific errors