using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RagemakerToPDF
{
    class Face : RageItem
    {
        /* 
         * <Face>
            <itemx>13.5</itemx>
            <itemy>67</itemy>
            <facename>Neutral/37.png</facename>
            <scalex>0.6868686868686867</scalex>
            <scaley>0.6868686868686867</scaley>
            <mirrored>true</mirrored>
            <rotation>0</rotation>
            <opacity>1</opacity>
          </Face>
         * 
         */
        public string file = "";
        public float scalex = 0.0f;
        public float scaley = 0.0f;
        public bool mirrored = false;
    }
}
