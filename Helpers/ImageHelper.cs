using System;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using Microsoft.Win32;
using VoltBettaManager.Models;
using VoltBettaManager.Services;

namespace VoltBettaManager.Helpers
{
    /// <summary>
    /// Helper class for working with images
    /// </summary>
    public static class ImageHelper
    {
        private static readonly ImageService _imageService = new ImageService();
        
        /// <summary>
        /// Opens a file dialog to select an image, and adds it to the application's image storage
        /// </summary>
        /// <param name="caption">Caption for the image</param>
        /// <param name="category">Category for the image</param>
        /// <param name="isTreatmentPhoto">Whether this is a treatment photo</param>
        /// <param name="treatmentPlanId">Associated treatment plan ID, if applicable</param>
        /// <returns>The newly created FishPhoto object, or null if canceled</returns>
        public static FishPhoto SelectAndSaveImage(string caption, string category, bool isTreatmentPhoto = false, int? treatmentPlanId = null)
        {
            var openFileDialog = new OpenFileDialog
            {
                Title = "Select an image",
                Filter = "Image files (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp",
                Multiselect = false
            };
            
            if (openFileDialog.ShowDialog() == true)
            {
                try
                {
                    return _imageService.SaveImage(
                        openFileDialog.FileName,
                        caption,
                        category,
                        isTreatmentPhoto,
                        treatmentPlanId);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Error saving image: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    return null;
                }
            }
            
            return null;
        }
        
        /// <summary>
        /// Loads an image from a FishPhoto object
        /// </summary>
        /// <param name="photo">The FishPhoto to load</param>
        /// <returns>A BitmapImage, or null if loading fails</returns>
        public static BitmapImage LoadImage(FishPhoto photo)
        {
            if (photo == null)
                return null;
                
            return _imageService.LoadImage(photo);
        }
        
        /// <summary>
        /// Loads an image from a FishPhoto object and resizes it
        /// </summary>
        /// <param name="photo">The FishPhoto to load</param>
        /// <param name="maxWidth">Maximum width</param>
        /// <param name="maxHeight">Maximum height</param>
        /// <returns>A resized BitmapImage, or null if loading fails</returns>
        public static BitmapImage LoadResizedImage(FishPhoto photo, double maxWidth, double maxHeight)
        {
            var image = LoadImage(photo);
            if (image == null)
                return null;
                
            return _imageService.ResizeImage(image, maxWidth, maxHeight);
        }
        
        /// <summary>
        /// Deletes an image
        /// </summary>
        /// <param name="photo">The FishPhoto to delete</param>
        /// <returns>True if deletion was successful</returns>
        public static bool DeleteImage(FishPhoto photo)
        {
            if (photo == null)
                return false;
                
            return _imageService.DeleteImage(photo);
        }
        
        /// <summary>
        /// Creates a thumbnail image for displaying in lists
        /// </summary>
        /// <param name="photo">The FishPhoto to create a thumbnail for</param>
        /// <returns>A thumbnail BitmapImage, or null if creation fails</returns>
        public static BitmapImage CreateThumbnail(FishPhoto photo)
        {
            return LoadResizedImage(photo, 200, 200);
        }
        
        /// <summary>
        /// Checks if a file is a valid image
        /// </summary>
        /// <param name="filePath">Path to the file</param>
        /// <returns>True if the file is a valid image</returns>
        public static bool IsValidImageFile(string filePath)
        {
            if (!File.Exists(filePath))
                return false;
                
            try
            {
                using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
                {
                    var decoder = BitmapDecoder.Create(
                        stream,
                        BitmapCreateOptions.None,
                        BitmapCacheOption.Default);
                        
                    return decoder.Frames.Count > 0;
                }
            }
            catch
            {
                return false;
            }
        }
    }
}
