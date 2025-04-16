// Script to start the Expo app on port 5000
const express = require('express');
const app = express();
const port = 5000;

// Serve static files
app.use(express.static(__dirname + '/public'));
app.use(express.json());

// Mock data for the prototype
let fishData = {
  name: 'Volt',
  species: 'Betta splendens',
  variant: 'Halfmoon',
  color: 'Blue/Red',
  age: '~1 year 3 months',
  tank: '5 gallon planted',
  acquisitionDate: 'January 15, 2024'
};

let maintenanceTasks = [
  { id: 1, task: 'Water Change (25%)', dueIn: '3 days', recurring: 'Weekly', lastDone: '4 days ago' },
  { id: 2, task: 'Replace Filter Media', dueIn: '1 week', recurring: 'Monthly', lastDone: '3 weeks ago' },
  { id: 3, task: 'Clean Gravel', dueIn: '10 days', recurring: 'Bi-weekly', lastDone: '4 days ago' },
  { id: 4, task: 'Test Water Parameters', dueIn: '2 days', recurring: 'Weekly', lastDone: '5 days ago' },
  { id: 5, task: 'Trim Plants', dueIn: '5 days', recurring: 'Bi-weekly', lastDone: '9 days ago' }
];

let tankLogs = [
  { id: 1, date: '2025-04-10', ammonia: '0', nitrite: '0', nitrate: '5', ph: '7.2', temp: '78', notes: 'Parameters looking good. Added 1mL of plant fertilizer.' },
  { id: 2, date: '2025-04-03', ammonia: '0', nitrite: '0', nitrate: '10', ph: '7.0', temp: '79', notes: 'Did a 25% water change. Replaced filter media.' },
  { id: 3, date: '2025-03-27', ammonia: '0', nitrite: '0', nitrate: '15', ph: '7.1', temp: '78', notes: 'Volt seems very active today. Rearranged plants slightly.' }
];

// Simple Express server to display app information with interactive elements
app.get('/', (req, res) => {
  res.send(`
    <html>
      <head>
        <title>Volt Betta Manager App</title>
        <style>
          body {
            font-family: Arial, sans-serif;
            max-width: 900px;
            margin: 0 auto;
            padding: 20px;
            line-height: 1.6;
            background-color: #fff;
            color: #333;
          }
          header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 20px;
            border-bottom: 3px solid #F04F94;
            padding-bottom: 10px;
          }
          nav {
            display: flex;
            gap: 15px;
          }
          .nav-button {
            background-color: #F04F94;
            color: white;
            border: none;
            padding: 8px 12px;
            border-radius: 20px;
            cursor: pointer;
            font-weight: bold;
            transition: all 0.3s ease;
          }
          .nav-button:hover {
            background-color: #ED4FF0;
            transform: translateY(-2px);
          }
          h1 { color: #F04F94; }
          h2 { color: #F0734F; margin-top: 30px; border-bottom: 2px dashed #F08F4F; padding-bottom: 5px; }
          .feature {
            background: #fff9f7;
            padding: 20px;
            border-radius: 10px;
            margin-bottom: 20px;
            border-left: 5px solid #F05950;
            box-shadow: 0 4px 8px rgba(0,0,0,0.1);
            transition: all 0.3s ease;
          }
          .feature:hover {
            transform: translateY(-5px);
            box-shadow: 0 6px 12px rgba(0,0,0,0.15);
          }
          .feature h3 {
            margin-top: 0;
            color: #ED4FF0;
            display: flex;
            align-items: center;
          }
          .feature h3:before {
            content: '✦';
            margin-right: 8px;
            color: #F51A0F;
          }
          .feature:nth-child(odd) {
            border-left-color: #F08F4F;
          }
          .feature:nth-child(even) h3 {
            color: #F51A0F;
          }
          button {
            background-color: #F04F94;
            color: white;
            border: none;
            padding: 10px 15px;
            border-radius: 5px;
            cursor: pointer;
            margin-top: 10px;
            font-weight: bold;
            transition: all 0.2s ease;
          }
          button:hover {
            background-color: #ED4FF0;
            transform: scale(1.05);
          }
          .content-section {
            display: none; /* Hidden by default */
          }
          .content-section.active {
            display: block;
          }
          #home-section {
            display: block; /* Show by default */
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
          .fish-stats {
            display: flex;
            flex-wrap: wrap;
            margin-top: 15px;
          }
          .fish-stat {
            background-color: rgba(255,255,255,0.2);
            padding: 8px 12px;
            border-radius: 5px;
            margin-right: 10px;
            margin-bottom: 10px;
          }
          .stat-label {
            font-size: 12px;
            opacity: 0.8;
          }
          .stat-value {
            font-weight: bold;
          }
          .quick-stats {
            background-color: #fff;
            padding: 15px;
            border-radius: 10px;
            box-shadow: 0 4px 6px rgba(0,0,0,0.1);
            margin-bottom: 20px;
          }
          .stat-item {
            display: flex;
            align-items: center;
            margin-bottom: 10px;
            padding-bottom: 10px;
            border-bottom: 1px solid #eee;
          }
          .stat-item:last-child {
            border-bottom: none;
            margin-bottom: 0;
            padding-bottom: 0;
          }
          .stat-icon {
            width: 40px;
            height: 40px;
            background-color: #F0734F;
            color: white;
            border-radius: 50%;
            display: flex;
            justify-content: center;
            align-items: center;
            margin-right: 15px;
            font-weight: bold;
          }
          .maintenance-task {
            background-color: #fff;
            padding: 15px;
            border-radius: 8px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
            margin-bottom: 10px;
            display: flex;
            align-items: center;
            justify-content: space-between;
            transition: all 0.2s ease;
          }
          .maintenance-task:hover {
            transform: translateX(5px);
            box-shadow: 0 4px 8px rgba(0,0,0,0.15);
          }
          .task-info {
            flex: 1;
          }
          .task-name {
            font-weight: bold;
            color: #F04F94;
          }
          .task-due {
            font-size: 14px;
            color: #666;
          }
          .task-badge {
            background-color: #F05950;
            color: white;
            padding: 5px 10px;
            border-radius: 15px;
            font-size: 12px;
            font-weight: bold;
          }
          .tank-log-entry {
            background-color: #fff;
            padding: 15px;
            border-radius: 8px;
            box-shadow: 0 2px 4px rgba(0,0,0,0.1);
            margin-bottom: 15px;
          }
          .log-date {
            font-weight: bold;
            color: #F04F94;
            margin-bottom: 8px;
          }
          .log-params {
            display: flex;
            flex-wrap: wrap;
            margin-bottom: 8px;
          }
          .param {
            background-color: #f0f0f0;
            padding: 5px 10px;
            border-radius: 5px;
            margin-right: 8px;
            margin-bottom: 8px;
            font-size: 14px;
          }
          .param-label {
            font-weight: bold;
            color: #F0734F;
          }
          .log-notes {
            font-style: italic;
            color: #666;
          }
          .add-button {
            position: fixed;
            bottom: 20px;
            right: 20px;
            width: 60px;
            height: 60px;
            background-color: #F04F94;
            color: white;
            border-radius: 50%;
            display: flex;
            justify-content: center;
            align-items: center;
            font-size: 30px;
            box-shadow: 0 4px 8px rgba(0,0,0,0.2);
            cursor: pointer;
            transition: all 0.3s ease;
          }
          .add-button:hover {
            transform: scale(1.1);
            background-color: #ED4FF0;
          }
        </style>
      </head>
      <body>
        <header>
          <h1>Volt Betta Manager</h1>
          <nav>
            <button class="nav-button" onclick="showSection('home-section')">Home</button>
            <button class="nav-button" onclick="showSection('tanklog-section')">Tank Log</button>
            <button class="nav-button" onclick="showSection('maintenance-section')">Maintenance</button>
            <button class="nav-button" onclick="showSection('features-section')">Features</button>
          </nav>
        </header>
        
        <!-- Home Section -->
        <section id="home-section" class="content-section active">
          <div class="fish-profile">
            <div class="fish-profile-info">
              <div class="fish-avatar">🐠</div>
              <h2>${fishData.name}</h2>
              <p>${fishData.species} - ${fishData.variant}</p>
              
              <div class="fish-stats">
                <div class="fish-stat">
                  <div class="stat-label">Age</div>
                  <div class="stat-value">${fishData.age}</div>
                </div>
                <div class="fish-stat">
                  <div class="stat-label">Color</div>
                  <div class="stat-value">${fishData.color}</div>
                </div>
                <div class="fish-stat">
                  <div class="stat-label">Tank</div>
                  <div class="stat-value">${fishData.tank}</div>
                </div>
                <div class="fish-stat">
                  <div class="stat-label">Acquired</div>
                  <div class="stat-value">${fishData.acquisitionDate}</div>
                </div>
              </div>
            </div>
          </div>
          
          <h2>Quick Stats</h2>
          <div class="quick-stats">
            <div class="stat-item">
              <div class="stat-icon">💧</div>
              <div>
                <div>Last Water Change</div>
                <div><strong>4 days ago</strong></div>
              </div>
            </div>
            <div class="stat-item">
              <div class="stat-icon">🧪</div>
              <div>
                <div>Last Water Test</div>
                <div><strong>1 week ago</strong></div>
              </div>
            </div>
            <div class="stat-item">
              <div class="stat-icon">📷</div>
              <div>
                <div>Photos Taken</div>
                <div><strong>28 total</strong></div>
              </div>
            </div>
          </div>
          
          <h2>Upcoming Maintenance</h2>
          ${maintenanceTasks.slice(0, 3).map(task => `
            <div class="maintenance-task">
              <div class="task-info">
                <div class="task-name">${task.task}</div>
                <div class="task-due">Due in ${task.dueIn} • ${task.recurring}</div>
              </div>
              <div class="task-badge">Due soon</div>
            </div>
          `).join('')}
          
          <button style="width: 100%; margin-top: 15px;" onclick="showSection('maintenance-section')">View All Tasks</button>
        </section>
        
        <!-- Tank Log Section -->
        <section id="tanklog-section" class="content-section">
          <h2>Tank Log</h2>
          <p>Track water parameters and tank maintenance over time.</p>
          
          ${tankLogs.map(log => `
            <div class="tank-log-entry">
              <div class="log-date">${new Date(log.date).toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' })}</div>
              <div class="log-params">
                <div class="param"><span class="param-label">Ammonia:</span> ${log.ammonia} ppm</div>
                <div class="param"><span class="param-label">Nitrite:</span> ${log.nitrite} ppm</div>
                <div class="param"><span class="param-label">Nitrate:</span> ${log.nitrate} ppm</div>
                <div class="param"><span class="param-label">pH:</span> ${log.ph}</div>
                <div class="param"><span class="param-label">Temp:</span> ${log.temp}°F</div>
              </div>
              <div class="log-notes">${log.notes}</div>
            </div>
          `).join('')}
        </section>
        
        <!-- Maintenance Section -->
        <section id="maintenance-section" class="content-section">
          <h2>Maintenance Schedule</h2>
          <p>Keep track of recurring tank maintenance tasks.</p>
          
          ${maintenanceTasks.map(task => `
            <div class="maintenance-task">
              <div class="task-info">
                <div class="task-name">${task.task}</div>
                <div class="task-due">Due in ${task.dueIn} • ${task.recurring} • Last done: ${task.lastDone}</div>
              </div>
              <div class="task-badge">Due soon</div>
            </div>
          `).join('')}
        </section>
        
        <!-- Features Section -->
        <section id="features-section" class="content-section">
          <h2>App Features</h2>
          
          <div class="feature">
            <h3>Home Dashboard</h3>
            <p>Shows fish profile with key information like age, variant, and tank size. Also displays quick stats and upcoming maintenance tasks.</p>
          </div>
          
          <div class="feature">
            <h3>Tank Log</h3>
            <p>Record and track water parameters, maintenance history, and water change schedules. Monitor ammonia, nitrite, nitrate, pH, and temperature over time.</p>
          </div>
          
          <div class="feature">
            <h3>Maintenance</h3>
            <p>Schedule and manage recurring tank maintenance tasks with notifications. Never miss a water change or filter replacement again.</p>
          </div>
          
          <div class="feature">
            <h3>Plants</h3>
            <p>Track aquarium plants, their care requirements, and growth progress. Includes reminders for fertilizer dosing and pruning needs.</p>
          </div>
          
          <div class="feature">
            <h3>Treatments</h3>
            <p>Document health issues, medications, and treatment progress for your betta fish. Keep a record of symptoms, medications, and outcomes.</p>
          </div>
          
          <div class="feature">
            <h3>Gallery</h3>
            <p>Store and organize photos of your betta fish to track growth and color changes over time. Create chronological visual documentation.</p>
          </div>
          
          <div class="feature">
            <h3>Notes</h3>
            <p>Keep journal entries and general observations about your betta fish and tank. Tag entries for easy organization and search.</p>
          </div>
        </section>
        
        <div class="add-button" title="Add New Entry">+</div>
        
        <script>
          function showSection(sectionId) {
            // Hide all sections
            const sections = document.querySelectorAll('.content-section');
            sections.forEach(section => {
              section.classList.remove('active');
            });
            
            // Show the selected section
            document.getElementById(sectionId).classList.add('active');
          }
          
          // Add button click alert
          document.querySelector('.add-button').addEventListener('click', function() {
            const activeSectionId = document.querySelector('.content-section.active').id;
            
            let message = 'Add new ';
            switch(activeSectionId) {
              case 'home-section':
                message += 'home entry';
                break;
              case 'tanklog-section':
                message += 'tank log entry';
                break;
              case 'maintenance-section':
                message += 'maintenance task';
                break;
              default:
                message += 'entry';
            }
            
            alert(message + ' (Feature coming soon!)');
          });
        </script>
      </body>
    </html>
  `);
});

app.listen(port, () => {
  console.log(`Volt Betta Manager prototype running at http://localhost:${port}`);
});