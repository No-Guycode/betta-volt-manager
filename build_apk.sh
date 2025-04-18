#!/bin/bash
# Build script for Volt Betta Manager Android APK

echo "===== Volt Betta Manager - Android APK Builder ====="
echo "This script will prepare the Android app for APK generation."

# Check if the web app is running
if ! curl -s http://localhost:5000 > /dev/null; then
  echo "ERROR: The Volt Betta web app must be running at http://localhost:5000"
  echo "Please start the web app by running: node VoltBettaApp/run.js"
  exit 1
fi

echo "Web app is running... proceeding with Android preparation."

# Create required directories
echo "Creating Android project directories..."
mkdir -p VoltBettaAndroid/app/src/main/assets/webapp
mkdir -p VoltBettaAndroid/app/src/main/res/drawable

# Make prepare_apk.sh executable
chmod +x VoltBettaAndroid/prepare_apk.sh

# Run the Android preparation script
echo "Running Android preparation script..."
cd VoltBettaAndroid
./prepare_apk.sh
cd ..

# Take a snapshot of the running web app for offline mode
echo "Capturing web app snapshot for offline mode..."
curl -s http://localhost:5000 > VoltBettaAndroid/app/src/main/assets/webapp/index.html

# Copy database for offline access
echo "Copying database for offline access..."
cp VoltBettaApp/database/voltbetta.sqlite VoltBettaAndroid/app/src/main/assets/webapp/

# Create sample data in case offline database is not available
echo "Preparing fallback data..."
cat > VoltBettaAndroid/app/src/main/assets/webapp/fallback_data.json << EOF
{
  "fishData": {
    "name": "Volt",
    "species": "Betta splendens",
    "variant": "Veiltail",
    "color": "Blue",
    "acquisitionDate": "April 5, 2025",
    "duration": "14 days",
    "profilePicture": "🐠",
    "isPictureEmoji": true
  }
}
EOF

echo "===== Android APK Preparation Complete ====="

# Check if Gradle is available
if command -v ./VoltBettaAndroid/gradlew &> /dev/null; then
  echo "Building APK using Gradle..."
  cd VoltBettaAndroid
  ./gradlew assembleDebug
  cd ..
  
  # Check if build was successful
  if [ -f "VoltBettaAndroid/app/build/outputs/apk/debug/app-debug.apk" ]; then
    echo "✅ APK SUCCESSFULLY GENERATED!"
    echo "Location: VoltBettaAndroid/app/build/outputs/apk/debug/app-debug.apk"
    
    # Create a copy in the root directory for easy access
    cp VoltBettaAndroid/app/build/outputs/apk/debug/app-debug.apk ./VoltBettaManager.apk
    echo "A copy has been placed in the root directory as: VoltBettaManager.apk"
  else
    echo "❌ APK generation failed. See error messages above."
    echo "You can try building manually with Android Studio."
  fi
else
  echo "The Android project is now ready for building. To create the APK:"
  echo "1. Open the VoltBettaAndroid project in Android Studio"
  echo "2. Build > Build Bundle(s) / APK(s) > Build APK(s)"
  echo "3. The APK will be available at: VoltBettaAndroid/app/build/outputs/apk/debug/app-debug.apk"
fi

echo ""
echo "For more detailed instructions, see: VoltBettaAndroid/build_instructions.md"