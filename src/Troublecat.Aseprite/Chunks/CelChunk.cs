using System.IO;
using Troublecat.Aseprite.PixelFormats;


namespace Troublecat.Aseprite.Chunks {
    public enum CelType : ushort {
        Raw = 0,
        Linked = 1,
        Compressed = 2
    }

    public class CelChunk : AsepriteFileChunk {
        /// <summary>
        /// Gets the index of the layer.
        /// </summary>
        /// <value>
        /// The index of the layer.
        /// </value>
        public ushort LayerIndex { get; private set; }

        /// <summary>
        /// Gets the x position of the cel.
        /// </summary>
        /// <value>
        /// The x position.
        /// </value>
        public short X { get; private set; }

        /// <summary>
        /// Gets the y position of the cel.
        /// </summary>
        /// <value>
        /// The y position.
        /// </value>
        public short Y { get; private set; }

        /// <summary>
        /// Gets or sets the width.
        /// </summary>
        /// <value>
        /// The width.
        /// </value>
        public virtual ushort Width { get; protected set; }

        /// <summary>
        /// Gets or sets the height.
        /// </summary>
        /// <value>
        /// The height.
        /// </value>
        public virtual ushort Height { get; protected set; }

        /// <summary>
        /// Gets or sets the opacity.
        /// </summary>
        /// <value>
        /// The opacity.
        /// </value>
        public byte Opacity { get; set; }

        /// <summary>
        /// Gets or sets the type of the cel.
        /// </summary>
        /// <value>
        /// The type of the cel.
        /// </value>
        public CelType CelType { get; set; }

        /// <summary>
        /// Gets the raw pixel data.
        /// </summary>
        /// <value>
        /// The raw pixel data.
        /// </value>
        public virtual PixelBase[] RawPixelData { get; protected set; }

        public CelChunk(uint length, ushort layerIndex, short x, short y, byte opacity, CelType type) : base(length, ChunkType.Cel) {
            LayerIndex = layerIndex;
            X = x;
            Y = y;
            Opacity = opacity;
            CelType = type;
            RawPixelData = System.Array.Empty<PixelBase>();
        }

        protected void ReadPixelData(BinaryReader reader, Frame frame) {
            int size = Width * Height;
            RawPixelData = new PixelBase[size];

            if (frame.File == null) return;
            switch (frame.File.Header.ColorDepth) {
                case ColorDepth.RGBA:
                    for (int i = 0; i < size; i++) {
                        byte[] color = reader.ReadBytes(4);

                        RawPixelData[i] = new RGBAPixel(frame, color);
                    }
                    break;
                case ColorDepth.Grayscale:
                    for (int i = 0; i < size; i++) {
                        byte[] color = reader.ReadBytes(2);

                        RawPixelData[i] = new GrayscalePixel(frame, color);
                    }
                    break;
                case ColorDepth.Indexed:
                    for (int i = 0; i < size; i++) {
                        byte color = reader.ReadByte();

                        RawPixelData[i] = new IndexedPixel(frame, color);
                    }
                    break;
            }
        }

        public static CelChunk? ReadCelChunk(uint length, BinaryReader reader, Frame frame) {
            ushort layerIndex = reader.ReadUInt16();
            short x = reader.ReadInt16();
            short y = reader.ReadInt16();
            byte opacity = reader.ReadByte();
            CelType type = (CelType)reader.ReadUInt16();

            reader.ReadBytes(7); // For Future


            return type switch {
                CelType.Raw => new RawCelChunk(length, layerIndex, x, y, opacity, frame, reader),
                CelType.Linked => new LinkedCelChunk(length, layerIndex, x, y, opacity, frame, reader),
                CelType.Compressed => new CompressedCelChunk(length, layerIndex, x, y, opacity, frame, reader),
                _ => null,
            };
        }

    }
}
