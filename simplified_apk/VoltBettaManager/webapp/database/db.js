const { Sequelize, DataTypes } = require('sequelize');
const path = require('path');

// Create a SQLite database that's saved to a file
const sequelize = new Sequelize({
  dialect: 'sqlite',
  storage: path.join(__dirname, 'voltbetta.sqlite'),
  logging: false // Set to console.log to see SQL queries
});

// Define Fish Profile Model
const FishProfile = sequelize.define('FishProfile', {
  id: {
    type: DataTypes.INTEGER,
    primaryKey: true,
    autoIncrement: true
  },
  name: {
    type: DataTypes.STRING,
    allowNull: false
  },
  species: {
    type: DataTypes.STRING,
    allowNull: false
  },
  variant: {
    type: DataTypes.STRING
  },
  color: {
    type: DataTypes.STRING
  },
  age: {
    type: DataTypes.STRING
  },
  tank: {
    type: DataTypes.STRING
  },
  acquisitionDate: {
    type: DataTypes.DATEONLY
  },
  profilePicture: {
    type: DataTypes.STRING,
    defaultValue: '🐠'
  },
  isPictureEmoji: {
    type: DataTypes.BOOLEAN,
    defaultValue: true
  }
});

// Define Tank Log Model
const TankLog = sequelize.define('TankLog', {
  id: {
    type: DataTypes.INTEGER,
    primaryKey: true,
    autoIncrement: true
  },
  date: {
    type: DataTypes.DATEONLY,
    allowNull: false
  },
  ammonia: {
    type: DataTypes.FLOAT,
    defaultValue: 0
  },
  nitrite: {
    type: DataTypes.FLOAT,
    defaultValue: 0
  },
  nitrate: {
    type: DataTypes.FLOAT,
    defaultValue: 0
  },
  ph: {
    type: DataTypes.FLOAT
  },
  temp: {
    type: DataTypes.FLOAT
  },
  notes: {
    type: DataTypes.TEXT
  }
});

// Define Maintenance Task Model
const MaintenanceTask = sequelize.define('MaintenanceTask', {
  id: {
    type: DataTypes.INTEGER,
    primaryKey: true,
    autoIncrement: true
  },
  task: {
    type: DataTypes.STRING,
    allowNull: false
  },
  dueDate: {
    type: DataTypes.DATEONLY
  },
  recurring: {
    type: DataTypes.STRING // 'Weekly', 'Monthly', 'Bi-weekly'
  },
  lastDone: {
    type: DataTypes.DATEONLY
  },
  notify: {
    type: DataTypes.BOOLEAN,
    defaultValue: true
  },
  priority: {
    type: DataTypes.STRING, // 'high', 'medium', 'low'
    defaultValue: 'medium'
  }
});

// Define Plant Model
const Plant = sequelize.define('Plant', {
  id: {
    type: DataTypes.INTEGER,
    primaryKey: true,
    autoIncrement: true
  },
  name: {
    type: DataTypes.STRING,
    allowNull: false
  },
  species: {
    type: DataTypes.STRING
  },
  planted: {
    type: DataTypes.DATEONLY
  },
  lightRequirement: {
    type: DataTypes.STRING // 'low', 'medium', 'high'
  },
  fertilizer: {
    type: DataTypes.STRING
  },
  notes: {
    type: DataTypes.TEXT
  }
});

// Define Treatment Plan Model
const TreatmentPlan = sequelize.define('TreatmentPlan', {
  id: {
    type: DataTypes.INTEGER,
    primaryKey: true,
    autoIncrement: true
  },
  issue: {
    type: DataTypes.STRING,
    allowNull: false
  },
  startDate: {
    type: DataTypes.DATEONLY,
    allowNull: false
  },
  endDate: {
    type: DataTypes.DATEONLY
  },
  medication: {
    type: DataTypes.STRING
  },
  dosage: {
    type: DataTypes.STRING
  },
  notes: {
    type: DataTypes.TEXT
  },
  status: {
    type: DataTypes.STRING, // 'active', 'completed', 'discontinued'
    defaultValue: 'active'
  }
});

// Define Photo Model
const Photo = sequelize.define('Photo', {
  id: {
    type: DataTypes.INTEGER,
    primaryKey: true,
    autoIncrement: true
  },
  url: {
    type: DataTypes.STRING,
    allowNull: false
  },
  date: {
    type: DataTypes.DATEONLY,
    allowNull: false
  },
  title: {
    type: DataTypes.STRING
  },
  description: {
    type: DataTypes.TEXT
  }
});

// Define Note Model
const Note = sequelize.define('Note', {
  id: {
    type: DataTypes.INTEGER,
    primaryKey: true,
    autoIncrement: true
  },
  title: {
    type: DataTypes.STRING,
    allowNull: false
  },
  content: {
    type: DataTypes.TEXT
  },
  date: {
    type: DataTypes.DATEONLY,
    allowNull: false
  },
  category: {
    type: DataTypes.STRING
  }
});

// Define Notification Model
const Notification = sequelize.define('Notification', {
  id: {
    type: DataTypes.INTEGER,
    primaryKey: true,
    autoIncrement: true
  },
  title: {
    type: DataTypes.STRING,
    allowNull: false
  },
  message: {
    type: DataTypes.TEXT,
    allowNull: false
  },
  read: {
    type: DataTypes.BOOLEAN,
    defaultValue: false
  },
  date: {
    type: DataTypes.DATEONLY,
    allowNull: false
  },
  type: {
    type: DataTypes.STRING // 'maintenance', 'reminder', 'summary'
  }
});

// Setup relationships
FishProfile.hasMany(TankLog);
TankLog.belongsTo(FishProfile);

FishProfile.hasMany(MaintenanceTask);
MaintenanceTask.belongsTo(FishProfile);

FishProfile.hasMany(TreatmentPlan);
TreatmentPlan.belongsTo(FishProfile);

FishProfile.hasMany(Photo);
Photo.belongsTo(FishProfile);

FishProfile.hasMany(Note);
Note.belongsTo(FishProfile);

FishProfile.hasMany(Notification);
Notification.belongsTo(FishProfile);

// Sync the database (create tables if they don't exist)
async function syncDatabase() {
  try {
    await sequelize.sync();
    console.log('Database synchronized successfully');
  } catch (error) {
    console.error('Error synchronizing database:', error);
  }
}

// Initialize with sample data if the database is empty
async function initializeWithSampleData() {
  try {
    // Check if fish profile exists
    const fishCount = await FishProfile.count();
    
    if (fishCount === 0) {
      // Create Volt's profile
      const volt = await FishProfile.create({
        name: 'Volt',
        species: 'Betta splendens',
        variant: 'Veiltail',
        color: 'Red/Blue',
        age: '~8 months',
        tank: '3.5 gallon planted',
        acquisitionDate: '2025-04-05',
        profilePicture: '🐠',
        isPictureEmoji: true
      });
      
      // Create maintenance tasks
      await MaintenanceTask.bulkCreate([
        {
          task: 'Water Change (25%)',
          dueDate: '2025-04-19',
          recurring: 'Weekly (Tuesday)',
          lastDone: '2025-04-12',
          notify: true,
          priority: 'high',
          FishProfileId: volt.id
        },
        {
          task: 'Replace Filter Media',
          dueDate: '2025-05-10',
          recurring: 'Monthly',
          lastDone: '2025-04-10',
          notify: true,
          priority: 'medium',
          FishProfileId: volt.id
        },
        {
          task: 'Clean Gravel',
          dueDate: '2025-04-24',
          recurring: 'Bi-weekly',
          lastDone: '2025-04-10',
          notify: true,
          priority: 'medium',
          FishProfileId: volt.id
        },
        {
          task: 'Test Water Parameters',
          dueDate: '2025-04-18',
          recurring: 'Weekly',
          lastDone: '2025-04-11',
          notify: true,
          priority: 'high',
          FishProfileId: volt.id
        },
        {
          task: 'Trim Plants',
          dueDate: '2025-04-21',
          recurring: 'Bi-weekly',
          lastDone: '2025-04-07',
          notify: true,
          priority: 'low',
          FishProfileId: volt.id
        }
      ]);
      
      // Create tank logs
      await TankLog.bulkCreate([
        {
          date: '2025-04-10',
          ammonia: 0,
          nitrite: 0,
          nitrate: 5,
          ph: 7.2,
          temp: 78,
          notes: 'Parameters looking good. Added 1mL of plant fertilizer.',
          FishProfileId: volt.id
        },
        {
          date: '2025-04-03',
          ammonia: 0,
          nitrite: 0,
          nitrate: 10,
          ph: 7.0,
          temp: 79,
          notes: 'Did a 25% water change. Replaced filter media.',
          FishProfileId: volt.id
        },
        {
          date: '2025-03-27',
          ammonia: 0,
          nitrite: 0,
          nitrate: 15,
          ph: 7.1,
          temp: 78,
          notes: 'Volt seems very active today. Rearranged plants slightly.',
          FishProfileId: volt.id
        }
      ]);
      
      // Create notifications
      await Notification.bulkCreate([
        {
          title: 'Water Change Today',
          message: 'Remember to change 25% of the water today (Tuesday)',
          read: false,
          date: '2025-04-16',
          type: 'maintenance',
          FishProfileId: volt.id
        },
        {
          title: 'Test Water Parameters',
          message: 'It\'s been a week since you last tested water parameters',
          read: true,
          date: '2025-04-14',
          type: 'reminder',
          FishProfileId: volt.id
        },
        {
          title: 'Weekly Task Summary',
          message: 'You have 3 maintenance tasks scheduled for this week',
          read: true,
          date: '2025-04-13',
          type: 'summary',
          FishProfileId: volt.id
        }
      ]);
      
      // Create plants
      await Plant.bulkCreate([
        {
          name: 'Java Fern',
          species: 'Microsorum pteropus',
          planted: '2025-01-15',
          lightRequirement: 'low',
          fertilizer: 'None required',
          notes: 'Attached to driftwood, do not bury rhizome'
        },
        {
          name: 'Anubias',
          species: 'Anubias barteri',
          planted: '2025-01-15',
          lightRequirement: 'low',
          fertilizer: 'Weekly liquid fertilizer',
          notes: 'Attached to rock, slow growing'
        },
        {
          name: 'Amazon Sword',
          species: 'Echinodorus amazonicus',
          planted: '2025-02-01',
          lightRequirement: 'medium',
          fertilizer: 'Root tabs every 3 months',
          notes: 'Center background plant, may grow large'
        }
      ]);
      
      console.log('Sample data has been initialized');
    } else {
      console.log('Database already contains data, skipping initialization');
    }
  } catch (error) {
    console.error('Error initializing sample data:', error);
  }
}

// Export models and functions
module.exports = {
  sequelize,
  FishProfile,
  TankLog,
  MaintenanceTask,
  Plant,
  TreatmentPlan,
  Photo,
  Note,
  Notification,
  syncDatabase,
  initializeWithSampleData
};