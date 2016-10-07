// This file is part of Collager.
// Copyright (C) 2016  Alex Malik
// 
// Collager is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 3 of the License, or
// (at your option) any later version.
// 
// Collager is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Collager.  If not, see<http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collager
{
    using Behavior;
    using Models;

    class Program
    {
        private const int MinPicturesCount = 1;
        private const int MaxPicturesCount = 3;
        private static readonly string CurrentDirectory = Directory.GetCurrentDirectory();
        private static readonly string InputPath = Path.Combine(CurrentDirectory, "Test Images");
        private static readonly string OutputPath = Directory.CreateDirectory(Path.Combine(CurrentDirectory, "Result Collages")).FullName;

        static void Main(string[] args)
        {
            Console.WriteLine($"Input directory:  {InputPath}");
            Console.WriteLine($"Output directory: {OutputPath}");

            Collager.Builder = new Method4();

            do
            {
                //IEnumerable<ImageWrapper> inputPictures = GetRandomPictures(MaxPicturesCount, InputPath);
                //IEnumerable<ImageRectangle> outputPictures = Collager.Create(inputPictures);//, 1200, 630);

                //SaveResult(outputPictures, 1200, 630);

                ////for (int picturesCount = MinPicturesCount; picturesCount <= MaxPicturesCount; picturesCount++)
                ////{
                ////    IEnumerable<ImageWrapper> inputPictures = GetRandomPictures(picturesCount, InputPath);
                ////    IEnumerable<ImageRectangle> outputPictures = Collager.Create2(inputPictures);

                ////    SaveResult(outputPictures);
                ////}

                Method4 builder = new Method4();

                IEnumerable<ImageWrapper> inputPictures = GetRandomPictures(MaxPicturesCount, InputPath);
                foreach (IEnumerable<ImageRectangle> outputPictures in builder.BuildTest(inputPictures))
                {
                    SaveResult(outputPictures);
                }
            }
            while (ShowEnterOrEscape());
        }
        
        private static IEnumerable<ImageWrapper> GetRandomPictures(int count, string path)
        {
            if (!Directory.Exists(Path.GetDirectoryName(path)))
                throw new DirectoryNotFoundException();
            var pictures = new List<ImageWrapper>();
            var filePaths = Directory.GetFiles(path).Where(f => f.EndsWith(".jpg") || f.EndsWith(".jpeg")  || f.EndsWith(".png"));

            foreach (string filePath in filePaths.OrderBy(p => Guid.NewGuid()).Take(count))
            {
                Image image = Image.FromFile(filePath);
                if (image != null)
                {
                    pictures.Add(new ImageWrapper(filePath, image.Width, image.Height));
                }
            }

            return pictures;
        }

        private static void SaveResult(IEnumerable<ImageRectangle> rectangles)
        {
            if (!rectangles.Any()) return;

            double backgroundWidth = rectangles.GroupBy(p => p.Y).Max(g => g.Sum(p => p.Width));
            double backgroundHeight = rectangles.GroupBy(p => p.X).Max(g => g.Sum(p => p.Height));

            Bitmap background = new Bitmap((int)Math.Round(backgroundWidth), (int)Math.Round(backgroundHeight));
            Graphics graphics = Graphics.FromImage(background);

            graphics.FillRectangle(Brushes.White, 0, 0, background.Width, background.Height);

            foreach (ImageRectangle rectangle in rectangles)
            {
                // Load image from file and arrange by corresponding rectangle.
                graphics.DrawImage(Image.FromFile(((ImageWrapper)rectangle.Source).Path),
                    (int)Math.Round(rectangle.X), (int)Math.Round(rectangle.Y),
                    (int)Math.Round(rectangle.Width), (int)Math.Round(rectangle.Height));
            }

            // Save image to the drive.
            background.Save(Path.Combine(OutputPath, $"{DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")} ({rectangles.Count()}).png"));

            // Show short info in console.
            Console.WriteLine($"Collage was successfully saved. Names of mixed pictures:");
            foreach (ImageRectangle rectangle in rectangles)
                Console.WriteLine(Path.GetFileName(((ImageWrapper)rectangle.Source).Path));
        }

        private static void SaveResult(IEnumerable<ImageRectangle> rectangles, double backgroundWidth, double backgroundHeight)
        {
            if (!rectangles.Any()) return;

            //double backgroundWidth = rectangles.GroupBy(p => p.Y).Max(g => g.Sum(p => p.Width));
            //double backgroundHeight = rectangles.GroupBy(p => p.X).Max(g => g.Sum(p => p.Height));

            Bitmap background = new Bitmap((int)Math.Round(backgroundWidth), (int)Math.Round(backgroundHeight));
            Graphics graphics = Graphics.FromImage(background);

            graphics.FillRectangle(Brushes.White, 0, 0, background.Width, background.Height);

            foreach (ImageRectangle rectangle in rectangles)
            {
                // Load image from file and arrange by corresponding rectangle.
                graphics.DrawImage(Image.FromFile(((ImageWrapper)rectangle.Source).Path), 
                    (int)Math.Round(rectangle.X), (int)Math.Round(rectangle.Y),
                    (int)Math.Round(rectangle.Width), (int)Math.Round(rectangle.Height));
            }

            // Save image to the drive.
            background.Save(Path.Combine(OutputPath, $"{DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")} ({rectangles.Count()}).png"));

            // Show short info in console.
            Console.WriteLine($"Collage was successfully saved. Names of mixed pictures:");
            foreach (ImageRectangle rectangle in rectangles)
                Console.WriteLine(Path.GetFileName(((ImageWrapper)rectangle.Source).Path));
        }
        
        private static bool ShowEnterOrEscape()
        {
            while (true)
            {
                Console.WriteLine("Press <Enter> to create a new collage, or <Escape> to exit.");
                ConsoleKeyInfo key = Console.ReadKey();
                if (key.Key == ConsoleKey.Enter)
                {
                    return true;
                }
                else if (key.Key == ConsoleKey.Escape)
                {
                    return false;
                }
                else
                {
                    continue;
                }
            }
        }
    }
}
