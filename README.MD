# RagecomicToPDF

To practice a bit real programming, I programmed a tool to convert ragecomics to PDFs in C#.

It takes .xml files from ragemaker.net and outputs PDFs. The benefit is that each face/image in them retains full quality, whereas ragemaker's normal png output rasterizes the image to a fixed and small resolution.

Text is vector-based too.

So you can print ragecomics as giant posters in best possible quality.

I know it's of limited real life use, but it was fun to make and it taught me a lot about C# and some libraries, most of all iTextSharp. 

## Features:
- Creates Zip file full of any individual png faces/graphics that are saved as base64 bytes in the .xml
- Takes rotation into account (I found this particularly difficult for text, which doesn't have as straightforward rotation and boundingbox-calculation functions as images do)
- Takes opacity into account (but is imperfect for text that has a background color, because the opacity is set separately for text and background, instead of setting it for both as a whole.
- Takes color of text and background color into account.
- Draws a grid and sets document size depending on the number of panels
- Reads grayscale JPEGs with a special library, because the normal library, for whatever reason, creates weird dithering artifacts on grayscale JPEGs.

### Under the hood:
- Somewhat object-oriented, with objects representing the comic and the individual items in it
- Extended List<T> for the items in the ragecomic, so that it can count a particular type of them (individual graphics) to know whether a Zip file must be generated
- Uses FluxJPEG, DotNetZip, iTextSharp libraries
- Has a GUI with a simple scrolling status window and a button that allows file selection
- Conversion is done asynchronously, which means that the status updates are in realtime instead of all coming at once in the end. In other words, it uses threading.
- Uses .NET Framework 4.5. Not really a feature, but interesting. I tried 4.0 first, but it lacked some async (threading) functions I wanted to use.
- Uses .NET's XMLReader for parsing the XML. Also a good experience.
- Uses WPF with a XAML interface. Nothing fancy, really basic, but it's a nice upgrade over commandline.

Overall lots of aspects about C# programming that this allowed me to learn, so I'm pretty happy.

The image in this post shows the improvement in the png (left) over the native png (right).

Sadly there is one bug I can't solve, which is that transparency often results in a  weird grey border in some places, which can destroy some of the comics. Apparently this is due to how PDFs work, but I believe there must be a solution to it. Couldn't find it yet tho.