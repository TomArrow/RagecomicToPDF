using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RagemakerToPDF
{
    class RageList<T> : List<T>
    {
        public int getDrawImageCount()
        {
            int count = 0;
            foreach(var item in this)
            {
                if(item is DrawImage)
                {
                    count++;
                }
            }
            return count;
        }
    }
}
