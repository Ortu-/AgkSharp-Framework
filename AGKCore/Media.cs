using AgkSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace AGKCore
{
    public class Media
    {
        public static List<ImageAsset> ImageList = new List<ImageAsset>();
        public static List<ObjectAsset> ObjectList = new List<ObjectAsset>();
        public static List<ShaderAsset> ShaderList = new List<ShaderAsset>();
        public static List<ObjectBinding> ObjectBindings = new List<ObjectBinding>();
        public static List<MediaAsset> SoundList = new List<MediaAsset>();

        public Media()
        {
            App.Log("UserInterface.cs", 2, "main", "> Begin Init Media");

            Dispatcher.Add(Media.UpdateBoundObjects);
            App.UpdateList.Add(new UpdateHandler("Media.UpdateBoundObjects", null, false));
        }

        public static ImageAsset GetImageAsset(string rFilename, float rScaleX, float rScaleY)
        {
            App.Log("Media.cs", 2, "media", "Requested image: " + rFilename + " " + rScaleX.ToString() + " " + rScaleY.ToString());
  
            foreach (var i in Media.ImageList)
            {
                if(i.File == rFilename)
                {
                    if(i.ScaleX == rScaleX && i.ScaleY == rScaleY)
                    {
                        if (Agk.GetImageExists(i.ResourceNumber) == 1)
                        {
                            App.Log("Media.cs", 2, "media", " > found image on " + i.ResourceNumber.ToString());
                            return i;
                        }
                        else
                        {
                            App.Log("Media.cs", 2, "media", " > found image on " + i.ResourceNumber.ToString() + "but is not valid: reload it");
  
                            if (System.IO.File.Exists(rFilename))
                            {
                                i.ResourceNumber = Agk.LoadImageResized(rFilename, rScaleX, rScaleY, 0);
                                return i;
                            }
                        }
                    }
                }
            }

            if (System.IO.File.Exists(rFilename))
            {
                App.Log("Media.cs", 2, "media", " > image not loaded: load it");
  
                var tImg = Agk.LoadImageResized(rFilename, rScaleX, rScaleY, 0);
                var i = new ImageAsset(){
                    ResourceNumber = tImg,
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
                    App.Log("Media.cs", 5, "error", "ERROR: File not found: " + rFilename + " on Media.GetImageAsset");
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

            App.Log("Media.cs", 2, "media", " > made image from color");
  
            tImg = new ImageAsset()
            {
                ResourceNumber = tNum,
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
                Agk.DeleteImage(i.ResourceNumber);
            }
            Media.ImageList.Clear();
        }



        public static ObjectAsset LoadObjectWithChildrenAsset(string rFilename, bool rCanInstance, string rID)
        {
            App.Log("Media.cs", 2, "media", "Requested load object: " + rFilename + " " + rCanInstance.ToString() + " " + rID);

            if (System.IO.File.Exists(rFilename))
            {
                var tObj = new ObjectAsset();
                    tObj.File = rFilename;
                    tObj.Id = String.IsNullOrEmpty(rID) ? Guid.NewGuid().ToString() : rID;

                var sourceObject = Media.ObjectList.FirstOrDefault(o => o.File == rFilename && o.InstanceType == 0);
                if(sourceObject != null)
                {
                    if (Agk.IsObjectExists(sourceObject.ResourceNumber))
                    {
                        if (rCanInstance)
                        {
                            tObj.ResourceNumber = Agk.InstanceObject(sourceObject.ResourceNumber);
                            tObj.InstanceType = 2;
                        }
                        else
                        {
                            tObj.ResourceNumber = Agk.CloneObject(sourceObject.ResourceNumber);
                            tObj.InstanceType = 1;
                        }
                    }
                    else
                    {
                        Agk.LoadObjectWithChildren(sourceObject.ResourceNumber, rFilename);
                        sourceObject.Id = tObj.Id;
                        return sourceObject;
                    }
                }
                else
                {
                    tObj.ResourceNumber = Agk.LoadObjectWithChildren(rFilename);
                    tObj.InstanceType = 0;
                }

                Media.ObjectList.Add(tObj);
                return tObj;
            }
            else
            {
                App.Log("Media", 4, "error", "ERROR: File not found: " + rFilename + " during Media_LoadObjectWithChildren.");
                App.StopRunning(true);
                return null;
            }
        }

        public static void UnloadAllObjectAssets()
        {
            foreach (var i in Media.ObjectList)
            {
                //TODO: unload textures/shaders?
                Agk.DeleteObjectWithChildren(i.ResourceNumber);
            }
            Media.ObjectList.Clear();
        }

        public static void UpdateBoundObjects(object rArgs)
        {
            foreach(var objBinding in Media.ObjectBindings)
            {
                if (Data.GetBit((int)ObjectBinding.ModeBit.Location, objBinding.Mode) == 1)
                {
                    Agk.SetObjectPosition
                    (
                        objBinding.Object.ResourceNumber,
                        Agk.GetObjectWorldX(objBinding.Parent.ResourceNumber),
                        Agk.GetObjectWorldY(objBinding.Parent.ResourceNumber),
                        Agk.GetObjectWorldZ(objBinding.Parent.ResourceNumber)
                    );
                }
                if (Data.GetBit((int)ObjectBinding.ModeBit.Rotation, objBinding.Mode) == 1)
                {
                    Agk.SetObjectRotation
                    (
                        objBinding.Object.ResourceNumber,
                        Agk.GetObjectWorldAngleX(objBinding.Parent.ResourceNumber),
                        Agk.GetObjectWorldAngleY(objBinding.Parent.ResourceNumber),
                        Agk.GetObjectWorldAngleZ(objBinding.Parent.ResourceNumber)
                    );
                }
                if (Data.GetBit((int)ObjectBinding.ModeBit.Scale, objBinding.Mode) == 1)
                {

                }
                if (Data.GetBit((int)ObjectBinding.ModeBit.Collision, objBinding.Mode) == 1)
                {

                }
                if (Data.GetBit((int)ObjectBinding.ModeBit.Frame, objBinding.Mode) == 1)
                {
                    Agk.SetObjectAnimationFrame
                    (
                        objBinding.Object.ResourceNumber,
                        "",
                        Agk.GetObjectAnimationTime(objBinding.Parent.ResourceNumber),
                        0.0f
                    );
                }
                if (Data.GetBit((int)ObjectBinding.ModeBit.Light, objBinding.Mode) == 1)
                {

                }
            }
        }

        public static void UnbindAllObjects()
        {
            Media.ObjectBindings.Clear();
        }


        public static ShaderAsset GetShaderAsset(string rVsFile, string rPsFile, bool rCanInstance)
        {
            App.Log("Media.cs", 2, "media", "Requested shader: " + rVsFile + " " + rPsFile);

            if (rCanInstance)
            {
                foreach (var i in Media.ShaderList)
                {
                    if (i.VS == rVsFile && i.PS == rPsFile)
                    {
                        if (Agk.IsShaderExists(i.ResourceNumber))
                        {
                            App.Log("Media.cs", 2, "media", " > found shader on " + i.ResourceNumber.ToString());
                            return i;
                        }
                        else
                        {
                            App.Log("Media.cs", 2, "media", " > found shader on " + i.ResourceNumber.ToString() + "but is not valid: reload it");

                            if (System.IO.File.Exists(rVsFile) && System.IO.File.Exists(rPsFile))
                            {
                                i.ResourceNumber = Agk.LoadShader(rVsFile, rPsFile);
                                return i;
                            }
                        }
                    }
                }
            }

            if (System.IO.File.Exists(rVsFile) && System.IO.File.Exists(rPsFile))
            {
                App.Log("Media.cs", 2, "media", " > image not loaded: load it");

                var tShd = Agk.LoadShader(rVsFile, rPsFile);
                var i = new ShaderAsset()
                {
                    ResourceNumber = tShd,
                    VS = rVsFile,
                    PS = rPsFile
                };
                Media.ShaderList.Add(i);
                return i;
            }
            else
            {
                if (rVsFile.Contains("media") || rPsFile.Contains("media"))
                {
                    //if filename does not include the media folder, we are looking for a generated shader from string, just return null and keep running
                    //if filename does include media folder, the file is not found and we got problems.
                    App.Log("Media.cs", 5, "error", "ERROR: File not found: " + rVsFile + ", " + rPsFile + " on Media.GetShaderAsset");
                    App.StopRunning(true);
                }
            }

            return null;
        }

        public static void UnloadAllShaderAssets()
        {
            foreach (var i in Media.ShaderList)
            {
                //TODO: remove from objects?
                Agk.DeleteShader(i.ResourceNumber);
            }
            Media.ShaderList.Clear();
        }
    }


    public class MediaAsset
    {
        public uint ResourceNumber;
        public string File;
    }

    public class ImageAsset : MediaAsset
    {
        public float ScaleX;
        public float ScaleY;

        public void UnloadAsset()
        {
            Media.ImageList.Remove(this);
            Agk.DeleteImage(this.ResourceNumber);
        }
    }

    public class ObjectAsset : MediaAsset
    {
        public string Id;
        public uint InstanceType;
        public ObjectAsset Parent;
        public int BindMode;

        public void UnloadAsset()
        {
            //TODO: unload textures/shaders?
            Media.ObjectList.Remove(this);
            Agk.DeleteObjectWithChildren(this.ResourceNumber);
        }

        public void ReplaceAsset(string rFilename, bool rCanInstance)
        {
            //TODO: unload textures/shaders?
            Agk.DeleteObjectWithChildren(this.ResourceNumber);

            if (System.IO.File.Exists(rFilename))
            {
                this.File = rFilename;

                var sourceObject = Media.ObjectList.FirstOrDefault(o => o.File == rFilename && o.InstanceType == 0);
                if (sourceObject != null)
                {
                    if (Agk.IsObjectExists(sourceObject.ResourceNumber))
                    {
                        if (rCanInstance)
                        {
                            this.ResourceNumber = Agk.InstanceObject(sourceObject.ResourceNumber);
                            this.InstanceType = 2;
                        }
                        else
                        {
                            this.ResourceNumber = Agk.CloneObject(sourceObject.ResourceNumber);
                            this.InstanceType = 1;
                        }
                    }
                    else
                    {
                        Agk.LoadObjectWithChildren(sourceObject.ResourceNumber, rFilename);
                        sourceObject.Id = this.Id;
                    }
                }
                else
                {
                    this.ResourceNumber = Agk.LoadObjectWithChildren(rFilename);
                    this.InstanceType = 0;
                }
            }
            else
            {
                App.Log("Media", 4, "error", "ERROR: File not found: " + rFilename + " during Media_LoadObjectWithChildren.");
                App.StopRunning(true);
            }
        }

        public void BindToObject(ObjectAsset rParent, int rMode)
        {
            Media.ObjectBindings.Add(new ObjectBinding()
            {
                Object = this,
                Parent = rParent,
                Mode = rMode
            });
        }

        public void UnbindObject()
        {
            Media.ObjectBindings.Remove
            (
                Media.ObjectBindings.FirstOrDefault(o => o.Object == this)
            );
        }

    }

    public class ObjectBinding
    {
        public ObjectAsset Object;
        public ObjectAsset Parent;
        public int Mode;

        public enum ModeBit
        {
            Location,
            Rotation,
            Scale,
            Frame,
            Collision,
            Light
        }
    }

    public class ShaderAsset : MediaAsset
    {
        public string VS;
        public string PS;
        public bool ReceiveDirectionalLight = true;

        public void UnloadAsset()
        {
            //TODO: unset from any linked objects

            Media.ShaderList.Remove(this);
            Agk.DeleteShader(this.ResourceNumber);
        }
    }

}
