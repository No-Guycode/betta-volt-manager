// Script to start the Expo app on port 5000
const express = require('express');
const app = express();
const port = 5000;
const path = require('path');
const db = require('./database/db');
const controllers = require('./database/controllers');

// Initialize database
async function initDatabase() {
  try {
    // Sync database tables
    await db.syncDatabase();
    
    // Initialize with sample data if needed
    await db.initializeWithSampleData();
    
    console.log('Database initialized successfully');
  } catch (error) {
    console.error('Error initializing database:', error);
  }
}

// Initialize the database
initDatabase();

// Serve static files
app.use(express.static(path.join(__dirname, 'public')));
app.use('/uploads', express.static(path.join(__dirname, 'uploads')));
app.use(express.json());

// Set up multer for file uploads
const multer = require('multer');
const storage = multer.diskStorage({
  destination: function (req, file, cb) {
    cb(null, path.join(__dirname, 'uploads'));
  },
  filename: function (req, file, cb) {
    const uniqueSuffix = Date.now() + '-' + Math.round(Math.random() * 1E9);
    const ext = path.extname(file.originalname);
    cb(null, file.fieldname + '-' + uniqueSuffix + ext);
  }
});

const upload = multer({ 
  storage: storage,
  limits: { fileSize: 5 * 1024 * 1024 }, // 5MB limit
  fileFilter: (req, file, cb) => {
    // Accept images only
    if (!file.originalname.match(/\.(jpg|jpeg|png|gif)$/)) {
      return cb(new Error('Only image files are allowed!'), false);
    }
    cb(null, true);
  }
});

// Store default avatar options
const profilePictures = [
  { id: 1, name: 'Volt Default', url: '🐠', isEmoji: true },
  { id: 2, name: 'Betta Red', url: '🐟', isEmoji: true },
  { id: 3, name: 'Betta Blue', url: '🐡', isEmoji: true },
  { id: 4, name: 'Custom Upload', url: '', isEmoji: false }
];

// Will be set from database in frontend

// API endpoints
// Get all notifications
app.get('/api/notifications', async (req, res) => {
  try {
    // Default to fish ID 1 (Volt) for this prototype
    const fishId = 1;
    const notifications = await controllers.NotificationController.getAllForFish(fishId);
    res.json(notifications);
  } catch (error) {
    console.error('Error fetching notifications:', error);
    res.status(500).json({ success: false, message: 'Error fetching notifications' });
  }
});

// Mark notification as read
app.post('/api/mark-notification-read', async (req, res) => {
  try {
    const { id } = req.body;
    const result = await controllers.NotificationController.markAsRead(id);
    
    if (result) {
      res.json({ success: true });
    } else {
      res.status(404).json({ success: false, message: 'Notification not found' });
    }
  } catch (error) {
    console.error('Error marking notification as read:', error);
    res.status(500).json({ success: false, message: 'Error marking notification as read' });
  }
});

// Get fish profile
app.get('/api/fish/:id', async (req, res) => {
  try {
    const { id } = req.params;
    const fish = await controllers.FishController.getById(id);
    
    if (fish) {
      res.json(fish);
    } else {
      res.status(404).json({ success: false, message: 'Fish profile not found' });
    }
  } catch (error) {
    console.error('Error fetching fish profile:', error);
    res.status(500).json({ success: false, message: 'Error fetching fish profile' });
  }
});

// Update fish profile
app.put('/api/fish/:id', async (req, res) => {
  try {
    const { id } = req.params;
    const fishData = req.body;
    const updatedFish = await controllers.FishController.update(id, fishData);
    
    if (updatedFish) {
      res.json({ success: true, fish: updatedFish });
    } else {
      res.status(404).json({ success: false, message: 'Fish profile not found' });
    }
  } catch (error) {
    console.error('Error updating fish profile:', error);
    res.status(500).json({ success: false, message: 'Error updating fish profile' });
  }
});

// Update profile picture - emoji only
app.post('/api/upload-profile', async (req, res) => {
  try {
    const { fishId, profilePicture, isPictureEmoji } = req.body;
    
    if (isPictureEmoji) {
      // For emoji profiles, just save the emoji
      const updatedFish = await controllers.FishController.updateProfilePicture(
        fishId || 1, // Default to fish ID 1 (Volt) for this prototype
        profilePicture,
        true
      );
      
      if (updatedFish) {
        res.json({ success: true, fish: updatedFish });
      } else {
        res.status(404).json({ success: false, message: 'Fish profile not found' });
      }
    } else {
      // For non-emoji profiles, we'll handle in the file upload endpoint
      res.status(400).json({ success: false, message: 'Use /api/upload-profile-image for file uploads' });
    }
  } catch (error) {
    console.error('Error updating profile picture:', error);
    res.status(500).json({ success: false, message: 'Error updating profile picture' });
  }
});

// Upload profile picture image
app.post('/api/upload-profile-image', upload.single('profile'), async (req, res) => {
  try {
    if (!req.file) {
      return res.status(400).json({ success: false, message: 'No file uploaded' });
    }
    
    const fishId = req.body.fishId || 1; // Default to fish ID 1 (Volt) for this prototype
    const profilePicture = `/uploads/${req.file.filename}`;
    
    const updatedFish = await controllers.FishController.updateProfilePicture(
      fishId,
      profilePicture,
      false // Not an emoji
    );
    
    if (updatedFish) {
      res.json({ 
        success: true, 
        fish: updatedFish,
        url: profilePicture
      });
    } else {
      res.status(404).json({ success: false, message: 'Fish profile not found' });
    }
  } catch (error) {
    console.error('Error uploading profile picture:', error);
    res.status(500).json({ success: false, message: 'Error uploading profile picture' });
  }
});

// Get maintenance tasks
app.get('/api/maintenance/:fishId', async (req, res) => {
  try {
    const { fishId } = req.params;
    const tasks = await controllers.MaintenanceController.getAllForFish(fishId);
    res.json(tasks);
  } catch (error) {
    console.error('Error fetching maintenance tasks:', error);
    res.status(500).json({ success: false, message: 'Error fetching maintenance tasks' });
  }
});

// Get upcoming maintenance tasks
app.get('/api/maintenance/:fishId/upcoming', async (req, res) => {
  try {
    const { fishId } = req.params;
    const { days } = req.query;
    const tasks = await controllers.MaintenanceController.getUpcoming(fishId, days || 7);
    res.json(tasks);
  } catch (error) {
    console.error('Error fetching upcoming maintenance tasks:', error);
    res.status(500).json({ success: false, message: 'Error fetching upcoming maintenance tasks' });
  }
});

// Mark maintenance task as done
app.post('/api/maintenance/:id/done', async (req, res) => {
  try {
    const { id } = req.params;
    const updatedTask = await controllers.MaintenanceController.markAsDone(id);
    
    if (updatedTask) {
      res.json({ success: true, task: updatedTask });
    } else {
      res.status(404).json({ success: false, message: 'Maintenance task not found' });
    }
  } catch (error) {
    console.error('Error marking maintenance task as done:', error);
    res.status(500).json({ success: false, message: 'Error marking maintenance task as done' });
  }
});

// Add new maintenance task
app.post('/api/maintenance', async (req, res) => {
  try {
    const taskData = req.body;
    const newTask = await controllers.MaintenanceController.create(taskData);
    res.status(201).json({ success: true, task: newTask });
  } catch (error) {
    console.error('Error creating maintenance task:', error);
    res.status(500).json({ success: false, message: 'Error creating maintenance task' });
  }
});

// Get tank logs
app.get('/api/tank-logs/:fishId', async (req, res) => {
  try {
    const { fishId } = req.params;
    const logs = await controllers.TankLogController.getAllForFish(fishId);
    res.json(logs);
  } catch (error) {
    console.error('Error fetching tank logs:', error);
    res.status(500).json({ success: false, message: 'Error fetching tank logs' });
  }
});

// Add tank log
app.post('/api/tank-logs', async (req, res) => {
  try {
    const logData = req.body;
    const newLog = await controllers.TankLogController.create(logData);
    res.status(201).json({ success: true, log: newLog });
  } catch (error) {
    console.error('Error creating tank log:', error);
    res.status(500).json({ success: false, message: 'Error creating tank log' });
  }
});

// Get plants
app.get('/api/plants', async (req, res) => {
  try {
    const plants = await controllers.PlantController.getAll();
    res.json(plants);
  } catch (error) {
    console.error('Error fetching plants:', error);
    res.status(500).json({ success: false, message: 'Error fetching plants' });
  }
});

// Add new plant
app.post('/api/plants', async (req, res) => {
  try {
    const plantData = req.body;
    const newPlant = await controllers.PlantController.create(plantData);
    res.status(201).json({ success: true, plant: newPlant });
  } catch (error) {
    console.error('Error creating plant:', error);
    res.status(500).json({ success: false, message: 'Error creating plant' });
  }
});

// Get treatments
app.get('/api/treatments/:fishId', async (req, res) => {
  try {
    const { fishId } = req.params;
    const treatments = await controllers.TreatmentController.getAllForFish(fishId);
    res.json(treatments);
  } catch (error) {
    console.error('Error fetching treatments:', error);
    res.status(500).json({ success: false, message: 'Error fetching treatments' });
  }
});

// Add new treatment
app.post('/api/treatments', async (req, res) => {
  try {
    const treatmentData = req.body;
    const newTreatment = await controllers.TreatmentController.create(treatmentData);
    res.status(201).json({ success: true, treatment: newTreatment });
  } catch (error) {
    console.error('Error creating treatment:', error);
    res.status(500).json({ success: false, message: 'Error creating treatment' });
  }
});

// Get photos
app.get('/api/photos/:fishId', async (req, res) => {
  try {
    const { fishId } = req.params;
    const photos = await controllers.PhotoController.getAllForFish(fishId);
    res.json(photos);
  } catch (error) {
    console.error('Error fetching photos:', error);
    res.status(500).json({ success: false, message: 'Error fetching photos' });
  }
});

// Add new photo with file upload
app.post('/api/photos', upload.single('photo'), async (req, res) => {
  try {
    if (!req.file) {
      return res.status(400).json({ success: false, message: 'No file uploaded' });
    }
    
    const photoData = {
      ...req.body,
      url: `/uploads/${req.file.filename}`, 
      FishProfileId: req.body.fishId || 1 // Default to Volt
    };
    
    // Save to database
    const newPhoto = await controllers.PhotoController.create(photoData);
    res.status(201).json({ 
      success: true, 
      photo: newPhoto,
      url: photoData.url
    });
  } catch (error) {
    console.error('Error creating photo:', error);
    res.status(500).json({ success: false, message: 'Error creating photo' });
  }
});

// Get notes
app.get('/api/notes/:fishId', async (req, res) => {
  try {
    const { fishId } = req.params;
    const notes = await controllers.NoteController.getAllForFish(fishId);
    res.json(notes);
  } catch (error) {
    console.error('Error fetching notes:', error);
    res.status(500).json({ success: false, message: 'Error fetching notes' });
  }
});

// Add new note
app.post('/api/notes', async (req, res) => {
  try {
    const noteData = req.body;
    const newNote = await controllers.NoteController.create(noteData);
    res.status(201).json({ success: true, note: newNote });
  } catch (error) {
    console.error('Error creating note:', error);
    res.status(500).json({ success: false, message: 'Error creating note' });
  }
});

// Serve the appropriate data for the frontend
async function getAppData() {
  try {
    // Default to fish ID 1 (Volt) for this prototype
    const fishId = 1;
    
    // Get fish profile
    const fish = await controllers.FishController.getById(fishId);
    
    // If fish doesn't exist yet (first run), use default data
    if (!fish) {
      return {
        fishData: {
          name: 'Volt',
          species: 'Betta splendens',
          variant: 'Veiltail',
          color: 'Red/Blue',
          age: '~8 months',
          tank: '3.5 gallon planted',
          acquisitionDate: 'April 5, 2025',
          profilePicture: '🐠',
          isPictureEmoji: true
        },
        maintenanceTasks: [],
        tankLogs: [],
        notifications: []
      };
    }
    
    // Format fish data for the frontend
    const fishData = {
      id: fish.id,
      name: fish.name,
      species: fish.species,
      variant: fish.variant,
      color: fish.color,
      age: fish.age,
      tank: fish.tank,
      acquisitionDate: new Date(fish.acquisitionDate).toLocaleDateString('en-US', {
        year: 'numeric',
        month: 'long',
        day: 'numeric'
      }),
      profilePicture: fish.profilePicture,
      isPictureEmoji: fish.isPictureEmoji
    };
    
    // Get maintenance tasks and format for frontend
    const tasks = await controllers.MaintenanceController.getAllForFish(fishId);
    const maintenanceTasks = tasks.map(task => {
      // Calculate "due in" days
      const dueDate = new Date(task.dueDate);
      const today = new Date();
      const diffTime = dueDate - today;
      const diffDays = Math.ceil(diffTime / (1000 * 60 * 60 * 24));
      
      let dueIn = diffDays <= 0 ? 'Today' : `${diffDays} day${diffDays !== 1 ? 's' : ''}`;
      
      // Calculate "last done" days ago
      const lastDoneDate = new Date(task.lastDone);
      const lastDoneTime = today - lastDoneDate;
      const lastDoneDays = Math.floor(lastDoneTime / (1000 * 60 * 60 * 24));
      
      let lastDone = lastDoneDays === 0 ? 'Today' : `${lastDoneDays} day${lastDoneDays !== 1 ? 's' : ''} ago`;
      
      return {
        id: task.id,
        task: task.task,
        dueIn,
        recurring: task.recurring,
        lastDone,
        notify: task.notify,
        priority: task.priority
      };
    });
    
    // Get tank logs
    const logs = await controllers.TankLogController.getAllForFish(fishId);
    const tankLogs = logs.map(log => ({
      id: log.id,
      date: log.date,
      ammonia: log.ammonia.toString(),
      nitrite: log.nitrite.toString(),
      nitrate: log.nitrate.toString(),
      ph: log.ph.toString(),
      temp: log.temp.toString(),
      notes: log.notes
    }));
    
    // Get notifications
    const notifications = await controllers.NotificationController.getAllForFish(fishId);
    
    // Get plants
    let plants = [];
    try {
      plants = await controllers.PlantController.getAllForFish(fishId);
    } catch (error) {
      console.log('No plants data yet, using empty array');
    }
    
    // Get treatments
    let treatments = [];
    try {
      treatments = await controllers.TreatmentController.getAllForFish(fishId);
    } catch (error) {
      console.log('No treatments data yet, using empty array');
    }
    
    // Get photos
    let photos = [];
    try {
      photos = await controllers.PhotoController.getAllForFish(fishId);
    } catch (error) {
      console.log('No photos data yet, using empty array');
    }
    
    // Get notes
    let notes = [];
    try {
      notes = await controllers.NoteController.getAllForFish(fishId);
    } catch (error) {
      console.log('No notes data yet, using empty array');
    }
    
    return {
      fishData,
      maintenanceTasks,
      tankLogs,
      notifications,
      plants,
      treatments,
      photos,
      notes
    };
  } catch (error) {
    console.error('Error getting app data:', error);
    
    // Return default data in case of error
    return {
      fishData: {
        name: 'Volt',
        species: 'Betta splendens',
        variant: 'Veiltail',
        color: 'Red/Blue',
        age: '~8 months',
        tank: '3.5 gallon planted',
        acquisitionDate: 'April 5, 2025',
        profilePicture: '🐠',
        isPictureEmoji: true
      },
      maintenanceTasks: [],
      tankLogs: [],
      notifications: [],
      plants: [],
      treatments: [],
      photos: [],
      notes: []
    };
  }
}

// Simple Express server to display app information with interactive elements
app.get('/', async (req, res) => {
  // Get data from database
  const appData = await getAppData();
  
  // Define profile pictures options for the frontend
  const clientProfilePictures = JSON.stringify(profilePictures);
  
  res.send(`
    <html>
      <head>
        <title>Volt Betta Manager App</title>
        <script src="https://cdn.jsdelivr.net/npm/chart.js"></script>
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
            position: relative;
            cursor: pointer;
            overflow: hidden;
          }
          .fish-avatar:hover::after {
            content: 'Change';
            position: absolute;
            bottom: 0;
            left: 0;
            right: 0;
            background-color: rgba(0,0,0,0.5);
            color: white;
            font-size: 12px;
            text-align: center;
            padding: 4px 0;
          }
          .fish-avatar img {
            width: 100%;
            height: 100%;
            object-fit: cover;
          }
          .profile-picker {
            display: none;
            position: fixed;
            top: 0;
            left: 0;
            right: 0;
            bottom: 0;
            background-color: rgba(0,0,0,0.7);
            z-index: 1000;
            justify-content: center;
            align-items: center;
          }
          .profile-picker.active {
            display: flex;
          }
          .profile-picker-content {
            background-color: white;
            padding: 20px;
            border-radius: 10px;
            width: 90%;
            max-width: 500px;
            max-height: 80vh;
            overflow-y: auto;
          }
          .profile-picker-header {
            display: flex;
            justify-content: space-between;
            align-items: center;
            margin-bottom: 20px;
          }
          .profile-picker-close {
            background: none;
            border: none;
            font-size: 24px;
            cursor: pointer;
            color: #666;
          }
          .profile-options {
            display: grid;
            grid-template-columns: repeat(2, 1fr);
            gap: 15px;
          }
          .profile-option {
            border: 2px solid #eee;
            border-radius: 10px;
            padding: 15px;
            display: flex;
            flex-direction: column;
            align-items: center;
            cursor: pointer;
            transition: all 0.2s ease;
          }
          .profile-option:hover {
            border-color: #F04F94;
            transform: translateY(-3px);
          }
          .profile-option.selected {
            border-color: #F04F94;
            background-color: #fff9f7;
          }
          .profile-emoji {
            font-size: 40px;
            margin-bottom: 10px;
          }
          .profile-upload {
            margin-top: 20px;
            text-align: center;
          }
          .notification-badge {
            position: relative;
            display: inline-block;
          }
          .notification-badge::after {
            content: attr(data-count);
            position: absolute;
            top: -10px;
            right: -10px;
            background-color: #F51A0F;
            color: white;
            font-size: 12px;
            width: 18px;
            height: 18px;
            border-radius: 50%;
            display: flex;
            justify-content: center;
            align-items: center;
          }
          .notifications-panel {
            display: none;
            position: fixed;
            top: 0;
            right: 0;
            bottom: 0;
            width: 300px;
            background-color: white;
            box-shadow: -2px 0 5px rgba(0,0,0,0.1);
            z-index: 1000;
            transform: translateX(100%);
            transition: transform 0.3s ease;
            overflow-y: auto;
          }
          .notifications-panel.active {
            display: block;
            transform: translateX(0);
          }
          .notifications-header {
            background-color: #F04F94;
            color: white;
            padding: 15px;
            display: flex;
            justify-content: space-between;
            align-items: center;
          }
          .notifications-close {
            background: none;
            border: none;
            color: white;
            font-size: 20px;
            cursor: pointer;
          }
          .notification-item {
            padding: 15px;
            border-bottom: 1px solid #eee;
            cursor: pointer;
            transition: all 0.2s ease;
          }
          .notification-item:hover {
            background-color: #f9f9f9;
          }
          .notification-item.unread {
            border-left: 3px solid #F04F94;
          }
          .notification-title {
            font-weight: bold;
            margin-bottom: 5px;
          }
          .notification-message {
            font-size: 14px;
            color: #666;
          }
          .notification-date {
            font-size: 12px;
            color: #999;
            margin-top: 5px;
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
          
          /* Gallery styles */
          .gallery-grid {
            display: grid;
            grid-template-columns: repeat(auto-fill, minmax(250px, 1fr));
            gap: 20px;
            margin-top: 20px;
          }
          
          .gallery-item {
            border-radius: 10px;
            overflow: hidden;
            box-shadow: 0 4px 8px rgba(0,0,0,0.1);
            transition: all 0.3s ease;
          }
          
          .gallery-item:hover {
            transform: translateY(-5px);
            box-shadow: 0 8px 16px rgba(0,0,0,0.15);
          }
          
          .gallery-image {
            height: 200px;
            overflow: hidden;
          }
          
          .gallery-image img {
            width: 100%;
            height: 100%;
            object-fit: cover;
            transition: transform 0.3s ease;
          }
          
          .gallery-item:hover .gallery-image img {
            transform: scale(1.05);
          }
          
          .gallery-info {
            padding: 15px;
            background-color: white;
          }
          
          .gallery-title {
            font-weight: bold;
            color: #F04F94;
            margin-bottom: 5px;
          }
          
          .gallery-date {
            font-size: 12px;
            color: #999;
            margin-bottom: 8px;
          }
          
          .gallery-notes {
            font-style: italic;
            color: #666;
            font-size: 14px;
          }
          
          /* Mobile responsive styles */
          @media (max-width: 768px) {
            header {
              flex-direction: column;
              align-items: flex-start;
            }
            
            nav {
              margin-top: 15px;
              flex-wrap: wrap;
              gap: 10px;
            }
            
            .nav-button {
              font-size: 12px;
              padding: 6px 10px;
            }
            
            .fish-profile {
              flex-direction: column;
            }
            
            .gallery-grid {
              grid-template-columns: repeat(auto-fill, minmax(150px, 1fr));
            }
            
            .add-button {
              bottom: 15px;
              right: 15px;
              width: 50px;
              height: 50px;
              font-size: 24px;
            }
            
            .profile-picker-content {
              width: 95%;
            }
            
            .profile-options {
              grid-template-columns: 1fr;
            }
          }
          
          /* Hamburger menu for mobile */
          .menu-toggle {
            display: none;
            flex-direction: column;
            justify-content: space-between;
            width: 30px;
            height: 21px;
            cursor: pointer;
          }
          
          .menu-toggle span {
            height: 3px;
            width: 100%;
            background-color: #F04F94;
            border-radius: 3px;
          }
          
          @media (max-width: 600px) {
            .menu-toggle {
              display: flex;
            }
            
            nav {
              display: none;
              flex-direction: column;
              width: 100%;
            }
            
            nav.active {
              display: flex;
            }
            
            .nav-button {
              width: 100%;
              text-align: center;
              margin-bottom: 5px;
            }
          }
          
          /* Form styles */
          .form-content {
            padding: 0 20px 20px 20px;
          }
          .form-group {
            margin-bottom: 15px;
          }
          .form-group label {
            display: block;
            margin-bottom: 5px;
            font-weight: bold;
            color: #666;
          }
          .form-group input[type="text"],
          .form-group input[type="date"],
          .form-group input[type="number"],
          .form-group select,
          .form-group textarea {
            width: 100%;
            padding: 10px;
            border: 1px solid #ddd;
            border-radius: 5px;
            font-size: 16px;
          }
          .form-group textarea {
            resize: vertical;
          }
          .form-group.checkbox {
            display: flex;
            align-items: center;
          }
          .form-group.checkbox input {
            margin-right: 10px;
          }
          .form-group.checkbox label {
            margin-bottom: 0;
          }
          .form-actions {
            display: flex;
            justify-content: flex-end;
            gap: 10px;
            margin-top: 20px;
          }
          .primary-button {
            background-color: #F04F94;
          }
          
          /* Add menu styles */
          .add-menu {
            display: none;
            position: fixed;
            bottom: 90px;
            right: 20px;
            background-color: white;
            border-radius: 10px;
            box-shadow: 0 4px 15px rgba(0,0,0,0.2);
            z-index: 100;
            width: 220px;
            overflow: hidden;
            transform: translateY(20px);
            opacity: 0;
            transition: transform 0.3s ease, opacity 0.3s ease;
          }
          
          .add-menu.active {
            display: block;
            transform: translateY(0);
            opacity: 1;
          }
          
          .add-menu-item {
            display: flex;
            align-items: center;
            padding: 12px 15px;
            transition: all 0.2s ease;
            cursor: pointer;
          }
          
          .add-menu-item:hover {
            background-color: #f7f7f7;
          }
          
          .add-menu-icon {
            width: 40px;
            height: 40px;
            background-color: #F0734F;
            color: white;
            border-radius: 50%;
            display: flex;
            justify-content: center;
            align-items: center;
            margin-right: 15px;
            font-size: 20px;
          }
          
          .add-menu-text {
            font-weight: bold;
            color: #444;
          }
          
          .global-add {
            z-index: 101;
          }
          
          .global-add.active {
            background-color: #ED4FF0;
            transform: rotate(45deg);
          }
          
          /* Chart styles */
          .chart-container {
            margin: 20px 0;
            padding: 15px;
            background-color: white;
            border-radius: 10px;
            box-shadow: 0 2px 10px rgba(0,0,0,0.05);
          }
          
          .parameter-buttons {
            display: flex;
            gap: 10px;
            margin-bottom: 15px;
            flex-wrap: wrap;
          }
          
          .parameter-button {
            background-color: #f0f0f0;
            border: none;
            padding: 8px 12px;
            border-radius: 20px;
            cursor: pointer;
            font-size: 14px;
            transition: all 0.2s;
          }
          
          .parameter-button.active {
            background-color: #F04F94;
            color: white;
          }
          
          /* Treatment Card Styles */
          .treatments-grid {
            display: grid;
            grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
            gap: 20px;
            margin-top: 20px;
          }
          
          .treatment-card {
            position: relative;
            border-radius: 8px;
            padding: 15px;
            box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
            background-color: white;
            overflow: hidden;
          }
          
          .treatment-active {
            border-left: 4px solid #ED4FF0;
          }
          
          .treatment-completed {
            border-left: 4px solid #4CAF50;
          }
          
          .treatment-status {
            position: absolute;
            top: 10px;
            right: 10px;
          }
          
          .status-badge {
            display: inline-block;
            padding: 3px 8px;
            border-radius: 12px;
            font-size: 12px;
            font-weight: bold;
            color: white;
          }
          
          .status-badge.active {
            background-color: #ED4FF0;
          }
          
          .status-badge.completed {
            background-color: #4CAF50;
          }
          
          .treatment-header {
            margin-bottom: 15px;
            padding-right: 70px;
          }
          
          .treatment-header h3 {
            margin: 0 0 5px 0;
            color: #333;
          }
          
          .treatment-condition {
            font-size: 14px;
            color: #666;
            font-style: italic;
          }
          
          .treatment-details {
            display: flex;
            flex-direction: column;
            gap: 10px;
          }
          
          .treatment-dates {
            display: flex;
            flex-direction: column;
            gap: 5px;
          }
          
          .treatment-date, .treatment-dosage {
            display: flex;
            justify-content: space-between;
            font-size: 14px;
            padding: 5px 0;
            border-bottom: 1px dashed #eee;
          }
          
          .date-label, .dosage-label, .notes-label {
            font-weight: bold;
            color: #555;
          }
          
          .treatment-notes {
            margin-top: 10px;
            padding-top: 5px;
            border-top: 1px solid #eee;
          }
          
          .notes-label {
            margin-bottom: 5px;
          }
          
          .notes-content {
            font-size: 14px;
            line-height: 1.5;
            color: #666;
          }
          
          /* Notes styling */
          .notes-grid {
            display: grid;
            grid-template-columns: repeat(auto-fill, minmax(300px, 1fr));
            gap: 20px;
            margin-top: 20px;
          }
          
          .note-card {
            position: relative;
            border-radius: 8px;
            padding: 15px;
            box-shadow: 0 2px 10px rgba(0, 0, 0, 0.1);
            background-color: white;
            border-left: 4px solid #F08F4F;
          }
          
          .note-header {
            margin-bottom: 15px;
            border-bottom: 1px solid #eee;
            padding-bottom: 10px;
          }
          
          .note-header h3 {
            margin: 0 0 5px 0;
            color: #333;
          }
          
          .note-date {
            font-size: 12px;
            color: #777;
          }
          
          .note-content {
            font-size: 14px;
            line-height: 1.6;
            color: #555;
          }
          
          .note-content p {
            margin: 0;
          }
          
          /* Empty state styling */
          .empty-state {
            text-align: center;
            padding: 30px;
            background-color: #f9f9f9;
            border-radius: 8px;
            margin: 20px 0;
          }
          
          .empty-icon {
            font-size: 48px;
            margin-bottom: 15px;
          }
        </style>
      </head>
      <body>
        <header>
          <h1>Volt Betta Manager</h1>
          <div class="menu-toggle" onclick="toggleMenu()">
            <span></span>
            <span></span>
            <span></span>
          </div>
          <nav>
            <button class="nav-button" onclick="showSection('home-section')">Home</button>
            <button class="nav-button" onclick="showSection('tanklog-section')">Tank Log</button>
            <button class="nav-button" onclick="showSection('maintenance-section')">Maintenance</button>
            <button class="nav-button" onclick="showSection('plants-section')">Plants</button>
            <button class="nav-button" onclick="showSection('treatments-section')">Treatments</button>
            <button class="nav-button" onclick="showSection('gallery-section')">Gallery</button>
            <button class="nav-button" onclick="showSection('notes-section')">Notes</button>
            <div class="notification-badge" data-count="1" onclick="toggleNotifications()">
              <button class="nav-button">📋</button>
            </div>
          </nav>
        </header>
        
        <!-- Notification Panel -->
        <div id="notifications-panel" class="notifications-panel">
          <div class="notifications-header">
            <h3>Notifications</h3>
            <button class="notifications-close" onclick="toggleNotifications()">×</button>
          </div>
          <div id="notifications-list">
            <!-- Notifications will be loaded here -->
          </div>
        </div>
        
        <!-- Profile Picture Picker -->
        <div id="profile-picker" class="profile-picker">
          <div class="profile-picker-content">
            <div class="profile-picker-header">
              <h3>Change Profile Picture</h3>
              <button class="profile-picker-close" onclick="toggleProfilePicker()">×</button>
            </div>
            <div class="profile-options">
              <!-- Profile options will be loaded here -->
            </div>
            <div class="profile-upload">
              <p>Or upload your own image:</p>
              <input type="file" id="profile-upload" accept="image/*">
              <button onclick="uploadProfilePicture()">Upload</button>
            </div>
          </div>
        </div>
        
        <!-- Maintenance Task Form Modal -->
        <div id="task-form-modal" class="profile-picker">
          <div class="profile-picker-content">
            <div class="profile-picker-header">
              <h3>Add New Maintenance Task</h3>
              <button class="profile-picker-close" onclick="hideTaskForm()">×</button>
            </div>
            <div class="form-content">
              <div class="form-group">
                <label for="task-name">Task Name:</label>
                <input type="text" id="task-name" placeholder="e.g., Water Change" required>
              </div>
              <div class="form-group">
                <label for="task-date">Due Date:</label>
                <input type="date" id="task-date" required>
              </div>
              <div class="form-group">
                <label for="task-recurring">Recurring:</label>
                <select id="task-recurring">
                  <option value="none">None</option>
                  <option value="daily">Daily</option>
                  <option value="weekly">Weekly</option>
                  <option value="biweekly">Bi-weekly</option>
                  <option value="monthly">Monthly</option>
                </select>
              </div>
              <div class="form-group">
                <label for="task-priority">Priority:</label>
                <select id="task-priority">
                  <option value="low">Low</option>
                  <option value="medium" selected>Medium</option>
                  <option value="high">High</option>
                </select>
              </div>
              <div class="form-group checkbox">
                <input type="checkbox" id="task-notify" checked>
                <label for="task-notify">Send Notification</label>
              </div>
              <div class="form-actions">
                <button onclick="hideTaskForm()">Cancel</button>
                <button onclick="saveMaintenanceTask()" class="primary-button">Save Task</button>
              </div>
            </div>
          </div>
        </div>
        
        <!-- Tank Log Form Modal -->
        <div id="log-form-modal" class="profile-picker">
          <div class="profile-picker-content">
            <div class="profile-picker-header">
              <h3>Add New Tank Log</h3>
              <button class="profile-picker-close" onclick="hideLogForm()">×</button>
            </div>
            <div class="form-content">
              <div class="form-group">
                <label for="log-date">Date:</label>
                <input type="date" id="log-date" required>
              </div>
              <div class="form-group">
                <label for="log-ammonia">Ammonia (ppm):</label>
                <input type="number" id="log-ammonia" min="0" step="0.25" placeholder="0">
              </div>
              <div class="form-group">
                <label for="log-nitrite">Nitrite (ppm):</label>
                <input type="number" id="log-nitrite" min="0" step="0.25" placeholder="0">
              </div>
              <div class="form-group">
                <label for="log-nitrate">Nitrate (ppm):</label>
                <input type="number" id="log-nitrate" min="0" step="1" placeholder="0">
              </div>
              <div class="form-group">
                <label for="log-ph">pH:</label>
                <input type="number" id="log-ph" min="0" step="0.1" placeholder="7.0">
              </div>
              <div class="form-group">
                <label for="log-temp">Temperature (°F):</label>
                <input type="number" id="log-temp" min="0" step="0.1" placeholder="78">
              </div>
              <div class="form-group">
                <label for="log-notes">Notes:</label>
                <textarea id="log-notes" rows="3" placeholder="Any observations..."></textarea>
              </div>
              <div class="form-actions">
                <button onclick="hideLogForm()">Cancel</button>
                <button onclick="saveTankLog()" class="primary-button">Save Log</button>
              </div>
            </div>
          </div>
        </div>
        
        <!-- Plant Form Modal -->
        <div id="plant-form-modal" class="profile-picker">
          <div class="profile-picker-content">
            <div class="profile-picker-header">
              <h3>Add New Plant</h3>
              <button class="profile-picker-close" onclick="hidePlantForm()">×</button>
            </div>
            <div class="form-content">
              <div class="form-group">
                <label for="plant-name">Plant Name:</label>
                <input type="text" id="plant-name" placeholder="e.g., Java Fern" required>
              </div>
              <div class="form-group">
                <label for="plant-species">Species:</label>
                <input type="text" id="plant-species" placeholder="Scientific name (optional)">
              </div>
              <div class="form-group">
                <label for="plant-date">Date Added:</label>
                <input type="date" id="plant-date" required>
              </div>
              <div class="form-group">
                <label for="plant-care">Care Instructions:</label>
                <textarea id="plant-care" rows="3" placeholder="Light, fertilizer, trimming needs..."></textarea>
              </div>
              <div class="form-actions">
                <button onclick="hidePlantForm()">Cancel</button>
                <button onclick="savePlant()" class="primary-button">Save Plant</button>
              </div>
            </div>
          </div>
        </div>
        
        <!-- Treatment Form Modal -->
        <div id="treatment-form-modal" class="profile-picker">
          <div class="profile-picker-content">
            <div class="profile-picker-header">
              <h3>Add New Treatment</h3>
              <button class="profile-picker-close" onclick="hideTreatmentForm()">×</button>
            </div>
            <div class="form-content">
              <div class="form-group">
                <label for="treatment-name">Treatment Name:</label>
                <input type="text" id="treatment-name" placeholder="e.g., Aquarium Salt" required>
              </div>
              <div class="form-group">
                <label for="treatment-condition">Condition Treating:</label>
                <input type="text" id="treatment-condition" placeholder="e.g., Fin Rot" required>
              </div>
              <div class="form-group">
                <label for="treatment-start-date">Start Date:</label>
                <input type="date" id="treatment-start-date" required>
              </div>
              <div class="form-group">
                <label for="treatment-end-date">End Date:</label>
                <input type="date" id="treatment-end-date">
              </div>
              <div class="form-group">
                <label for="treatment-dosage">Dosage:</label>
                <input type="text" id="treatment-dosage" placeholder="e.g., 1 tsp per gallon">
              </div>
              <div class="form-group">
                <label for="treatment-notes">Notes:</label>
                <textarea id="treatment-notes" rows="3" placeholder="Treatment observations..."></textarea>
              </div>
              <div class="form-actions">
                <button onclick="hideTreatmentForm()">Cancel</button>
                <button onclick="saveTreatment()" class="primary-button">Save Treatment</button>
              </div>
            </div>
          </div>
        </div>
        
        <!-- Photo Form Modal -->
        <div id="photo-form-modal" class="profile-picker">
          <div class="profile-picker-content">
            <div class="profile-picker-header">
              <h3>Add New Photo</h3>
              <button class="profile-picker-close" onclick="hidePhotoForm()">×</button>
            </div>
            <div class="form-content">
              <div class="form-group">
                <label for="photo-title">Photo Title:</label>
                <input type="text" id="photo-title" placeholder="e.g., Volt's New Colors" required>
              </div>
              <div class="form-group">
                <label for="photo-date">Date Taken:</label>
                <input type="date" id="photo-date" required>
              </div>
              <div class="form-group">
                <label for="photo-file">Select Photo:</label>
                <input type="file" id="photo-file" accept="image/*" required>
              </div>
              <div class="form-group">
                <label for="photo-notes">Description:</label>
                <textarea id="photo-notes" rows="3" placeholder="Photo description..."></textarea>
              </div>
              <div class="form-actions">
                <button onclick="hidePhotoForm()">Cancel</button>
                <button onclick="savePhoto()" class="primary-button">Save Photo</button>
              </div>
            </div>
          </div>
        </div>
        
        <!-- Note Form Modal -->
        <div id="note-form-modal" class="profile-picker">
          <div class="profile-picker-content">
            <div class="profile-picker-header">
              <h3>Add New Note</h3>
              <button class="profile-picker-close" onclick="hideNoteForm()">×</button>
            </div>
            <div class="form-content">
              <div class="form-group">
                <label for="note-title">Title:</label>
                <input type="text" id="note-title" placeholder="e.g., Behavior Observation" required>
              </div>
              <div class="form-group">
                <label for="note-date">Date:</label>
                <input type="date" id="note-date" required>
              </div>
              <div class="form-group">
                <label for="note-content">Content:</label>
                <textarea id="note-content" rows="5" placeholder="Your notes..." required></textarea>
              </div>
              <div class="form-actions">
                <button onclick="hideNoteForm()">Cancel</button>
                <button onclick="saveNote()" class="primary-button">Save Note</button>
              </div>
            </div>
          </div>
        </div>
        
        <!-- Home Section -->
        <section id="home-section" class="content-section active">
          <div class="fish-profile">
            <div class="fish-profile-info">
              <div class="fish-avatar" id="fish-avatar-container">
                ${appData.fishData.isPictureEmoji 
                  ? appData.fishData.profilePicture 
                  : `<img src="${appData.fishData.profilePicture}" alt="${appData.fishData.name}" />`
                }
              </div>
              <h2>${appData.fishData.name}</h2>
              <p>${appData.fishData.species} - ${appData.fishData.variant}</p>
              
              <div class="fish-stats">
                <div class="fish-stat">
                  <div class="stat-label">Age</div>
                  <div class="stat-value">${appData.fishData.age}</div>
                </div>
                <div class="fish-stat">
                  <div class="stat-label">Color</div>
                  <div class="stat-value">${appData.fishData.color}</div>
                </div>
                <div class="fish-stat">
                  <div class="stat-label">Tank</div>
                  <div class="stat-value">${appData.fishData.tank}</div>
                </div>
                <div class="fish-stat">
                  <div class="stat-label">Acquired</div>
                  <div class="stat-value">${appData.fishData.acquisitionDate}</div>
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
                <div><strong>${appData.maintenanceTasks.find(t => t.task.toLowerCase().includes('water change'))?.lastDone || 'Not recorded'}</strong></div>
              </div>
            </div>
            <div class="stat-item">
              <div class="stat-icon">🧪</div>
              <div>
                <div>Last Water Test</div>
                <div><strong>${appData.tankLogs.length > 0 ? 
                  new Date(appData.tankLogs[0].date).toLocaleDateString('en-US', { month: 'short', day: 'numeric' }) : 
                  'Not recorded'}</strong></div>
              </div>
            </div>
            <div class="stat-item">
              <div class="stat-icon">📷</div>
              <div>
                <div>Photos Taken</div>
                <div><strong>${appData.photos.length} total</strong></div>
              </div>
            </div>
          </div>
          
          <h2>Upcoming Maintenance</h2>
          ${appData.maintenanceTasks.slice(0, 3).map(task => `
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
          
          <!-- Chart Container -->
          <div class="chart-container">
            <div class="parameter-buttons">
              <button class="parameter-button active" data-param="ammonia">Ammonia</button>
              <button class="parameter-button" data-param="nitrite">Nitrite</button>
              <button class="parameter-button" data-param="nitrate">Nitrate</button>
              <button class="parameter-button" data-param="ph">pH</button>
              <button class="parameter-button" data-param="temp">Temperature</button>
            </div>
            <canvas id="paramsChart"></canvas>
          </div>
          
          ${appData.tankLogs.map(log => `
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
          
          <div class="add-button" onclick="showLogForm()">+</div>
        </section>
        
        <!-- Maintenance Section -->
        <section id="maintenance-section" class="content-section">
          <h2>Maintenance Schedule</h2>
          <p>Keep track of recurring tank maintenance tasks.</p>
          
          ${appData.maintenanceTasks.map(task => `
            <div class="maintenance-task">
              <div class="task-info">
                <div class="task-name">${task.task}</div>
                <div class="task-due">Due in ${task.dueIn} • ${task.recurring} • Last done: ${task.lastDone}</div>
              </div>
              <div class="task-badge">Due soon</div>
            </div>
          `).join('')}
          
          <div class="add-button" onclick="showTaskForm()">+</div>
        </section>
        
        <!-- Plants Section -->
        <section id="plants-section" class="content-section">
          <h2>Plant Care</h2>
          <p>Track aquarium plants and their care requirements.</p>
          
          ${appData.plants && appData.plants.length > 0 ? appData.plants.map(plant => `
            <div class="tank-log-entry">
              <div class="log-date">${plant.name}</div>
              <div class="log-params">
                <div class="param"><span class="param-label">Species:</span> ${plant.species || 'Not specified'}</div>
                <div class="param"><span class="param-label">Added:</span> ${new Date(plant.dateAdded).toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' })}</div>
              </div>
              <div class="log-notes">${plant.careInstructions || 'No care instructions provided.'}</div>
            </div>
          `).join('') : `<p>No plants added yet. Add your first plant using the + button below.</p>`}
          
          <div class="add-button" onclick="showPlantForm()">+</div>
        </section>
        
        <!-- Treatments Section -->
        <section id="treatments-section" class="content-section">
          <h2>Treatment Plans</h2>
          <p>Track medication, supplements, and treatment plans for your fish.</p>
          
          ${appData.treatments && appData.treatments.length > 0 ? `
            <div class="treatments-grid">
              ${appData.treatments.map(treatment => {
                // Determine if treatment is active or completed
                const isActive = !treatment.endDate;
                const statusClass = isActive ? 'treatment-active' : 'treatment-completed';
                
                return `
                <div class="treatment-card ${statusClass}">
                  <div class="treatment-status">
                    ${isActive ? 
                      '<span class="status-badge active">Active</span>' : 
                      '<span class="status-badge completed">Completed</span>'
                    }
                  </div>
                  <div class="treatment-header">
                    <h3>${treatment.name}</h3>
                    <div class="treatment-condition">For: ${treatment.condition}</div>
                  </div>
                  <div class="treatment-details">
                    <div class="treatment-dates">
                      <div class="treatment-date">
                        <span class="date-label">Started:</span>
                        <span class="date-value">${new Date(treatment.startDate).toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' })}</span>
                      </div>
                      ${treatment.endDate ? `
                        <div class="treatment-date">
                          <span class="date-label">Ended:</span>
                          <span class="date-value">${new Date(treatment.endDate).toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' })}</span>
                        </div>
                      ` : ''}
                    </div>
                    ${treatment.dosage ? `
                      <div class="treatment-dosage">
                        <span class="dosage-label">Dosage:</span>
                        <span class="dosage-value">${treatment.dosage}</span>
                      </div>
                    ` : ''}
                    ${treatment.notes ? `
                      <div class="treatment-notes">
                        <div class="notes-label">Notes:</div>
                        <div class="notes-content">${treatment.notes}</div>
                      </div>
                    ` : ''}
                  </div>
                </div>
              `;
              }).join('')}
            </div>
          ` : `
            <div class="empty-state">
              <div class="empty-icon">💊</div>
              <p>No treatments added yet. Add your first treatment using the + button below.</p>
            </div>
          `}
          
          <div class="add-button" onclick="showTreatmentForm()">+</div>
        </section>
        
        <!-- Gallery Section -->
        <section id="gallery-section" class="content-section">
          <h2>Photo Gallery</h2>
          <p>Capture and save memories of your betta fish.</p>
          
          ${appData.photos && appData.photos.length > 0 ? `
            <div class="gallery-grid">
              ${appData.photos.map(photo => `
                <div class="gallery-item">
                  <div class="gallery-image">
                    <img src="${photo.url}" alt="${photo.title}">
                  </div>
                  <div class="gallery-info">
                    <div class="gallery-title">${photo.title}</div>
                    <div class="gallery-date">${new Date(photo.date).toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' })}</div>
                    <div class="gallery-notes">${photo.description || ''}</div>
                  </div>
                </div>
              `).join('')}
            </div>
          ` : `
            <div class="empty-state">
              <div class="empty-icon">📷</div>
              <p>No photos added yet. Add your first photo using the + button below.</p>
            </div>
          `}
          
          <div class="add-button" onclick="showPhotoForm()">+</div>
        </section>
        
        <!-- Notes Section -->
        <section id="notes-section" class="content-section">
          <h2>Notes</h2>
          <p>Record observations and important information about your betta.</p>
          
          ${appData.notes && appData.notes.length > 0 ? `
            <div class="notes-grid">
              ${appData.notes.map(note => `
                <div class="note-card">
                  <div class="note-header">
                    <h3>${note.title}</h3>
                    <div class="note-date">${new Date(note.date).toLocaleDateString('en-US', { year: 'numeric', month: 'short', day: 'numeric' })}</div>
                  </div>
                  <div class="note-content">
                    <p>${note.content}</p>
                  </div>
                </div>
              `).join('')}
            </div>
          ` : `
            <div class="empty-state">
              <div class="empty-icon">📝</div>
              <p>No notes added yet. Add your first note using the + button below.</p>
            </div>
          `}
          
          <div class="add-button" onclick="showNoteForm()">+</div>
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
        
        <!-- Global Add Button with Menu -->
        <div class="add-button global-add" title="Add New Entry" onclick="toggleAddMenu()">+</div>
        
        <!-- Add Menu Options -->
        <div id="add-menu" class="add-menu">
          <div class="add-menu-item" onclick="showLogForm()">
            <div class="add-menu-icon">🧪</div>
            <div class="add-menu-text">Tank Log</div>
          </div>
          <div class="add-menu-item" onclick="showTaskForm()">
            <div class="add-menu-icon">📋</div>
            <div class="add-menu-text">Maintenance</div>
          </div>
          <div class="add-menu-item" onclick="showPlantForm()">
            <div class="add-menu-icon">🌿</div>
            <div class="add-menu-text">Plant</div>
          </div>
          <div class="add-menu-item" onclick="showTreatmentForm()">
            <div class="add-menu-icon">💊</div>
            <div class="add-menu-text">Treatment</div>
          </div>
          <div class="add-menu-item" onclick="showPhotoForm()">
            <div class="add-menu-icon">📷</div>
            <div class="add-menu-text">Photo</div>
          </div>
          <div class="add-menu-item" onclick="showNoteForm()">
            <div class="add-menu-icon">📝</div>
            <div class="add-menu-text">Note</div>
          </div>
        </div>
        
        <script>
          function showSection(sectionId) {
            // Hide all sections
            const sections = document.querySelectorAll('.content-section');
            sections.forEach(section => {
              section.classList.remove('active');
            });
            
            // Show the selected section
            document.getElementById(sectionId).classList.add('active');
            
            // For gallery section, check if we have any photos
            if (sectionId === 'gallery-section') {
              // This section can be enhanced to fetch latest photos if needed
              console.log('Showing gallery section');
            }
          }
          
          // Initialize profile pictures array from server
          const profilePictures = ${clientProfilePictures};
          
          // Initialize variables for profile management
          let currentProfilePic = {
            id: 1,
            url: '${appData.fishData.profilePicture}',
            name: 'Current Profile', 
            isEmoji: ${appData.fishData.isPictureEmoji}
          };
          
          // Initialize the profile picture picker
          function initProfilePicker() {
            const profileOptions = document.querySelector('.profile-options');
            profileOptions.innerHTML = '';
            
            // Create options from the profile pictures array
            profilePictures.forEach(pic => {
              const option = document.createElement('div');
              option.className = 'profile-option' + ((pic.id === 1 && currentProfilePic.isEmoji && pic.url === currentProfilePic.url) ? ' selected' : '');
              option.setAttribute('data-id', pic.id);
              
              if (pic.isEmoji) {
                const emoji = document.createElement('div');
                emoji.className = 'profile-emoji';
                emoji.textContent = pic.url;
                option.appendChild(emoji);
              } else if (pic.url) {
                const img = document.createElement('img');
                img.src = pic.url;
                img.alt = pic.name;
                img.style.width = '80px';
                img.style.height = '80px';
                img.style.objectFit = 'cover';
                img.style.borderRadius = '50%';
                option.appendChild(img);
              } else {
                const placeholder = document.createElement('div');
                placeholder.style.width = '80px';
                placeholder.style.height = '80px';
                placeholder.style.backgroundColor = '#eee';
                placeholder.style.borderRadius = '50%';
                placeholder.style.display = 'flex';
                placeholder.style.justifyContent = 'center';
                placeholder.style.alignItems = 'center';
                placeholder.textContent = '📷';
                option.appendChild(placeholder);
              }
              
              const name = document.createElement('div');
              name.textContent = pic.name;
              option.appendChild(name);
              
              option.addEventListener('click', function() {
                // Remove selected class from all options
                document.querySelectorAll('.profile-option').forEach(opt => {
                  opt.classList.remove('selected');
                });
                
                // Add selected class to this option
                option.classList.add('selected');
                
                // Update current profile picture
                currentProfilePic = profilePictures.find(p => p.id === pic.id);
                
                // Update the avatar in the UI
                if (currentProfilePic.isEmoji) {
                  document.querySelector('.fish-avatar').innerHTML = currentProfilePic.url;
                } else if (currentProfilePic.url) {
                  document.querySelector('.fish-avatar').innerHTML = '<img src="' + currentProfilePic.url + '" alt="' + currentProfilePic.name + '" />';
                }
              });
              
              profileOptions.appendChild(option);
            });
          }
          
          // Toggle profile picture picker
          function toggleProfilePicker() {
            const picker = document.getElementById('profile-picker');
            picker.classList.toggle('active');
            
            if (picker.classList.contains('active')) {
              initProfilePicker();
            }
          }
          
          // Upload profile picture
          function uploadProfilePicture() {
            const fileInput = document.getElementById('profile-upload');
            if (fileInput.files && fileInput.files[0]) {
              const formData = new FormData();
              formData.append('profile', fileInput.files[0]);
              formData.append('fishId', 1); // Default to Volt
              
              fetch('/api/upload-profile-image', {
                method: 'POST',
                body: formData
              })
              .then(response => response.json())
              .then(data => {
                if (data.success) {
                  alert('Profile picture uploaded successfully!');
                  
                  // Update custom upload option
                  const customPic = profilePictures.find(p => p.id === 4);
                  customPic.url = data.url;
                  customPic.isEmoji = false;
                  
                  // Select the custom option
                  document.querySelector('[data-id="4"]').click();
                  
                  // Close the picker
                  toggleProfilePicker();
                  
                  // Refresh page to show the new profile picture
                  setTimeout(() => {
                    location.reload();
                  }, 1000);
                } else {
                  alert('Error uploading profile picture: ' + data.message);
                }
              })
              .catch(error => {
                console.error('Error uploading profile picture:', error);
                alert('Error uploading profile picture. Please try again.');
              });
            } else {
              alert('Please select a file first.');
            }
          }
          
          // Initialize notifications
          function loadNotifications() {
            const notificationsList = document.getElementById('notifications-list');
            notificationsList.innerHTML = '';
            
            // Get notifications from the server
            fetch('/api/notifications')
              .then(response => response.json())
              .then(notifications => {
                // Get unread count
                const unreadCount = notifications.filter(n => !n.read).length;
                document.querySelector('.notification-badge').setAttribute('data-count', unreadCount);
                
                if (unreadCount === 0) {
                  document.querySelector('.notification-badge').style.display = 'none';
                } else {
                  document.querySelector('.notification-badge').style.display = 'inline-block';
                }
                
                // Create notification items
                notifications.forEach(notification => {
                  const item = document.createElement('div');
                  item.className = 'notification-item' + (notification.read ? '' : ' unread');
                  item.setAttribute('data-id', notification.id);
                  
                  const title = document.createElement('div');
                  title.className = 'notification-title';
                  title.textContent = notification.title;
                  
                  const message = document.createElement('div');
                  message.className = 'notification-message';
                  message.textContent = notification.message;
                  
                  const date = document.createElement('div');
                  date.className = 'notification-date';
                  date.textContent = new Date(notification.date).toLocaleDateString();
                  
                  item.appendChild(title);
                  item.appendChild(message);
                  item.appendChild(date);
                  
                  item.addEventListener('click', function() {
                    // Mark notification as read via API
                    fetch('/api/mark-notification-read', {
                      method: 'POST',
                      headers: {
                        'Content-Type': 'application/json'
                      },
                      body: JSON.stringify({ id: notification.id })
                    })
                    .then(response => response.json())
                    .then(data => {
                      if (data.success) {
                        loadNotifications();
                      }
                    });
                    
                    // Show alert with notification message
                    alert(notification.message);
                  });
                  
                  notificationsList.appendChild(item);
                });
              })
              .catch(error => {
                console.error('Error loading notifications:', error);
              });
          }
          
          // Toggle notifications panel
          function toggleNotifications() {
            const panel = document.getElementById('notifications-panel');
            panel.classList.toggle('active');
            
            if (panel.classList.contains('active')) {
              loadNotifications();
            }
          }
          
          // Task form modal
          function showTaskForm() {
            const taskFormModal = document.getElementById('task-form-modal');
            taskFormModal.classList.add('active');
          }
          
          function hideTaskForm() {
            const taskFormModal = document.getElementById('task-form-modal');
            taskFormModal.classList.remove('active');
          }
          
          // Save new maintenance task
          function saveMaintenanceTask() {
            const taskName = document.getElementById('task-name').value;
            const taskDate = document.getElementById('task-date').value;
            const taskRecurring = document.getElementById('task-recurring').value;
            const taskPriority = document.getElementById('task-priority').value;
            const taskNotify = document.getElementById('task-notify').checked;
            
            if (!taskName || !taskDate) {
              alert('Please fill in all required fields.');
              return;
            }
            
            // Prepare task data
            const taskData = {
              fishId: 1, // Default to Volt
              task: taskName,
              dueDate: taskDate,
              recurring: taskRecurring,
              priority: taskPriority,
              notify: taskNotify,
              lastDone: new Date().toISOString()
            };
            
            // Send to server
            fetch('/api/maintenance', {
              method: 'POST',
              headers: {
                'Content-Type': 'application/json'
              },
              body: JSON.stringify(taskData)
            })
            .then(response => response.json())
            .then(data => {
              if (data.success) {
                alert('Task added successfully!');
                hideTaskForm();
                // Refresh page to show new task
                location.reload();
              } else {
                alert('Error adding task: ' + data.message);
              }
            })
            .catch(error => {
              console.error('Error adding task:', error);
              alert('Error adding task. Please try again.');
            });
          }
          
          // Add button click handler
          document.querySelector('.add-button').addEventListener('click', function() {
            const activeSectionId = document.querySelector('.content-section.active').id;
            
            switch(activeSectionId) {
              case 'maintenance-section':
                showTaskForm();
                break;
              case 'tanklog-section':
                showLogForm();
                break;
              case 'plants-section':
                showPlantForm();
                break;
              case 'treatments-section':
                showTreatmentForm();
                break;
              case 'gallery-section':
                showPhotoForm();
                break;
              case 'notes-section':
                showNoteForm();
                break;
              default:
                alert('Add new entry (Feature coming soon for this section)');
            }
          });
          
          // Tank Log form functions
          function showLogForm() {
            const logFormModal = document.getElementById('log-form-modal');
            logFormModal.classList.add('active');
          }
          
          function hideLogForm() {
            const logFormModal = document.getElementById('log-form-modal');
            logFormModal.classList.remove('active');
          }
          
          function saveTankLog() {
            const logDate = document.getElementById('log-date').value;
            const logAmmonia = document.getElementById('log-ammonia').value;
            const logNitrite = document.getElementById('log-nitrite').value;
            const logNitrate = document.getElementById('log-nitrate').value;
            const logPh = document.getElementById('log-ph').value;
            const logTemp = document.getElementById('log-temp').value;
            const logNotes = document.getElementById('log-notes').value;
            
            if (!logDate) {
              alert('Please select a date.');
              return;
            }
            
            // Prepare log data
            const logData = {
              fishId: 1, // Default to Volt
              date: logDate,
              ammonia: logAmmonia || '0',
              nitrite: logNitrite || '0',
              nitrate: logNitrate || '0',
              ph: logPh || '7.0',
              temp: logTemp || '78',
              notes: logNotes || ''
            };
            
            // Send to server
            fetch('/api/tank-logs', {
              method: 'POST',
              headers: {
                'Content-Type': 'application/json'
              },
              body: JSON.stringify(logData)
            })
            .then(response => response.json())
            .then(data => {
              if (data.success) {
                alert('Tank log added successfully!');
                hideLogForm();
                // Refresh page to show new log
                location.reload();
              } else {
                alert('Error adding tank log: ' + data.message);
              }
            })
            .catch(error => {
              console.error('Error adding tank log:', error);
              alert('Error adding tank log. Please try again.');
            });
          }
          
          // Plant form functions
          function showPlantForm() {
            const plantFormModal = document.getElementById('plant-form-modal');
            plantFormModal.classList.add('active');
          }
          
          function hidePlantForm() {
            const plantFormModal = document.getElementById('plant-form-modal');
            plantFormModal.classList.remove('active');
          }
          
          function savePlant() {
            const plantName = document.getElementById('plant-name').value;
            const plantSpecies = document.getElementById('plant-species').value;
            const plantDate = document.getElementById('plant-date').value;
            const plantCare = document.getElementById('plant-care').value;
            
            if (!plantName || !plantDate) {
              alert('Please fill in all required fields.');
              return;
            }
            
            // Prepare plant data
            const plantData = {
              fishId: 1, // Default to Volt's tank
              name: plantName,
              species: plantSpecies,
              dateAdded: plantDate,
              careInstructions: plantCare
            };
            
            // Send to server
            fetch('/api/plants', {
              method: 'POST',
              headers: {
                'Content-Type': 'application/json'
              },
              body: JSON.stringify(plantData)
            })
            .then(response => response.json())
            .then(data => {
              if (data.success) {
                alert('Plant added successfully!');
                hidePlantForm();
                // Refresh page to show new plant
                location.reload();
              } else {
                alert('Error adding plant: ' + data.message);
              }
            })
            .catch(error => {
              console.error('Error adding plant:', error);
              alert('Error adding plant. Please try again.');
            });
          }
          
          // Treatment form functions
          function showTreatmentForm() {
            const treatmentFormModal = document.getElementById('treatment-form-modal');
            treatmentFormModal.classList.add('active');
          }
          
          function hideTreatmentForm() {
            const treatmentFormModal = document.getElementById('treatment-form-modal');
            treatmentFormModal.classList.remove('active');
          }
          
          function saveTreatment() {
            const treatmentName = document.getElementById('treatment-name').value;
            const treatmentCondition = document.getElementById('treatment-condition').value;
            const treatmentStartDate = document.getElementById('treatment-start-date').value;
            const treatmentEndDate = document.getElementById('treatment-end-date').value;
            const treatmentDosage = document.getElementById('treatment-dosage').value;
            const treatmentNotes = document.getElementById('treatment-notes').value;
            
            if (!treatmentName || !treatmentCondition || !treatmentStartDate) {
              alert('Please fill in all required fields.');
              return;
            }
            
            // Prepare treatment data
            const treatmentData = {
              fishId: 1, // Default to Volt
              name: treatmentName,
              condition: treatmentCondition,
              startDate: treatmentStartDate,
              endDate: treatmentEndDate || null,
              dosage: treatmentDosage,
              notes: treatmentNotes
            };
            
            // Send to server
            fetch('/api/treatments', {
              method: 'POST',
              headers: {
                'Content-Type': 'application/json'
              },
              body: JSON.stringify(treatmentData)
            })
            .then(response => response.json())
            .then(data => {
              if (data.success) {
                alert('Treatment added successfully!');
                hideTreatmentForm();
                // Refresh page to show new treatment
                location.reload();
              } else {
                alert('Error adding treatment: ' + data.message);
              }
            })
            .catch(error => {
              console.error('Error adding treatment:', error);
              alert('Error adding treatment. Please try again.');
            });
          }
          
          // Photo form functions
          function showPhotoForm() {
            const photoFormModal = document.getElementById('photo-form-modal');
            photoFormModal.classList.add('active');
          }
          
          function hidePhotoForm() {
            const photoFormModal = document.getElementById('photo-form-modal');
            photoFormModal.classList.remove('active');
          }
          
          function savePhoto() {
            const photoTitle = document.getElementById('photo-title').value;
            const photoDate = document.getElementById('photo-date').value;
            const photoNotes = document.getElementById('photo-notes').value;
            const photoFile = document.getElementById('photo-file').files[0];
            
            if (!photoTitle || !photoDate || !photoFile) {
              alert('Please fill in all required fields and select a file.');
              return;
            }
            
            // Prepare form data for file upload
            const formData = new FormData();
            formData.append('title', photoTitle);
            formData.append('date', photoDate);
            formData.append('description', photoNotes);
            formData.append('fishId', 1); // Default to Volt
            formData.append('photo', photoFile);
            
            // Send to server
            fetch('/api/photos', {
              method: 'POST',
              body: formData
            })
            .then(response => response.json())
            .then(data => {
              if (data.success) {
                alert('Photo added successfully!');
                hidePhotoForm();
                
                // Show the gallery section
                showSection('gallery-section');
                
                // Refresh page to show new photo (need to fully reload to get updated data)
                setTimeout(() => {
                  location.reload();
                }, 500);
              } else {
                alert('Error adding photo: ' + data.message);
              }
            })
            .catch(error => {
              console.error('Error adding photo:', error);
              alert('Error adding photo. Please try again.');
            });
          }
          
          // Note form functions
          function showNoteForm() {
            const noteFormModal = document.getElementById('note-form-modal');
            noteFormModal.classList.add('active');
          }
          
          function hideNoteForm() {
            const noteFormModal = document.getElementById('note-form-modal');
            noteFormModal.classList.remove('active');
          }
          
          function saveNote() {
            const noteTitle = document.getElementById('note-title').value;
            const noteDate = document.getElementById('note-date').value;
            const noteContent = document.getElementById('note-content').value;
            
            if (!noteTitle || !noteDate || !noteContent) {
              alert('Please fill in all required fields.');
              return;
            }
            
            // Prepare note data
            const noteData = {
              fishId: 1, // Default to Volt
              title: noteTitle,
              date: noteDate,
              content: noteContent
            };
            
            // Send to server
            fetch('/api/notes', {
              method: 'POST',
              headers: {
                'Content-Type': 'application/json'
              },
              body: JSON.stringify(noteData)
            })
            .then(response => response.json())
            .then(data => {
              if (data.success) {
                alert('Note added successfully!');
                hideNoteForm();
                // Refresh page to show new note
                location.reload();
              } else {
                alert('Error adding note: ' + data.message);
              }
            })
            .catch(error => {
              console.error('Error adding note:', error);
              alert('Error adding note. Please try again.');
            });
          }
          
          // Toggle mobile menu
          function toggleMenu() {
            document.querySelector('nav').classList.toggle('active');
          }
          
          // Toggle add menu
          function toggleAddMenu() {
            const addMenu = document.getElementById('add-menu');
            addMenu.classList.toggle('active');
            
            // Toggle active state on the plus button
            document.querySelector('.global-add').classList.toggle('active');
            
            // Close the menu when clicking outside
            if (addMenu.classList.contains('active')) {
              setTimeout(() => {
                document.addEventListener('click', closeAddMenuOnClickOutside);
              }, 100);
            }
          }
          
          // Close add menu when clicking outside
          function closeAddMenuOnClickOutside(event) {
            const addMenu = document.getElementById('add-menu');
            const addButton = document.querySelector('.global-add');
            
            if (!addMenu.contains(event.target) && event.target !== addButton) {
              addMenu.classList.remove('active');
              addButton.classList.remove('active');
              document.removeEventListener('click', closeAddMenuOnClickOutside);
            }
          }
          
          // Initialize water parameter chart
          let waterParamsChart = null;
          
          function initializeParamsChart() {
            const ctx = document.getElementById('paramsChart').getContext('2d');
            
            // Get water parameter data from tank logs
            const tankLogs = ${JSON.stringify(appData.tankLogs || [])};
            
            // Sort logs by date (oldest to newest)
            tankLogs.sort((a, b) => new Date(a.date) - new Date(b.date));
            
            // Extract dates and parameter values
            const labels = tankLogs.map(log => new Date(log.date).toLocaleDateString('en-US', { month: 'short', day: 'numeric' }));
            const ammoniaData = tankLogs.map(log => log.ammonia);
            const nitriteData = tankLogs.map(log => log.nitrite);
            const nitrateData = tankLogs.map(log => log.nitrate);
            const phData = tankLogs.map(log => log.ph);
            const tempData = tankLogs.map(log => log.temp);
            
            // Initialize chart
            waterParamsChart = new Chart(ctx, {
              type: 'line',
              data: {
                labels: labels,
                datasets: [{
                  label: 'Ammonia (ppm)',
                  data: ammoniaData,
                  borderColor: '#F04F94',
                  backgroundColor: 'rgba(240, 79, 148, 0.1)',
                  borderWidth: 2,
                  tension: 0.3,
                  fill: true
                }]
              },
              options: {
                responsive: true,
                plugins: {
                  legend: {
                    position: 'top',
                  },
                  tooltip: {
                    mode: 'index',
                    intersect: false,
                  }
                },
                scales: {
                  y: {
                    beginAtZero: true
                  }
                }
              }
            });
          }
          
          // Handle parameter button clicks
          function setupParamButtons() {
            const buttons = document.querySelectorAll('.parameter-button');
            const tankLogs = ${JSON.stringify(appData.tankLogs || [])};
            
            // Sort logs by date (oldest to newest)
            tankLogs.sort((a, b) => new Date(a.date) - new Date(b.date));
            
            // Extract dates and parameter values
            const labels = tankLogs.map(log => new Date(log.date).toLocaleDateString('en-US', { month: 'short', day: 'numeric' }));
            
            buttons.forEach(button => {
              button.addEventListener('click', function() {
                // Update active button
                buttons.forEach(b => b.classList.remove('active'));
                this.classList.add('active');
                
                // Get selected parameter
                const param = this.getAttribute('data-param');
                let data, color, label;
                
                // Set data based on selected parameter
                switch(param) {
                  case 'ammonia':
                    data = tankLogs.map(log => log.ammonia);
                    color = '#F04F94';
                    label = 'Ammonia (ppm)';
                    break;
                  case 'nitrite':
                    data = tankLogs.map(log => log.nitrite);
                    color = '#F0734F';
                    label = 'Nitrite (ppm)';
                    break;
                  case 'nitrate':
                    data = tankLogs.map(log => log.nitrate);
                    color = '#F05950';
                    label = 'Nitrate (ppm)';
                    break;
                  case 'ph':
                    data = tankLogs.map(log => log.ph);
                    color = '#ED4FF0';
                    label = 'pH';
                    break;
                  case 'temp':
                    data = tankLogs.map(log => log.temp);
                    color = '#F08F4F';
                    label = 'Temperature (°F)';
                    break;
                }
                
                // Update chart
                waterParamsChart.data.datasets[0].data = data;
                waterParamsChart.data.datasets[0].label = label;
                waterParamsChart.data.datasets[0].borderColor = color;
                waterParamsChart.data.datasets[0].backgroundColor = color.replace(')', ', 0.1)').replace('rgb', 'rgba');
                waterParamsChart.update();
              });
            });
          }
          
          // Initialize the app
          document.addEventListener('DOMContentLoaded', function() {
            // Load notifications
            loadNotifications();
            
            // Initialize water parameters chart if we have any tank logs
            if (document.getElementById('paramsChart')) {
              initializeParamsChart();
              setupParamButtons();
            }
            
            // Set up fish avatar click handler
            document.querySelector('.fish-avatar').addEventListener('click', toggleProfilePicker);
            
            // Set the date inputs to today's date by default
            const today = new Date().toISOString().split('T')[0];
            const dateInputs = document.querySelectorAll('input[type="date"]');
            dateInputs.forEach(input => {
              input.value = today;
            });
            
            // Close menu when a section is selected on mobile
            const navButtons = document.querySelectorAll('.nav-button');
            navButtons.forEach(button => {
              button.addEventListener('click', function() {
                if (window.innerWidth <= 600) {
                  document.querySelector('nav').classList.remove('active');
                }
              });
            });
          });
        </script>
      </body>
    </html>
  `);
});

app.listen(port, () => {
  console.log(`Volt Betta Manager prototype running at http://localhost:${port}`);
});