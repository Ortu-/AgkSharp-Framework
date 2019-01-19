using AgkSharp;
using System;
using System.Collections.Generic;

namespace AGKCore.UI
{
    public static class UserInterface
    {
        public static UI.StatusData Status = new StatusData();
        public static UI.ElementDragData ElementDrag = new ElementDragData();
        public static List<UI.TransitionData> TransitionList = new List<TransitionData>();
        public static List<UI.Element> ElementList = new List<Element>();
        public static List<UI.StyleClass> StyleClassList = new List<StyleClass>();
    }

    public class Element
    {
        public string Id;
        public string Name;
        public string Tag;
        public Element Parent;
        public static List<UI.StyleClass> StyleClassList = new List<StyleClass>();
        public StylePropertyData Style = new StylePropertyData();
        public StylePropertyData ResolvedStyle = new StylePropertyData();
        public string Value;
        public int ScrollX;
        public int ScrollY;
        public string OnPress; //name of callback function to execute on event
        public string OnRelease;
        public string OnMouseIn;
        public string OnMouseOut;
        public bool MouseIsOver; //mouse curser is within element bounds?
        public bool PressIsHeld; //mouseclick or hotkey press is held down?
        public uint KeyBind; //key id to act as hotkey to trigger onPress event
        public uint EnableEvents; //0 disabled | 1 enabled mouse | 2 enabled keybind | 3 enabled mouse and keybind | 4 enable drag and drop
        public uint EnableMove; //0-disabled | 1-freely drag around within parent bounds | 2-freely drag parent around within parent's parent bounds | 3-drag and drop from sockets
        public uint EnableSize; //grabbing an edge (2px?) or border will allow to resize the width/height
        public bool HoldPause; //gameplay is paused while element is visible
        public bool HoldMouseFocus; //gameplay ignores mouse input while element is visible
        public bool HoldKeyFocus; //gameplay ignores key input while element is visible
        public int SelectedIndex;
        public bool IsDirty;
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
        public int _IsResolved { get; private set; }
        public uint _FlowPropertyEnabled { get; private set; }
        public uint _VisualPropertyEnabled { get; private set; }

        public void SetProp(string rProp, string rValue)
        {
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
