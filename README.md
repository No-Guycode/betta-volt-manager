# Volt Betta Manager

A comprehensive Betta fish care tracking application, designed to help Betta fish owners monitor their fish's health, tank parameters, maintenance, and more.

## Project Overview

The Volt Betta Manager consists of two main components:

1. **Web Application (VoltBettaApp)**: A responsive web interface built with Node.js, Express, and SQLite
2. **Android Package (VoltBettaAndroid)**: An Android WebView wrapper for the web application

## Features

- **Fish Profile Management**: Track your Betta's information including acquisition date and display time with your fish
- **Tank Parameter Logging**: Record and monitor water parameters (pH, temperature, ammonia, nitrites, nitrates)
- **Maintenance Tracking**: Schedule and log water changes, filter cleanings, and other maintenance tasks
- **Treatment Plans**: Document medications and treatments
- **Photo Gallery**: Upload and view pictures of your Betta fish
- **Notes**: Add and review observations or notes about your fish and tank
- **Mobile-Responsive**: Works on desktop and mobile devices with touch support
- **Offline Mode**: Basic functionality when internet connection is unavailable

## Web Application (VoltBettaApp)

### Setup and Running

1. Navigate to the VoltBettaApp directory:
   ```
   cd VoltBettaApp
   ```

2. Start the application:
   ```
   node run.js
   ```

3. Access the web interface at http://localhost:5000

### Technology Stack

- **Backend**: Node.js with Express
- **Database**: SQLite with Sequelize ORM
- **Frontend**: HTML, CSS, JavaScript

## Android Package (VoltBettaAndroid)

The Android package wraps the web application in a WebView for native Android deployment.

### Building the APK

See the detailed instructions in [VoltBettaAndroid/build_instructions.md](VoltBettaAndroid/build_instructions.md)

### Features of the Android App

- Native Android experience
- Offline mode when server isn't accessible
- Deep integration with Android system
- Touch-optimized interface

## Development Notes

- The web application is designed to be responsive for both desktop and mobile views
- Database is automatically initialized with sample data on first run
- The Android package requires the web server to be running and accessible
- For development, the Android app points to 10.0.2.2:5000 (localhost for Android emulator)

## Future Enhancements

- Push notifications for maintenance reminders
- Data backup and restore functionality
- Statistical analysis of tank parameters over time
- Community sharing features
- Standalone Android app without requiring the server

## License

This project is licensed under the MIT License - see the LICENSE file for details.