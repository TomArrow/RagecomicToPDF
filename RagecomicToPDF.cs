using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using iTextSharp;
using iTextSharp.text;
using iTextSharp.text.pdf;

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

                if (!comic.gridAboveAll)
                {
                    DrawGrid(cb, width, height, rows, comic);
                }

                BaseFont[] fonts = new BaseFont[3];
                fonts[0] = BaseFont.CreateFont("fonts/TRCourierNew.ttf", BaseFont.CP1252, BaseFont.EMBEDDED);
                fonts[1] = BaseFont.CreateFont("fonts/TRCourierNewBold.ttf", BaseFont.CP1252, BaseFont.EMBEDDED);
                fonts[2] = BaseFont.CreateFont("fonts/Tahoma-Bold.ttf", BaseFont.CP1252, BaseFont.EMBEDDED);

                int index = 0;
                foreach (var item in comic.items)
                {
                    if(item is Face)
                    {
                        Face face = (Face)item;
                        System.Drawing.Image pngImage = System.Drawing.Image.FromFile("images/"+face.file);
                        if (face.mirrored){
                            pngImage.RotateFlip(System.Drawing.RotateFlipType.RotateNoneFlipX);
                        }
                        Image pdfImage = Image.GetInstance(pngImage, System.Drawing.Imaging.ImageFormat.Png);
                        pdfImage.ScalePercent(face.scalex * 100, face.scaley * 100);
                        pdfImage.SetAbsolutePosition(item.x, (height - item.y)-pdfImage.ScaledHeight);
                        cb.AddImage(pdfImage);
                    }

                    else if(item is DrawImage)
                    {
                        DrawImage drawimage = (DrawImage)item;
                        System.Drawing.Image pngImage = System.Drawing.Image.FromStream(new MemoryStream(drawimage.imagedata));
                        
                        Image pdfImage = Image.GetInstance(pngImage, System.Drawing.Imaging.ImageFormat.Png);
                        pdfImage.SetAbsolutePosition(item.x, (height - item.y) - pdfImage.ScaledHeight);
                        cb.AddImage(pdfImage);
                    }

                    else if(item is Text)
                    {

                        int padding = 5;
                        Text text = (Text)item;
                        //TextField tf = new TextField(writer, new Rectangle(item.x,height - item.y - text.height,text.width,text.height), "text" + (index.ToString()));
                        //tf.Text = text.text;
                        //cb.AddAnnotation(tf.GetTextField(), true);
                        Rectangle textangle = new Rectangle(item.x +padding, height - item.y, item.x+text.width-padding, height - item.y- text.height - padding*2);
                        ColumnText ct = new ColumnText(cb);
                        ct.SetSimpleColumn(textangle);
                        Paragraph paragraph = new Paragraph(text.text);
                        Font myFont = new Font(fonts[text.style]);
                       
                        myFont.Size = text.size;
                        System.Drawing.Color color = (System.Drawing.Color) (new System.Drawing.ColorConverter()).ConvertFromString(text.color);
                        myFont.Color = new BaseColor(color);
                        paragraph.Font =  myFont;
                        paragraph.Alignment = text.align == Text.ALIGN.LEFT ? PdfContentByte.ALIGN_LEFT : (text.align == Text.ALIGN.RIGHT ? PdfContentByte.ALIGN_RIGHT : PdfContentByte.ALIGN_CENTER);
                        paragraph.SetLeading(0,1.12f);
                        ct.AddElement(paragraph);
                        ct.Go();
                    }


                    index++;
                }

                if (comic.gridAboveAll)
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
