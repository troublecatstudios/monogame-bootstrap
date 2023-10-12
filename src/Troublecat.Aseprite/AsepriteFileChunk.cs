using Troublecat.Aseprite.Chunks;
using System.IO;

namespace Troublecat.Aseprite {
    public enum ChunkType : ushort {
        OldPalette = 0x0004,
        OldPalette2 = 0x0011,
        Layer = 0x2004,
        Cel = 0x2005,
        CelExtra = 0x2006,
        Mask = 0x2016, // DEPRECATED
        Path = 0x2017, // NEVER USED
        FrameTags = 0x2018,
        Palette = 0x2019,
        UserData = 0x2020,
        Slice = 0x2022,
        TileSet = 0x2023
    }

    public class AsepriteFileChunk {
        public const int HEADER_SIZE = 6;

        protected Frame? Frame = null;

        /// <summary>
        /// Gets the length of the chunk in bytes.
        /// </summary>
        /// <value>
        /// The length.
        /// </value>
        public uint Length { get; private set; }

        /// <summary>
        /// Gets the type of the chunk.
        /// </summary>
        /// <value>
        /// The type of the chunk.
        /// </value>
        public ChunkType ChunkType { get; private set; }

        public AsepriteFileChunk(uint length, ChunkType type) {
            Length = length;
            ChunkType = type;
        }

        /// <summary>
        /// Reads a chunk from a binary stream.
        /// </summary>
        /// <param name="frame">The frame.</param>
        /// <param name="reader">The reader.</param>
        /// <returns></returns>
        public static AsepriteFileChunk? ReadChunk(Frame frame, BinaryReader reader) {
            uint length = reader.ReadAsepriteDword();
            ChunkType type = (ChunkType)reader.ReadAsepriteWord();

            switch (type) {
                case ChunkType.Cel:
                    return CelChunk.ReadCelChunk(length, reader, frame);
                case ChunkType.CelExtra:
                    return new CelExtraChunk(length, reader) { Frame = frame };
                case ChunkType.Layer:
                    return new LayerChunk(length, reader) { Frame = frame };
                case ChunkType.FrameTags:
                    return new FrameTagsChunk(length, reader) { Frame = frame };
                case ChunkType.Palette:
                    return new PaletteChunk(length, reader) { Frame = frame };
                case ChunkType.Slice:
                    return new SliceChunk(length, reader) { Frame = frame };
            }

            reader.BaseStream.Position += length - AsepriteFileChunk.HEADER_SIZE;
            return null;
        }
    }
}
