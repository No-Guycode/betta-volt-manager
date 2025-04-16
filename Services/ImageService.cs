using System;
using System.IO;
using System.Windows.Media.Imaging;
using VoltBettaManager.Models;

namespace VoltBettaManager.Services
{
    /// <summary>
    /// Service for handling image operations
    /// </summary>
    public class ImageService
    {
        /// <summary>
        /// Saves an image file to the application's image storage directory
        /// </summary>
        /// <param name="sourceFilePath">Path to the source image file</param>
        /// <param name="caption">Caption for the image</param>
        /// <param name="category">Category for the image</param>
        /// <param name="isTreatmentPhoto">Whether this is a treatment photo</param>
        /// <param name="treatmentPlanId">Associated treatment plan ID, if applicable</param>
        /// <returns>The newly created FishPhoto object</returns>
        public FishPhoto SaveImage(string sourceFilePath, string caption, string category, bool isTreatmentPhoto = false, int? treatmentPlanId = null)
        {
            // Verify the source file exists
            if (!File.Exists(sourceFilePath))
                throw new FileNotFoundException("The source image file was not found.", sourceFilePath);
            
            string targetDir = App.GetImageStoragePath();
            string fileName = $"{DateTime.Now:yyyyMMdd_HHmmss}_{Path.GetFileName(sourceFilePath)}";
            string targetPath = Path.Combine(targetDir, fileName);
            
            // Copy the file to the application's storage
            File.Copy(sourceFilePath, targetPath, true);
            
            // Create a new photo object
            var photo = new FishPhoto
            {
                Caption = caption,
                DateTaken = DateTime.Now,
                FilePath = targetDir,
                FileName = fileName,
                Category = category,
                IsTreatmentPhoto = isTreatmentPhoto,
                TreatmentPlanId = treatmentPlanId
            };
            
            // Save to database
            photo.Id = App.DatabaseManager.AddPhoto(photo);
            
            return photo;
        }
        
        /// <summary>
        /// Loads an image from its file path
        /// </summary>
        /// <param name="photo">The FishPhoto object to load</param>
        /// <returns>A BitmapImage object</returns>
        public BitmapImage LoadImage(FishPhoto photo)
        {
            try
            {
                string fullPath = photo.FullPath;
                
                // Verify the file exists
                if (!File.Exists(fullPath))
                    return null;
                
                BitmapImage image = new BitmapImage();
                
                // Initialize the image
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad;
                image.UriSource = new Uri(fullPath);
                image.EndInit();
                image.Freeze(); // Freeze the image for cross-thread usage
                
                return image;
            }
            catch (Exception)
            {
                return null;
            }
        }
        
        /// <summary>
        /// Deletes an image and removes it from the database
        /// </summary>
        /// <param name="photo">The FishPhoto to delete</param>
        /// <returns>True if the deletion was successful</returns>
        public bool DeleteImage(FishPhoto photo)
        {
            try
            {
                // Delete the file if it exists
                string fullPath = photo.FullPath;
                if (File.Exists(fullPath))
                {
                    File.Delete(fullPath);
                }
                
                // Remove from database
                App.DatabaseManager.DeletePhoto(photo.Id);
                
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
        
        /// <summary>
        /// Resizes an image to fit within the specified dimensions while maintaining aspect ratio
        /// </summary>
        /// <param name="image">The BitmapImage to resize</param>
        /// <param name="maxWidth">Maximum width</param>
        /// <param name="maxHeight">Maximum height</param>
        /// <returns>A resized BitmapImage</returns>
        public BitmapImage ResizeImage(BitmapImage image, double maxWidth, double maxHeight)
        {
            double width = image.Width;
            double height = image.Height;
            
            double aspectRatio = width / height;
            
            if (width > maxWidth)
            {
                width = maxWidth;
                height = width / aspectRatio;
            }
            
            if (height > maxHeight)
            {
                height = maxHeight;
                width = height * aspectRatio;
            }
            
            // Create a new resized bitmap
            TransformedBitmap resizedBitmap = new TransformedBitmap(
                image,
                new System.Windows.Media.ScaleTransform(
                    width / image.Width,
                    height / image.Height));
            
            // Convert back to BitmapImage
            BitmapImage result = new BitmapImage();
            
            using (MemoryStream stream = new MemoryStream())
            {
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(resizedBitmap));
                encoder.Save(stream);
                
                stream.Position = 0;
                result.BeginInit();
                result.CacheOption = BitmapCacheOption.OnLoad;
                result.StreamSource = stream;
                result.EndInit();
                result.Freeze();
            }
            
            return result;
        }
    }
}
