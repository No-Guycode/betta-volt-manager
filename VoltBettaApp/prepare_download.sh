#!/bin/bash

# This script prepares the VoltBettaApp project for download and local building
# It creates a zip file with all necessary files

echo "Preparing VoltBettaApp for download..."

# Create a temporary directory for packaging
mkdir -p ./temp_package

# Copy all necessary files
cp -r app assets components constants database hooks ios models public screens scripts uploads .env .gitignore App.tsx app.json eas.json package.json package-lock.json run.js tsconfig.json BUILD_INSTRUCTIONS.md ./temp_package/

# Create README file with instructions
cat > ./temp_package/README.md << 'EOL'
# Volt Betta Manager App

This is the Volt Betta Manager application, packaged for local building.

## Quick Start

1. Extract all files from this zip archive
2. Follow the instructions in BUILD_INSTRUCTIONS.md to build the APK
3. Run `npm install` in this directory to install dependencies
4. Run `eas build --platform android --profile apk` to build with Expo Application Services

## Application Features

- Betta fish care management 
- Tank parameters tracking
- Maintenance scheduling
- Treatment plans
- Photo gallery
- Notes and observations

## Technical Details

This app is built with React Native and Expo, with a SQLite database for local storage.
The APK can be built using EAS CLI as detailed in the build instructions.

EOL

# Create the zip file
cd temp_package
zip -r ../VoltBettaApp_Build_Package.zip ./*
cd ..

# Clean up
rm -rf temp_package

echo "Download package created: VoltBettaApp_Build_Package.zip"
echo "You can download this file and follow the instructions in BUILD_INSTRUCTIONS.md to build the APK"