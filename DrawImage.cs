using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RagemakerToPDF
{
    // For both types "Draw" and "Image", they are essentially the same, they only differ in function inside ragemaker
    class DrawImage : RageItem
    {
        // PNG afaik
        public byte[] imagedata = new byte[1];
    }
}
