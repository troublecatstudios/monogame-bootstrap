using System.Collections.Generic;
using System.IO;

namespace Troublecat.Aseprite.Chunks {
    /// <summary>
    /// Represents a slice within a frame
    /// </summary>
    /// <seealso cref="Aseprite.Chunk" />
    /// <remarks>
    ///    DWORD       Number of "slice keys"<br />
    ///    DWORD Flags<br />
    ///              1 = It's a 9-patches slice<br />
    ///              2 = Has pivot information<br />
    ///    DWORD       Reserved<br />
    ///    STRING      Name<br />
    /// </remarks>
    public class SliceChunk : AsepriteFileChunk {

        private readonly IList<SliceEntry> _entries = new List<SliceEntry>();

        /// <summary>
        /// Initializes a new instance of the <see cref="SliceChunk"/> class.
        /// </summary>
        /// <param name="length">The length.</param>
        /// <param name="reader">The reader.</param>
        public SliceChunk(uint length, BinaryReader reader) : base(length, ChunkType.Slice) {
            KeyCount = reader.ReadAsepriteDword();
            Flags = (SliceFlags)reader.ReadAsepriteDword();
            reader.ReadAsepriteDword(); // Reserved, Ignored
            SliceName = reader.ReadAsepriteString();

            for (int i = 0; i < KeyCount; i++) {
                _entries.Add(new SliceEntry(reader, Flags));
            }
        }

        /// <summary>
        /// Gets the entries.
        /// </summary>
        /// <value>
        /// The entries.
        /// </value>
        public IList<SliceEntry> Entries => _entries;

        /// <summary>
        /// Gets the key count.
        /// </summary>
        /// <value>
        /// The key count.
        /// </value>
        public uint KeyCount { get; private set; }

        /// <summary>
        /// Gets the flags.
        /// </summary>
        /// <value>
        /// The flags.
        /// </value>
        public SliceFlags Flags { get; private set; }

        /// <summary>
        /// Gets the name of the slice.
        /// </summary>
        /// <value>
        /// The name of the slice.
        /// </value>
        public string SliceName { get; private set; }
    }
}
