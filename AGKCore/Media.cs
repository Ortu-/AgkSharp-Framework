using AgkSharp;
using System;
using System.Collections.Generic;

namespace AGKCore
{
    public static class Media
    {
        public static List<ImageAsset> ImageList = new List<ImageAsset>();
        public static List<MediaAsset> SoundList = new List<MediaAsset>();

        public static ImageAsset GetImageAsset(string rFilename, float rScaleX, float rScaleY)
        {
#if DEBUG
            App.Log("Media.cs", 2, "media", "Requested image: " + rFilename + " " + rScaleX.ToString() + " " + rScaleY.ToString());
#endif  
            foreach (var i in Media.ImageList)
            {
                if(i.File == rFilename)
                {
                    if(i.ScaleX == rScaleX && i.ScaleY == rScaleY)
                    {
                        if (Agk.GetImageExists(i.Number) == 1)
                        {
#if DEBUG
                            App.Log("Media.cs", 2, "media", " > found image on " + i.Number.ToString());
#endif  
                            return i;
                        }
                        else
                        {
#if DEBUG
                            App.Log("Media.cs", 2, "media", " > found image on " + i.Number.ToString() + "but is not valid: reload it");
#endif  
                            if (Agk.GetFileExists(rFilename) == 1)
                            {
                                i.Number = Agk.LoadImageResized(rFilename, rScaleX, rScaleY, 0);
                                return i;
                            }
                        }
                    }
                }
            }

            if (Agk.GetFileExists(rFilename) == 1)
            {
#if DEBUG
                App.Log("Media.cs", 2, "media", " > image not loaded: load it");
#endif  
                var tImg = Agk.LoadImageResized(rFilename, rScaleX, rScaleY, 0);
                var i = new ImageAsset(){
                    Number = tImg,
                    File = rFilename,
                    ScaleX = rScaleX,
                    ScaleY = rScaleY
                };
                Media.ImageList.Add(i);
                return i;
            }
            else
            {
                if (rFilename.Contains("media"))
                {
                    //if filename does not include the media folder, we are looking for a generated sprite color image, just return null and keep running
                    //if filename does include media folder, the file is not found and we got problems.
#if DEBUG
                    App.Log("Media.cs", 5, "error", "ERROR: File not found: " + rFilename + " on Media.GetImageAsset");
#endif  
                    App.StopRunning(true);
                }
            }

            return null;
        }

        public static ImageAsset MakeColorImage(uint rSizeX, uint rSizeY, uint rColor1, uint rColor2, uint rColor3, uint rColor4, int rFill)
        {
            string tName = rSizeX.ToString() + "," + rSizeY.ToString() + "," + rColor1.ToString() + "," + rColor2.ToString() + "," + rColor3.ToString() + "," + rColor4.ToString() + "," + rFill.ToString();

            var tImg = Media.GetImageAsset(tName, 1.0f, 1.0f);
            if (tImg != null)
            {
                return tImg;
            }

            Agk.Swap();
            Agk.DrawBox(0, 0, rSizeX, rSizeY, rColor1, rColor2, rColor3, rColor4, rFill);
            Agk.Render();
            var tNum = Agk.GetImage(0, 0, rSizeX, rSizeY);
            Agk.ClearScreen();
            Agk.Swap();
#if DEBUG
            App.Log("Media.cs", 2, "media", " > made image from color");
#endif  
            tImg = new ImageAsset()
            {
                Number = tNum,
                File = tName,
                ScaleX = 1.0f,
                ScaleY = 1.0f
            };
            Media.ImageList.Add(tImg);
            return tImg;
        }

        public static void UnloadAllImageAssets()
        {
            foreach(var i in Media.ImageList)
            {
                Agk.DeleteImage(i.Number);
            }
            Media.ImageList.Clear();
        }
        
    }

    public class MediaAsset
    {
        public uint Number;
        public string File;
    }

    public class ImageAsset : MediaAsset
    {
        public float ScaleX;
        public float ScaleY;

        public void UnloadAsset()
        {
            Media.ImageList.Remove(this);
            Agk.DeleteImage(this.Number);
        }
    }

}
