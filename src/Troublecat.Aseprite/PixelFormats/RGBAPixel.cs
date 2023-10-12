namespace Troublecat.Aseprite.PixelFormats {
    public class RGBAPixel : PixelBase {
        /// <summary>
        /// Gets the color.
        /// </summary>
        /// <value>
        /// The color.
        /// </value>
        public byte[] Color { get; private set; }

        public RGBAPixel(Frame frame, byte[] color) : base(frame) {
            Color = color;
        }

        public override AsepriteColor GetColor() {
            if (Color.Length == 4) {
                float red = (float)Color[0] / 255f;
                float green = (float)Color[1] / 255f;
                float blue = (float)Color[2] / 255f;
                float alpha = (float)Color[3] / 255f;

                return new AsepriteColor(red, green, blue, alpha);
            } else {
                return _magenta;
            }
        }
    }
}
