using AgkSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Forms;

namespace AGKCore.UI
{
    public class UserInterface
    {
        public static UI.StatusData Status = new StatusData();
        public static UI.ElementDragData ElementDrag = new ElementDragData();
        public static List<UI.TransitionData> TransitionList = new List<TransitionData>();
        public static List<UI.Element> ElementList = new List<Element>();
        public static List<UI.StyleClass> StyleClassList = new List<StyleClass>();
        public static List<UI.Controller> ControllerList = new List<UI.Controller>();

        public UserInterface()
        {

            App.Log("UserInterface.cs", 2, "main", "> Begin Init UI");

            Dispatcher.Add(UserInterface.UpdatePageFlow);
            App.UpdateList.Add(new UpdateHandler("UserInterface.UpdatePageFlow", null, true));

            Dispatcher.Add(UserInterface.GetInterfaceInput);
            App.UpdateList.Add(new UpdateHandler("UserInterface.GetInterfaceInput", "UserInterface.UpdatePageFlow", true));

            Dispatcher.Add(UserInterface.UpdateTransitions);
            App.UpdateList.Add(new UpdateHandler("UserInterface.UpdateTransitions", null, false));

            //set up root element
            UI.Element tElement = new UI.Element();
            tElement.Id = "root";
            tElement.Style.SetProp("width", App.Config.Screen.Width.ToString() + "px");
            tElement.Style.SetProp("height", App.Config.Screen.Height.ToString() + "px");
            ElementList.Add(tElement);

            Status.MouseMode = "gameplay";
            Status.KeyMode = "gameplay";

            App.Log("UserInterface.cs", 2, "main", "> End Init UI");

        }

        public static void LoadPartial(string rFile)
        {
            string doc = System.IO.File.ReadAllText(rFile);
            var view = JsonConvert.DeserializeObject<List<dynamic>>(doc);
            foreach (var component in view)
            {
                if(component.type == "styleClass")
                {
                    StyleClass tStyleClass = new StyleClass();
                    tStyleClass.ClassName = Data.HasOwnProperty(component, "name") ? component.name : "";
                    if (Data.HasOwnProperty(component, "style"))
                    {
                        foreach (var prop in ((JObject)component.style).Properties())
                        {
                            tStyleClass.Style.SetProp(prop.Name, prop.Value.ToString());
                        }
                    }
                    StyleClassList.Add(tStyleClass);
                }
                if (component.type == "element")
                {
                    LoadElement(component, null);
                }
            }
        }

        public static void LoadElement(dynamic component, string rParentId)
        {
            Element tElement = new Element();
            tElement.Id = Data.HasOwnProperty(component, "id") ? component.id : "";
            tElement.Name = Data.HasOwnProperty(component, "name") ? component.name : "";
            tElement.Tag = Data.HasOwnProperty(component, "tag") ? component.tag : "";
            tElement.Value = Data.HasOwnProperty(component, "value") ? component.value : "";
            tElement.OnPress = Data.HasOwnProperty(component, "onPress") ? component.onPress : "";
            tElement.OnRelease = Data.HasOwnProperty(component, "onRelease") ? component.onRelease : "";
            tElement.OnMouseIn = Data.HasOwnProperty(component, "onMouseIn") ? component.onMouseIn : "";
            tElement.OnMouseOut = Data.HasOwnProperty(component, "onMouseOut") ? component.onMouseOut : "";
            tElement.KeyBind = Data.HasOwnProperty(component, "keyBind") ? Convert.ToInt32(component.keyBind) : 0;
            tElement.EnableEvents = Data.HasOwnProperty(component, "enableEvents") ? Convert.ToUInt32(component.enableEvents) : 0u;
            tElement.HoldPause = Data.HasOwnProperty(component, "holdPause") ? Convert.ToBoolean(component.holdPause) : false;
            tElement.HoldMouseFocus = Data.HasOwnProperty(component, "holdMouseFocus") ? Convert.ToBoolean(component.holdMouseFocus) : false;
            tElement.HoldKeyFocus = Data.HasOwnProperty(component, "holdKeyFocus") ? Convert.ToBoolean(component.holdKeyFocus) : false;

            if (String.IsNullOrEmpty(rParentId))
            {
                tElement.SetParent("root");
            }
            else
            {
                tElement.SetParent(rParentId);
            }

            if (Data.HasOwnProperty(component, "styleClass"))
            {
                var classList = component.styleClass.ToString().Split(' ');
                foreach (var c in classList)
                {
                    tElement.AddStyleClass(c);
                }
            }

            if (Data.HasOwnProperty(component, "style"))
            {
                foreach (var prop in ((JObject)component.style).Properties())
                {
                    tElement.Style.SetProp(prop.Name, prop.Value.ToString());
                }
            }

            ElementList.Add(tElement);

            if (Data.HasOwnProperty(component, "children"))
            {
                foreach(var child in component.children)
                LoadElement(child, tElement.Id);
            }
        }

        public static void PreloadImages()
        {
            foreach(var i in StyleClassList)
            {
                if (!String.IsNullOrEmpty(i.Style.BackgroundImage))
                {
                    Media.GetImageAsset(i.Style.BackgroundImage, 1.0f, 1.0f);
                }
            }
            foreach (var i in ElementList)
            {
                if (!String.IsNullOrEmpty(i.Style.BackgroundImage))
                {
                    Media.GetImageAsset(i.Style.BackgroundImage, 1.0f, 1.0f);
                }
            }
        }

        public static void ResetResolvedStyleProps()
        {

            App.Log("UserInterface.cs", 1, "ui", "> Begin ResetResolvedStyleProps");

            ElementList[0].IsDirty = false; //skip root element

            if (UserInterface.ElementDrag.DragElement != null)
            {
                UserInterface.ElementDrag.DragElement.IsDirty = true; //always resolve dragging element
            }

            foreach (var e in ElementList)
            {
                //skip root
                if (e == ElementList[0])
                {
                    continue;
                }

                //if parent is dirty, child also needs to be resolved
                if (e.Parent.IsDirty)
                {
                    e.IsDirty = true;
                }
                if (e.IsDirty)
                {
                    e.ResolvedStyle = new StylePropertyData(e);
                }
            }

            //root element is always = app window dimensions and provides context for all child %
            ElementList[0].ResolvedStyle.ResolveAsScreen();

            App.Log("UserInterface.cs", 1, "ui", "> End ResetResolvedStyleProps");

        }

        public static void UpdatePageFlow(object rArgs)
        {

            App.Log("UserInterface.cs", 1, "ui", "> Begin UpdatePageFlow");

            int dirtyCount = 0;
            if(ElementList.Count == 0)
            {
                return;
            }

            //TODO: updateTransitions()
            UserInterface.ResetResolvedStyleProps();

            //----- resolve styling and page flow ----------------------

            bool keepResolvingDeferred = true;
            while (keepResolvingDeferred)
            {
                keepResolvingDeferred = false;

                for (uint iElementIndex = 0; iElementIndex < ElementList.Count; iElementIndex++)
                {
                    //skip root
                    if (ElementList[(int)iElementIndex] == ElementList[0])
                    {
                        continue; 
                    }

                    bool resolveThis = true;

                    //skip if already resolved
                    if (ElementList[(int)iElementIndex].ResolvedStyle._IsResolved)
                    {
                        resolveThis = false; 
                    }

                    //defer if parent is not yet resolved
                    if(resolveThis && !ElementList[(int)iElementIndex].Parent.ResolvedStyle._IsResolved)
                    {
                        resolveThis = false;
                        keepResolvingDeferred = true;
                    }

                    if (resolveThis)
                    {
                        ElementList[(int)iElementIndex].ResolvedStyle.ApplyInheritedStyleProps();
                        foreach(var iStyleClass in ElementList[(int)iElementIndex].StyleClassList)
                        {
                            ElementList[(int)iElementIndex].ResolvedStyle.ApplyStyleProps(iStyleClass.Style);
                        }
                        ElementList[(int)iElementIndex].ResolvedStyle.ApplyStyleProps(ElementList[(int)iElementIndex].Style);
                        ElementList[(int)iElementIndex].ResolvedStyle.ResolveFlowValues();

                        App.Log("UserInterface.cs", 1, "ui", "  resolved " + ElementList[(int)iElementIndex].Id + " " + ElementList[(int)iElementIndex].ResolvedStyle._FinalX + "," + ElementList[(int)iElementIndex].ResolvedStyle._FinalY + " : " + ElementList[(int)iElementIndex].ResolvedStyle._FinalW + "," + ElementList[(int)iElementIndex].ResolvedStyle._FinalH);

                    }
                }
            }

            //----- render UI ----------------------

            for (uint iElementIndex = 0; iElementIndex < ElementList.Count; iElementIndex++)
            {
                //skip root
                if (iElementIndex == 0)
                {
                    continue;
                }

                if(ElementList[(int)iElementIndex].ResolvedStyle.Display == "visible" && ElementList[(int)iElementIndex].ResolvedStyle.Opacity > 0)
                {
                    if(ElementList[(int)iElementIndex].IsDirty || !String.IsNullOrEmpty(ElementList[(int)iElementIndex].Value))
                    {
                        //get flow data
                        int borderSizeT = Convert.ToInt32(ElementList[(int)iElementIndex].ResolvedStyle.BorderTop.Size);
                        int borderSizeB = Convert.ToInt32(ElementList[(int)iElementIndex].ResolvedStyle.BorderBottom.Size);
                        int borderSizeL = Convert.ToInt32(ElementList[(int)iElementIndex].ResolvedStyle.BorderLeft.Size);
                        int borderSizeR = Convert.ToInt32(ElementList[(int)iElementIndex].ResolvedStyle.BorderRight.Size);
                        int finalX = ElementList[(int)iElementIndex].ResolvedStyle._FinalX;
                        int finalY = ElementList[(int)iElementIndex].ResolvedStyle._FinalY;
                        int finalW = ElementList[(int)iElementIndex].ResolvedStyle._FinalW;
                        int finalH = ElementList[(int)iElementIndex].ResolvedStyle._FinalH;
                        int contentX = ElementList[(int)iElementIndex].ResolvedStyle._InnerX;
                        int contentY = ElementList[(int)iElementIndex].ResolvedStyle._InnerY;
                        int contentW = ElementList[(int)iElementIndex].ResolvedStyle._InnerW;
                        int contentH = ElementList[(int)iElementIndex].ResolvedStyle._InnerH;

                        //draw element background
                        if(ElementList[(int)iElementIndex].ResolvedStyle.BackgroundOpacity > 0)
                        {
                            //TODO: handle overflow and repeat
                            if (String.IsNullOrEmpty(ElementList[(int)iElementIndex].ResolvedStyle.BackgroundImage))
                            {

                                App.Log("UserInterface.cs", 1, "ui", "  no backgroundImage, make a color image");

                                //no image, make color
                                var tColor = Media.MakeColorImage((uint)finalW, (uint)finalH, ElementList[(int)iElementIndex].ResolvedStyle.BackgroundColor, ElementList[(int)iElementIndex].ResolvedStyle.BackgroundColor, ElementList[(int)iElementIndex].ResolvedStyle.BackgroundColor, ElementList[(int)iElementIndex].ResolvedStyle.BackgroundColor, 1);
                                ElementList[(int)iElementIndex].Style.SetProp("background-image", tColor.File);
                                ElementList[(int)iElementIndex].ResolvedStyle.SetProp("background-image", tColor.File);
                            }
                            //check for animated frames
                            ImageAsset tImg;
                            if (ElementList[(int)iElementIndex].ResolvedStyle.BackgroundImage.Contains("|"))
                            {
                                var imgList = ElementList[(int)iElementIndex].ResolvedStyle.BackgroundImage.Split('|');
                                tImg = Media.GetImageAsset(imgList[0], 1.0f, 1.0f);
                                Agk.CreateSprite(iElementIndex, tImg.Number);
                                for(int i = 1; i < imgList.Count(); i++)
                                {
                                    var frame = Media.GetImageAsset(imgList[i], 1.0f, 1.0f);
                                    Agk.AddSpriteAnimationFrame(iElementIndex, frame.Number);
                                }
                                Agk.PlaySprite(iElementIndex, 10.0f, 1, 1, imgList.Count()); //TODO: add style prop to control speed
                                ElementList[(int)iElementIndex].Style.SetProp("background-image", imgList[0]);
                                ElementList[(int)iElementIndex].ResolvedStyle.SetProp("background-image", imgList[0]);
                            }
                            else
                            {
                                tImg = Media.GetImageAsset(ElementList[(int)iElementIndex].ResolvedStyle.BackgroundImage, 1.0f, 1.0f);
                                if (!Agk.IsSpriteExists(iElementIndex))
                                {

                                    App.Log("UserInterface.cs", 1, "ui", $"  no sprite for background. make a sprite {iElementIndex.ToString()}");

                                    Agk.CreateSprite(iElementIndex, tImg.Number);
                                }
                                Agk.SetSpriteImage(iElementIndex, tImg.Number);
                            }

                            //Agk.SetSpriteImage(iElementIndex, tImg.Number);
                            Agk.SetSpriteScale(iElementIndex, contentW / Agk.GetImageWidth(tImg.Number), contentH / Agk.GetImageHeight(tImg.Number));
                            Agk.SetSpritePosition(iElementIndex, contentX, contentY);
                            Agk.SetSpriteColorAlpha(iElementIndex, (int)(ElementList[(int)iElementIndex].ResolvedStyle.BackgroundOpacity * 0.01 * 255));
                            Agk.SetSpriteDepth(iElementIndex, (int)ElementList[(int)iElementIndex].ResolvedStyle.ZIndex);
                            Agk.SetSpriteAngle(iElementIndex, ElementList[(int)iElementIndex].ResolvedStyle.Rotation);
                        }

                        //draw borders
                        //TODO: convert to sprite, apply backgroundOpacity
                        if(borderSizeT > 0)
                        {
                            Agk.DrawBox(finalX, finalY, (finalX + finalW), (finalY + borderSizeT), ElementList[(int)iElementIndex].ResolvedStyle.BorderTop.Color, ElementList[(int)iElementIndex].ResolvedStyle.BorderTop.Color, ElementList[(int)iElementIndex].ResolvedStyle.BorderTop.Color, ElementList[(int)iElementIndex].ResolvedStyle.BorderTop.Color, 1);
                        }
                        if (borderSizeB > 0)
                        {
                            Agk.DrawBox(finalX, (finalY + finalH - borderSizeB), (finalX + finalW), (finalY + finalH), ElementList[(int)iElementIndex].ResolvedStyle.BorderBottom.Color, ElementList[(int)iElementIndex].ResolvedStyle.BorderBottom.Color, ElementList[(int)iElementIndex].ResolvedStyle.BorderBottom.Color, ElementList[(int)iElementIndex].ResolvedStyle.BorderBottom.Color, 1);
                        }
                        if (borderSizeR > 0)
                        {
                            Agk.DrawBox((finalX + finalW - borderSizeR), (finalY + borderSizeT), (finalX + finalW), (finalY + finalH - borderSizeB), ElementList[(int)iElementIndex].ResolvedStyle.BorderRight.Color, ElementList[(int)iElementIndex].ResolvedStyle.BorderRight.Color, ElementList[(int)iElementIndex].ResolvedStyle.BorderRight.Color, ElementList[(int)iElementIndex].ResolvedStyle.BorderRight.Color, 1);
                        }
                        if (borderSizeL > 0)
                        {
                            Agk.DrawBox(finalX, (finalY + borderSizeT), (finalX + borderSizeL), (finalY + finalH - borderSizeB), ElementList[(int)iElementIndex].ResolvedStyle.BorderLeft.Color, ElementList[(int)iElementIndex].ResolvedStyle.BorderLeft.Color, ElementList[(int)iElementIndex].ResolvedStyle.BorderLeft.Color, ElementList[(int)iElementIndex].ResolvedStyle.BorderLeft.Color, 1);
                        }

                        //draw text
                        if (!String.IsNullOrEmpty(ElementList[(int)iElementIndex].Value))
                        {
                            if (!Agk.IsTextExists(iElementIndex))
                            {

                                App.Log("UserInterface.cs", 1, "ui", $"  no text for element {iElementIndex.ToString()}. make a text");

                                Agk.CreateText(iElementIndex, ElementList[(int)iElementIndex].Value);
                            }
                            switch(ElementList[(int)iElementIndex].ResolvedStyle.TextTransform.ToLower())
                            {
                                case "lower":
                                    Agk.SetTextString(iElementIndex, ElementList[(int)iElementIndex].Value.ToLower());
                                    break;
                                case "upper":
                                    Agk.SetTextString(iElementIndex, ElementList[(int)iElementIndex].Value.ToUpper());
                                    break;
                                case "capitalize":
                                    Agk.SetTextString(iElementIndex, ElementList[(int)iElementIndex].Value[0].ToString().ToUpper() + ElementList[(int)iElementIndex].Value.Substring(1));
                                    break;
                                default:
                                    Agk.SetTextString(iElementIndex, ElementList[(int)iElementIndex].Value);
                                    break;
                            }
                            Agk.SetTextMaxWidth(iElementIndex, contentW);
                            Agk.SetTextSize(iElementIndex, ElementList[(int)iElementIndex].ResolvedStyle.FontSize);
                            Agk.SetTextColor(iElementIndex, Agk.GetColorRed(ElementList[(int)iElementIndex].ResolvedStyle.Color), Agk.GetColorGreen(ElementList[(int)iElementIndex].ResolvedStyle.Color), Agk.GetColorBlue(ElementList[(int)iElementIndex].ResolvedStyle.Color), (uint)(ElementList[(int)iElementIndex].ResolvedStyle.Opacity * 0.01 * 255));
                            Agk.SetTextDepth(iElementIndex, (int)(ElementList[(int)iElementIndex].ResolvedStyle.ZIndex - 1));
                            if (ElementList[(int)iElementIndex].ResolvedStyle.TextDecoration.ToLower().Contains("bold"))
                            {
                                Agk.SetTextBold(iElementIndex, 1);
                            }
                            else
                            {
                                Agk.SetTextBold(iElementIndex, 0);
                            }

                            int textPosH = 0;
                            int textPosV = 0;
                            switch (ElementList[(int)iElementIndex].ResolvedStyle.TextAlignH)
                            {
                                case "right":
                                    Agk.SetTextAlignment(iElementIndex, 2);
                                    textPosH = contentX + contentW;
                                    break;
                                case "center":
                                    Agk.SetTextAlignment(iElementIndex, 1);
                                    textPosH = contentX + (int)(contentW * 0.5);
                                    break;
                                default: //left
                                    Agk.SetTextAlignment(iElementIndex, 0);
                                    textPosH = contentX;
                                    break;
                            }
                            switch (ElementList[(int)iElementIndex].ResolvedStyle.TextAlignV)
                            {
                                case "bottom":
                                    textPosV = contentY + contentH - (int)(Agk.GetTextTotalHeight(iElementIndex));
                                    break;
                                case "center":
                                    textPosV = contentY + (int)(contentH * 0.5) - (int)(Agk.GetTextTotalHeight(iElementIndex) * 0.5);
                                    break;
                                default: //top
                                    textPosV = contentY;
                                    break;
                            }
                            Agk.SetTextPosition(iElementIndex, textPosH, textPosV);
                            Agk.SetTextAngle(iElementIndex, ElementList[(int)iElementIndex].ResolvedStyle.Rotation);
                        }
                    }
                }

                if (ElementList[(int)iElementIndex].IsDirty)
                {
                    ElementList[(int)iElementIndex].IsDirty = false;
                    ++dirtyCount;
                }
            }

            App.Log("UserInterface.cs", 1, "ui", "> End UpdatePageFlow");

        }

        public static void GetInterfaceInput(object rArgs)
        {
            if (Math.Abs(App.Timing.Timer - Status.InputMark) > 200)
            {
                Status.InputReady = true;
            }
            else
            {
                Status.InputReady = false;
            }
            
            Data.SetBit((int)TimingStatus.PauseType.UI, App.Timing.PauseHold, 0);

            for (int iElementIndex = 0; iElementIndex < ElementList.Count; iElementIndex++)
            {
                var iElement = ElementList[iElementIndex];

                //skip root
                if (iElementIndex == 0 || iElement.ResolvedStyle.Display == "hidden")
                {
                    continue;
                }
                if (iElement.HoldPause)
                {
                    Data.SetBit((int)TimingStatus.PauseType.UI, App.Timing.PauseHold, 1);
                }
                if (iElement.HoldMouseFocus)
                {
                    Status.MouseMode = "ui";
                }
                if (iElement.HoldKeyFocus)
                {
                    Status.KeyMode = "ui";
                }
                if (Status.MouseMode == "ui" && Status.KeyMode == "ui")
                {
                    if (Data.GetBit((int)TimingStatus.PauseType.UI, App.Timing.PauseHold) == 1)
                    {
                        break;
                    }
                }
            }

            if (Status.MouseMode == "ui")
            {
                Agk.SetRawMouseVisible(1);
            }
            else
            {
                Agk.SetRawMouseVisible(0);
            }

            if (!Status.InputReady)
            {
                return;
            }

            for (uint iElementIndex = 0; iElementIndex < ElementList.Count; iElementIndex++)
            {
                var iElement = ElementList[(int)iElementIndex];

                //skip root
                if (iElementIndex == 0)
                {
                    continue;
                }

                //mouse events
                if (Status.MouseMode == "ui" && iElement.ResolvedStyle.Display != "hidden")
                {
                    if (iElement.EnableEvents == 1 || iElement.EnableEvents == 3 || (iElement.EnableEvents == 4 && ElementDrag.IsActive))
                    {
                        var oldPressHold = iElement.PressIsHeld;
                        var oldMouseOver = iElement.MouseIsOver;
                        iElement.PressIsHeld = false;
                        iElement.MouseIsOver = false;

                        //if this element is the actively dragging element, enforce mouseOver
                        if (ElementDrag.DragElement == iElement)
                        {
                            oldMouseOver = true;
                            iElement.MouseIsOver = true;
                        }

                        //detect mouse over
                        var x1 = iElement.ResolvedStyle._InnerX;
                        var y1 = iElement.ResolvedStyle._InnerY;
                        var x2 = x1 + iElement.ResolvedStyle._InnerW;
                        var y2 = y1 + iElement.ResolvedStyle._InnerH;

                        bool mouseInBounds;
                        if(iElement.ResolvedStyle.BackgroundOpacity == 0)
                        {
                            mouseInBounds = Agk.IsTextHitTest(iElementIndex, Agk.ScreenToWorldX(Hardware.Mouse.PosX), Agk.ScreenToWorldY(Hardware.Mouse.PosY));
                        }
                        else
                        {
                            mouseInBounds = Agk.IsSpriteHitTest(iElementIndex, Agk.ScreenToWorldX(Hardware.Mouse.PosX), Agk.ScreenToWorldY(Hardware.Mouse.PosY));
                        }

                        if (mouseInBounds)
                        {
                            iElement.MouseIsOver = true;
                            if (!oldMouseOver)
                            {
                                //new mouseIn
                                if (!String.IsNullOrEmpty(iElement.OnMouseIn))
                                {
                                    Dispatcher.Invoke(iElement.OnMouseIn, iElementIndex.ToString());
                                    return;
                                }
                            }
                            //handle press
                            if (Data.GetBit(1, Hardware.Input[Hardware.MouseEnum((int)MouseButtons.Left)]) == 1)
                            {
                                iElement.PressIsHeld = true;
                                if (!oldPressHold)
                                {
                                    //new press
                                    Status.InputMark = App.Timing.Timer;
                                    if (!String.IsNullOrEmpty(iElement.OnPress))
                                    {
                                        Dispatcher.Invoke(iElement.OnPress, iElementIndex.ToString());
                                        return;
                                    }
                                }
                            }
                            else
                            {
                                if (oldPressHold)
                                {
                                    //new release
                                    if (!String.IsNullOrEmpty(iElement.OnRelease))
                                    {
                                        Dispatcher.Invoke(iElement.OnRelease, iElementIndex.ToString());
                                        return;
                                    }
                                }
                            }
                        }

                        //detect mouse out
                        if (oldMouseOver && !iElement.MouseIsOver)
                        {
                            if (!String.IsNullOrEmpty(iElement.OnMouseOut))
                            {
                                Dispatcher.Invoke(iElement.OnMouseOut, iElementIndex.ToString());
                                return;
                            }
                        }
                    }
                }
                else
                {
                    //element is hidden or bound to gameplay, if mouse was over, trigger mouseOut
                    if (iElement.MouseIsOver)
                    {
                        iElement.MouseIsOver = false;
                        iElement.PressIsHeld = false;
                        if (!String.IsNullOrEmpty(iElement.OnMouseOut))
                        {
                            Dispatcher.Invoke(iElement.OnMouseOut, iElementIndex.ToString());
                            return;
                        }
                    }
                }

                //key events
                if(Status.KeyMode == "ui")
                {
                    //TODO:
                    //this handles things like entering text into a chatbox where keys should not trigger regular ui events or character control
                    //this would include such events as enter key to finalize an input, escape to cancel an input etc
                    //alphanumerics go to entry buffer, and buffer content will be applied to the active element's value.				
                }
                else
                {
                    //check keybinds
                    if (iElement.EnableEvents == 2 || iElement.EnableEvents == 3)
                    {
                        
                        bool oldPressHold = iElement.PressIsHeld;
                        iElement.PressIsHeld = false;
                        if(iElement.KeyBind > 0)
                        {
                            if(Data.GetBit(1, Hardware.Input[iElement.KeyBind]) == 1)
                            {
                                iElement.PressIsHeld = true;
                                if (!oldPressHold)
                                {
                                    //new press
                                    Status.InputMark = App.Timing.Timer;
                                    if (!String.IsNullOrEmpty(iElement.OnPress))
                                    {
                                        Dispatcher.Invoke(iElement.OnPress, iElementIndex.ToString());
                                        return;
                                    }
                                }
                                else
                                {
                                    if (oldPressHold)
                                    {
                                        //new release
                                        if (!String.IsNullOrEmpty(iElement.OnRelease))
                                        {
                                            Dispatcher.Invoke(iElement.OnRelease, iElementIndex.ToString());
                                            return;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public static void UpdateTransitions(object rArgs)
        {
            if(TransitionList.Count == 0)
            {
                return;
            }

            float tElapsed;
            float tInit;
            float tTarget;
            int tVal;

            for(int i = TransitionList.Count - 1; i >= 0; --i)
            {
                var tran = TransitionList[i];
                tElapsed = App.Timing.Timer - tran.Start * 1.0f;

                if (tran.InitialValue.Contains("px"))
                {
                    tInit = Convert.ToSingle(tran.InitialValue.Substring(0, tran.InitialValue.Length - 2));
                }
                else
                {
                    if (tran.InitialValue.Contains("%"))
                    {
                        tInit = Convert.ToSingle(tran.InitialValue.Substring(0, tran.InitialValue.Length - 1));
                    }
                    else
                    {
                        tInit = Convert.ToSingle(tran.InitialValue);
                    }
                }

                if (tran.TargetValue.Contains("px"))
                {
                    tTarget = Convert.ToSingle(tran.TargetValue.Substring(0, tran.TargetValue.Length - 2));
                }
                else
                {
                    if (tran.TargetValue.Contains("%"))
                    {
                        tTarget = Convert.ToSingle(tran.TargetValue.Substring(0, tran.TargetValue.Length - 1));
                    }
                    else
                    {
                        tTarget = Convert.ToSingle(tran.TargetValue);
                    }
                }

                if (tElapsed < tran.Duration)
                {
                    tVal = (int)Math.Floor(tInit + (((tTarget - tInit) / tran.Duration) * tElapsed));
                    tran.Element.Style.SetProp(tran.Property, tVal.ToString());
                    if (tTarget > tInit)
                    {
                        if(tVal >= tTarget)
                        {
                            if (!String.IsNullOrEmpty(tran.Callback))
                            {
                                Dispatcher.Invoke(tran.Callback, tran.Element.Id);
                            }
                            TransitionList.RemoveAt(i);
                        }
                    }
                    else
                    {
                        if(tVal <= tTarget)
                        {
                            if (!String.IsNullOrEmpty(tran.Callback))
                            {
                                Dispatcher.Invoke(tran.Callback, tran.Element.Id);
                            }
                            TransitionList.RemoveAt(i);
                        }
                    }
                }
                else
                {
                    tran.Element.Style.SetProp(tran.Property, tTarget.ToString());
                    if (!String.IsNullOrEmpty(tran.Callback))
                    {
                        Dispatcher.Invoke(tran.Callback, tran.Element.Id);
                    }
                    TransitionList.RemoveAt(i);
                }
            }
        }

        public static void FadeScreen(int r, int g, int b, int a, int t)
        {
            var tSprite = Agk.CreateSprite(0);
            Agk.SetSpriteDepth(tSprite, 0);
            Agk.SetSpriteTransparency(tSprite, 1);
            Agk.SetSpriteSize(tSprite, App.Config.Screen.Width, App.Config.Screen.Height);
            float fadeLoading;
            var tMark = Agk.GetMilliseconds();
            if(a > 0)
            {
                Agk.SetSpriteColor(tSprite, r, g, b, 0);
                fadeLoading = 0.0f;
                while(fadeLoading < 255.0f)
                {
                    fadeLoading = 0.0f + ((255.0f / t) * Agk.Abs(Agk.GetMilliseconds() - tMark));
                    Agk.SetSpriteColorAlpha(tSprite, (int)fadeLoading);
                    Agk.Sync();
                }
            }
            else
            {
                Agk.SetSpriteColor(tSprite, r, g, b, 255);
                fadeLoading = 255.0f;
                while (fadeLoading > 0.0f)
                {
                    fadeLoading = 255.0f - ((255.0f / t) * Agk.Abs(Agk.GetMilliseconds() - tMark));
                    Agk.SetSpriteColorAlpha(tSprite, (int)fadeLoading);
                    Agk.Sync();
                }
            }
            Agk.DeleteSprite(tSprite);
        }

        public static Element GetElementById(string rId)
        {
            return ElementList.FirstOrDefault(el => el.Id == rId);
        }

        public static List<Element> GetElementsByName(string rName)
        {
            return ElementList.FindAll(el => el.Name == rName).ToList();
        }

        public static List<Element> GetElementsByTagName(string rName)
        {
            return ElementList.FindAll(el => el.Tag == rName).ToList();
        }

        public static List<Element> GetElementsByClassName(string rName)
        {
            return ElementList.FindAll(el => el.StyleClassList.First(cl => cl.ClassName == rName) != null).ToList();
        }

        public static StyleClass GetStyleClassByName(string rStyleClass)
        {
            return UserInterface.StyleClassList.FirstOrDefault(cl => cl.ClassName == rStyleClass);
        }

    }

    public class Element : INotifyPropertyChanged
    {
        public string Id;
        public string Name;
        public string Tag;
        public Element Parent;
        public ObservableCollection<UI.StyleClass> StyleClassList = new ObservableCollection<StyleClass>();
        public StylePropertyData Style;
        public StylePropertyData ResolvedStyle;
        public string Value;
        public int ScrollX;
        public int ScrollY;
        public string OnPress; //name of callback function to execute on event
        public string OnRelease;
        public string OnMouseIn;
        public string OnMouseOut;
        public bool MouseIsOver; //mouse curser is within element bounds?
        public bool PressIsHeld; //mouseclick or hotkey press is held down?
        public int KeyBind = -1; //key id to act as hotkey to trigger onPress event
        public uint EnableEvents; //0 disabled | 1 enabled mouse | 2 enabled keybind | 3 enabled mouse and keybind | 4 enable drag and drop
        public uint EnableMove; //0-disabled | 1-freely drag around within parent bounds | 2-freely drag parent around within parent's parent bounds | 3-drag and drop from sockets
        public uint EnableSize; //grabbing an edge (2px?) or border will allow to resize the width/height
        public bool HoldPause; //gameplay is paused while element is visible
        public bool HoldMouseFocus; //gameplay ignores mouse input while element is visible
        public bool HoldKeyFocus; //gameplay ignores key input while element is visible
        public int SelectedIndex = -1;
        public bool IsDirty = true;

        public Element()
        {
            Style = new StylePropertyData(this);
            ResolvedStyle = new StylePropertyData(this);
            StyleClassList.CollectionChanged += StyleClassList_Changed;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public void SetParent(string rParentId)
        {
            Parent = UserInterface.ElementList.FirstOrDefault(el => el.Id == rParentId);
            Parent.IsDirty = true;
            IsDirty = true;
        }

        private void StyleClassList_Changed(object sender, NotifyCollectionChangedEventArgs e)
        {
            Parent.IsDirty = true;
            IsDirty = true;
        }

        public void AddStyleClass(string rStyleClass)
        {
            if(UserInterface.StyleClassList == null)
            {
                return;
            }
            var tStyleClass = UserInterface.StyleClassList.FirstOrDefault(cl => cl.ClassName == rStyleClass);
            if (tStyleClass != null)
            {
                StyleClassList.Add(tStyleClass);
            }
            else
            {
                //log warning
                Console.WriteLine("could not find class " + rStyleClass);
            }
        }

        public void RemoveStyleClass(string rStyleClass)
        {
            if (UserInterface.StyleClassList == null)
            {
                return;
            }
            var tStyleClass = UserInterface.StyleClassList.FirstOrDefault(cl => cl.ClassName == rStyleClass);
            if (tStyleClass != null)
            {
                StyleClassList.Remove(tStyleClass);
            }
            else
            {
                //log warning
            }
        }
    }

    public class StyleClass
    {
        public string ClassName;
        public StylePropertyData Style;

        public StyleClass()
        {
            Style = new StylePropertyData(this);
        }
    }

    public class BorderData
    {
        public string Size = "0px";
        public uint Color = 0x00000000;
    }

    public class SizeData
    {
        public bool IsPercent;
        public int Value;
    }

    public class StylePropertyData
    {
        dynamic Owner;

        //Flow Props
        public string PositionAlignH { get; private set; } = "left";            //left|right|center
        public string PositionAlignV { get; private set; } = "top";             //top|bottom|center
        public string Position { get; private set; } = "relative";              //relative|absolute
        public string Top { get; private set; } = "0px";                        //#px|#%
        public string Left { get; private set; } = "0px";                       //#px|#%
        public string PaddingTop { get; private set; } = "0px";                 //#px|#%
        public string PaddingBottom { get; private set; } = "0px";              //#px|#%
        public string PaddingLeft { get; private set; } = "0px";                //#px|#%
        public string PaddingRight { get; private set; } = "0px";               //#px|#%
        public string Width { get; private set; } = "100%";                     //#px|#%
        public string Height { get; private set; } = "100%";                    //#px|#%
        public BorderData BorderTop { get; private set; } = new BorderData();
        public BorderData BorderBottom { get; private set; } = new BorderData();
        public BorderData BorderLeft { get; private set; } = new BorderData();
        public BorderData BorderRight { get; private set; } = new BorderData();
        //TODO: BorderImage
        public string MarginTop { get; private set; } = "0px";                  //#px|#%
        public string MarginBottom { get; private set; } = "0px";               //#px|#%
        public string MarginLeft { get; private set; } = "0px";                 //#px|#%
        public string MarginRight { get; private set; } = "0px";                //#px|#%
        public string MinWidth { get; private set; } = "0px";                   //#px
        public string MaxWidth { get; private set; } = "0px";                   //#px
        public string MinHeight { get; private set; } = "0px";                  //#px
        public string MaxHeight { get; private set; } = "0px";                  //#px

        //Visual Props
        public uint    BackgroundColor { get; private set; } = 0x00000000;
        public uint    BackgroundOpacity { get; private set; } = 100;            //% as 0-100
        public string  BackgroundImage { get; private set; } = "";               //filepath
        public string  BackgroundRepeat { get; private set; } = "no-repeat";     //no-repeat|repeat|repeat-x|repeat-y|cover
        public string  BackgroundAlignH { get; private set; } = "left";          //left|right|center
        public string  BackgroundAlignV { get; private set; } = "top";           //top|bottom|center
        public string  Display { get; private set; } = "visible";                //visible|hidden :: inheritable
        public uint    Opacity { get; private set; } = 100;                      //% as 0-100 :: inheritable
        public uint    ZIndex { get; private set; } = 10;                        //0 = top most, 10000 = back most :: inheritable
        public string  Cursor { get; private set; } = "default";                 // :: inheritable
        public uint    Color { get; private set; } = 0xffffffff;                 // :: inheritable
        public string  Font { get; private set; } = "Arial";                     // font name :: inheritable
        public uint    FontSize { get; private set; } = 18;                      // :: inheritable
        public string  TextDecoration { get; private set; } = "none";            //none|bold //TODO:italic|underline :: inheritable
        public string  TextTransform { get; private set; } = "none";             //upper|lower|capitalize :: inheritable
        public string  TextIndent { get; private set; } = "0px";                 //#px|#% first line indent :: inheritable
        public string  TextAlignH { get; private set; } = "left";                //left|right|center :: inheritable
        public string  TextAlignV { get; private set; } = "top";                 //top|bottom|center :: inheritable
        public uint    Rotation { get; private set; } = 0;                       //angle as 0-360 :: inheritable

        //Resolved Props
        public int _FinalX { get; private set; }                                 //final resolved px coord
        public int _FinalY { get; private set; }
        public int _FinalW { get; private set; }
        public int _FinalH { get; private set; }
        public int _InnerX { get; private set; }
        public int _InnerY { get; private set; }
        public int _InnerW { get; private set; }
        public int _InnerH { get; private set; }
        public bool _IsResolved { get; private set; } = false;
        public uint _FlowPropertyEnabled { get; private set; }
        public uint _VisualPropertyEnabled { get; private set; }

        public StylePropertyData(dynamic rObject)
        {
            Owner = rObject;
        }

        public void SetProp(string rProp, string rValue)
        {

            App.Log("UserInterface.cs", 1, "ui", "- SetProp " + rProp + ": " + rValue);

            try
            {
                //styleClass owner won't have this.
                Owner.IsDirty = true;
            }
            catch(Exception ex) { }

            if (rProp.ToLower().Contains("color"))
            {
                rValue = Data.ParseColor(rValue).ToString();
            }

            switch (rProp.ToLower())
            {
                //Flow Props
                case "position-alignh":
                    PositionAlignH = rValue;
                    _FlowPropertyEnabled = Data.SetBit((int)UI.FlowPropBit.PositionAlignH, _FlowPropertyEnabled, 1);
                    break;
                case "position-alignv":
                    PositionAlignV = rValue;
                    _FlowPropertyEnabled = Data.SetBit((int)UI.FlowPropBit.PositionAlignV, _FlowPropertyEnabled, 1);
                    break;
                case "position":
                    Position = rValue;
                    _FlowPropertyEnabled = Data.SetBit((int)UI.FlowPropBit.Position, _FlowPropertyEnabled, 1);
                    break;
                case "top":
                    Top = rValue;
                    _FlowPropertyEnabled = Data.SetBit((int)UI.FlowPropBit.Top, _FlowPropertyEnabled, 1);
                    break;
                case "left":
                    Left = rValue;
                    _FlowPropertyEnabled = Data.SetBit((int)UI.FlowPropBit.Left, _FlowPropertyEnabled, 1);
                    break;
                case "padding":
                    PaddingTop = rValue;
                    _FlowPropertyEnabled = Data.SetBit((int)UI.FlowPropBit.PaddingTop, _FlowPropertyEnabled, 1);
                    PaddingBottom = rValue;
                    _FlowPropertyEnabled = Data.SetBit((int)UI.FlowPropBit.PaddingBottom, _FlowPropertyEnabled, 1);
                    PaddingLeft = rValue;
                    _FlowPropertyEnabled = Data.SetBit((int)UI.FlowPropBit.PaddingLeft, _FlowPropertyEnabled, 1);
                    PaddingRight = rValue;
                    _FlowPropertyEnabled = Data.SetBit((int)UI.FlowPropBit.PaddingRight, _FlowPropertyEnabled, 1);
                    break;
                case "padding-top":
                    PaddingTop = rValue;
                    _FlowPropertyEnabled = Data.SetBit((int)UI.FlowPropBit.PaddingTop, _FlowPropertyEnabled, 1);
                    break;
                case "padding-bottom":
                    PaddingBottom = rValue;
                    _FlowPropertyEnabled = Data.SetBit((int)UI.FlowPropBit.PaddingBottom, _FlowPropertyEnabled, 1);
                    break;
                case "padding-left":
                    PaddingLeft = rValue;
                    _FlowPropertyEnabled = Data.SetBit((int)UI.FlowPropBit.PaddingLeft, _FlowPropertyEnabled, 1);
                    break;
                case "padding-right":
                    PaddingRight = rValue;
                    _FlowPropertyEnabled = Data.SetBit((int)UI.FlowPropBit.PaddingRight, _FlowPropertyEnabled, 1);
                    break;
                case "width":
                    Width = rValue;
                    _FlowPropertyEnabled = Data.SetBit((int)UI.FlowPropBit.Width, _FlowPropertyEnabled, 1);
                    break;
                case "height":
                    Height = rValue;
                    _FlowPropertyEnabled = Data.SetBit((int)UI.FlowPropBit.Height, _FlowPropertyEnabled, 1);
                    break;
                case "border-width":
                    BorderTop.Size = rValue;
                    _FlowPropertyEnabled = Data.SetBit((int)UI.FlowPropBit.BorderTop, _FlowPropertyEnabled, 1);
                    BorderBottom.Size = rValue;
                    _FlowPropertyEnabled = Data.SetBit((int)UI.FlowPropBit.BorderBottom, _FlowPropertyEnabled, 1);
                    BorderLeft.Size = rValue;
                    _FlowPropertyEnabled = Data.SetBit((int)UI.FlowPropBit.BorderLeft, _FlowPropertyEnabled, 1);
                    BorderRight.Size = rValue;
                    _FlowPropertyEnabled = Data.SetBit((int)UI.FlowPropBit.BorderRight, _FlowPropertyEnabled, 1);
                    break;
                case "border-top-width":
                    BorderTop.Size = rValue;
                    _FlowPropertyEnabled = Data.SetBit((int)UI.FlowPropBit.BorderTop, _FlowPropertyEnabled, 1);
                    break;
                case "border-bottom-width":
                    BorderBottom.Size = rValue;
                    _FlowPropertyEnabled = Data.SetBit((int)UI.FlowPropBit.BorderBottom, _FlowPropertyEnabled, 1);
                    break;
                case "border-left-width":
                    BorderLeft.Size = rValue;
                    _FlowPropertyEnabled = Data.SetBit((int)UI.FlowPropBit.BorderLeft, _FlowPropertyEnabled, 1);
                    break;
                case "border-right-width":
                    BorderRight.Size = rValue;
                    _FlowPropertyEnabled = Data.SetBit((int)UI.FlowPropBit.BorderRight, _FlowPropertyEnabled, 1);
                    break;
                case "border-color":
                    BorderTop.Color = Convert.ToUInt32(rValue);
                    _FlowPropertyEnabled = Data.SetBit((int)UI.FlowPropBit.BorderTop, _FlowPropertyEnabled, 1);
                    BorderBottom.Color = Convert.ToUInt32(rValue);
                    _FlowPropertyEnabled = Data.SetBit((int)UI.FlowPropBit.BorderBottom, _FlowPropertyEnabled, 1);
                    BorderLeft.Color = Convert.ToUInt32(rValue);
                    _FlowPropertyEnabled = Data.SetBit((int)UI.FlowPropBit.BorderLeft, _FlowPropertyEnabled, 1);
                    BorderRight.Color = Convert.ToUInt32(rValue);
                    _FlowPropertyEnabled = Data.SetBit((int)UI.FlowPropBit.BorderRight, _FlowPropertyEnabled, 1);
                    break;
                case "border-top-color":
                    BorderTop.Color = Convert.ToUInt32(rValue);
                    _FlowPropertyEnabled = Data.SetBit((int)UI.FlowPropBit.BorderTop, _FlowPropertyEnabled, 1);
                    break;
                case "border-bottom-color":
                    BorderBottom.Color = Convert.ToUInt32(rValue);
                    _FlowPropertyEnabled = Data.SetBit((int)UI.FlowPropBit.BorderBottom, _FlowPropertyEnabled, 1);
                    break;
                case "border-left-color":
                    BorderLeft.Color = Convert.ToUInt32(rValue);
                    _FlowPropertyEnabled = Data.SetBit((int)UI.FlowPropBit.BorderLeft, _FlowPropertyEnabled, 1);
                    break;
                case "border-right-color":
                    BorderRight.Color = Convert.ToUInt32(rValue);
                    _FlowPropertyEnabled = Data.SetBit((int)UI.FlowPropBit.BorderRight, _FlowPropertyEnabled, 1);
                    break;
                case "margin":
                    MarginTop = rValue;
                    _FlowPropertyEnabled = Data.SetBit((int)UI.FlowPropBit.MarginTop, _FlowPropertyEnabled, 1);
                    MarginBottom = rValue;
                    _FlowPropertyEnabled = Data.SetBit((int)UI.FlowPropBit.MarginBottom, _FlowPropertyEnabled, 1);
                    MarginLeft = rValue;
                    _FlowPropertyEnabled = Data.SetBit((int)UI.FlowPropBit.MarginLeft, _FlowPropertyEnabled, 1);
                    MarginRight = rValue;
                    _FlowPropertyEnabled = Data.SetBit((int)UI.FlowPropBit.MarginRight, _FlowPropertyEnabled, 1);
                    break;
                case "margin-top":
                    MarginTop = rValue;
                    _FlowPropertyEnabled = Data.SetBit((int)UI.FlowPropBit.MarginTop, _FlowPropertyEnabled, 1);
                    break;
                case "margin-bottom":
                    MarginBottom = rValue;
                    _FlowPropertyEnabled = Data.SetBit((int)UI.FlowPropBit.MarginBottom, _FlowPropertyEnabled, 1);
                    break;
                case "margin-left":
                    MarginLeft = rValue;
                    _FlowPropertyEnabled = Data.SetBit((int)UI.FlowPropBit.MarginLeft, _FlowPropertyEnabled, 1);
                    break;
                case "margin-right":
                    MarginRight = rValue;
                    _FlowPropertyEnabled = Data.SetBit((int)UI.FlowPropBit.MarginRight, _FlowPropertyEnabled, 1);
                    break;
                case "min-width":
                    MinWidth = rValue;
                    _FlowPropertyEnabled = Data.SetBit((int)UI.FlowPropBit.MinWidth, _FlowPropertyEnabled, 1);
                    break;
                case "max-width":
                    MaxWidth = rValue;
                    _FlowPropertyEnabled = Data.SetBit((int)UI.FlowPropBit.MaxWidth, _FlowPropertyEnabled, 1);
                    break;
                case "min-height":
                    MinWidth = rValue;
                    _FlowPropertyEnabled = Data.SetBit((int)UI.FlowPropBit.MinWidth, _FlowPropertyEnabled, 1);
                    break;
                case "max-height":
                    MaxWidth = rValue;
                    _FlowPropertyEnabled = Data.SetBit((int)UI.FlowPropBit.MaxWidth, _FlowPropertyEnabled, 1);
                    break;

                //Visual Props
                case "background-color":
                    BackgroundColor = Convert.ToUInt32(rValue);
                    _VisualPropertyEnabled = Data.SetBit((int)UI.VisualPropBit.BackgroundColor, _VisualPropertyEnabled, 1);
                    break;
                case "background-opacity":
                    BackgroundOpacity = Convert.ToUInt32(rValue);
                    _VisualPropertyEnabled = Data.SetBit((int)UI.VisualPropBit.BackgroundOpacity, _VisualPropertyEnabled, 1);
                    break;
                case "background-image":
                    BackgroundImage = rValue;
                    _VisualPropertyEnabled = Data.SetBit((int)UI.VisualPropBit.BackgroundImage, _VisualPropertyEnabled, 1);
                    if(Data.GetBit((int)UI.VisualPropBit.BackgroundOpacity, _VisualPropertyEnabled) == 0)
                    {
                        BackgroundOpacity = 100;
                        _VisualPropertyEnabled = Data.SetBit((int)UI.VisualPropBit.BackgroundOpacity, _VisualPropertyEnabled, 1);
                    }
                    break;
                case "background-alignh":
                    BackgroundAlignH = rValue;
                    _VisualPropertyEnabled = Data.SetBit((int)UI.VisualPropBit.BackgroundAlignH, _VisualPropertyEnabled, 1);
                    break;
                case "background-alignv":
                    BackgroundAlignV = rValue;
                    _VisualPropertyEnabled = Data.SetBit((int)UI.VisualPropBit.BackgroundAlignV, _VisualPropertyEnabled, 1);
                    break;
                case "display":
                    Display = rValue;
                    _VisualPropertyEnabled = Data.SetBit((int)UI.VisualPropBit.Display, _VisualPropertyEnabled, 1);
                    break;
                case "opacity":
                    Opacity = Convert.ToUInt32(rValue);
                    _VisualPropertyEnabled = Data.SetBit((int)UI.VisualPropBit.Opacity, _VisualPropertyEnabled, 1);
                    break;
                case "z-index":
                    ZIndex = Convert.ToUInt32(rValue);
                    _VisualPropertyEnabled = Data.SetBit((int)UI.VisualPropBit.ZIndex, _VisualPropertyEnabled, 1);
                    break;
                case "cursor":
                    Cursor = rValue;
                    _VisualPropertyEnabled = Data.SetBit((int)UI.VisualPropBit.Cursor, _VisualPropertyEnabled, 1);
                    break;
                case "color":
                    Color = Convert.ToUInt32(rValue);
                    _VisualPropertyEnabled = Data.SetBit((int)UI.VisualPropBit.Color, _VisualPropertyEnabled, 1);
                    break;
                case "font":
                    Font = rValue;
                    _VisualPropertyEnabled = Data.SetBit((int)UI.VisualPropBit.Font, _VisualPropertyEnabled, 1);
                    break;
                case "font-size":
                    FontSize = Convert.ToUInt32(rValue);
                    _VisualPropertyEnabled = Data.SetBit((int)UI.VisualPropBit.FontSize, _VisualPropertyEnabled, 1);
                    break;
                case "text-decoration":
                    TextDecoration = rValue;
                    _VisualPropertyEnabled = Data.SetBit((int)UI.VisualPropBit.TextDecoration, _VisualPropertyEnabled, 1);
                    break;
                case "text-transform":
                    TextTransform = rValue;
                    _VisualPropertyEnabled = Data.SetBit((int)UI.VisualPropBit.TextTransform, _VisualPropertyEnabled, 1);
                    break;
                case "text-indent":
                    TextIndent = rValue;
                    _VisualPropertyEnabled = Data.SetBit((int)UI.VisualPropBit.TextIndent, _VisualPropertyEnabled, 1);
                    break;
                case "text-alignh":
                    TextAlignH = rValue;
                    _VisualPropertyEnabled = Data.SetBit((int)UI.VisualPropBit.TextAlignH, _VisualPropertyEnabled, 1);
                    break;
                case "text-alignv":
                    TextAlignV = rValue;
                    _VisualPropertyEnabled = Data.SetBit((int)UI.VisualPropBit.TextAlignV, _VisualPropertyEnabled, 1);
                    break;
                case "rotation":
                    Rotation = Convert.ToUInt32(rValue);
                    _VisualPropertyEnabled = Data.SetBit((int)UI.VisualPropBit.Rotation, _VisualPropertyEnabled, 1);
                    break;
            }

        }

        public void ResolveAsScreen()
        {
            Width = App.Config.Screen.Width.ToString();
            Height = App.Config.Screen.Height.ToString();
            _FinalW = App.Config.Screen.Width;
            _FinalH = App.Config.Screen.Height;
            _InnerW = App.Config.Screen.Width;
            _InnerH = App.Config.Screen.Height;
            _FinalX = 0;
            _FinalY = 0;
            _InnerX = 0;
            _InnerY = 0;
            _IsResolved = true;
        }

        public void ApplyInheritedStyleProps()
        {

            App.Log("UserInterface.cs", 1, "ui", "> Begin ApplyInheritedStyleProps");

            //this should be applied to element.ResolvedStyle
            Display = Owner.ResolvedStyle.Display;
            Opacity = Owner.ResolvedStyle.Opacity;
            ZIndex = Owner.ResolvedStyle.ZIndex;
            Cursor = Owner.ResolvedStyle.Cursor;
            Color = Owner.ResolvedStyle.Color;
            Font = Owner.ResolvedStyle.Font;
            FontSize = Owner.ResolvedStyle.FontSize;
            TextDecoration = Owner.ResolvedStyle.TextDecoration;
            TextTransform = Owner.ResolvedStyle.TextTransform;
            TextIndent = Owner.ResolvedStyle.TextIndent;
            TextAlignH = Owner.ResolvedStyle.TextAlignH;
            TextAlignV = Owner.ResolvedStyle.TextAlignV;
            Rotation = Owner.ResolvedStyle.Rotation;

            App.Log("UserInterface.cs", 1, "ui", "> End ApplyInheritedStyleProps");

        }

        public void ApplyStyleProps(StylePropertyData rSource)
        {

            App.Log("UserInterface.cs", 1, "ui", "> Begin ApplyStyleProps");

            //this should be applied to element.ResolvedStyle
            //Flow props
            if (Data.GetBit((int)UI.FlowPropBit.PositionAlignH, rSource._FlowPropertyEnabled) == 1)
            {
                PositionAlignH = rSource.PositionAlignH;
            }
            if (Data.GetBit((int)UI.FlowPropBit.PositionAlignV, rSource._FlowPropertyEnabled) == 1)
            {
                PositionAlignV = rSource.PositionAlignV;
            }
            if (Data.GetBit((int)UI.FlowPropBit.Position, rSource._FlowPropertyEnabled) == 1)
            {
                Position = rSource.Position;
            }
            if (Data.GetBit((int)UI.FlowPropBit.Top, rSource._FlowPropertyEnabled) == 1)
            {
                Top = rSource.Top;
            }
            if (Data.GetBit((int)UI.FlowPropBit.Left, rSource._FlowPropertyEnabled) == 1)
            {
                Left = rSource.Left;
            }
            if (Data.GetBit((int)UI.FlowPropBit.PaddingTop, rSource._FlowPropertyEnabled) == 1)
            {
                PaddingTop = rSource.PaddingTop;
            }
            if (Data.GetBit((int)UI.FlowPropBit.PaddingBottom, rSource._FlowPropertyEnabled) == 1)
            {
                PaddingBottom = rSource.PaddingBottom;
            }
            if (Data.GetBit((int)UI.FlowPropBit.PaddingLeft, rSource._FlowPropertyEnabled) == 1)
            {
                PaddingLeft = rSource.PaddingLeft;
            }
            if (Data.GetBit((int)UI.FlowPropBit.PaddingRight, rSource._FlowPropertyEnabled) == 1)
            {
                PaddingRight = rSource.PaddingRight;
            }
            if (Data.GetBit((int)UI.FlowPropBit.Width, rSource._FlowPropertyEnabled) == 1)
            {
                Width = rSource.Width;
            }
            if (Data.GetBit((int)UI.FlowPropBit.Height, rSource._FlowPropertyEnabled) == 1)
            {
                Height = rSource.Height;
            }
            if (Data.GetBit((int)UI.FlowPropBit.BorderTop, rSource._FlowPropertyEnabled) == 1)
            {
                BorderTop.Size = rSource.BorderTop.Size;
                BorderTop.Color = rSource.BorderTop.Color;
            }
            if (Data.GetBit((int)UI.FlowPropBit.BorderBottom, rSource._FlowPropertyEnabled) == 1)
            {
                BorderBottom.Size = rSource.BorderBottom.Size;
                BorderBottom.Color = rSource.BorderBottom.Color;
            }
            if (Data.GetBit((int)UI.FlowPropBit.BorderLeft, rSource._FlowPropertyEnabled) == 1)
            {
                BorderLeft.Size = rSource.BorderLeft.Size;
                BorderLeft.Color = rSource.BorderLeft.Color;
            }
            if (Data.GetBit((int)UI.FlowPropBit.BorderRight, rSource._FlowPropertyEnabled) == 1)
            {
                BorderRight.Size = rSource.BorderRight.Size;
                BorderRight.Color = rSource.BorderRight.Color;
            }
            if (Data.GetBit((int)UI.FlowPropBit.MarginTop, rSource._FlowPropertyEnabled) == 1)
            {
                MarginTop = rSource.MarginTop;
            }
            if (Data.GetBit((int)UI.FlowPropBit.MarginBottom, rSource._FlowPropertyEnabled) == 1)
            {
                MarginBottom = rSource.MarginBottom;
            }
            if (Data.GetBit((int)UI.FlowPropBit.MarginLeft, rSource._FlowPropertyEnabled) == 1)
            {
                MarginLeft = rSource.MarginLeft;
            }
            if (Data.GetBit((int)UI.FlowPropBit.MarginRight, rSource._FlowPropertyEnabled) == 1)
            {
                MarginRight = rSource.MarginRight;
            }
            if (Data.GetBit((int)UI.FlowPropBit.MinWidth, rSource._FlowPropertyEnabled) == 1)
            {
                MinWidth = rSource.MinWidth;
            }
            if (Data.GetBit((int)UI.FlowPropBit.Height, rSource._FlowPropertyEnabled) == 1)
            {
                MinHeight = rSource.MinHeight;
            }
            if (Data.GetBit((int)UI.FlowPropBit.MaxWidth, rSource._FlowPropertyEnabled) == 1)
            {
                MaxWidth = rSource.MaxWidth;
            }
            if (Data.GetBit((int)UI.FlowPropBit.Height, rSource._FlowPropertyEnabled) == 1)
            {
                MaxHeight = rSource.MaxHeight;
            }

            //Flow props
            if (Data.GetBit((int)UI.VisualPropBit.BackgroundColor, rSource._VisualPropertyEnabled) == 1)
            {
                BackgroundColor = rSource.BackgroundColor;
            }
            if (Data.GetBit((int)UI.VisualPropBit.BackgroundOpacity, rSource._VisualPropertyEnabled) == 1)
            {
                BackgroundOpacity = rSource.BackgroundOpacity;
            }
            if (Data.GetBit((int)UI.VisualPropBit.BackgroundImage, rSource._VisualPropertyEnabled) == 1)
            {
                BackgroundImage = rSource.BackgroundImage;
            }
            if (Data.GetBit((int)UI.VisualPropBit.BackgroundRepeat, rSource._VisualPropertyEnabled) == 1)
            {
                BackgroundRepeat = rSource.BackgroundRepeat;
            }
            if (Data.GetBit((int)UI.VisualPropBit.BackgroundAlignH, rSource._VisualPropertyEnabled) == 1)
            {
                BackgroundAlignH = rSource.BackgroundAlignH;
            }
            if (Data.GetBit((int)UI.VisualPropBit.Display, rSource._VisualPropertyEnabled) == 1)
            {
                Display = rSource.Display;
            }
            if (Data.GetBit((int)UI.VisualPropBit.Opacity, rSource._VisualPropertyEnabled) == 1)
            {
                Opacity = rSource.Opacity;
            }
            if (Data.GetBit((int)UI.VisualPropBit.ZIndex, rSource._VisualPropertyEnabled) == 1)
            {
                ZIndex = rSource.ZIndex;
            }
            if (Data.GetBit((int)UI.VisualPropBit.Cursor, rSource._VisualPropertyEnabled) == 1)
            {
                Cursor = rSource.Cursor;
            }
            if (Data.GetBit((int)UI.VisualPropBit.Color, rSource._VisualPropertyEnabled) == 1)
            {
                Color = rSource.Color;
            }
            if (Data.GetBit((int)UI.VisualPropBit.Font, rSource._VisualPropertyEnabled) == 1)
            {
                Font = rSource.Font;
            }
            if (Data.GetBit((int)UI.VisualPropBit.FontSize, rSource._VisualPropertyEnabled) == 1)
            {
                FontSize = rSource.FontSize;
            }
            if (Data.GetBit((int)UI.VisualPropBit.TextDecoration, rSource._VisualPropertyEnabled) == 1)
            {
                TextDecoration = rSource.TextDecoration;
            }
            if (Data.GetBit((int)UI.VisualPropBit.TextTransform, rSource._VisualPropertyEnabled) == 1)
            {
                TextTransform = rSource.TextTransform;
            }
            if (Data.GetBit((int)UI.VisualPropBit.TextIndent, rSource._VisualPropertyEnabled) == 1)
            {
                TextIndent = rSource.TextIndent;
            }
            if (Data.GetBit((int)UI.VisualPropBit.TextAlignH, rSource._VisualPropertyEnabled) == 1)
            {
                TextAlignH = rSource.TextAlignH;
            }
            if (Data.GetBit((int)UI.VisualPropBit.TextAlignV, rSource._VisualPropertyEnabled) == 1)
            {
                TextAlignV = rSource.TextAlignV;
            }
            if (Data.GetBit((int)UI.VisualPropBit.Rotation, rSource._VisualPropertyEnabled) == 1)
            {
                Rotation = rSource.Rotation;
            }

            App.Log("UserInterface.cs", 1, "ui", "> End ApplyStyleProps");

        }

        public void ResolveFlowValues()
        {

            App.Log("UserInterface.cs", 1, "ui", "> Begin ResolveFlowValues");

            //this should be applied to element.ResolvedStyle
            //if position is absolute, use root values as parent values;
            UI.Element parent;
            if (Position.ToLower() == "absolute")
            {
                parent = UserInterface.ElementList[0];
            }
            else
            {
                parent = Owner.Parent;
            }

            //get parent values
            int parentW = Convert.ToInt32(parent.ResolvedStyle.Width);
            int parentH = Convert.ToInt32(parent.ResolvedStyle.Height);

            int parentContentW = Convert.ToInt32(parent.ResolvedStyle._InnerW);
            int parentContentH = Convert.ToInt32(parent.ResolvedStyle._InnerH);

            int parentX = Convert.ToInt32(parent.ResolvedStyle._FinalX);
            int parentY = Convert.ToInt32(parent.ResolvedStyle._FinalY);

            int parentContentX = Convert.ToInt32(parent.ResolvedStyle._InnerX);
            int parentContentY = Convert.ToInt32(parent.ResolvedStyle._InnerY);

            //get element values
            int resolvedW;
            int resolvedH;
            int resolvedContentW;
            int resolvedContentH;
            int resolvedPadT;
            int resolvedPadB;
            int resolvedPadL;
            int resolvedPadR;
            int resolvedMargT;
            int resolvedMargB;
            int resolvedMargL;
            int resolvedMargR;
            int resolvedBorderT;
            int resolvedBorderB;
            int resolvedBorderL;
            int resolvedBorderR;
            int resolvedTextIndent;
            int resolvedTop;
            int resolvedLeft;
            int resolvedX;
            int resolvedY;

            //width
            var parsedSize = Data.ParseSize(Width);
            resolvedW = parsedSize.Value;
            if (parsedSize.IsPercent)
            {
                //remove parent padding from child final width if child width is % of parent
                //NOTE: when the parent was resolved, it's padding gets added to its width making the resolved width larger than its content width.
                //	child % needs to look at the parents final content width.		
                float tFactor = parsedSize.Value * 0.01f;
                resolvedW = (int)Math.Floor(parentContentW * tFactor);
            }
            //min/max should only be specified in pixels, after the element % mhas been converted to pixels, we can check against the min/max values
            if (MinWidth[0] != '0')
            {
                int minW = Data.ParseSize(MinWidth).Value;
                if(resolvedW < minW)
                {
                    resolvedW = minW;
                }
            }
            if (MaxWidth[0] != '0')
            {
                int maxW = Data.ParseSize(MaxWidth).Value;
                if (resolvedW < maxW)
                {
                    resolvedW = maxW;
                }
            }
            //padding may expand resolvedW after this, capture current size as the inner content size
            resolvedContentW = resolvedW;

            //height
            parsedSize = Data.ParseSize(Height);
            resolvedH = parsedSize.Value;
            if (parsedSize.IsPercent)
            {
                float tFactor = parsedSize.Value * 0.01f;
                resolvedH = (int)Math.Floor(parentContentH * tFactor);
            }
            if (MinHeight[0] != '0')
            {
                int minH = Data.ParseSize(MinHeight).Value;
                if (resolvedH < minH)
                {
                    resolvedH = minH;
                }
            }
            if (MaxHeight[0] != '0')
            {
                int maxH = Data.ParseSize(MaxHeight).Value;
                if (resolvedH < maxH)
                {
                    resolvedH = maxH;
                }
            }
            resolvedContentH = resolvedH;

            //TODO: handle overflow: visible|hidden|scroll

            //padding
            parsedSize = Data.ParseSize(PaddingTop);
            resolvedPadT = parsedSize.Value;
            if (parsedSize.IsPercent)
            {
                float tFactor = parsedSize.Value * 0.01f;
                resolvedPadT = (int)Math.Floor(parentContentH * tFactor);
                resolvedH += resolvedPadT;
            }
            parsedSize = Data.ParseSize(PaddingBottom);
            resolvedPadB = parsedSize.Value;
            if (parsedSize.IsPercent)
            {
                float tFactor = parsedSize.Value * 0.01f;
                resolvedPadB = (int)Math.Floor(parentContentH * tFactor);
                resolvedH += resolvedPadB;
            }
            parsedSize = Data.ParseSize(PaddingLeft);
            resolvedPadL = parsedSize.Value;
            if (parsedSize.IsPercent)
            {
                float tFactor = parsedSize.Value * 0.01f;
                resolvedPadL = (int)Math.Floor(parentContentW * tFactor);
                resolvedW += resolvedPadL;
            }
            parsedSize = Data.ParseSize(PaddingRight);
            resolvedPadR = parsedSize.Value;
            if (parsedSize.IsPercent)
            {
                float tFactor = parsedSize.Value * 0.01f;
                resolvedPadR = (int)Math.Floor(parentContentW * tFactor);
                resolvedW += resolvedPadR;
            }

            //margin
            parsedSize = Data.ParseSize(MarginTop);
            resolvedMargT = parsedSize.Value;
            if (parsedSize.IsPercent)
            {
                float tFactor = parsedSize.Value * 0.01f;
                resolvedMargT = (int)Math.Floor(parentContentH * tFactor);
            }
            parsedSize = Data.ParseSize(MarginBottom);
            resolvedMargB = parsedSize.Value;
            if (parsedSize.IsPercent)
            {
                float tFactor = parsedSize.Value * 0.01f;
                resolvedMargB = (int)Math.Floor(parentContentH * tFactor);
            }
            parsedSize = Data.ParseSize(MarginLeft);
            resolvedMargL = parsedSize.Value;
            if (parsedSize.IsPercent)
            {
                float tFactor = parsedSize.Value * 0.01f;
                resolvedMargL = (int)Math.Floor(parentContentW * tFactor);
            }
            parsedSize = Data.ParseSize(MarginRight);
            resolvedMargR = parsedSize.Value;
            if (parsedSize.IsPercent)
            {
                float tFactor = parsedSize.Value * 0.01f;
                resolvedMargR = (int)Math.Floor(parentContentW * tFactor);
            }

            //border-width
            //NOTE: per css spec, border cannot be assigned as %
            resolvedBorderT = Data.ParseSize(BorderTop.Size).Value;
            resolvedBorderB = Data.ParseSize(BorderBottom.Size).Value;
            resolvedBorderL = Data.ParseSize(BorderLeft.Size).Value;
            resolvedBorderR = Data.ParseSize(BorderRight.Size).Value;

            //text-indent
            parsedSize = Data.ParseSize(TextIndent);
            resolvedTextIndent = parsedSize.Value;
            if (parsedSize.IsPercent)
            {
                float tFactor = parsedSize.Value * 0.01f;
                resolvedTextIndent = (int)Math.Floor(parentContentW * tFactor);
            }

            //position
            if (UserInterface.ElementDrag.IsActive && UserInterface.ElementDrag.DragElement == Owner)
            {
                //if this element is being dragged, position will be set by the mouse
                resolvedTop = 0;
                resolvedLeft = 0;
                resolvedX = Hardware.Mouse.PosX - UserInterface.ElementDrag.OffsetX;
                resolvedY = Hardware.Mouse.PosY - UserInterface.ElementDrag.OffsetY;
            }
            else
            {
                parsedSize = Data.ParseSize(Top);
                resolvedTop = parsedSize.Value;
                if (parsedSize.IsPercent)
                {
                    float tFactor = parsedSize.Value * 0.01f;
                    resolvedTop = (int)Math.Floor(parentContentH * tFactor);
                }
                parsedSize = Data.ParseSize(Left);
                resolvedLeft = parsedSize.Value;
                if (parsedSize.IsPercent)
                {
                    float tFactor = parsedSize.Value * 0.01f;
                    resolvedLeft = (int)Math.Floor(parentContentW * tFactor);
                }

                switch (PositionAlignH)
                {
                    case "left":
                        resolvedX = parentContentX + resolvedMargL + resolvedBorderL + resolvedLeft;
                        break;
                    case "right":
                        resolvedX = (parentContentX + parentContentW) - (resolvedMargR + resolvedW + resolvedBorderR) + resolvedLeft;
                        break;
                    case "center":
                        resolvedX = (int)Math.Floor((parentContentX + (parentContentW * 0.5)) - (resolvedMargR + (resolvedW * 0.5) + resolvedBorderR) + resolvedLeft);
                        break;
                    default: //left
                        resolvedX = parentContentX + resolvedMargL + resolvedBorderL + resolvedLeft;
                        break;
                }

                switch (PositionAlignV)
                {
                    case "top":
                        resolvedY = parentContentY + resolvedMargT + resolvedBorderT + resolvedTop;
                        break;
                    case "bottom":
                        resolvedY = (parentContentY + parentContentH) - (resolvedMargB + resolvedH + resolvedBorderB) + resolvedTop;
                        break;
                    case "center":
                        resolvedY = (int)Math.Floor((parentContentY + (parentContentH * 0.5)) - (resolvedMargB + (resolvedH * 0.5) + resolvedBorderB) + resolvedTop);
                        break;
                    default: //top
                        resolvedY = parentContentY + resolvedMargT + resolvedBorderT + resolvedTop;
                        break;
                }
            }

            //apply it all down
            Width = resolvedW.ToString();
            Height = resolvedH.ToString();
            PaddingTop = resolvedPadT.ToString();
            PaddingBottom = resolvedPadB.ToString();
            PaddingLeft = resolvedPadL.ToString();
            PaddingRight = resolvedPadR.ToString();
            MarginTop = resolvedMargT.ToString();
            MarginBottom = resolvedMargB.ToString();
            MarginLeft = resolvedMargL.ToString();
            MarginRight = resolvedMargR.ToString();
            BorderTop.Size = resolvedBorderT.ToString();
            BorderBottom.Size = resolvedBorderB.ToString();
            BorderLeft.Size = resolvedBorderL.ToString();
            BorderRight.Size = resolvedBorderR.ToString();
            TextIndent = resolvedTextIndent.ToString();
            Top = resolvedTop.ToString();
            Left = resolvedLeft.ToString();
            _FinalX = resolvedX;
            _FinalY = resolvedY;
            _FinalW = resolvedW;
            _FinalH = resolvedH;
            _InnerX = resolvedX + resolvedPadL;
            _InnerY = resolvedY + resolvedPadT;
            _InnerW = resolvedContentW;
            _InnerH = resolvedContentH;

            //enforce visibility
            if (parent.ResolvedStyle.Display == "hidden")
            {
                Display = "hidden";
            }

            int tVisible = Display == "hidden" ? 0 : 1;
            uint tIndex = (uint)UserInterface.ElementList.IndexOf(Owner);
            if (Agk.IsSpriteExists(tIndex))
            {
                if (Agk.GetSpriteVisible(tIndex) != tVisible)
                {
                    Agk.SetSpriteVisible(tIndex, tVisible);
                }
            }
            if (Agk.IsTextExists(tIndex))
            { 
                if (Agk.GetTextVisible(tIndex) != tVisible)
                {
                    Agk.SetTextVisible(tIndex, tVisible);
                }
            }

            _IsResolved = true;

            App.Log("UserInterface.cs", 1, "ui", "> End ResolveFlowValues");

        }
    }

    public enum FlowPropBit
    {
        None,
        PositionAlignH,
        PositionAlignV,
        Position,
        Top,
        Left,
        PaddingTop,
        PaddingBottom,
        PaddingLeft,
        PaddingRight,
        Width,
        Height,
        BorderTop,
        BorderBottom,
        BorderLeft,
        BorderRight,
        BorderImage,
        MarginTop,
        MarginBottom,
        MarginLeft,
        MarginRight,
        MinWidth,
        MaxWidth,
        MinHeight,
        MaxHeight
    }

    public enum VisualPropBit
    {
        None,
        BackgroundColor,
        BackgroundOpacity,
        BackgroundImage,
        BackgroundRepeat,
        BackgroundAlignH,
        BackgroundAlignV,
        Display,
        Opacity,
        ZIndex,
        Cursor,
        Color,
        Font,
        FontSize,
        TextDecoration,
        TextTransform,
        TextIndent,
        TextAlignH,
        TextAlignV,
        Rotation
    }

    public class StatusData
    {
        public string MouseMode; //ui|gameplay|disabled
        public string KeyMode; //ui|gameplay|disabled
        public string MouseModeForced;
        public string KeyModeForced;
        public bool InputReady;
        public uint InputMark;
        public uint LastUpdate;
    }

    public class ElementDragData
    {
        public bool IsActive;
        public int OffsetX;
        public int OffsetY;
        public UI.Element DragElement;
        public UI.Element TargetElement;
    }

    public class TransitionData
    {
        public UI.Element Element;
        public string Property;
        public string InitialValue;
        public string TargetValue;
        public uint Duration;
        public uint Start;
        public string Callback;
    }

    public class Controller
    {

    }

}
