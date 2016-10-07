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

namespace Collager.Behavior
{
    using Models;

    /// <summary>
    /// This method builds a binary tree of image pairs and then glue them to each other.
    /// </summary>
    public class Method2 : ICollageBuildBehavior
    {
        public IEnumerable<ImageRectangle> Build(IEnumerable<IImage> source)
        {
            return Arrange(source);
        }

        public IEnumerable<ImageRectangle> Build(IEnumerable<IImage> source, double targetWidth, double targetHeight)
        {
            throw new NotImplementedException();
        }

        private IEnumerable<ImageRectangle> Arrange(IEnumerable<IImage> images)
        {
            if (!images.Any()) return Enumerable.Empty<ImageRectangle>();

            //images = images.OrderBy(p => p.Width * p.Height);

            var root = BuildTree(images);

            var collage = Arrange(new ImageRectangle(root));

            return collage;
        }

        private IImage BuildTree(IEnumerable<IImage> source)
        {
            List<IImage> nodes = new List<IImage>();
            List<IImage> images = source.ToList();

            while (images.Count > 1)
            {
                IImage img1 = images.Pop();

                // List of condidates for current image
                List<IImage> selection = images.ToList();

                IImage img2 = selection.Pop();

                IImage bestByWidth = img2;
                IImage bestByHeight = img2;
                double diffByWidth = Math.Abs(img1.Width - img2.Width);
                double diffByHeight = Math.Abs(img1.Height - img2.Height);

                while (selection.Count > 0)
                {
                    // Get differences for another one condidate and then 
                    // compare it the previous best condidate
                    img2 = selection.Pop();

                    double newDiffByWidth = Math.Abs(img1.Width - img2.Width);
                    double newDiffByHeight = Math.Abs(img1.Height - img2.Height);

                    if (newDiffByWidth < diffByWidth)
                    {
                        bestByWidth = img2;
                        diffByWidth = newDiffByWidth;
                    }
                    if (newDiffByHeight < diffByHeight)
                    {
                        bestByHeight = img2;
                        diffByHeight = newDiffByHeight;
                    }
                }

                // Choose which on use to build the node (img with best width or height)
                if (diffByWidth < diffByHeight)
                {
                    img2 = bestByWidth;
                }
                else
                {
                    img2 = bestByHeight;
                }

                images.Remove(img2);

                nodes.Add(BuildNode(img1, img2, diffByWidth, diffByHeight));
            }

            // Continue to build images tree if here is more then one node
            if (nodes.Any() && images.Any())
            {
                IImage img1 = BuildTree(nodes);
                IImage img2 = images.First();

                return BuildNode(img1, img2);
            }
            else if (images.Any())
            {
                IImage img = images.Pop();
                Type imgType = img.GetType();

                if (imgType == typeof(ImageRectangle) || imgType == typeof(ImageNode))
                {
                    return img;
                }
                else
                {
                    return new ImageRectangle(img);
                }
            }
            else
            {
                return BuildTree(nodes);
            }
        }

        private ImageNode BuildNode(IImage img1, IImage img2)
        {
            double diffByWidth = Math.Abs(img1.Width - img2.Width);
            double diffByHeight = Math.Abs(img1.Height - img2.Height);

            return BuildNode(img1, img2, diffByWidth, diffByHeight);
        }

        private ImageNode BuildNode(IImage img1, IImage img2, double diffByWidth, double diffByHeight)
        {
            // Wrap each image in rectangle (to set X and Y) and choose position
            ImageRectangle rect1 = new ImageRectangle(img1);
            ImageRectangle rect2 = new ImageRectangle(img2);

            double nodeWidth = 0.0;
            double nodeHeight = 0.0;

            if (diffByWidth < diffByHeight)
            {
                img2 = new ImageRectangle(img2);

                // The second image will be appended on the bottom of the first
                if (rect1.Width > img2.Width)
                {
                    // Make the first rect smaller and put second on the bottom of the first
                    rect1.X = 0.0;
                    rect1.Y = 0.0;
                    rect1.Height = GetHeight(rect1.Width, rect1.Height, img2.Width);
                    rect1.Width = img2.Width;

                    rect2.X = 0.0;
                    rect2.Y = img1.Height;
                }
                else
                {
                    // Make the second rect smaller and put it on the bottom of the first
                    rect1.X = 0.0;
                    rect1.Y = 0.0;

                    rect2.X = 0.0;
                    rect2.Y = img1.Height;
                    rect2.Height = GetHeight(rect2.Width, rect2.Height, img1.Width);
                    rect2.Width = img1.Width;
                }

                ScaleNode(img1 as ImageNode, rect1.Width / img1.Width, rect1.Height / img1.Height);
                ScaleNode(img2 as ImageNode, rect2.Width / img2.Width, rect2.Height / img2.Height);

                nodeWidth = img1.Width;
                nodeHeight = img1.Height + img2.Height;
            }
            else
            {
                img2 = new ImageRectangle(img2);

                // Append on the right (or left side)
                if (img1.Height > img2.Height)
                {
                    // Make the first rect smaller and put second on the right side of the first
                    rect1.X = 0.0;
                    rect1.Y = 0.0;
                    rect1.Height = img2.Height;
                    rect1.Width = GetWidth(rect1.Width, rect1.Height, img2.Height);

                    rect2.X = img1.Width;
                    rect2.Y = 0.0;
                }
                else
                {
                    // Make the second rect smaller and put it on the right side of the first
                    rect1.X = 0.0;
                    rect1.Y = 0.0;

                    rect2.X = img1.Width;
                    rect2.Y = 0.0;
                    rect2.Height = img2.Height;
                    rect2.Width = GetWidth(rect2.Width, rect2.Height, img1.Height);
                }

                ScaleNode(img1 as ImageNode, rect1.Width / img1.Width, rect1.Height / img1.Height);
                ScaleNode(img2 as ImageNode, rect2.Width / img2.Width, rect2.Height / img2.Height);

                nodeWidth = img1.Width + img2.Width;
                nodeHeight = img1.Height;
            }

            return new ImageNode(rect1, rect2, nodeWidth, nodeHeight);
        }

        private IEnumerable<ImageRectangle> Arrange(IImage source)
        {
            ImageRectangle rect = source as ImageRectangle;

            if (rect.Source.GetType() == typeof(ImageNode))
            {
                List<ImageRectangle> retval = new List<ImageRectangle>();
                ImageNode node = rect.Source as ImageNode;
                ImageRectangle nodeRect1 = node.Image1 as ImageRectangle;
                ImageRectangle nodeRect2 = node.Image2 as ImageRectangle;

                nodeRect1.X += rect.X;
                nodeRect1.Y += rect.Y;
                nodeRect2.X += rect.X;
                nodeRect2.Y += rect.Y;

                retval.AddRange(Arrange(nodeRect1));
                retval.AddRange(Arrange(nodeRect2));

                return retval;
            }
            else
            {
                return new List<ImageRectangle> { rect };
            }
        }

        private double GetWidth(double width, double height, double targetHeight)
        {
            return targetHeight * width / height;
        }

        private double GetHeight(double width, double height, double targetWidth)
        {
            return targetWidth * height / width;
        }

        private void ScaleNode(ImageNode node, double scaleWidth, double scaleHeight)
        {
            if (node == null) return;

            ScaleRect(node.Image1 as ImageRectangle, scaleWidth, scaleHeight);
            ScaleRect(node.Image2 as ImageRectangle, scaleWidth, scaleHeight);
        }

        private void ScaleRect(ImageRectangle rect, double scaleWidth, double scaleHeight)
        {
            if (rect == null) return;

            rect.X *= scaleWidth;
            rect.Y *= scaleHeight;
            rect.Width *= scaleWidth;
            rect.Height *= scaleHeight;

            ScaleNode(rect.Source as ImageNode, scaleWidth, scaleHeight);
        }

        private class ImageNode : IImage
        {
            public ImageNode(IImage image1, IImage image2, double nodeWidth, double nodeHeight)
            {
                Image1 = image1;
                Image2 = image2;
                Width = nodeWidth;
                Height = nodeHeight;
            }

            public IImage Image1 { get; }

            public IImage Image2 { get; }

            public double Width { get; }

            public double Height { get; }

            public override string ToString() => $"NODE[{Width}x{Height} -> L({Image1.Width}x{Image1.Height}) R({Image2.Width}x{Image2.Height})]";
        }
    }
}
