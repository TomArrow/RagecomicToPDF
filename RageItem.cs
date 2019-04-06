using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;
using System.Web;

namespace RagemakerToPDF
{
    class RageItem
    {
        public float x, y;
        public float opacity = 1f;
        public float rotation = 0.0f;

        public override string ToString()
        {
            return this.GetType().Name + "(" + this.x + "," + this.y + "," + this.opacity + "," + this.rotation + ")";
        }

        public static async Task<RageItem> createRageItem(string elementName, XmlReader subtree)
        {
            RageItem item = new RageItem();
            switch (elementName)
            {
                case "Face":
                    item = new Face();
                    break;
                case "Text":
                    item = new Text();
                    break;
                case "AnyFont":
                    item = new AnyFontText();
                    break;
                case "Draw":
                case "Image":
                    item = new DrawImage(); 
                    break;
                default:
                    break;
            }

            while(await subtree.ReadAsync())
            {
                switch (subtree.Name)
                {
                    case "itemx":
                        item.x = float.Parse(subtree.ReadInnerXml());
                        break;
                    case "itemy":
                        item.y = float.Parse(subtree.ReadInnerXml());
                        break;
                    case "opacity":
                        item.opacity = float.Parse(subtree.ReadInnerXml());
                        break;
                    case "rotation":
                        item.rotation = float.Parse(subtree.ReadInnerXml());
                        break;
                    default:
                        break;
                }

                //Image or Draw
                if(item is DrawImage)
                {
                    switch (subtree.Name)
                    {
                        //DrawImage
                        case "bytes":
                            byte[] decoded = System.Convert.FromBase64String(subtree.ReadInnerXml());
                            ((DrawImage)item).imagedata = decoded;
                            break;
                        default:
                            break;
                    }
                }

                //Text
                if (item is Text)
                {
                    switch (subtree.Name)
                    {
                        case "width":
                            ((Text)item).width = float.Parse(subtree.ReadInnerXml());
                            break;
                        case "height":
                            ((Text)item).height = float.Parse(subtree.ReadInnerXml());
                            break;
                        case "color":
                            ((Text)item).color = "#" + (int.Parse(subtree.ReadInnerXml())).ToString("X").PadLeft(6, "0"[0]);
                            break;
                        case "style":
                            ((Text)item).style = int.Parse(subtree.ReadInnerXml());
                            break;
                        case "size":
                            ((Text)item).size = float.Parse(subtree.ReadInnerXml());
                            break;
                        case "text":
                            ((Text)item).text = HttpUtility.HtmlDecode(subtree.ReadInnerXml());
                            break;
                        case "align":
                            string align = subtree.ReadInnerXml();
                            ((Text)item).align = align == "left" ? Text.ALIGN.LEFT : (align == "right" ? Text.ALIGN.RIGHT : Text.ALIGN.CENTER);
                            break;
                        case "bgOn":
                            ((Text)item).bgOn = bool.Parse(subtree.ReadInnerXml());
                            break;
                        case "bgColor":
                            ((Text)item).bgColor = "#" + (int.Parse(subtree.ReadInnerXml())).ToString("X").PadLeft(6, "0"[0]);
                            break;
                        default:
                            break;
                    }

                    if(item is AnyFontText)
                    {
                        switch (subtree.Name)
                        {
                            case "font":
                                ((AnyFontText)item).font = HttpUtility.HtmlDecode(subtree.ReadInnerXml());
                                break;
                            case "bold":
                                ((AnyFontText)item).bold = bool.Parse(subtree.ReadInnerXml());
                                break;
                            case "italic":
                                ((AnyFontText)item).italic = bool.Parse(subtree.ReadInnerXml());
                                break;
                            case "underline":
                                ((AnyFontText)item).underline = bool.Parse(subtree.ReadInnerXml());
                                break;
                            default:
                                break;
                        }
                    }
                }

                //Face
                if (item is Face) { 
                    switch (subtree.Name)
                    {
                        case "facename":
                            ((Face)item).file = HttpUtility.UrlDecode(subtree.ReadInnerXml());
                            break;
                        case "scalex":
                            ((Face)item).scalex = float.Parse(subtree.ReadInnerXml());
                            break;
                        case "scaley":
                            ((Face)item).scaley = float.Parse(subtree.ReadInnerXml());
                            break;
                        case "mirrored":
                            ((Face)item).mirrored = bool.Parse(subtree.ReadInnerXml());
                            break;
                        default:
                            break;
                    }
                }
            }

            return item;
        }
    }
}
