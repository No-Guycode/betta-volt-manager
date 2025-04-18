const db = require('./db');
const { Op } = require('sequelize');

// Fish Profile Controllers
const FishController = {
  // Get all fish profiles
  getAll: async () => {
    try {
      return await db.FishProfile.findAll();
    } catch (error) {
      console.error('Error fetching fish profiles:', error);
      throw error;
    }
  },
  
  // Get a fish profile by ID
  getById: async (id) => {
    try {
      return await db.FishProfile.findByPk(id);
    } catch (error) {
      console.error(`Error fetching fish profile with ID ${id}:`, error);
      throw error;
    }
  },
  
  // Create a new fish profile
  create: async (fishData) => {
    try {
      return await db.FishProfile.create(fishData);
    } catch (error) {
      console.error('Error creating fish profile:', error);
      throw error;
    }
  },
  
  // Update a fish profile
  update: async (id, fishData) => {
    try {
      const fish = await db.FishProfile.findByPk(id);
      if (!fish) return null;
      
      return await fish.update(fishData);
    } catch (error) {
      console.error(`Error updating fish profile with ID ${id}:`, error);
      throw error;
    }
  },
  
  // Update profile picture
  updateProfilePicture: async (id, profilePicture, isPictureEmoji = true) => {
    try {
      const fish = await db.FishProfile.findByPk(id);
      if (!fish) return null;
      
      return await fish.update({ profilePicture, isPictureEmoji });
    } catch (error) {
      console.error(`Error updating profile picture for fish with ID ${id}:`, error);
      throw error;
    }
  },
  
  // Delete a fish profile
  delete: async (id) => {
    try {
      const fish = await db.FishProfile.findByPk(id);
      if (!fish) return false;
      
      await fish.destroy();
      return true;
    } catch (error) {
      console.error(`Error deleting fish profile with ID ${id}:`, error);
      throw error;
    }
  }
};

// Tank Log Controllers
const TankLogController = {
  // Get all tank logs for a fish
  getAllForFish: async (fishId) => {
    try {
      return await db.TankLog.findAll({
        where: { FishProfileId: fishId },
        order: [['date', 'DESC']]
      });
    } catch (error) {
      console.error(`Error fetching tank logs for fish with ID ${fishId}:`, error);
      throw error;
    }
  },
  
  // Get a tank log by ID
  getById: async (id) => {
    try {
      return await db.TankLog.findByPk(id);
    } catch (error) {
      console.error(`Error fetching tank log with ID ${id}:`, error);
      throw error;
    }
  },
  
  // Create a new tank log
  create: async (logData) => {
    try {
      return await db.TankLog.create(logData);
    } catch (error) {
      console.error('Error creating tank log:', error);
      throw error;
    }
  },
  
  // Update a tank log
  update: async (id, logData) => {
    try {
      const log = await db.TankLog.findByPk(id);
      if (!log) return null;
      
      return await log.update(logData);
    } catch (error) {
      console.error(`Error updating tank log with ID ${id}:`, error);
      throw error;
    }
  },
  
  // Delete a tank log
  delete: async (id) => {
    try {
      const log = await db.TankLog.findByPk(id);
      if (!log) return false;
      
      await log.destroy();
      return true;
    } catch (error) {
      console.error(`Error deleting tank log with ID ${id}:`, error);
      throw error;
    }
  }
};

// Maintenance Task Controllers
const MaintenanceController = {
  // Get all tasks for a fish
  getAllForFish: async (fishId) => {
    try {
      return await db.MaintenanceTask.findAll({
        where: { FishProfileId: fishId },
        order: [['dueDate', 'ASC']]
      });
    } catch (error) {
      console.error(`Error fetching maintenance tasks for fish with ID ${fishId}:`, error);
      throw error;
    }
  },
  
  // Get tasks due within the next X days
  getUpcoming: async (fishId, days = 7) => {
    try {
      const today = new Date();
      const future = new Date();
      future.setDate(today.getDate() + days);
      
      return await db.MaintenanceTask.findAll({
        where: {
          FishProfileId: fishId,
          dueDate: {
            [Op.between]: [today, future]
          }
        },
        order: [['dueDate', 'ASC']]
      });
    } catch (error) {
      console.error(`Error fetching upcoming tasks for fish with ID ${fishId}:`, error);
      throw error;
    }
  },
  
  // Get a task by ID
  getById: async (id) => {
    try {
      return await db.MaintenanceTask.findByPk(id);
    } catch (error) {
      console.error(`Error fetching maintenance task with ID ${id}:`, error);
      throw error;
    }
  },
  
  // Create a new task
  create: async (taskData) => {
    try {
      return await db.MaintenanceTask.create(taskData);
    } catch (error) {
      console.error('Error creating maintenance task:', error);
      throw error;
    }
  },
  
  // Update a task
  update: async (id, taskData) => {
    try {
      const task = await db.MaintenanceTask.findByPk(id);
      if (!task) return null;
      
      return await task.update(taskData);
    } catch (error) {
      console.error(`Error updating maintenance task with ID ${id}:`, error);
      throw error;
    }
  },
  
  // Mark a task as done
  markAsDone: async (id) => {
    try {
      const task = await db.MaintenanceTask.findByPk(id);
      if (!task) return null;
      
      const today = new Date();
      const todayStr = today.toISOString().split('T')[0]; // Format as YYYY-MM-DD
      
      // Update last done date
      const updatedTask = await task.update({
        lastDone: todayStr
      });
      
      // Calculate next due date based on recurring pattern
      let nextDueDate = new Date(today);
      
      if (task.recurring.includes('Weekly')) {
        nextDueDate.setDate(today.getDate() + 7);
      } else if (task.recurring.includes('Bi-weekly')) {
        nextDueDate.setDate(today.getDate() + 14);
      } else if (task.recurring.includes('Monthly')) {
        nextDueDate.setMonth(today.getMonth() + 1);
      }
      
      // If it's a weekly task that happens on a specific day (e.g., Tuesday)
      if (task.recurring.includes('Weekly') && task.recurring.includes('(')) {
        const dayMatch = task.recurring.match(/\(([^)]+)\)/);
        if (dayMatch && dayMatch[1]) {
          const targetDay = dayMatch[1].trim();
          const dayMap = {
            'Sunday': 0, 'Monday': 1, 'Tuesday': 2, 'Wednesday': 3,
            'Thursday': 4, 'Friday': 5, 'Saturday': 6
          };
          
          if (dayMap[targetDay] !== undefined) {
            // Find the next occurrence of the target day
            let daysToAdd = (dayMap[targetDay] - today.getDay() + 7) % 7;
            if (daysToAdd === 0) daysToAdd = 7; // If today is the day, go to next week
            
            nextDueDate = new Date(today);
            nextDueDate.setDate(today.getDate() + daysToAdd);
          }
        }
      }
      
      // Update the due date
      return await updatedTask.update({
        dueDate: nextDueDate.toISOString().split('T')[0]
      });
    } catch (error) {
      console.error(`Error marking task with ID ${id} as done:`, error);
      throw error;
    }
  },
  
  // Delete a task
  delete: async (id) => {
    try {
      const task = await db.MaintenanceTask.findByPk(id);
      if (!task) return false;
      
      await task.destroy();
      return true;
    } catch (error) {
      console.error(`Error deleting maintenance task with ID ${id}:`, error);
      throw error;
    }
  }
};

// Plant Controllers
const PlantController = {
  // Get all plants
  getAll: async () => {
    try {
      return await db.Plant.findAll();
    } catch (error) {
      console.error('Error fetching plants:', error);
      throw error;
    }
  },
  
  // Get a plant by ID
  getById: async (id) => {
    try {
      return await db.Plant.findByPk(id);
    } catch (error) {
      console.error(`Error fetching plant with ID ${id}:`, error);
      throw error;
    }
  },
  
  // Create a new plant
  create: async (plantData) => {
    try {
      return await db.Plant.create(plantData);
    } catch (error) {
      console.error('Error creating plant:', error);
      throw error;
    }
  },
  
  // Update a plant
  update: async (id, plantData) => {
    try {
      const plant = await db.Plant.findByPk(id);
      if (!plant) return null;
      
      return await plant.update(plantData);
    } catch (error) {
      console.error(`Error updating plant with ID ${id}:`, error);
      throw error;
    }
  },
  
  // Delete a plant
  delete: async (id) => {
    try {
      const plant = await db.Plant.findByPk(id);
      if (!plant) return false;
      
      await plant.destroy();
      return true;
    } catch (error) {
      console.error(`Error deleting plant with ID ${id}:`, error);
      throw error;
    }
  }
};

// Treatment Plan Controllers
const TreatmentController = {
  // Get all treatment plans for a fish
  getAllForFish: async (fishId) => {
    try {
      return await db.TreatmentPlan.findAll({
        where: { FishProfileId: fishId },
        order: [['startDate', 'DESC']]
      });
    } catch (error) {
      console.error(`Error fetching treatment plans for fish with ID ${fishId}:`, error);
      throw error;
    }
  },
  
  // Get active treatment plans for a fish
  getActiveForFish: async (fishId) => {
    try {
      return await db.TreatmentPlan.findAll({
        where: {
          FishProfileId: fishId,
          status: 'active'
        },
        order: [['startDate', 'DESC']]
      });
    } catch (error) {
      console.error(`Error fetching active treatment plans for fish with ID ${fishId}:`, error);
      throw error;
    }
  },
  
  // Get a treatment plan by ID
  getById: async (id) => {
    try {
      return await db.TreatmentPlan.findByPk(id);
    } catch (error) {
      console.error(`Error fetching treatment plan with ID ${id}:`, error);
      throw error;
    }
  },
  
  // Create a new treatment plan
  create: async (treatmentData) => {
    try {
      return await db.TreatmentPlan.create(treatmentData);
    } catch (error) {
      console.error('Error creating treatment plan:', error);
      throw error;
    }
  },
  
  // Update a treatment plan
  update: async (id, treatmentData) => {
    try {
      const treatment = await db.TreatmentPlan.findByPk(id);
      if (!treatment) return null;
      
      return await treatment.update(treatmentData);
    } catch (error) {
      console.error(`Error updating treatment plan with ID ${id}:`, error);
      throw error;
    }
  },
  
  // Complete a treatment plan
  complete: async (id) => {
    try {
      const treatment = await db.TreatmentPlan.findByPk(id);
      if (!treatment) return null;
      
      const today = new Date();
      
      return await treatment.update({
        status: 'completed',
        endDate: today.toISOString().split('T')[0]
      });
    } catch (error) {
      console.error(`Error completing treatment plan with ID ${id}:`, error);
      throw error;
    }
  },
  
  // Delete a treatment plan
  delete: async (id) => {
    try {
      const treatment = await db.TreatmentPlan.findByPk(id);
      if (!treatment) return false;
      
      await treatment.destroy();
      return true;
    } catch (error) {
      console.error(`Error deleting treatment plan with ID ${id}:`, error);
      throw error;
    }
  }
};

// Photo Controllers
const PhotoController = {
  // Get all photos for a fish
  getAllForFish: async (fishId) => {
    try {
      return await db.Photo.findAll({
        where: { FishProfileId: fishId },
        order: [['date', 'DESC']]
      });
    } catch (error) {
      console.error(`Error fetching photos for fish with ID ${fishId}:`, error);
      throw error;
    }
  },
  
  // Get a photo by ID
  getById: async (id) => {
    try {
      return await db.Photo.findByPk(id);
    } catch (error) {
      console.error(`Error fetching photo with ID ${id}:`, error);
      throw error;
    }
  },
  
  // Create a new photo
  create: async (photoData) => {
    try {
      return await db.Photo.create(photoData);
    } catch (error) {
      console.error('Error creating photo:', error);
      throw error;
    }
  },
  
  // Update a photo
  update: async (id, photoData) => {
    try {
      const photo = await db.Photo.findByPk(id);
      if (!photo) return null;
      
      return await photo.update(photoData);
    } catch (error) {
      console.error(`Error updating photo with ID ${id}:`, error);
      throw error;
    }
  },
  
  // Delete a photo
  delete: async (id) => {
    try {
      const photo = await db.Photo.findByPk(id);
      if (!photo) return false;
      
      await photo.destroy();
      return true;
    } catch (error) {
      console.error(`Error deleting photo with ID ${id}:`, error);
      throw error;
    }
  }
};

// Note Controllers
const NoteController = {
  // Get all notes for a fish
  getAllForFish: async (fishId) => {
    try {
      return await db.Note.findAll({
        where: { FishProfileId: fishId },
        order: [['date', 'DESC']]
      });
    } catch (error) {
      console.error(`Error fetching notes for fish with ID ${fishId}:`, error);
      throw error;
    }
  },
  
  // Get a note by ID
  getById: async (id) => {
    try {
      return await db.Note.findByPk(id);
    } catch (error) {
      console.error(`Error fetching note with ID ${id}:`, error);
      throw error;
    }
  },
  
  // Create a new note
  create: async (noteData) => {
    try {
      return await db.Note.create(noteData);
    } catch (error) {
      console.error('Error creating note:', error);
      throw error;
    }
  },
  
  // Update a note
  update: async (id, noteData) => {
    try {
      const note = await db.Note.findByPk(id);
      if (!note) return null;
      
      return await note.update(noteData);
    } catch (error) {
      console.error(`Error updating note with ID ${id}:`, error);
      throw error;
    }
  },
  
  // Delete a note
  delete: async (id) => {
    try {
      const note = await db.Note.findByPk(id);
      if (!note) return false;
      
      await note.destroy();
      return true;
    } catch (error) {
      console.error(`Error deleting note with ID ${id}:`, error);
      throw error;
    }
  }
};

// Notification Controllers
const NotificationController = {
  // Get all notifications for a fish
  getAllForFish: async (fishId) => {
    try {
      return await db.Notification.findAll({
        where: { FishProfileId: fishId },
        order: [['date', 'DESC']]
      });
    } catch (error) {
      console.error(`Error fetching notifications for fish with ID ${fishId}:`, error);
      throw error;
    }
  },
  
  // Get unread notifications for a fish
  getUnreadForFish: async (fishId) => {
    try {
      return await db.Notification.findAll({
        where: {
          FishProfileId: fishId,
          read: false
        },
        order: [['date', 'DESC']]
      });
    } catch (error) {
      console.error(`Error fetching unread notifications for fish with ID ${fishId}:`, error);
      throw error;
    }
  },
  
  // Get notification by ID
  getById: async (id) => {
    try {
      return await db.Notification.findByPk(id);
    } catch (error) {
      console.error(`Error fetching notification with ID ${id}:`, error);
      throw error;
    }
  },
  
  // Create a new notification
  create: async (notificationData) => {
    try {
      return await db.Notification.create(notificationData);
    } catch (error) {
      console.error('Error creating notification:', error);
      throw error;
    }
  },
  
  // Mark a notification as read
  markAsRead: async (id) => {
    try {
      const notification = await db.Notification.findByPk(id);
      if (!notification) return null;
      
      return await notification.update({ read: true });
    } catch (error) {
      console.error(`Error marking notification with ID ${id} as read:`, error);
      throw error;
    }
  },
  
  // Mark all notifications as read for a fish
  markAllAsRead: async (fishId) => {
    try {
      await db.Notification.update(
        { read: true },
        { where: { FishProfileId: fishId, read: false } }
      );
      return true;
    } catch (error) {
      console.error(`Error marking all notifications as read for fish with ID ${fishId}:`, error);
      throw error;
    }
  },
  
  // Delete a notification
  delete: async (id) => {
    try {
      const notification = await db.Notification.findByPk(id);
      if (!notification) return false;
      
      await notification.destroy();
      return true;
    } catch (error) {
      console.error(`Error deleting notification with ID ${id}:`, error);
      throw error;
    }
  }
};

// Export all controllers
module.exports = {
  FishController,
  TankLogController,
  MaintenanceController,
  PlantController,
  TreatmentController,
  PhotoController,
  NoteController,
  NotificationController
};