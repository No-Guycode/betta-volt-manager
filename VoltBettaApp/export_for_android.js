/**
 * This script exports the VoltBettaApp web application for inclusion in the Android package.
 * It copies necessary files and creates static versions of dynamic content.
 */

const fs = require('fs');
const path = require('path');
const { exec } = require('child_process');

// Directories
const SOURCE_DIR = __dirname;
const TARGET_DIR = path.join(__dirname, '..', 'VoltBettaAndroid', 'app', 'src', 'main', 'assets', 'webapp');

// Create target directory if it doesn't exist
function createDirectories() {
  console.log('Creating target directories...');
  if (!fs.existsSync(TARGET_DIR)) {
    fs.mkdirSync(TARGET_DIR, { recursive: true });
  }
}

// Copy static files
function copyStaticFiles() {
  console.log('Copying static files...');
  
  // Copy public directory if it exists
  const publicDir = path.join(SOURCE_DIR, 'public');
  if (fs.existsSync(publicDir)) {
    exec(`cp -r ${publicDir}/* ${TARGET_DIR}/`, (error) => {
      if (error) {
        console.error(`Error copying public files: ${error}`);
      } else {
        console.log('Public files copied successfully');
      }
    });
  }
  
  // Copy uploads directory (for photos)
  const uploadsDir = path.join(SOURCE_DIR, 'uploads');
  const targetUploadsDir = path.join(TARGET_DIR, 'uploads');
  
  if (!fs.existsSync(targetUploadsDir)) {
    fs.mkdirSync(targetUploadsDir, { recursive: true });
  }
  
  if (fs.existsSync(uploadsDir)) {
    exec(`cp -r ${uploadsDir}/* ${targetUploadsDir}/`, (error) => {
      if (error) {
        console.error(`Error copying uploads: ${error}`);
      } else {
        console.log('Uploads copied successfully');
      }
    });
  }
}

// Copy database for offline access
function copyDatabase() {
  console.log('Copying database for offline access...');
  const dbSource = path.join(SOURCE_DIR, 'database', 'voltbetta.sqlite');
  const dbTarget = path.join(TARGET_DIR, 'voltbetta.sqlite');
  
  if (fs.existsSync(dbSource)) {
    fs.copyFileSync(dbSource, dbTarget);
    console.log('Database copied successfully');
  } else {
    console.error('Database file not found');
  }
}

// Create a simple index.html for offline mode
function createOfflineHTML() {
  console.log('Creating offline HTML file...');
  // Read the existing offline.html if available
  const offlineHtmlPath = path.join(SOURCE_DIR, '..', 'VoltBettaAndroid', 'app', 'src', 'main', 'assets', 'offline.html');
  
  if (fs.existsSync(offlineHtmlPath)) {
    const offlineHtml = fs.readFileSync(offlineHtmlPath, 'utf8');
    fs.writeFileSync(path.join(TARGET_DIR, 'index.html'), offlineHtml);
    console.log('Offline HTML created successfully');
  } else {
    console.error('Offline HTML template not found');
  }
}

// Main function
function main() {
  console.log('Starting export for Android...');
  createDirectories();
  copyStaticFiles();
  copyDatabase();
  createOfflineHTML();
  console.log('Export completed!');
}

// Run the export
main();