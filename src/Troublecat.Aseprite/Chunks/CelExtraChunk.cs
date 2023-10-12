using System.IO;

namespace Troublecat.Aseprite.Chunks {
    public class CelExtraChunk : AsepriteFileChunk {
        /// <summary>
        /// Gets the flags.
        /// </summary>
        /// <value>
        /// The flags.
        /// </value>
        public uint Flags { get; private set; }

        /// <summary>
        /// Gets the precise x position.
        /// </summary>
        /// <value>
        /// The precise x position.
        /// </value>
        public double PreciseX { get; private set; }

        /// <summary>
        /// Gets the precise y position.
        /// </summary>
        /// <value>
        /// The precise y position.
        /// </value>
        public double PreciseY { get; private set; }

        /// <summary>
        /// Gets the width.
        /// </summary>
        /// <value>
        /// The width.
        /// </value>
        public double Width { get; private set; }

        /// <summary>
        /// Gets the height.
        /// </summary>
        /// <value>
        /// The height.
        /// </value>
        public double Height { get; private set; }

        public CelExtraChunk(uint length, BinaryReader reader) : base(length, ChunkType.CelExtra) {
            Flags = reader.ReadUInt32();
            PreciseX = reader.ReadDouble();
            PreciseY = reader.ReadDouble();
            Width = reader.ReadDouble();
            Height = reader.ReadDouble();

            reader.ReadBytes(16); // For Future
        }
    }
}
