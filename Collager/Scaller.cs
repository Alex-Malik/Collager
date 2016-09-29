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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collager
{
    public class Collager
    {
        public static IEnumerable<ImageRectangle> Create(IEnumerable<IImage> images)
        {
            return new Collager().Arrange(images);
        }

        private IEnumerable<ImageRectangle> Arrange(IEnumerable<IImage> images)
        {
            if (!images.Any()) return Enumerable.Empty<ImageRectangle>();
                                    
            images = images.OrderBy(p => p.Width * p.Height);

            var collage = new List<ImageRectangle> { new ImageRectangle(images.First()) };
            var shapeWidth = collage.First().Width;
            var shapeHeight = collage.First().Height;

            foreach (var image in images.Skip(1))
            {
                ImageRectangle imageRectangle = new ImageRectangle(image);

                // If the shape is horizontal-oriented and the next rect is 
                // horizontal-oriented, then put it on the bottom of the 
                // shape, otherwise put it on the right side of the shape.
                if (shapeWidth > shapeHeight && imageRectangle.Width > imageRectangle.Height)
                {
                    // Put rect on the bottom.
                    // If the rect is wider then current shape, then 
                    // resize rect, otherwise resize shape.
                    if (imageRectangle.Width > shapeWidth)
                    {
                        // Make rect smaller and put it on the right side.
                        imageRectangle.X = 0.0;
                        imageRectangle.Y = shapeHeight;
                        imageRectangle.Height = GetHeight(imageRectangle.Width, imageRectangle.Height, shapeWidth);
                        imageRectangle.Width = shapeWidth;

                        collage.Add(imageRectangle);

                        shapeHeight += imageRectangle.Height;
                    }
                    else
                    {
                        // Make shape smaller and put the rect on the right side.
                        shapeHeight = GetHeight(shapeWidth, shapeHeight, imageRectangle.Width);
                        shapeWidth = imageRectangle.Width;

                        ScaleShape(collage, shapeWidth, shapeHeight);

                        imageRectangle.X = 0.0;
                        imageRectangle.Y = shapeHeight;

                        collage.Add(imageRectangle);

                        shapeHeight += imageRectangle.Height;
                    }
                }
                else
                {
                    // In any case we will put next rect on the right side.
                    // If next rect is higher then current shape, then 
                    // resize rect, otherwise resize shape.
                    if (imageRectangle.Height > shapeHeight)
                    {
                        // Make rect smaller and put it on the right side.
                        imageRectangle.X = shapeWidth;
                        imageRectangle.Y = 0.0;
                        imageRectangle.Width = GetWidth(imageRectangle.Width, imageRectangle.Height, shapeHeight);
                        imageRectangle.Height = shapeHeight;

                        collage.Add(imageRectangle);

                        shapeWidth += imageRectangle.Width;
                    }
                    else
                    {
                        // Make shape smaller and put the rect on the right side.
                        shapeWidth = GetWidth(shapeWidth, shapeHeight, imageRectangle.Height);
                        shapeHeight = imageRectangle.Height;

                        ScaleShape(collage, shapeWidth, shapeHeight);

                        imageRectangle.X = shapeWidth;
                        imageRectangle.Y = 0.0;

                        collage.Add(imageRectangle);

                        shapeWidth += imageRectangle.Width;
                    }
                }
            }

            return collage;
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
    }
}
