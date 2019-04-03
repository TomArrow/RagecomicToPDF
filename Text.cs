using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RagemakerToPDF
{
    class Text : RageItem
    {
        /*
         * <Text>
            <itemx>463</itemx>
            <itemy>65</itemy>
            <width>181</width>
            <height>181</height>
            <color>39219</color>
            <style>1</style>
            <size>16</size>
            <rotation>0</rotation>
            <text>"HEY PAUL, I HEARD YOU WANTED TO BUY A NICE GOOD JUICY STEAK!"-"THAT'S RIGHT, JOSEHPHINE!"
        "WELL, IT'S YOUR LUCKY DAY, BECAUS</text>
            <align>right</align>
            <bgOn>false</bgOn>
            <bgColor>16777215</bgColor>
            <opacity>1</opacity>
          </Text>
          */ 
        public enum ALIGN
        {
            LEFT, CENTER, RIGHT
        };
        public float width = 10f;
        public float height = 10f;
        public string color = "#000000";

        /* 0: Courier (LD?) Regular
         * 1: Courier (LD?) Bold
         * 2: Verdana Bold or Tahoma Bold. Probably Tahoma Bold.
         */
        public int style = 0;
        public float size = 16f;
        public string text = "";
        public ALIGN align = ALIGN.LEFT;
        public bool bgOn = false;
        public string bgColor = "#FFFFFF";

        public override string ToString()
        {
            return this.GetType().Name + "(" + this.x + "," + this.y + "," + this.width + "," + this.height + "," + this.color +"," + this.style +"," + this.size  +"," + this.align + "," + this.opacity + "," + this.rotation + "," + this.text + ")";
        }
    }
}
