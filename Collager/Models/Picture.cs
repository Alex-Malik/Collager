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

namespace Collager.Models
{
    public interface IImage
    {
        double Width { get; }

        double Height { get; }
    }

    public class ImageRectangle : IImage
    {
        public ImageRectangle(IImage image)
        {
            Source = image;
            Width = image.Width;
            Height = image.Height;
        }

        public IImage Source { get; }

        public double Width { get; set; }

        public double Height { get; set; }

        public double X { get; set; }

        public double Y { get; set; }

        public override string ToString() => $"RECT[{X},{Y} {Width}x{Height} {Source.ToString()}]";
    }

    public class ImageWrapper : IImage
    {
        public ImageWrapper(string path, double width, double height)
        {
            Path = path;
            Width = width;
            Height = height;
        }

        public string Path { get; }

        public double Width { get; }

        public double Height { get; }

        public override string ToString() => $"WRAP[{Path} ({Width}x{Height})]";
    }
}
