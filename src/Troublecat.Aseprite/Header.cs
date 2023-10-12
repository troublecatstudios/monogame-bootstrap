using System;
using System.IO;


namespace Troublecat.Aseprite {
    public enum ColorDepth : ushort {
        RGBA = 32,
        Grayscale = 16,
        Indexed = 8
    }

    /// <summary>
    /// A 128-byte header (same as FLC/FLI header, but with other magic number):
    /// </summary>
    public class Header {

        /// <summary>
        /// Gets the size of the file.
        /// </summary>
        /// <value>
        /// The size of the file.
        /// </value>
        public uint FileSize { get; private set; }
        /// <summary>
        /// Gets the magic number. Always 0xA5E0.
        /// </summary>
        /// <value>
        /// The magic number.
        /// </value>
        public ushort MagicNumber { get; private set; }
        /// <summary>
        /// Gets the number of frames.
        /// </summary>
        /// <value>
        /// The number of frames.
        /// </value>
        public ushort Frames { get; private set; }
        /// <summary>
        /// Gets the width in pixels.
        /// </summary>
        /// <value>
        /// The width.
        /// </value>
        public ushort Width { get; private set; }
        /// <summary>
        /// Gets the height in pixels.
        /// </summary>
        /// <value>
        /// The height.
        /// </value>
        public ushort Height { get; private set; }
        /// <summary>
        /// Gets the color depth.
        /// </summary>
        /// <value>
        /// The color depth.
        /// </value>
        public ColorDepth ColorDepth { get; private set; }
        /// <summary>
        /// Gets the flags. If 1 then layer opacity has a valid value.
        /// </summary>
        /// <value>
        /// The flags.
        /// </value>
        public uint Flags { get; private set; }
        /// <summary>
        /// Gets the milliseconds before this frame, like in FLC files.
        /// </summary>
        /// <value>
        /// The speed.
        /// </value>
        [Obsolete("You should use the Frame.Duration instead.")]
        public ushort Speed { get; private set; }
        /// <summary>
        /// Gets the index of the palette entry which represents transparency in all non-background layers. Only used if ColorDepth = ColorDepth.Indexed.
        /// </summary>
        /// <value>
        /// The index of the transparent.
        /// </value>
        public byte TransparentIndex { get; private set; }
        /// <summary>
        /// Gets the number of colors present. 0 means 256 for old sprites.
        /// </summary>
        /// <value>
        /// The color count.
        /// </value>
        public ushort ColorCount { get; private set; }
        /// <summary>
        /// Gets the width of the pixel. Pixel ratio is pixel width/pixel height. If 0 then pixel ratio is 1:1.
        /// </summary>
        /// <value>
        /// The width of the pixel.
        /// </value>
        public byte PixelWidth { get; private set; }
        /// <summary>
        /// Gets the height of the pixel. Pixel ratio is pixel width/pixel height. If 0 then pixel ratio is 1:1.
        /// </summary>
        /// <value>
        /// The height of the pixel.
        /// </value>
        public byte PixelHeight { get; private set; }

        public Header(byte[] header) {
            if (header.Length != 128)
                return;

            Stream stream = new MemoryStream(header);
            BinaryReader reader = new BinaryReader(stream);

            FileSize = reader.ReadAsepriteDword();                      // File size
            MagicNumber = reader.ReadAsepriteWord();                    // Magic number (0xA5E0)

            if (MagicNumber != 0xA5E0) {
                throw new Exception("File is not in .ase format");
            }

            Frames = reader.ReadAsepriteWord();                         // Frames
            Width = reader.ReadAsepriteWord();                          // Width in pixels
            Height = reader.ReadAsepriteWord();                         // Height in pixels
            ColorDepth = (ColorDepth)reader.ReadAsepriteWord();         // Color depth (bits per pixel) [32 bpp = RGBA, 16 bpp = Grayscale, 8 bpp Indexed]
            Flags = reader.ReadAsepriteDword();                         // Flags: 1 = Layer opacity has valid value
#pragma warning disable CS0618 // Type or member is obsolete
            Speed = reader.ReadAsepriteWord();                          // Speed (milliseconds between frame, like in FLC files) DEPRECATED: You should use the frame duration field from each frame header
#pragma warning restore CS0618 // Type or member is obsolete

            reader.ReadAsepriteDword();                                 // Set be 0
            reader.ReadAsepriteDword();                                 // Set be 0

            TransparentIndex = reader.ReadAsepriteByte();               // Palette entry (index) which represent transparent color in all non-background layers (only for Indexed sprites)

            reader.ReadAsepriteBytes(3);                                // Ignore these bytes

            ColorCount = reader.ReadAsepriteWord();                     // Number of colors (0 means 256 for old sprites)
            PixelWidth = reader.ReadAsepriteByte();                     // Pixel width (pixel ratio is "pixel width/pixel height"). If pixel height field is zero, pixel ratio is 1:1
            PixelHeight = reader.ReadAsepriteByte();                    // Pixel height

            reader.ReadAsepriteBytes(92);                               // For future
        }

    }
}
