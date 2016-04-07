using System;
using System.Drawing;
using System.Drawing.Imaging;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace ImageTag
{
    public class ImageConverter : IValueConverter
    {
        public enum RatioType
        {
            NoResize,
            FitImage,
            FitWidth,
            FitHeight
        }

        public static double CalculateScale(int width, int height, int maxWidth, int maxHeight, RatioType type)
        {
            if (type == RatioType.NoResize)
                return 1;

            double ratio = ((double)width) / height; //original image ratio

            if (type == RatioType.FitImage)
            {
                if ((maxWidth / ratio) < maxHeight)
                    return ((double)maxWidth) / width; //fit height
                return ((double)maxHeight) / height; //fit width
            }
            if (type == RatioType.FitWidth)
                return ((double)maxWidth) / width;
            if (type == RatioType.FitHeight)
                return ((double)maxHeight) / height;

            return 1;
        }

        private static Bitmap BytesToThumbnailBitmap(byte[] bitmapBytes, int w, int h)
        {
            var newBitmap = new Bitmap(w, h);
            using (Graphics g = Graphics.FromImage(newBitmap))
            {
                try
                {
                    Bitmap bitmap;
                    using (var ms = new MemoryStream(bitmapBytes))
                        bitmap = new Bitmap(ms);

                    double scale = CalculateScale(bitmap.Size.Width, bitmap.Size.Height, newBitmap.Size.Width, newBitmap.Size.Height, RatioType.FitImage);
                    var size = new System.Drawing.Size((int)(bitmap.Size.Width * scale), (int)(bitmap.Size.Height * scale));
                    g.DrawImage(bitmap, new System.Drawing.Rectangle((w - size.Width) / 2, (h - size.Height) / 2, size.Width, size.Height));
                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex.ToString());
                }
            }
            return newBitmap;
        }

        public static BitmapImage ToBitmapImage(Bitmap bitmap)
        {
            using (var memory = new MemoryStream())
            {
                bitmap.Save(memory, ImageFormat.Png);
                memory.Position = 0;

                var bitmapImage = new BitmapImage();
                bitmapImage.BeginInit();
                bitmapImage.StreamSource = memory;
                bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                bitmapImage.EndInit();

                return bitmapImage;
            }
        }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return DependencyProperty.UnsetValue;

            if (string.IsNullOrEmpty(value as string))
                return DependencyProperty.UnsetValue;

            // KBR 20160406 : punt on letting BitmapImage load the images. Fails to handle DPI problem. This appears faster as well?
            try
            {
                // TODO verify file lock
                // TODO verify corrupt color profile - what image(s) ?
                byte[] imgBytes = File.ReadAllBytes(value as string);
                Bitmap tn = BytesToThumbnailBitmap(imgBytes, 200, 200);
                return ToBitmapImage(tn);
            }
            catch (Exception)
            {
                return DependencyProperty.UnsetValue;
            }

            /*
            try
            {
                var bi = new BitmapImage();
                bi.BeginInit();
                bi.DecodePixelWidth = 200;
                bi.DecodePixelHeight = 200;
                bi.CacheOption = BitmapCacheOption.OnLoad; // Don't lock the file
                bi.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                bi.UriSource = new Uri(value as string, UriKind.Absolute);
                bi.EndInit();
                return bi;
            }
            catch
            {
                try
                {
                    // Try to deal with some corrupt images
                    // See: http://www.hanselman.com/blog/DealingWithImagesWithBadMetadataCorruptedColorProfilesInWPF.aspx
                    var bi = new BitmapImage();
                    bi.BeginInit();
                    bi.DecodePixelWidth = 200;
                    bi.CreateOptions = BitmapCreateOptions.IgnoreColorProfile | BitmapCreateOptions.IgnoreImageCache;
                    bi.CacheOption = BitmapCacheOption.OnLoad; // Don't lock the file
                    bi.UriSource = new Uri(value as string);
                    bi.EndInit();
                    return bi;
                }
                catch
                {
                    return DependencyProperty.UnsetValue;
                }
            }
            */
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
