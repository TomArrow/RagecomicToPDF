using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Xml;
using System.Threading.Tasks;

namespace RagemakerToPDF
{
    class Ragecomic
    {
        /*
         * <panels>8</panels>
         * <gridLinesArray>111001110011100</gridLinesArray>
         * <gridAboveAll>true</gridAboveAll>
         * <showGrid>true</showGrid>
         * <redditWatermark>false</redditWatermark>
         */
        public int panels = 2;
        public bool[] gridLines = new bool[1];
        public bool gridAboveAll = true;
        public bool showGrid = true;
        public bool redditWatermark = false;
        public RageList<RageItem> items = new RageList<RageItem>();


    }
}
