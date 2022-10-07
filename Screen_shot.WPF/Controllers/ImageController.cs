using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Security.Policy;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Media.Imaging;
using Screen_shot.WPF.Model;

namespace Screen_shot.WPF.Controllers
{
    internal class ImageController
    {

        /// <summary>
        /// отправляет  контент  в апи  
        /// </summary>
        /// <param name="bitmap"></param>
        /// <param name="uru"></param>
        public static void SendPicture(Bitmap bitmap, string uru , string _key )
        {
            if (bitmap == null)
                return;

            try
            {
                byte[] bData = ImageToByte(bitmap );
                var b = CompressBytesToStream( bData);
                
                WebRequest request = WebRequest.Create(uru + @"/api/ScreenRetrieval/PictureReception");
                request.Method = "POST";
                ImagePost imagePost = new ImagePost()
                { 
                    bytes = b  , key = _key , timeShet = DateTime.Now
                };
                string jsonString = JsonSerializer.Serialize(imagePost);
                byte[] byteArray = Encoding.UTF8.GetBytes(jsonString);
                request.ContentType = "application/json";
                request.ContentLength = byteArray.Length;

                using (Stream dataStream = request.GetRequestStream())
                {
                    dataStream.Write(byteArray, 0, byteArray.Length);
                }
                WebResponse response = request.GetResponse();
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка работы АПИ \n" + ex.Message);
            }
        }

        private static byte [] Decompress(byte[] bytes)
        {
            return bytes; 
        }


        private static byte [] CompressBytesToStream( byte[] bData)
        {

            using (var stream =  new MemoryStream())
            {

                using (GZipStream compressionStream = new GZipStream(stream, CompressionMode.Compress, true))
                {
                    compressionStream.Write(bData, 0, bData.Length);
                    stream.CopyTo(compressionStream);
                    var buf = new byte[stream.Length];
                    stream.Read(buf, 0, buf.Length);
                    return buf;
                }
            }
        }

        public static void StopStram (string uru, string _key)
        {
            WebResponse response = null;
            try
            {
                WebRequest request = WebRequest.Create(uru + $@"/api/ScreenRetrieval/StopStream?key={_key}");
                request.Method = "GET";
                request.ContentType = "application/json";
                response = request.GetResponse();
                response.Close();
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка при обращению  к  АПИ \n" + ex.Message);
            }
            finally
            {
                if (response != null)
                    response.Close();
            }
        }

        public static bool IsChekedKey(string uru, string _key)
        {
            WebResponse response = null;
            try
            {
                WebRequest request = WebRequest.Create(uru + $@"/api/ScreenRetrieval/IsChekedKey?key={_key}");
                request.Method = "GET";
                request.ContentType = "application/json";
                response = request.GetResponse();

                using StreamReader stream = new StreamReader(response.GetResponseStream());
                var content = stream.ReadToEnd();


                return GetBoll(content)  ;
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка при получении  контента  из АПИ \n" + ex.Message);
            }
            finally
            {
                if (response != null)
                    response.Close();
            }
        }

        private static bool GetBoll(string content)
        {
            if (content == "false")
                return false;
            
            if (content == "true") 
                return true;

            return false;
        }

        public static   Bitmap GetScreenShot(int widht, int height, System.Drawing.Point pLT , DpiScale  dpi )
        {

            if (widht <= 0 || height <= 0) // todo  подумать  над мониторами 
                return null;

            Bitmap printscreen = new Bitmap(widht, height);
            using Graphics graphics = Graphics.FromImage(printscreen as Image);
            graphics.CopyFromScreen(pLT, new System.Drawing.Point(0, 0), printscreen.Size);
               printscreen.SetResolution( (float)dpi.PixelsPerInchX,(float) dpi.PixelsPerInchY);// todo  подумать   над Dpi 

            return printscreen;
        }

        public static  BitmapImage SetImage(string uri  , string _key )
        {
            WebResponse response = null;
            try
            {
                WebRequest request = WebRequest.Create(uri + $@"/api/ScreenRetrieval/GetPicture?key={_key}");
                request.Method = "GET";
                request.ContentType = "application/json";
                response = request.GetResponse();

                using StreamReader stream = new StreamReader(response.GetResponseStream());
                var content = stream.ReadToEnd();
                ImagePost bitmapImage = JsonSerializer.Deserialize<ImagePost>(content);
                if (bitmapImage.bytes != null)
                {
                   var byt =  Decompress( bitmapImage.bytes);
                   return ToImage(byt);
                }
                else
                    return null;
            }
            catch (Exception ex)
            {
                throw new Exception("Ошибка при получении  контента  из АПИ \n" + ex.Message);
            }
            finally
            {  
                if (response != null)
                response.Close();
            }
        }

      

        private static BitmapImage ToImage(byte[] array)
        {
            using (var ms = new System.IO.MemoryStream(array))
            {
                var image = new BitmapImage();
                image.BeginInit();
                image.CacheOption = BitmapCacheOption.OnLoad; // here
                image.StreamSource = ms;
                image.EndInit();
                return image;
            }
        }
        private static byte[] ImageToByte(Image img)
        {
            ImageConverter converter = new ImageConverter();
            return (byte[])converter.ConvertTo(img, typeof(byte[]));
        }
    }
}
