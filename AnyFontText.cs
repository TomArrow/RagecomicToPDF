using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RagemakerToPDF
{
    class AnyFontText : Text
    {
        /*
         *  Like a normal text, but with specified font, bold, underline & italic
         * 
                 * <AnyFont>
            <itemx>292</itemx>
            <itemy>101</itemy>
            <width>250</width>
            <height>50</height>
            <font>Cooperman</font>
            <color>0</color>
            <size>16</size>
            <text>test</text>
            <bold>false</bold>
            <italic>false</italic>
            <underline>false</underline>
            <align>left</align>
            <bgOn>false</bgOn>
            <bgColor>16777215</bgColor>
            <opacity>1</opacity>
          </AnyFont>
          */
        public string font = "Courier New";
        public bool bold = false;
        public bool italic = false;
        public bool underline = false;
    }
}
