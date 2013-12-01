using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace StarPan
{
    //internal static class IconGenerator
    //{        
    //    static Font myFont = new Font("Arial Black", 12, FontStyle.Bold, GraphicsUnit.Pixel);

    //    /// <summary>
    //    /// This method generates a nice icon for us
    //    /// </summary>
    //    /// <param name="c">Some letter to display on the icon</param>
    //    /// <param name="ballColor">The color of the ball to be drawn</param>
    //    /// <returns>a new Icon</returns>
    //    internal static Icon CreateIcon(char c, Color ballColor)
    //    {
    //        string letter = c.ToString();
    //        Bitmap bm = new Bitmap(16, 16);

    //        Graphics g = Graphics.FromImage((Image)bm);
    //        g.SmoothingMode = SmoothingMode.AntiAlias;
    //        DrawBall(g, new Rectangle(0, 0, 15, 15), ballColor);

    //        g.DrawString(letter, myFont, Brushes.Black, new Point(4, 4));
    //        g.DrawString(letter, myFont, Brushes.White, new Point(5, 3));

    //        return Icon.FromHandle(bm.GetHicon());
    //    }
    //    static void DrawBall(Graphics g, Rectangle rect, Color c)
    //    {
    //        GraphicsPath path = new GraphicsPath();
    //        path.AddEllipse(rect);

    //        PathGradientBrush pgbrush = new PathGradientBrush(path);
    //        pgbrush.CenterPoint = new Point((rect.Right - rect.Left) / 3 + rect.Left, (rect.Bottom - rect.Top) / 3 + rect.Top);
    //        pgbrush.CenterColor = Color.White;
    //        pgbrush.SurroundColors = new Color[] { c };

    //        g.FillEllipse(pgbrush, rect);
    //        g.DrawEllipse(new Pen(c), rect);
    //    }
    //}

    internal static class UtilityMethods
    {
        ///// <summary>
        ///// Makes the code easier to read by substituting 
        ///// (thisFolder == null) for thisFolder.Exists()
        ///// </summary>
        ///// <param name="item">An MemoryItem</param>
        ///// <returns>False if the item is "null", true otherwise</returns>
        //internal static bool Exists(this MemoryItem item)
        //{
        //    return item != null;
        //}

        /// <summary>
        ///     Returns the parent-path of a file or directory,
        ///     similar to Path.GetDirectoryName
        /// </summary>
        /// <param name="sourcePath">the full sourcepath</param>
        /// <returns>a path to it's parent</returns>
        internal static string GetPathPart(this string sourcePath)
        {
            return sourcePath.Substring(0, sourcePath.LastIndexOf('\\'));
        }

        /// <summary>
        ///     Returns the filename-part of a string that contains a full path,
        ///     similar to Path.GetFileName()
        /// </summary>
        /// <param name="sourcePath">a folder or file, with a full path</param>
        /// <returns>The item's name, without the path</returns>
        internal static string GetFilenamePart(this string sourcePath)
        {
            return sourcePath.Substring(sourcePath.LastIndexOf('\\') + 1);
        }

        /// <summary>
        ///     Used to retrieve the first available driveletter
        /// </summary>
        /// <returns>A driveletter that's not in use yet</returns>
        internal static char GetFirstAvailableDriveLetter()
        {
            // these are the driveletters that are in use;
            IEnumerable<char> usedDriveLetters =
                from drive
                    in DriveInfo.GetDrives()
                select drive.Name.ToUpperInvariant()[0];

            // these are all possible driveletters [D..Z] that
            // we can choose from (don't want "B" as drive);
            string allDrives = string.Empty;
            for (char c = 'D'; c < 'Z'; c++)
                allDrives += c;

            // these are the ones that are available;
            IEnumerable<char> availableDriveLetters = allDrives.Except(usedDriveLetters);

            if (!availableDriveLetters.Any())
                throw new DriveNotFoundException("No drives available!");

            return availableDriveLetters.First();
        }
    }
}