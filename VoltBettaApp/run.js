// Script to start the Expo app on port 5000
const express = require('express');
const app = express();
const port = 5000;

// Simple Express server to display app information since Expo isn't working properly yet
app.get('/', (req, res) => {
  res.send(`
    <html>
      <head>
        <title>Volt Betta Manager App</title>
        <style>
          body {
            font-family: Arial, sans-serif;
            max-width: 800px;
            margin: 0 auto;
            padding: 20px;
            line-height: 1.6;
          }
          h1 { color: #2196F3; }
          h2 { color: #0D47A1; margin-top: 30px; }
          .feature {
            background: #f5f5f5;
            padding: 15px;
            border-radius: 5px;
            margin-bottom: 10px;
          }
          .feature h3 {
            margin-top: 0;
            color: #1976D2;
          }
        </style>
      </head>
      <body>
        <h1>Volt Betta Manager App</h1>
        <p>A mobile application for managing care information, maintenance schedules, and photos for your betta fish named Volt.</p>
        
        <h2>App Features</h2>
        
        <div class="feature">
          <h3>Home Dashboard</h3>
          <p>Shows fish profile with key information like age, variant, and tank size. Also displays quick stats and upcoming maintenance tasks.</p>
        </div>
        
        <div class="feature">
          <h3>Tank Log</h3>
          <p>Record and track water parameters, maintenance history, and water change schedules.</p>
        </div>
        
        <div class="feature">
          <h3>Maintenance</h3>
          <p>Schedule and manage recurring tank maintenance tasks with notifications.</p>
        </div>
        
        <div class="feature">
          <h3>Plants</h3>
          <p>Track aquarium plants, their care requirements, and growth progress.</p>
        </div>
        
        <div class="feature">
          <h3>Treatments</h3>
          <p>Document health issues, medications, and treatment progress for your betta fish.</p>
        </div>
        
        <div class="feature">
          <h3>Gallery</h3>
          <p>Store and organize photos of your betta fish to track growth and color changes.</p>
        </div>
        
        <div class="feature">
          <h3>Notes</h3>
          <p>Keep journal entries and general observations about your betta fish and tank.</p>
        </div>
        
        <h2>Current Status</h2>
        <p>Basic screens have been set up for all major features. Currently resolving dependency and build issues.</p>
      </body>
    </html>
  `);
});

app.listen(port, () => {
  console.log(`Volt Betta Manager information page running at http://localhost:${port}`);
});