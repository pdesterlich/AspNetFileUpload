using System;

namespace AspNetFileUpload.Helpers
{
    public static class ImageHelpers
    {
        public static Tuple<int, int> GetNewSize(int newSize, int width, int height)
        {
            if (width >= height)
                return new Tuple<int, int>(newSize, height * newSize / width);

            return new Tuple<int, int>(width * newSize / height, newSize);
        }
    }
}