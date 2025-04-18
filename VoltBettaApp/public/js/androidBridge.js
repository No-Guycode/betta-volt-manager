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
  
  console.log('Android Bridge initialized - Web mode');
})();