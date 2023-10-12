using System.Collections.Generic;
using System.IO;

namespace Troublecat.Aseprite.Chunks {
    public class PaletteEntry {
        /// <summary>
        /// Gets the entry flags.
        /// </summary>
        /// <value>
        /// The entry flags.
        /// </value>
        public ushort EntryFlags { get; private set; }

        /// <summary>
        /// Gets the red channel value.
        /// </summary>
        /// <value>
        /// The red.
        /// </value>
        public byte Red { get; private set; }

        /// <summary>
        /// Gets the green channel value.
        /// </summary>
        /// <value>
        /// The green.
        /// </value>
        public byte Green { get; private set; }

        /// <summary>
        /// Gets the blue channel value.
        /// </summary>
        /// <value>
        /// The blue.
        /// </value>
        public byte Blue { get; private set; }

        /// <summary>
        /// Gets the alpha channel value.
        /// </summary>
        /// <value>
        /// The alpha.
        /// </value>
        public byte Alpha { get; private set; }

        /// <summary>
        /// Gets the name.
        /// </summary>
        /// <value>
        /// The name.
        /// </value>
        public string Name { get; private set; }

        public PaletteEntry(BinaryReader reader) {
            EntryFlags = reader.ReadUInt16();

            Red = reader.ReadByte();
            Green = reader.ReadByte();
            Blue = reader.ReadByte();
            Alpha = reader.ReadByte();

            Name = string.Empty;

            if ((EntryFlags & 1) != 0) {
                Name = reader.ReadString();
            }
        }
    }


    public class PaletteChunk : AsepriteFileChunk {
        /// <summary>
        /// Gets the size of the palette.
        /// </summary>
        /// <value>
        /// The size of the palette.
        /// </value>
        public uint PaletteSize { get; private set; }

        /// <summary>
        /// Gets the first index of the color.
        /// </summary>
        /// <value>
        /// The first index of the color.
        /// </value>
        public uint FirstColorIndex { get; private set; }

        /// <summary>
        /// Gets the last index of the color.
        /// </summary>
        /// <value>
        /// The last index of the color.
        /// </value>
        public uint LastColorIndex { get; private set; }

        // Future (8) bytes

        /// <summary>
        /// Gets the entries.
        /// </summary>
        /// <value>
        /// The entries.
        /// </value>
        public List<PaletteEntry> Entries { get; private set; }


        public PaletteChunk(uint length, BinaryReader reader) : base(length, ChunkType.Palette) {
            PaletteSize = reader.ReadUInt32();
            FirstColorIndex = reader.ReadUInt32();
            LastColorIndex = reader.ReadUInt32();

            reader.ReadBytes(8); // For Future

            Entries = new List<PaletteEntry>();

            for (int i = 0; i < PaletteSize; i++) {
                Entries.Add(new PaletteEntry(reader));
            }
        }

        /// <summary>
        /// Gets the color.
        /// </summary>
        /// <param name="index">The index.</param>
        /// <returns></returns>
        public AsepriteColor GetColor(byte index) {
            if (index >= FirstColorIndex && index <= LastColorIndex) {
                PaletteEntry entry = Entries[index];

                float red = (float)entry.Red / 255f;
                float green = (float)entry.Green / 255f;
                float blue = (float)entry.Blue / 255f;
                float alpha = (float)entry.Alpha / 255f;

                return new AsepriteColor(red, green, blue, alpha);
            } else {
                return PixelBase._magenta;
            }
        }
    }
}
