using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Collager
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
            Image = image;
            Width = image.Width;
            Height = image.Height;
        }

        public IImage Image { get; }

        public double Width { get; set; }

        public double Height { get; set; }

        public double X { get; set; }

        public double Y { get; set; }

        public override string ToString() => $"{X},{Y} {Width}x{Height} {Image.ToString()}";
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

        public override string ToString() => $"{Path} ({Width}x{Height})";
    }
}
