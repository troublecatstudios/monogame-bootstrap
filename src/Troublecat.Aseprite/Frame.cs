using System.Collections.Generic;
using System.IO;
using Troublecat.Aseprite.Chunks;

namespace Troublecat.Aseprite {
    public class Frame {
        public AsepriteFile? File = null;
        /// <summary>
        /// Gets the number of bytes in this frame.
        /// </summary>
        /// <value>
        /// The length.
        /// </value>
        public uint Length { get; private set; }

        /// <summary>
        /// Gets the magic number. Always 0xF1Fa.
        /// </summary>
        /// <value>
        /// The magic number.
        /// </value>
        public ushort MagicNumber { get; private set; }
        /// <summary>
        /// Gets the chunks count.
        /// </summary>
        /// <value>
        /// The chunks count.
        /// </value>
        public ushort ChunksCount { get; private set; }

        /// <summary>
        /// Gets the duration of the frame in milliseconds.
        /// </summary>
        /// <value>
        /// The duration of the frame.
        /// </value>
        public ushort FrameDuration { get; private set; }

        public List<AsepriteFileChunk> Chunks { get; private set; }

        public Frame(AsepriteFile file, BinaryReader reader) {
            File = file;

            Length = reader.ReadUInt32();
            MagicNumber = reader.ReadUInt16();

            ChunksCount = reader.ReadUInt16();
            FrameDuration = reader.ReadUInt16();

            reader.ReadBytes(6); // For Future

            Chunks = new();

            for (int i = 0; i < ChunksCount; i++) {
                var chunk = AsepriteFileChunk.ReadChunk(this, reader);
                if (chunk != null)
                    Chunks.Add(chunk);
            }
        }

        public T? GetCelChunk<T>(int layerIndex) where T : CelChunk {
            if (Chunks == null) return null;
            for (int i = 0; i < Chunks.Count; i++) {
                if (Chunks[i] != null && Chunks[i] is T t && (Chunks[i] as CelChunk)?.LayerIndex == layerIndex) {
                    return t;
                }
            }

            return null;
        }

        public List<T> GetChunks<T>() where T : AsepriteFileChunk {
            List<T> chunks = new List<T>();

            for (int i = 0; i < Chunks.Count; i++) {
                if (Chunks[i] is T t) {
                    chunks.Add(t);
                }
            }

            return chunks;
        }
    }
}
