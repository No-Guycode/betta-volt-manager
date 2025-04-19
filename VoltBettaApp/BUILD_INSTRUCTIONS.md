# Volt Betta Manager - Build Instructions

This document provides step-by-step instructions for building the Volt Betta Manager application into an APK file using EAS (Expo Application Services).

## Prerequisites

Before you begin, ensure you have the following installed on your system:

1. **Node.js** (version 16 or higher)
2. **npm** (Node Package Manager)
3. **Expo CLI** - Install with `npm install -g expo-cli`
4. **EAS CLI** - Install with `npm install -g eas-cli`
5. **Expo account** - Create one at [https://expo.dev/signup](https://expo.dev/signup)

## Building Steps

### 1. Clone or Download the Repository

Download the VoltBettaApp folder from this project.

### 2. Install Dependencies

Open a terminal in the VoltBettaApp directory and run:

```bash
npm install
```

### 3. Log in to Expo

```bash
eas login
```

Enter your Expo account credentials when prompted.

### 4. Build the Android APK

#### Option 1: Build locally (no Expo account required)

This option builds the APK locally on your machine:

```bash
eas build --platform android --profile local --local
```

Note: You need to have Android build tools installed for local builds.

#### Option 2: Build in the Expo cloud (recommended)

This option builds your APK in Expo's cloud servers:

```bash
eas build --platform android --profile apk
```

#### Option 3: Development build

For a development build with debugging enabled:

```bash
eas build --platform android --profile development
```

### 5. Download the APK

After the build completes:

- For local builds: The APK will be available in a directory specified in the terminal output
- For cloud builds: You'll receive a URL to download the APK directly from Expo's servers

### 6. Install the APK

Transfer the APK to your Android device and install it by opening the file.

## Troubleshooting

- If you encounter build errors, try running `expo doctor` to identify and fix potential issues
- For credential issues, run `eas credentials` to manage your app credentials
- For more detailed logs, add the `--verbose` flag to your build command

## Additional Resources

- [Expo Documentation](https://docs.expo.dev/)
- [EAS CLI Documentation](https://docs.expo.dev/eas/)
- [Expo Build Configuration](https://docs.expo.dev/build/introduction/)

## Version Information

This build setup was tested with:
- Expo SDK: 50.0.0
- EAS CLI: 5.9.1+