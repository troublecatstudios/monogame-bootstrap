using System;
using System.IO;
using System.Text;

namespace Troublecat.Aseprite.Chunks {
    [Flags]
    public enum LayerFlags : ushort {
        Visible = 1,
        Editable = 2,
        LockMovement = 4,
        Background = 8,
        PreferLinkedCels = 16,
        DisplayCollapsed = 32,
        ReferenceLayer = 64,
    }

    public enum LayerType : ushort {
        Normal = 0,
        Group = 1
    }

    public enum LayerBlendMode : ushort {
        Normal = 0,
        Multiply = 1,
        Screen = 2,
        Overlay = 3,
        Darken = 4,
        Lighten = 5,
        ColorDodge = 6,
        ColorBurn = 7,
        HardLight = 8,
        SoftLight = 9,
        Difference = 10,
        Exclusion = 11,
        Hue = 12,
        Saturation = 13,
        Color = 14,
        Luminosity = 15,
        Addition = 16,
        Subtract = 17,
        Divide = 18
    }

    public class LayerChunk : AsepriteFileChunk {
        /// <summary>
        /// Gets the flags.
        /// </summary>
        /// <value>
        /// The flags.
        /// </value>
        public LayerFlags Flags { get; private set; }

        /// <summary>
        /// Gets the type of the layer.
        /// </summary>
        /// <value>
        /// The type of the layer.
        /// </value>
        public LayerType LayerType { get; private set; }

        /// <summary>
        /// Gets the layer child level.
        /// </summary>
        /// <value>
        /// The layer child level.
        /// </value>
        public ushort LayerChildLevel { get; private set; }

        /// <summary>
        /// Gets the default width of the layer.
        /// </summary>
        /// <value>
        /// The default width of the layer.
        /// </value>
        public ushort DefaultLayerWidth { get; private set; } // Ignored

        /// <summary>
        /// Gets the default height of the layer.
        /// </summary>
        /// <value>
        /// The default height of the layer.
        /// </value>
        public ushort DefaultLayerHeight { get; private set; } // Ignored

        /// <summary>
        /// Gets the blend mode.
        /// </summary>
        /// <value>
        /// The blend mode.
        /// </value>
        public LayerBlendMode BlendMode { get; private set; }

        /// <summary>
        /// Gets the opacity.
        /// </summary>
        /// <value>
        /// The opacity.
        /// </value>
        public byte Opacity { get; private set; }

        /// <summary>
        /// Gets the name of the layer.
        /// </summary>
        /// <value>
        /// The name of the layer.
        /// </value>
        public string LayerName { get; private set; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="LayerChunk"/> is visible.
        /// </summary>
        /// <value>
        ///   <c>true</c> if visible; otherwise, <c>false</c>.
        /// </value>
        public bool Visible => Flags.HasFlag(LayerFlags.Visible);

        /// <summary>
        /// Gets a value indicating whether this <see cref="LayerChunk"/> is editable.
        /// </summary>
        /// <value>
        ///   <c>true</c> if editable; otherwise, <c>false</c>.
        /// </value>
        public bool Editable => Flags.HasFlag(LayerFlags.Editable);

        public LayerChunk(uint length, BinaryReader reader) : base(length, ChunkType.Layer) {
            Flags = (LayerFlags)reader.ReadUInt16();
            LayerType = (LayerType)reader.ReadUInt16();
            LayerChildLevel = reader.ReadUInt16();

            DefaultLayerWidth = reader.ReadUInt16(); // Ignored
            DefaultLayerHeight = reader.ReadUInt16(); // Ignored

            BlendMode = (LayerBlendMode)reader.ReadUInt16();
            Opacity = reader.ReadByte();

            reader.ReadBytes(3); // For future

            ushort nameLength = reader.ReadUInt16();
            ///int nameLength = (int)(length - 18) - Chunk.HEADER_SIZE;
            LayerName = Encoding.Default.GetString(reader.ReadBytes(nameLength));
        }
    }
}
