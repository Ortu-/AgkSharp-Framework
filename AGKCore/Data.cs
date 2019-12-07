using AgkSharp;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace AGKCore
{
    public static class Data
    {

        public static int GetBit(int rBit, int rData)
        {
            return Convert.ToInt32((rData & (1 << (rBit - 1))) != 0);
        }

        public static int GetBit(int rBit, uint rData)
        {
            return Convert.ToInt32((rData & (1 << (rBit - 1))) != 0);
        }

        public static int SetBit(int rBit, int rData, int rVal)
        {
            if(rVal > 0)
            {
                rData = rData | (1 << (rBit - 1));
            }
            else if((rData & (1 << (rBit - 1))) != 0)
            {
                rData = (rData & ~(1 << (rBit - 1)));
            }
            return rData;
        }

        public static uint SetBit(int rBit, uint rData, int rVal)
        {
            if (rVal > 0)
            {
                rData = rData | Convert.ToUInt32(1 << (rBit - 1));
            }
            else if ((rData & (1 << (rBit - 1))) != 0)
            {
                rData = (rData & ~Convert.ToUInt32(1 << (rBit - 1)));
            }
            return rData;
        }

        public static ParsedSizeData ParseSize(string rValue)
        {
            ParsedSizeData res = new ParsedSizeData();
            if (String.IsNullOrEmpty(rValue))
            {
                rValue = "0px";
            }
            if(rValue[rValue.Length - 1] == '%')
            {
                res.IsPercent = true;
                res.Value = Convert.ToInt32(rValue.Substring(0, rValue.Length - 1));
            }
            else if (rValue[rValue.Length - 1] == 'x')
            {
                res.IsPercent = false;
                res.Value = Convert.ToInt32(rValue.Substring(0, rValue.Length - 2));
            }
            else
            {
                res.IsPercent = false;
                res.Value = Convert.ToInt32(rValue);
            }
            return res;
        }

        public static uint ParseColor(string rValue)
        {
            string valR;
            string valG;
            string valB;
            if(rValue[0] == '#')
            {
                if (rValue.Length == 4)
                {
                    //short form hex #000
                    valR = rValue[1].ToString();
                    valG = rValue[2].ToString();
                    valB = rValue[3].ToString();
                    return Agk.MakeColor(
                                byte.Parse(valR + valR, NumberStyles.HexNumber, CultureInfo.InvariantCulture),
                                byte.Parse(valG + valG, NumberStyles.HexNumber, CultureInfo.InvariantCulture),
                                byte.Parse(valB + valB, NumberStyles.HexNumber, CultureInfo.InvariantCulture)
                           );
                }
                else if(rValue.Length == 7)
                {
                    //rgb hex #000000
                    valR = rValue.Substring(1, 2);
                    valG = rValue.Substring(3, 2);
                    valB = rValue.Substring(5, 2);
                    return Agk.MakeColor(
                                byte.Parse(valR, NumberStyles.HexNumber, CultureInfo.InvariantCulture),
                                byte.Parse(valG, NumberStyles.HexNumber, CultureInfo.InvariantCulture),
                                byte.Parse(valB, NumberStyles.HexNumber, CultureInfo.InvariantCulture)
                           );
                }
                else
                {
                    //argb hex #00000000
                    valR = rValue.Substring(3, 2);
                    valG = rValue.Substring(5, 2);
                    valB = rValue.Substring(7, 2);
                    return Agk.MakeColor(
                                byte.Parse(valR, NumberStyles.HexNumber, CultureInfo.InvariantCulture),
                                byte.Parse(valG, NumberStyles.HexNumber, CultureInfo.InvariantCulture),
                                byte.Parse(valB, NumberStyles.HexNumber, CultureInfo.InvariantCulture)
                           );
                }
            }
            else if(rValue[0] == 'r')
            {
                rValue = rValue.Substring(4, rValue.Length - 4);
                var c = rValue.Split(',');
                {
                    return Agk.MakeColor(Convert.ToUInt32(c[0]), Convert.ToUInt32(c[1]), Convert.ToUInt32(c[2]));
                }
            }

            return 0;
        }

        public static bool HasOwnProperty(dynamic rObject, string rProperty)
        {
            return ((JObject)rObject).Property(rProperty) != null;
        }

    }

    public struct ParsedSizeData
    {
        public bool IsPercent;
        public int Value;
    }

}
