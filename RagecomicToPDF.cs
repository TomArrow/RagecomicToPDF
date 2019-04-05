using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using iTextSharp;
using iTextSharp.awt.geom;
using iTextSharp.text;
using iTextSharp.text.pdf;
using System.Drawing.Drawing2D;
using System.Runtime.InteropServices;
using FluxJpeg.Core.Encoder;
using FluxJpeg.Core.Filtering;
using FluxJpeg.Core.Decoder;
using Ionic;
using Ionic.Zip;

namespace RagemakerToPDF
{
    class RagecomicToPDF
    {

        public static bool MakePDF(Ragecomic comic, string pdffile)
        {
            using (System.IO.FileStream fs = new FileStream(pdffile, FileMode.Create))
            {

                int rows = (int)Math.Ceiling((double)comic.panels / 2);
                int width = 651;
                int height = 239 * rows + 1 * rows + 1; // 239 per row, plus 1 pixel border per row, plus 1 pixel extra border

                ZipFile drawImageZip = new ZipFile();
                bool hasDrawImages = comic.items.getDrawImageCount() > 0;

                if (hasDrawImages)
                {
                    drawImageZip.Dispose();
                    string addition = "";
                    int nr = 1;
                    while(File.Exists(pdffile + ".drawimages"+addition+".zip"))
                    {
                        addition = "_"+(nr++).ToString();
                    }
                    drawImageZip = new ZipFile(pdffile + ".drawimages" + addition + ".zip");
                }


                // Create an instance of the document class which represents the PDF document itself.
                Rectangle pageSize = new Rectangle(width,height);
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

                // Add a simple and wellknown phrase to the document in a flow layout manner



                PdfContentByte cb = writer.DirectContent;

                if (!comic.gridAboveAll && comic.showGrid)
                {
                    DrawGrid(cb, width, height, rows, comic);
                }

                // Fill background with white
                Rectangle rect = new iTextSharp.text.Rectangle(0, 0, width, height);
                rect.BackgroundColor = new BaseColor(255, 255, 255);
                cb.Rectangle(rect);

                BaseFont[] fonts = new BaseFont[3];
                fonts[0] = BaseFont.CreateFont("fonts/TRCourierNew.ttf", BaseFont.CP1252, BaseFont.EMBEDDED);
                fonts[1] = BaseFont.CreateFont("fonts/TRCourierNewBold.ttf", BaseFont.CP1252, BaseFont.EMBEDDED);
                fonts[2] = BaseFont.CreateFont("fonts/Tahoma-Bold.ttf", BaseFont.CP1252, BaseFont.EMBEDDED);

                int drawimageindex = 0;

                int index = 0;
                foreach (var item in comic.items)
                {
                    if(item is Face)
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
                                if(myJpeg.Image.ColorModel.colorspace == FluxJpeg.Core.ColorSpace.Gray)
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
                        pdfImage.SetAbsolutePosition(item.x, (height - item.y)-pdfImage.ScaledHeight);

                        // Set opacity to proper value 
                        if(face.opacity < 1)
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

                    else if(item is DrawImage)
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


                    else if(item is Text)
                    {

                        int padding = 5;
                        Text text = (Text)item;

                        // Create template
                        PdfTemplate xobject = cb.CreateTemplate(text.width,text.height);

                        // Background color (if set)
                        if (text.bgOn)
                        {
                            Rectangle bgRectangle = new Rectangle(0, 0, text.width, text.height);
                            System.Drawing.Color bgColor = System.Drawing.ColorTranslator.FromHtml(text.bgColor);
                            rect.BackgroundColor = new BaseColor(bgColor.R, bgColor.G, bgColor.B, (int)Math.Floor(text.opacity*255));
                            
                            xobject.Rectangle(rect);
                        }

                        // Create text
                        Rectangle textangle = new Rectangle(padding, 0, text.width - padding, text.height);
                        ColumnText ct = new ColumnText(xobject);
                        ct.SetSimpleColumn(textangle);
                        Paragraph paragraph = new Paragraph(text.text);
                        Font myFont = new Font(fonts[text.style]);
                        myFont.Size = text.size;
                        System.Drawing.Color color = (System.Drawing.Color) (new System.Drawing.ColorConverter()).ConvertFromString(text.color);
                        myFont.Color = new BaseColor(color.R,color.G,color.B, (int)Math.Floor(text.opacity * 255));
                        paragraph.Font =  myFont;
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
                        translateMatrix.RotateAt(text.rotation, new System.Drawing.PointF(text.width/2, text.height/2));
                        gp.Transform(translateMatrix);
                        var gbp = gp.GetBounds();
                        float newWidth = gbp.Width, newHeight = gbp.Height;
                        
                        // Create correct placement
                        // Background info: I rotate around the center of the text box, thus the center of the text box is what I attempt to place correctly with the initial .Translate()
                        AffineTransform transform = new AffineTransform();
                        transform.Translate(item.x+newWidth/2-text.width/2,height-(item.y+newHeight/2-text.height/2)-text.height);
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


        public static void DrawGrid(PdfContentByte cb, int width, int height, int rows,Ragecomic comic)
        {
            // Outer box
            Rectangle rect = new iTextSharp.text.Rectangle(0, 0, width, height);
            //rect.Border = iTextSharp.text.Rectangle.LEFT_BORDER | iTextSharp.text.Rectangle.RIGHT_BORDER;
            rect.Border = iTextSharp.text.Rectangle.BOX;
            rect.BorderWidth = 1;
            rect.BorderColor = new BaseColor(0, 0, 0);
            cb.Rectangle(rect);

            // Rows
            // This section is really weird and wonky, but somehow it works. Someday gotta fix that shit.
            for (int i = 0; i <= rows; i++)
            {
                // Horizontal
                if (i != rows)
                {
                    rect = new iTextSharp.text.Rectangle(0, 0 + (239 + 1) * (i + 1), width, 239 + 1);
                    rect.Border = iTextSharp.text.Rectangle.BOTTOM_BORDER;
                    rect.BorderWidth = 1;
                    rect.BorderColor = new BaseColor(0, 0, 0);
                    cb.Rectangle(rect);
                }

                // Vertical
                if (comic.panels % 2 > 0 && i == 0) { continue; }
                rect = new iTextSharp.text.Rectangle(0, (239 + 1) * (i), 324 + 1 + 1, 239 + 1);
                rect.Border = iTextSharp.text.Rectangle.RIGHT_BORDER;
                rect.BorderWidth = 1;
                rect.BorderColor = new BaseColor(0, 0, 0);
                cb.Rectangle(rect);

            }
        }

    }
}
