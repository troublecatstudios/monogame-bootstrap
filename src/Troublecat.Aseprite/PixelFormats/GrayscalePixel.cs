namespace Troublecat.Aseprite.PixelFormats {
    public class GrayscalePixel : PixelBase {
        /// <summary>
        /// Gets the color.
        /// </summary>
        /// <value>
        /// The color.
        /// </value>
        public byte[] Color { get; private set; }

        public GrayscalePixel(Frame frame, byte[] color) : base(frame) {
            Color = color;
        }

        public override AsepriteColor GetColor() {
            float value = (float)Color[0] / 255;
            float alpha = (float)Color[1] / 255;

            return new AsepriteColor(value, value, value, alpha);
        }
    }
}
