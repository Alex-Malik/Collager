using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Collager.Models;

namespace Collager.Behavior
{
    /// <summary>
    /// This is the another one (improved) implementation of the method 3.
    /// </summary>
    public class Method4 : ICollageBuildBehavior
    {
        public IEnumerable<ImageRectangle> Build(IEnumerable<IImage> source)
        {
            // 1. Get a list of possible combinations. (ret L<L<I>>)*
            // 2. Get trees of possible positions. (ret L<N>)
            // 3. Build each tree for each combination of images. (ret L<L<IR>>)
            // 4. Choose the best pair tree-combination by square and loss percent. (ret L<IR>)
            // * L<> - List; I - IImage; N - INode; IR - ImageRectange.

            // Get a list of possible combinations for source images
            IEnumerable<IEnumerable<IImage>> combinations = GetPossibleCombinations(source);

            // Get a list of possible trees (a list of roots of tree)
            IEnumerable<INode> roots = GetPossibleNodes(source.Count());
            
            List<IEnumerable<ImageRectangle>> collages = new List<IEnumerable<ImageRectangle>>();

            // Build each tree for each combination of images
            foreach (INode root in roots)
            {
                foreach (IEnumerable<IImage> combination in combinations)
                {
                    collages.Add(Arrange(root, combination.ToList()).Images);
                }
            }

            throw new NotImplementedException();
        }

        public IEnumerable<ImageRectangle> Build(IEnumerable<IImage> source, double targetWidth, double targetHeight)
        {
            throw new NotImplementedException();
        }

        public List<IEnumerable<ImageRectangle>> BuildTest(IEnumerable<IImage> source)
        {
            // 1. Get a list of possible combinations. (ret L<L<I>>)*
            // 2. Get trees of possible positions. (ret L<N>)
            // 3. Build each tree for each combination of images. (ret L<L<IR>>)
            // 4. Choose the best pair tree-combination by square and loss percent. (ret L<IR>)
            // * L<> - List; I - IImage; N - INode; IR - ImageRectange.

            // Get a list of possible combinations for source images
            IEnumerable<IEnumerable<IImage>> combinations = GetPossibleCombinations(source);

            // Get a list of possible trees (a list of roots of tree)
            IEnumerable<INode> roots = GetPossibleNodes(source.Count());

            List<IEnumerable<ImageRectangle>> collages = new List<IEnumerable<ImageRectangle>>();

            // Build each tree for each combination of images
            foreach (INode root in roots)
            {
                foreach (IEnumerable<IImage> combination in combinations)
                {
                    collages.Add(Arrange(root, combination.ToList()).Images);
                }
            }

            return collages;
        }

        private ImageShape Arrange(INode node, List<IImage> images)
        {
            if (node.GetType() == typeof(Node))
            {
                return new ImageShape(new ImageRectangleWrapper(images.Pop()));
            }
            else if (node.GetType() == typeof(VNode))
            {
                ImageShape tShape = Arrange(((VNode)node).TNode, images);
                ImageShape bShape = Arrange(((VNode)node).BNode, images);

                // If the top shape (image) is wider then resize it to the
                // width of the bottom shape, otherwise resize bottom shape
                if (tShape.Width > bShape.Width)
                {
                    // Scale top shape to the width of bottom shape
                    ScaleShape(tShape, bShape.Width, GetHeight(tShape.Width, tShape.Height, bShape.Width));
                }
                else
                {
                    // Scale bottom shape to the width of top shape
                    ScaleShape(bShape, tShape.Width, GetHeight(bShape.Width, bShape.Height, tShape.Width));
                }

                // Locate bottom shape under the top shape
                LocateShape(bShape, .0, tShape.Height);

                // Add rects of each shape to the result shape
                return new ImageShape(tShape, bShape, tShape.Width, tShape.Height + bShape.Height);
            }
            else if (node.GetType() == typeof(HNode))
            {
                ImageShape lShape = Arrange(((HNode)node).LNode, images);
                ImageShape rShape = Arrange(((HNode)node).RNode, images);

                if (lShape.Height > rShape.Height)
                {
                    ScaleShape(lShape, GetWidth(lShape.Width, lShape.Height, rShape.Height), rShape.Height);
                }
                else
                {
                    ScaleShape(rShape, GetWidth(rShape.Width, rShape.Height, lShape.Height), lShape.Height);
                }

                LocateShape(rShape, lShape.Width, .0);

                return new ImageShape(lShape, rShape, lShape.Width + rShape.Width, lShape.Height);
            }
            else
            {
                throw new NotImplementedException();
            }
        }

        private void ScaleShape(ImageShape shape, double targetWidth, double targetHeight)
        {
            double scaleWidth = targetWidth / shape.Width;
            double scaleHeight = targetHeight / shape.Height;

            foreach (ImageRectangle rect in shape.Images)
            {
                rect.X *= scaleWidth;
                rect.Y *= scaleHeight;
                rect.Width *= scaleWidth;
                rect.Height *= scaleHeight;
            }

            shape.Width = targetWidth;
            shape.Height = targetHeight;
        }

        private void LocateShape(ImageShape shape, double targetX, double targetY)
        {
            foreach (ImageRectangle rect in shape.Images)
            {
                rect.X += targetX;
                rect.Y += targetY;
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

        /// <summary>
        /// Returns all unque combinatinos of elements of given source collection.
        /// </summary>
        private IEnumerable<IEnumerable<T>> GetPossibleCombinations<T>(IEnumerable<T> source)
        {
            // If source collection contains only one element then return it
            if (source.Count() == 1) return new[] { new[] { source.First() } };
            
            // Stack contains more then one element so we are
            // able to create different combinations. 
            // Create list of combinations and start to iterate variants
            List<IEnumerable<T>> combinations = new List<IEnumerable<T>>();

            // Get a tail combinations for each element
            foreach (T element in source)
            {
                IEnumerable<IEnumerable<T>> combinationTails = GetPossibleCombinations(source.Except(new[] { element }));

                // Add each tail combination to the element and
                // add result combination to the general list of combinations
                foreach (IEnumerable<T> combinationTail in combinationTails)
                {
                    List<T> combination = new List<T> { element };
                    combination.AddRange(combinationTail);
                    combinations.Add(combination);
                }
            }

            return combinations;
        }

        /// <summary>
        /// Returns root nodes for all possible combinations.
        /// </summary>
        private IEnumerable<INode> GetPossibleNodes(int n)
        {
            // If N is equal to one then it is a leaf node
            if (n == 1) return new[] { new Node() };

            // Otherwise get combinations for two possible 
            // situations: vertical and horizontal
            List<INode> nodes = new List<INode>();

            // Get combinations for vertical node; for each
            // combination of top node add combination of bottom node
            foreach (INode tNode in GetPossibleNodes(n - 1))
            {
                // If we are able to create a vnode then get
                // possible combinations for it, otherwise create a leaf node 
                if (n > 2)
                {
                    foreach (INode bNode in GetPossibleNodes(n - 2))
                    {
                        nodes.Add(new VNode { TNode = tNode, BNode = bNode });
                    }
                }
                else
                {
                    nodes.Add(new VNode { TNode = tNode, BNode = new Node() });
                }
            }

            // The same with horizontal node, for each left node
            // add right each right node
            foreach (INode lNode in GetPossibleNodes(n - 1))
            {
                // If we are able to create a hnode then get
                // possible combinations for it, otherwise create a leaf node
                if (n > 2)
                {
                    foreach (INode rNode in GetPossibleNodes(n - 2))
                    {
                        nodes.Add(new HNode { LNode = lNode, RNode = rNode });
                    }
                }
                else
                {
                    nodes.Add(new HNode { LNode = lNode, RNode = new Node() });
                }
            }

            return nodes;
        }

        /// <summary>
        /// Represents a general node interface.
        /// </summary>
        private interface INode { }

        /// <summary>
        /// Represents a leaf node with the container for the image.
        /// </summary>
        private class Node : INode
        {
            /// <summary>
            /// Gets or sets the image contained by this node.
            /// </summary>
            public IImage Image { get; set; }

            //public override string ToString() => $"[{Image?.ToString()??"Empty"}]";
        }

        /// <summary>
        /// Represents a horizontal node with two (left and right) brunches.
        /// </summary>
        private class HNode : INode
        {
            /// <summary>
            /// Gets or sets the left node.
            /// </summary>
            public INode LNode { get; set; }

            /// <summary>
            /// Gets or sets the right node.
            /// </summary>
            public INode RNode { get; set; }

            //public override string ToString() => $"L[{LNode}] R[{RNode}]";
        }

        /// <summary>
        /// Represents a vertical node with two (top and bottom) brunches.
        /// </summary>
        private class VNode : INode
        {
            /// <summary>
            /// Gets or sets the top node.
            /// </summary>
            public INode TNode { get; set; }

            /// <summary>
            /// Gets or sets the bottom node.
            /// </summary>
            public INode BNode { get; set; }

            //public override string ToString() => $"T[{TNode}] B[{BNode}]";
        }

        private class ImageRectangleWrapper : ImageRectangle
        {
            public ImageRectangleWrapper(IImage image)
                : base(image)
            {

            }

            /// <summary>
            /// Gets or sets the percentage value of a the image compression.
            /// </summary>
            public double Compression { get; set; }
        }

        private class ImageShape
        {
            public ImageShape(ImageRectangleWrapper rect)
            {
                Images = new List<ImageRectangleWrapper> { rect };

                Width = rect.Width;
                Height = rect.Height;
            }

            public ImageShape(ImageShape shape1, ImageShape shape2, double width, double height)
            {
                Images = new List<ImageRectangleWrapper>();

                Images.AddRange(shape1.Images);
                Images.AddRange(shape2.Images);

                Width = width;
                Height = height;
            }

            public List<ImageRectangleWrapper> Images { get; }

            public double Width { get; set; }

            public double Height { get; set; }
        }
    }
}
