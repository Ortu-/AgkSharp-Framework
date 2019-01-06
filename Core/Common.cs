using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AGKCore
{
    public static class Data
    {

        public static int GetBit(int rBit, int rData)
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
    }
}
