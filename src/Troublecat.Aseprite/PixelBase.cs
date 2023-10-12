namespace Troublecat.Aseprite {
    public abstract class PixelBase {
        internal static AsepriteColor _magenta = new(1, 0, 1, 1);
        protected Frame? Frame;

        public PixelBase(Frame frame) {
            Frame = frame;
        }

        /// <summary>
        /// Gets the color.
        /// </summary>
        /// <returns></returns>
        public abstract AsepriteColor GetColor();
    }
}

