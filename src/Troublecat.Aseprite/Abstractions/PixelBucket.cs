namespace Troublecat.Aseprite.Abstractions {
    public class PixelBucket {
        public static PixelBucket CreateTransparent(int width, int height) {
            var bucket = new PixelBucket(width, height);
            for(var y = 0; y < height; y++) {
                for(var x = 0; x < width; x++) {
                    bucket.SetPixel(x, y, new AsepriteColor(0, 0, 0, 0));
                }
            }
            return bucket;
        }
        private readonly AsepriteColor[] _pixels;

        public AsepriteColor[] Pixels => _pixels;

        public int Width { get; private set; }
        public int Height { get; private set; }

        public string Name { get; set; } = string.Empty;

        public PixelBucket(int width, int height) {
            _pixels = new AsepriteColor[width * height];
            Width = width;
            Height = height;
        }

        public PixelBucket(int width, int height, AsepriteColor[] pixels) : this(width, height) {
            _pixels = pixels;
        }

        public void SetPixel(int x, int y, AsepriteColor color) {
            var index = CalculateIndex(x, y);
            _pixels[index] = color;
        }

        //  _ _ _    _ _
        // |_|_|_|  |_|_|  = 0,0, 2,2
        // |_|_|_|  |_|_|
        // |_|_|_|
        public void SetPixels(int x, int y, int width, int height, AsepriteColor[] colors) {
            for(var i = 0; i < colors.Length; i++) {
                var localImageX = i % width + x;
                var localImageY = (int)(i/width) + y;
                SetPixel(localImageX, localImageY, colors[i]);
            }
        }

        public AsepriteColor GetPixel(int x, int y) {
            var index = CalculateIndex(x, y);
            return _pixels[index];
        }

        protected virtual int CalculateIndex(int x, int y) {
            return y * Width + x;
        }
    }
}
