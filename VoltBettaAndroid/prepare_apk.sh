#!/bin/bash
# Prepare Android app for packaging

echo "Preparing Volt Betta Manager for Android packaging..."

# Ensure directories exist
mkdir -p app/src/main/assets/webapp
mkdir -p app/src/main/res/drawable

# Download the web app from the running server
echo "Downloading web app resources..."
curl -s http://localhost:5000 > app/src/main/assets/webapp/index.html

# Create JavaScript bridge for native Android integration
echo "Creating JavaScript bridge..."
cat > app/src/main/assets/webapp/androidBridge.js << 'EOF'
// Android Bridge to handle native platform features
(function() {
  // Create a global AndroidBridge object for web-to-Android communication
  window.AndroidBridge = {
    // Check if running in Android WebView
    isRunningInAndroid: function() {
      return typeof AndroidInterface !== 'undefined';
    },
    
    // Send a toast message to the Android app
    showToast: function(message) {
      if (this.isRunningInAndroid()) {
        AndroidInterface.showToast(message);
      } else {
        console.log('Toast (web): ' + message);
      }
    },
    
    // Get device information from Android
    getDeviceInfo: function() {
      if (this.isRunningInAndroid()) {
        return AndroidInterface.getDeviceInfo();
      } else {
        return 'Web browser';
      }
    },
    
    // Open external links in the device browser
    openExternalLink: function(url) {
      if (this.isRunningInAndroid()) {
        AndroidInterface.openExternalLink(url);
      } else {
        window.open(url, '_blank');
      }
    },
    
    // Get current location (with permission)
    getLocation: function(callback) {
      if (this.isRunningInAndroid()) {
        // The location will be returned via the setLocation function
        AndroidInterface.requestLocation();
        // Set a global callback that Android will call
        window.setLocation = function(latitude, longitude) {
          callback({ latitude: latitude, longitude: longitude });
        };
      } else {
        // Use browser geolocation API as fallback
        if (navigator.geolocation) {
          navigator.geolocation.getCurrentPosition(
            function(position) {
              callback({
                latitude: position.coords.latitude,
                longitude: position.coords.longitude
              });
            },
            function(error) {
              console.error('Geolocation error:', error);
              callback(null);
            }
          );
        } else {
          console.error('Geolocation not available in this browser');
          callback(null);
        }
      }
    },
    
    // Take a photo using device camera (Android only)
    takePicture: function() {
      if (this.isRunningInAndroid()) {
        AndroidInterface.takePicture();
      } else {
        console.log('Camera functionality is only available in the Android app');
        alert('Camera functionality is only available in the Android app');
      }
    }
  };
  
  // When running in Android, alert user that app is ready
  document.addEventListener('DOMContentLoaded', function() {
    if (AndroidBridge.isRunningInAndroid()) {
      console.log('Volt Betta Manager running in Android WebView');
      AndroidBridge.showToast('Welcome to Volt Betta Manager!');
    }
  });
})();
EOF

# Copy the SQLite database for offline use
echo "Copying SQLite database..."
cp ../VoltBettaApp/database/voltbetta.sqlite app/src/main/assets/webapp/

# Create offline mode HTML file
echo "Creating offline mode HTML..."
cat > app/src/main/assets/webapp/offline.html << 'EOF'
<!DOCTYPE html>
<html>
<head>
  <title>Volt Betta Manager - Offline Mode</title>
  <meta name="viewport" content="width=device-width, initial-scale=1.0">
  <style>
    body {
      font-family: Arial, sans-serif;
      max-width: 700px;
      margin: 0 auto;
      padding: 20px;
      background-color: #f5f5f5;
      color: #333;
    }
    .offline-notice {
      background-color: #F04F94;
      color: white;
      padding: 15px;
      border-radius: 10px;
      margin-bottom: 20px;
      text-align: center;
      box-shadow: 0 4px 8px rgba(0,0,0,0.1);
    }
    .fish-profile {
      background: linear-gradient(135deg, #F04F94, #F0734F);
      color: white;
      padding: 20px;
      border-radius: 10px;
      margin-bottom: 20px;
      display: flex;
      flex-wrap: wrap;
      justify-content: space-between;
    }
    .fish-profile-info {
      flex: 1;
      min-width: 300px;
    }
    .fish-avatar {
      width: 100px;
      height: 100px;
      background-color: #fff;
      border-radius: 50%;
      display: flex;
      justify-content: center;
      align-items: center;
      margin-bottom: 15px;
      font-size: 40px;
    }
    h1 { color: #F04F94; }
    h2 { color: #F0734F; margin-top: 30px; border-bottom: 2px dashed #F08F4F; padding-bottom: 5px; }
  </style>
</head>
<body>
  <header>
    <h1>Volt Betta Manager - Offline Mode</h1>
  </header>
  
  <div class="offline-notice">
    <h2 style="color:white; margin-top:0;">You're currently offline</h2>
    <p>You're viewing a limited version of the Volt Betta Manager app. Some features may not be available.</p>
  </div>
  
  <div class="fish-profile">
    <div class="fish-profile-info">
      <div class="fish-avatar" id="fishAvatar">🐠</div>
      <h2 id="fishName">Your Betta</h2>
      <div id="fishInfo">Loading saved data...</div>
    </div>
  </div>
  
  <div class="maintenance-reminders">
    <h2>Maintenance Reminders</h2>
    <ul id="remindersList">
      <li>Water change (25%) - Every 7 days</li>
      <li>Check filter - Every 14 days</li>
      <li>Clean substrate - Every 30 days</li>
    </ul>
  </div>
  
  <script>
    // Try to load saved fish data from local storage
    document.addEventListener('DOMContentLoaded', function() {
      // Check if we have stored data
      const storedData = localStorage.getItem('voltBettaFishData');
      
      if (storedData) {
        try {
          const fishData = JSON.parse(storedData);
          document.getElementById('fishName').textContent = fishData.name || 'Your Betta';
          
          let infoHTML = '';
          if (fishData.species) infoHTML += '<div>Species: ' + fishData.species + '</div>';
          if (fishData.color) infoHTML += '<div>Color: ' + fishData.color + '</div>';
          if (fishData.acquisitionDate) infoHTML += '<div>With you since: ' + fishData.acquisitionDate + '</div>';
          
          document.getElementById('fishInfo').innerHTML = infoHTML || 'No additional information available offline.';
          
          if (fishData.profilePicture) {
            if (fishData.isPictureEmoji) {
              document.getElementById('fishAvatar').textContent = fishData.profilePicture;
            } else {
              document.getElementById('fishAvatar').innerHTML = '<img src="' + fishData.profilePicture + '" alt="Fish profile" style="width:100%; height:100%; object-fit:cover; border-radius:50%;">';
            }
          }
        } catch (e) {
          console.error('Error parsing stored fish data:', e);
        }
      }
    });
  </script>
</body>
</html>
EOF

# Create app icon if it doesn't exist
if [ ! -f "app/src/main/res/drawable/ic_launcher.png" ]; then
  echo "Creating app icon..."
  # Create a simple colored rectangle as a placeholder
  convert -size 192x192 xc:#F04F94 \
    -gravity center -pointsize 120 -fill white -annotate 0 "VB" \
    -gravity SouthEast -pointsize 40 -fill white -annotate +10+10 "Beta" \
    app/src/main/res/drawable/ic_launcher.png || echo "Icon creation skipped (ImageMagick not available)"
fi

echo "Android preparation complete!"