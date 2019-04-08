using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using MyMedia = System.Windows.Media;
using iTextSharp;
using iTextSharp.awt.geom;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using FluxJpeg.Core.Encoder;
using FluxJpeg.Core.Filtering;
using Draw = System.Drawing;
using FluxJpeg.Core.Decoder;
using Ionic;
using Ionic.Zip;

namespace RagemakerToPDF
{
    class RagecomicToPDF
    {



        static int rowHeight = 239; // row height excluding borders/gridlines
        static int rowWidth = 324; // row width excluding borders/gridlines
        static int borderThickness = 1; // It's just 1. Won't really change, but whatever. If you change it, things will get fucky.

        public static bool MakePDF(Ragecomic comic, string pdffile)
        {
            using (System.IO.FileStream fs = new FileStream(pdffile, FileMode.Create))
            {

                int rows = (int)Math.Ceiling((double)comic.panels / 2);
                int width = rowWidth * 2 + borderThickness * 3;
                int height = rowHeight * rows + borderThickness * rows + borderThickness; // 239 per row, plus 1 pixel border per row, plus 1 pixel extra border

                ZipFile drawImageZip = new ZipFile();
                bool hasDrawImages = comic.items.getDrawImageCount() > 0;

                if (hasDrawImages)
                {
                    drawImageZip.Dispose();
                    string addition = "";
                    int nr = 1;
                    while (File.Exists(pdffile + ".drawimages" + addition + ".zip"))
                    {
                        addition = "_" + (nr++).ToString();
                    }
                    drawImageZip = new ZipFile(pdffile + ".drawimages" + addition + ".zip");
                }


                // Create an instance of the document class which represents the PDF document itself.
                Rectangle pageSize = new Rectangle(width, height);
                Document document = new Document(pageSize, 0, 0, 0, 0);
                // Create an instance to the PDF file by creating an instance of the PDF
                // Writer class using the document and the filestrem in the constructor.

                PdfWriter writer = PdfWriter.GetInstance(document, fs);

                document.AddAuthor("Derp");

                document.AddCreator("RagemakerToPDF");

                document.AddKeywords("rage, comic");

                document.AddSubject("A rage comic");

                document.AddTitle("A rage comic");
                // Open the document to enable you to write to the document

                document.Open();


                PdfContentByte cb = writer.DirectContent;

                // Fill background with white
                Rectangle rect = new iTextSharp.text.Rectangle(0, 0, width, height);
                rect.BackgroundColor = new BaseColor(255, 255, 255);
                cb.Rectangle(rect);

                // Draw grid on bottom if it isn't set to be on top.
                if (!comic.gridAboveAll && comic.showGrid)
                {
                    DrawGrid(cb, width, height, rows, comic);
                }

                //List<MyMedia.FontFamily> families =  MyMedia.Fonts.SystemFontFamilies.ToList();

                // This was a neat idea, but it's much too slow
                //List<string> files = FontFinder.GetFilesForFont("Courier New").ToList();

                //string filename = FontFinder.GetSystemFontFileName(families[0].)

                //Draw.Font testFont = new Draw.Font(new Draw.FontFamily("Courier New"),12f,Draw.FontStyle.Regular  | Draw.FontStyle.Bold);

                string courierPath = FontFinder.GetSystemFontFileName("Courier New", true);
                string courierBoldPath = FontFinder.GetSystemFontFileName("Courier New", true, Draw.FontStyle.Bold);
                string tahomaPath = FontFinder.GetSystemFontFileName("Tahoma", true, Draw.FontStyle.Bold);

                // Define base fonts
                Font[] fonts = new Font[3];
                fonts[0] = new Font(BaseFont.CreateFont(courierPath, BaseFont.CP1252, BaseFont.EMBEDDED));
                fonts[1] = new Font(BaseFont.CreateFont(courierBoldPath, BaseFont.CP1252, BaseFont.EMBEDDED));
                fonts[2] = new Font(BaseFont.CreateFont(tahomaPath, BaseFont.CP1252, BaseFont.EMBEDDED));
                /*fonts[0] = BaseFont.CreateFont("fonts/TRCourierNew.ttf", BaseFont.CP1252, BaseFont.EMBEDDED);
                   fonts[1] = BaseFont.CreateFont("fonts/TRCourierNewBold.ttf", BaseFont.CP1252, BaseFont.EMBEDDED);
                   fonts[2] = new Font(BaseFont.CreateFont("fonts/Tahoma-Bold.ttf", BaseFont.CP1252, BaseFont.EMBEDDED));*/

                int drawimageindex = 0;

                int index = 0;
                foreach (var item in comic.items)
                {
                    if (item is Face)
                    {
                        Face face = (Face)item;
                        Image pdfImage;
                        System.Drawing.Image faceImage;
                        // If mirroring is necessary, we have to read the image via C# and use RotateFlip, as iTextSharp doesn't support mirroring.
                        if (face.mirrored)
                        {
                            // If it's a JPEG, open it with Flux.JPEG.Core, because the internal JPEG decoder (and also LibJpeg.NET) creates weird dithering artifacts with greyscale JPGs, which some of the rage faces are.
                            if (Path.GetExtension(face.file).ToLower() == ".jpeg" || Path.GetExtension(face.file).ToLower() == ".jpg")
                            {

                                FileStream jpegstream = File.OpenRead("images/" + face.file);
                                FluxJpeg.Core.DecodedJpeg myJpeg = new JpegDecoder(jpegstream).Decode();

                                // Only use this JPEG decoder if the colorspace is Gray. Otherwise the normal one is just fine.
                                if (myJpeg.Image.ColorModel.colorspace == FluxJpeg.Core.ColorSpace.Gray)
                                {

                                    myJpeg.Image.ChangeColorSpace(FluxJpeg.Core.ColorSpace.YCbCr);
                                    faceImage = myJpeg.Image.ToBitmap();
                                } else
                                {
                                    faceImage = System.Drawing.Image.FromFile("images/" + face.file);
                                }
                            }
                            else
                            {
                                faceImage = System.Drawing.Image.FromFile("images/" + face.file);
                            }

                            // Apply mirroring
                            faceImage.RotateFlip(System.Drawing.RotateFlipType.RotateNoneFlipX);
                            pdfImage = Image.GetInstance(faceImage, System.Drawing.Imaging.ImageFormat.Png);
                        }
                        else
                        {
                            // Just let iTextSharp handle it if no mirroring is required. Will also save space (presumably)
                            pdfImage = Image.GetInstance("images/" + face.file);
                        }

                        pdfImage.ScalePercent(face.scalex * 100, face.scaley * 100);
                        pdfImage.Rotation = -(float)Math.PI * face.rotation / 180.0f;
                        pdfImage.SetAbsolutePosition(item.x, (height - item.y) - pdfImage.ScaledHeight);

                        // Set opacity to proper value 
                        if (face.opacity < 1)
                        {
                            PdfGState graphicsState = new PdfGState();
                            graphicsState.FillOpacity = face.opacity;
                            cb.SetGState(graphicsState);
                        }

                        cb.AddImage(pdfImage);

                        // Set back to normal
                        if (face.opacity < 1)
                        {
                            PdfGState graphicsState = new PdfGState();
                            graphicsState.FillOpacity = 1f;
                            cb.SetGState(graphicsState);
                        }
                    }

                    else if (item is DrawImage)
                    {
                        DrawImage drawimage = (DrawImage)item;

                        drawImageZip.AddEntry("drawimage_" + (drawimageindex++).ToString() + ".png", drawimage.imagedata);

                        System.Drawing.Image pngImage = System.Drawing.Image.FromStream(new MemoryStream(drawimage.imagedata));
                        Image pdfImage = Image.GetInstance(pngImage, System.Drawing.Imaging.ImageFormat.Png);

                        // Rotation is NOT to be applied. Ragemaker actually has a bug that causes it to save rotated images in their rotated form, but save the rotation value anyway
                        // Thus rotating the image by the rotation value will actually rotate them double compared to what they originally looked like
                        // The irony is that ragemaker *itself* cannot properly load an xml it created with this rotation value, as it will also apply the rotation
                        // As such, this tool is currently the only way to correctly display that .xml file as it was originally meant to look, not even ragemaker itself can properly load it again.
                        //pdfImage.Rotation = -(float)Math.PI * item.rotation / 180.0f;

                        pdfImage.SetAbsolutePosition(item.x, (height - item.y) - pdfImage.ScaledHeight);

                        // Opacity likewise seems to be baked in, and in fact the opacity value doesn't even exist.
                        // Implementing it anyway, in case it ever becomes a thing.
                        if (drawimage.opacity < 1)
                        {
                            PdfGState graphicsState = new PdfGState();
                            graphicsState.FillOpacity = drawimage.opacity;
                            cb.SetGState(graphicsState);
                        }
                        cb.AddImage(pdfImage);

                        // Set back to normal
                        if (drawimage.opacity < 1)
                        {
                            PdfGState graphicsState = new PdfGState();
                            graphicsState.FillOpacity = 1f;
                            cb.SetGState(graphicsState);
                        }
                    }


                    else if (item is Text)
                    {

                        int padding = 4;
                        Text text = (Text)item;

                        // Create template
                        PdfTemplate xobject = cb.CreateTemplate(text.width, text.height);

                        // Background color (if set)
                        if (text.bgOn)
                        {
                            Rectangle bgRectangle = new Rectangle(0, 0, text.width, text.height);
                            System.Drawing.Color bgColor = System.Drawing.ColorTranslator.FromHtml(text.bgColor);
                            rect.BackgroundColor = new BaseColor(bgColor.R, bgColor.G, bgColor.B, (int)Math.Floor(text.opacity * 255));

                            xobject.Rectangle(rect);
                        }

                        // Create text
                        Rectangle textangle = new Rectangle(padding, 0, text.width - padding, text.height);
                        ColumnText ct = new ColumnText(xobject);
                        ct.SetSimpleColumn(textangle);
                        Paragraph paragraph = new Paragraph(text.text);

                        Font myFont = fonts[text.style];



                        // More specific treatment if it's an AnyFont element which allows the user to select any font and styles, not just the normal 3 presets
                        // This isn't perfect, as the current FontFinder doesn't indicate whether he actually found an Italic/Bold typeface, hence it's not possible
                        // to determine whether faux-italic/faux-bold should be applied. Currently it will only work correctly if each used font has a specific typeface
                        // for the needed styles (bold or italic), otherwise incorrect results. 
                        // TODO Fix, for example let FontFinder return array of strings, one of which is indicating the suffix that was found.
                        if (text is AnyFontText)
                        {
                            AnyFontText anyfont = (AnyFontText)text;
                            string fontname = anyfont.font;
                            string fontfile = "";
                            if (anyfont.bold)
                            {
                                fontfile = FontFinder.GetSystemFontFileName(fontname, true, Draw.FontStyle.Bold);
                                int fontStyle = 0;
                                if (anyfont.italic)
                                {
                                    fontStyle |= Font.ITALIC;
                                }
                                if (anyfont.underline)
                                {
                                    fontStyle |= Font.UNDERLINE;
                                }
                                myFont = new Font(BaseFont.CreateFont(fontfile, BaseFont.CP1252, BaseFont.EMBEDDED), 100f, fontStyle);
                            } else if (anyfont.italic)
                            {
                                fontfile = FontFinder.GetSystemFontFileName(fontname, true, Draw.FontStyle.Italic);
                                int fontStyle = 0;
                                if (anyfont.underline)
                                {
                                    fontStyle |= Font.UNDERLINE;
                                }
                                myFont = new Font(BaseFont.CreateFont(fontfile, BaseFont.CP1252, BaseFont.EMBEDDED), 100f, fontStyle);
                            } else
                            {
                                fontfile = FontFinder.GetSystemFontFileName(fontname, true, Draw.FontStyle.Regular);
                                int fontStyle = 0;
                                if (anyfont.underline)
                                {
                                    fontStyle |= Font.UNDERLINE;
                                }
                                myFont = new Font(BaseFont.CreateFont(fontfile, BaseFont.CP1252, BaseFont.EMBEDDED), 100f, fontStyle);
                            }
                        }



                        myFont.Size = text.size;
                        System.Drawing.Color color = (System.Drawing.Color)(new System.Drawing.ColorConverter()).ConvertFromString(text.color);
                        myFont.Color = new BaseColor(color.R, color.G, color.B, (int)Math.Floor(text.opacity * 255));
                        paragraph.Font = myFont;
                        paragraph.Alignment = text.align == Text.ALIGN.LEFT ? PdfContentByte.ALIGN_LEFT : (text.align == Text.ALIGN.RIGHT ? PdfContentByte.ALIGN_RIGHT : PdfContentByte.ALIGN_CENTER);
                        paragraph.SetLeading(0, 1.12f);
                        ct.AddElement(paragraph);
                        ct.Go();


                        // Angle to radians
                        float angle = (float)Math.PI * text.rotation / 180.0f;

                        // Calculate Bounding Box size for correct placement later
                        GraphicsPath gp = new GraphicsPath();
                        gp.AddRectangle(new System.Drawing.Rectangle(0, 0, (int)Math.Round(text.width), (int)Math.Round(text.height)));
                        Matrix translateMatrix = new Matrix();
                        translateMatrix.RotateAt(text.rotation, new System.Drawing.PointF(text.width / 2, text.height / 2));
                        gp.Transform(translateMatrix);
                        var gbp = gp.GetBounds();
                        float newWidth = gbp.Width, newHeight = gbp.Height;

                        // Create correct placement
                        // Background info: I rotate around the center of the text box, thus the center of the text box is what I attempt to place correctly with the initial .Translate()
                        AffineTransform transform = new AffineTransform();
                        transform.Translate(item.x + newWidth / 2 - text.width / 2, height - (item.y + newHeight / 2 - text.height / 2) - text.height);
                        transform.Rotate(-angle, text.width / 2, text.height / 2);

                        cb.AddTemplate(xobject, transform);
                    }


                    index++;
                }

                if (comic.gridAboveAll && comic.showGrid)
                {
                    DrawGrid(cb, width, height, rows, comic);
                }

                //document.Add(new Paragraph("Hello World!"));
                // Close the document

                document.Close();
                // Close the writer instance

                writer.Close();
                // Always close open filehandles explicity
                fs.Close();

                if (hasDrawImages)
                {
                    drawImageZip.Save();
                }
                drawImageZip.Dispose();

            }


            return false;
        }


        public static void DrawGrid(PdfContentByte cb, int width, int height, int rows, Ragecomic comic)
        {

            if (!comic.showGrid)
            {
                return;
            }

            // Outer box
            Rectangle rect = new iTextSharp.text.Rectangle(0, 0, width, height);
            //rect.Border = iTextSharp.text.Rectangle.LEFT_BORDER | iTextSharp.text.Rectangle.RIGHT_BORDER;
            rect.Border = iTextSharp.text.Rectangle.BOX;
            rect.BorderWidth = 1;
            rect.BorderColor = new BaseColor(0, 0, 0);
            cb.Rectangle(rect);


            int matricesCount = comic.gridLines.GetLength(0);


            cb.SetColorStroke(new BaseColor(0, 0, 0));
            PdfGState graphicsState = new PdfGState();
            graphicsState.FillOpacity = 0f;
            cb.SetGState(graphicsState);

            // The first dimension of the array represents "rows" in a sense.
            // And the second dimension has 5 values each, representing various gridlines.
            // Gridlines row format:
            // 0 - bottom left
            // 1 - center
            // 2 - bottom right
            // 3 - left diagonal 
            // 4 - right diagonal
            // diagonals can be 0,1 or 2. 
            // 0 - disabled
            // 1 - bottom left to top right
            // 2 - top left to bottom right
            for (int i = 0; i < matricesCount; i++)
            {
                int ipp = i + 1;

                // bottom left
                if (comic.gridLines[i, 0] > 0) {
                    cb.MoveTo(0,height-rowHeight*ipp- borderThickness * ipp);
                    cb.LineTo(borderThickness + rowWidth,height-rowHeight* ipp - borderThickness * ipp);
                    cb.Stroke();
                }

                // center vertical
                if (comic.gridLines[i, 1] > 0)
                {
                    cb.MoveTo(borderThickness + rowWidth, height - rowHeight * ipp - borderThickness * ipp);
                    cb.LineTo(borderThickness + rowWidth, height - rowHeight * i - borderThickness * i);
                    cb.Stroke();
                }

                // bottom right
                if (comic.gridLines[i, 2] > 0)
                {
                    cb.MoveTo(borderThickness + rowWidth, height - rowHeight * ipp - borderThickness * ipp);
                    cb.LineTo(borderThickness * 2 + rowWidth * 2, height - rowHeight * ipp - borderThickness * ipp);
                    cb.Stroke();
                }

                // left: bottom left to top right
                if (comic.gridLines[i, 3] == 1)
                {
                    cb.MoveTo(0, height - rowHeight * ipp - borderThickness * ipp);
                    cb.LineTo(borderThickness + rowWidth, height - rowHeight * i - borderThickness * i);
                    cb.Stroke();
                }
                // left: top left to bottom right
                else if(comic.gridLines[i, 3] == 2) {
                    cb.MoveTo(0, height - rowHeight * i - borderThickness * i);
                    cb.LineTo(borderThickness + rowWidth, height - rowHeight * ipp - borderThickness * ipp);
                    cb.Stroke();
                }

                // right: bottom left to top right
                if (comic.gridLines[i, 4] == 1)
                {
                    cb.MoveTo(borderThickness+ rowWidth, height - rowHeight * ipp - borderThickness * ipp);
                    cb.LineTo(borderThickness*2 + rowWidth*2, height - rowHeight * i - borderThickness * i);
                    cb.Stroke();
                }
                // right: top left to bottom right
                else if (comic.gridLines[i, 4] == 2) {
                    cb.MoveTo(borderThickness + rowWidth, height - rowHeight * i - borderThickness * i);
                    cb.LineTo(borderThickness * 2 + rowWidth * 2, height - rowHeight * ipp - borderThickness * ipp);
                    cb.Stroke();
                }

            }

            // Lastly, the existence of the center gridline in the bottommost row is determined by the panel count
            if (comic.panels % 2 == 0) {

                int i = matricesCount;
                int ipp = i + 1;

                // center vertical
                cb.MoveTo(borderThickness + rowWidth, height - rowHeight * ipp - borderThickness * ipp);
                cb.LineTo(borderThickness + rowWidth, height - rowHeight * i - borderThickness * i);
                cb.Stroke();
            }


            graphicsState.FillOpacity = 1f;
            cb.SetGState(graphicsState);
        }

    }
}
