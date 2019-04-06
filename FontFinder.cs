using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using System.Drawing.Text;
using System.Drawing;
using System.Text.RegularExpressions;
using Microsoft.Win32;

using Draw = System.Drawing;

namespace RagemakerToPDF
{
    class FontFinder
    {

        public static string systemFontPath = Environment.GetFolderPath(Environment.SpecialFolder.Fonts);

        //Found here: https://stackoverflow.com/questions/21525377/retrieve-filename-of-a-font

        static Dictionary<string, List<string>> _fontNameToFiles;
        /// <summary>
        /// This is a brute force way of finding the files that represent a particular
        /// font family.
        /// The first call may be quite slow.
        /// Only finds font files that are installed in the standard directory.
        /// Will not discover font files installed after the first call.
        /// </summary>
        /// <returns>enumeration of file paths (possibly none) that contain data
        /// for the specified font name</returns>
        public static IEnumerable<string> GetFilesForFont(string fontName)
        {
            if (_fontNameToFiles == null)
            {
                _fontNameToFiles = new Dictionary<string, List<string>>();
                foreach (var fontFile in Directory.GetFiles(Environment.GetFolderPath(Environment.SpecialFolder.Fonts)))
                {
                    var fc = new PrivateFontCollection();
                    try
                    {
                        fc.AddFontFile(fontFile);
                    }
                    catch (FileNotFoundException)
                    {
                        continue; // not sure how this can happen but I've seen it.
                    }
                    var name = fc.Families[0].Name;
                    // If you care about bold, italic, etc, you can filter here.
                    List<string> files;
                    if (!_fontNameToFiles.TryGetValue(name, out files))
                    {
                        files = new List<string>();
                        _fontNameToFiles[name] = files;
                    }
                    files.Add(fontFile);
                }
            }
            List<string> result;
            if (!_fontNameToFiles.TryGetValue(fontName, out result))
                return new string[0];
            return result;
        }

        public static string GetSystemFontFileName(string fontFamilyName, bool absolute = false, FontStyle style = FontStyle.Regular)
        {
            Draw.Font font = new Draw.Font(new Draw.FontFamily(fontFamilyName), 12f, style);
            string fontName = GetSystemFontFileName(font);
            string fontPath = systemFontPath + "/" + fontName;
            return absolute ? fontPath : fontName;
        }

        public static string GetSystemFontFileName(Font font)
            {
                RegistryKey fonts = null;
                try
                {
                    fonts = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows NT\CurrentVersion\Fonts", false);
                    if (fonts == null)
                    {
                        fonts = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\Fonts", false);
                        if (fonts == null)
                        {
                            throw new Exception("Can't find font registry database.");
                        }
                    }

                    string suffix = "";
                    if (font.Bold)
                        suffix += "(?: Bold)?";
                    if (font.Italic)
                        suffix += "(?: Italic)?";

                    var regex = new Regex(@"^(?:.+ & )?" + Regex.Escape(font.Name) + @"(?: & .+)?(?<suffix>" + suffix + @") \(TrueType\)$", RegexOptions.Compiled);

                    string[] names = fonts.GetValueNames();

                    string name = names.Select(n => regex.Match(n)).Where(m => m.Success).OrderByDescending(m => m.Groups["suffix"].Length).Select(m => m.Value).FirstOrDefault();

                    if (name != null)
                    {
                        return fonts.GetValue(name).ToString();
                    }
                    else
                    {
                        return null;
                    }
                }
                finally
                {
                    if (fonts != null)
                    {
                        fonts.Dispose();
                    }
                }
            }



    }
}
