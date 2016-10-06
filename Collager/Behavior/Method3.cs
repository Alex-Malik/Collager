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
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Collager.Models;

namespace Collager.Behavior
{
    using Models;

    /// <summary>
    /// This methods choose iteratively the best option of images positioning by best square value.
    /// </summary>
    public class Method3 : ICollageBuildBehavior
    {
        public IEnumerable<ImageRectangle> Build(IEnumerable<IImage> source)
        {
            IEnumerable<IEnumerable<ImageRectangle>> results = Arrange(source);
            IEnumerable<ImageRectangle> retval = results.First();
            double retvalSquare = results.First().Sum(i => i.Width * i.Height);

            foreach (IEnumerable<ImageRectangle> result in results)
            {
                //double minWidth = result.GroupBy(i => i.Y).Select(g => g.Sum(i => i.Width)).Min();
                //double minHeight = result.GroupBy(i => i.X).Select(g => g.Sum(i => i.Height)).Min();

                //foreach (IEnumerable<ImageRectangle> group in result.GroupBy(i => i.Y))
                //{
                //    group
                //}

                double square = retval.Sum(i => i.Width * i.Height);
                if (square > retvalSquare)
                {
                    retval = result;
                }
            }

            return retval;
        }

        public IEnumerable<ImageRectangle> Build(IEnumerable<IImage> source, double targetWidth, double targetHeight)
        {
            IEnumerable<IEnumerable<ImageRectangle>> results = Arrange(source);
            IEnumerable<ImageRectangle> retval = null;
            double targetSquare = targetWidth * targetHeight;
            double retvalSquareDiff = -1;

            foreach (IEnumerable<ImageRectangle> result in results)
            {
                double collageWidth = result.GroupBy(i => i.Y).Select(g => g.Sum(i => i.Width)).Max();
                double collageHeight = result.Where(i => i.X == 0).Sum(i => i.Height);

                if (collageHeight > targetHeight)
                {
                    collageWidth = GetWidth(collageWidth, collageHeight, targetHeight);
                    collageHeight = targetHeight;

                    ScaleShape(result, collageWidth, collageHeight);
                }

                if (collageWidth > targetWidth)
                {
                    collageHeight = GetHeight(collageWidth, collageHeight, targetWidth);
                    collageWidth = targetWidth;

                    ScaleShape(result, collageWidth, collageHeight);
                }

                double x = (targetWidth - collageWidth) / 2;
                double y = (targetHeight - collageHeight) / 2;

                foreach (var image in result)
                {
                    image.X += x;
                    image.Y += y;
                }

                double diff = targetSquare - result.Sum(i => i.Width * i.Height);

                if (retvalSquareDiff == -1)
                {
                    retvalSquareDiff = diff;
                    retval = result;
                }

                if (diff < retvalSquareDiff)
                {
                    retval = result;
                }
            }

            return retval;
        }

        public IEnumerable<IEnumerable<ImageRectangle>> Arrange(IEnumerable<IImage> source)
        {
            int n = source.Count();
            if (n == 1) return new[] { new[] {new ImageRectangle(source.First()) } };

            IEnumerable<IEnumerable<ImagePosition>> positions = GetPositions(n);
            IEnumerable<IEnumerable<IImage>> options = GetOptions(source);

            List<IEnumerable<ImageRectangle>> results = new List<IEnumerable<ImageRectangle>>();

            foreach (IEnumerable<ImagePosition> position in positions)
            {
                foreach (IEnumerable<IImage> option in options)
                {
                    List<IImage> img = option.ToList();
                    List<ImagePosition> pos = position.ToList();
                    List<ImageRectangle> result = new List<ImageRectangle>();
                    double x = .0;
                    double y = .0;
                    int pX = 0; // prev pos
                    int pY = 0;
                    IImage last = null;
                    
                    for (int i = 0; i < n; i++)
                    {
                        ImageRectangle rect = new ImageRectangle(img[i]);
                        
                        if (pos[i].X > pX)
                        {
                            x += last.Width;

                            rect.Width = GetWidth(rect.Width, rect.Height, last.Height);
                            rect.Height = last.Height;
                        }
                        else if (pos[i].Y > pY)
                        {
                            x = .0;
                            y += last.Height;
                            
                            pX = 0; // WARNING: Y can be changed only when all X in row have been passed
                        }

                        rect.X = x;
                        rect.Y = y;

                        result.Add(rect);
                        last = rect;
                    }

                    results.Add(result);
                }
            }
            
            return results;
        }

        private IEnumerable<IEnumerable<ImagePosition>> GetPositions(int n)
        {
            List<IEnumerable<ImagePosition>> positions = new List<IEnumerable<ImagePosition>>();

            if (n == 1)
            {
                positions.Add(new List<ImagePosition> { new ImagePosition(0, 0) });
            }
            else if (n == 2)
            {
                positions.Add(new List<ImagePosition>
                {
                    new ImagePosition(0, 0),
                    new ImagePosition(1, 0)
                });
                positions.Add(new List<ImagePosition>
                {
                    new ImagePosition(0, 0),
                    new ImagePosition(0, 1)
                });
            }
            else if (n == 3)
            {
                positions.Add(new List<ImagePosition>
                {
                    new ImagePosition(0, 0),
                    new ImagePosition(1, 0),
                    new ImagePosition(2, 0)
                });
                positions.Add(new List<ImagePosition>
                {
                    new ImagePosition(0, 0),
                    new ImagePosition(1, 0),
                    new ImagePosition(0, 1)
                });
                positions.Add(new List<ImagePosition>
                {
                    new ImagePosition(0, 0),
                    new ImagePosition(0, 1),
                    new ImagePosition(1, 0),
                });
                positions.Add(new List<ImagePosition>
                {
                    new ImagePosition(0, 0),
                    new ImagePosition(0, 1),
                    new ImagePosition(0, 2),
                });
            }
            else if (n == 4)
            {
                positions.Add(new List<ImagePosition>
                {
                    new ImagePosition(0, 0), new ImagePosition(1, 0), new ImagePosition(2, 0), new ImagePosition(3, 0),
                });

                positions.Add(new List<ImagePosition>
                {
                    new ImagePosition(0, 0), new ImagePosition(1, 0), new ImagePosition(2, 0),
                    new ImagePosition(0, 1),
                });

                positions.Add(new List<ImagePosition>
                {
                    new ImagePosition(0, 0), new ImagePosition(1, 0),
                    new ImagePosition(0, 1), new ImagePosition(1, 1),
                });

                positions.Add(new List<ImagePosition>
                {
                    new ImagePosition(0, 0),
                    new ImagePosition(0, 1), new ImagePosition(1, 1), new ImagePosition(2, 1),
                });

                positions.Add(new List<ImagePosition>
                {
                    new ImagePosition(0, 0), new ImagePosition(1, 0),
                    new ImagePosition(0, 1),
                    new ImagePosition(0, 2),
                });

                positions.Add(new List<ImagePosition>
                {
                    new ImagePosition(0, 0),
                    new ImagePosition(0, 1), new ImagePosition(1, 1),
                    new ImagePosition(0, 2),
                });

                positions.Add(new List<ImagePosition>
                {
                    new ImagePosition(0, 0),
                    new ImagePosition(0, 1),
                    new ImagePosition(0, 2), new ImagePosition(1, 2),
                });

                positions.Add(new List<ImagePosition>
                {
                    new ImagePosition(0, 0),
                    new ImagePosition(0, 1),
                    new ImagePosition(0, 2),
                    new ImagePosition(0, 3),
                });
            }

            return positions;
        }

        private IEnumerable<IEnumerable<IImage>> GetOptions(IEnumerable<IImage> source)
        {
            if (source.Count() == 1) return new List<IEnumerable<IImage>> { new List<IImage> { source.First() } };

            List<IEnumerable<IImage>> retval = new List<IEnumerable<IImage>>();

            foreach (IImage image in source)
            {
                foreach (IEnumerable<IImage> option in GetOptions(source.Except(new[] { image })))
                {
                    List<IImage> options = new List<IImage> { image };

                    options.AddRange(option);

                    retval.Add(options);
                }
            }

            return retval;
        }

        private double GetWidth(double width, double height, double targetHeight)
        {
            return targetHeight * width / height;
        }

        private double GetHeight(double width, double height, double targetWidth)
        {
            return targetWidth * height / width;
        }

        private void ScaleShape(IEnumerable<ImageRectangle> shape, double targetWidth, double targetHeight)
        {
            double shapeWidth = shape.Where(ir => ir.Y == 0).Sum(ir => ir.Width);
            double shapeHeight = shape.Where(ir => ir.X == 0).Sum(ir => ir.Height);

            double scaleWidth = targetWidth / shapeWidth;
            double scaleHeight = targetHeight / shapeHeight;

            foreach (ImageRectangle rect in shape)
            {
                rect.X *= scaleWidth;
                rect.Y *= scaleHeight;
                rect.Width *= scaleWidth;
                rect.Height *= scaleHeight;
            }
        }

        private class ImagePosition
        {
            public ImagePosition(int x, int y)
            {
                X = x;
                Y = y;
            }

            public int X { get; }

            public int Y { get; }

            public override string ToString() => $"X:{X} Y:{Y}";
        }
    }
}
