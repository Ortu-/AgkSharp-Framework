using AgkSharp;
using System;
using System.Collections.Generic;
using System.Windows.Forms;

namespace AGKCore.UI
{
    public static class UserInterface
    {
        public static UI.StatusData Status = new StatusData();
        public static UI.ElementDragData ElementDrag = new ElementDragData();
        public static List<UI.TransitionData> TransitionList = new List<TransitionData>();
        public static List<UI.Element> ElementList = new List<Element>();
        public static List<UI.StyleClass> StyleClassList = new List<StyleClass>();

        public static void ResetResolvedStyleProps()
        {
            ElementList[0].IsDirty = false; //skip root element
            foreach(var e in ElementList)
            {
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
        }
    }

    public class Element
    {
        public string Id;
        public string Name;
        public string Tag;
        public Element Parent;
        public static List<UI.StyleClass> StyleClassList = new List<StyleClass>();
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
        }
    }

    public class StyleClass
    {
        public string ClassName;
        public StylePropertyData Style;
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
        public BorderData BorderTop { get; private set; }
        public BorderData BorderBottom { get; private set; }
        public BorderData BorderLeft { get; private set; }
        public BorderData BorderRight { get; private set; }
        //TODO: BorderImage
        public string MarginTop { get; private set; } = "0px";                  //#px|#%
        public string MarginBottom { get; private set; } = "0px";               //#px|#%
        public string MarginLeft { get; private set; } = "0px";                 //#px|#%
        public string MarginRight { get; private set; } = "0px";                //#px|#%
        public string MinWidth { get; private set; } = "0px";                   //#px|#%
        public string MaxWidth { get; private set; } = "0px";                   //#px|#%
        public string MinHeight { get; private set; } = "0px";                  //#px|#%
        public string MaxHeight { get; private set; } = "0px";                  //#px|#%

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
        public string  TextDecoration { get; private set; } = "none";            //none|bold|italic|underline :: inheritable
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
                case "text-alignH":
                    TextAlignH = rValue;
                    _VisualPropertyEnabled = Data.SetBit((int)UI.VisualPropBit.TextAlignH, _VisualPropertyEnabled, 1);
                    break;
                case "text-alignV":
                    TextAlignV = rValue;
                    _VisualPropertyEnabled = Data.SetBit((int)UI.VisualPropBit.TextAlignV, _VisualPropertyEnabled, 1);
                    break;
                case "rotation":
                    Rotation = Convert.ToUInt32(rValue);
                    _VisualPropertyEnabled = Data.SetBit((int)UI.VisualPropBit.TextDecoration, _VisualPropertyEnabled, 1);
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
        }

        public void ApplyStyleProps(StylePropertyData rSource)
        {
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
        }

        public void ResolveFlowValues()
        {
            //this should be applied to element.ResolvedStyle

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

}
