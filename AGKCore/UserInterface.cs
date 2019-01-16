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
        public readonly string  PositionAlignH = "left";            //left|right|center
        public readonly string  PositionAlignV = "top";             //top|bottom|center
        public readonly string  Position = "relative";              //relative|absolute
        public readonly string  Top = "0px";                        //#px|#%
        public readonly string  Left = "0px";                       //#px|#%
        public readonly string  PaddingTop = "0px";                 //#px|#%
        public readonly string  PaddingBottom = "0px";              //#px|#%
        public readonly string  PaddingLeft = "0px";                //#px|#%
        public readonly string  PaddingRight = "0px";               //#px|#%
        public readonly string  Width = "100%";                     //#px|#%
        public readonly string  Height = "100%";                    //#px|#%
        public readonly BorderData BorderTop;
        public readonly BorderData BorderBottom;
        public readonly BorderData BorderLeft;
        public readonly BorderData BorderRight;
        //TODO: BorderImage
        public readonly string  MarginTop = "0px";                  //#px|#%
        public readonly string  MarginBottom = "0px";               //#px|#%
        public readonly string  MarginLeft = "0px";                 //#px|#%
        public readonly string  MarginRight = "0px";                //#px|#%
        public readonly string  MinWidth = "0px";                   //#px|#%
        public readonly string  MaxWidth = "0px";                   //#px|#%
        public readonly string  MinHeight = "0px";                  //#px|#%
        public readonly string  MaxHeight = "0px";                  //#px|#%

        //Visual Props
        public readonly uint    BackgroundColor = 0x00000000;
        public readonly uint    BackgroundOpacity = 100;            //% as 0-100
        public readonly string  BackgroundImage = "";               //filepath
        public readonly string  BackgroundRepeat = "no-repeat";     //no-repeat|repeat|repeat-x|repeat-y|cover
        public readonly string  BackgroundAlignH = "left";          //left|right|center
        public readonly string  BackgroundAlignV = "top";           //top|bottom|center
        public readonly string  Display = "visible";                //visible|hidden :: inheritable
        public readonly uint    Opacity = 100;                      //% as 0-100 :: inheritable
        public readonly uint    ZIndex = 10;                        //0 = top most, 10000 = back most :: inheritable
        public readonly string  Cursor = "default";                 // :: inheritable
        public readonly uint    Color = 0xffffffff;                 // :: inheritable
        public readonly string  Font = "Arial";                     // font name :: inheritable
        public readonly uint    FontSize = 18;                      // :: inheritable
        public readonly string  TextDecoration = "none";            //none|bold|italic|underline :: inheritable
        public readonly string  TextTransform = "none";             //upper|lower|capitalize :: inheritable
        public readonly string  TextIndent = "0px";                 //#px|#% first line indent :: inheritable
        public readonly string  TextAlignH = "left";                //left|right|center :: inheritable
        public readonly string  TextAlignV = "top";                 //top|bottom|center :: inheritable
        public readonly uint    Rotation = 0;                       //angle as 0-360 :: inheritable

        //Resolved Props
        public readonly int _FinalX;                                //final resolved px coord
        public readonly int _FinalY;
        public readonly int _FinalW;
        public readonly int _FinalH;
        public readonly int _InnerX;
        public readonly int _InnerY;
        public readonly int _InnerW;
        public readonly int _InnerH;
        public readonly int _IsResolved;
        public readonly uint _FlowPropertyEnabled;
        public readonly uint _VisualPropertyEnabled;
    }

    public enum FlowPropBit
    {
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
