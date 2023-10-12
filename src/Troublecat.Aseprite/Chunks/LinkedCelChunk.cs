using System;
using System.IO;

namespace Troublecat.Aseprite.Chunks {
    public class LinkedCelChunk : CelChunk {
        private readonly AsepriteFile? _file = null;
        private CelChunk? _linkedCelChunk = null;

        /// <summary>
        /// Gets the linked cel.
        /// </summary>
        /// <value>
        /// The linked cel.
        /// </value>
        public CelChunk? LinkedCel {
            get {
                if (_linkedCelChunk == null && _file != null) {
                    _linkedCelChunk = _file.Frames[FramePosition].GetCelChunk<CelChunk>(LayerIndex);
                }

                return _linkedCelChunk;
            }
        }

        /// <summary>
        /// Gets the frame position.
        /// </summary>
        /// <value>
        /// The frame position.
        /// </value>
        public ushort FramePosition { get; private set; }

        public override ushort Width => LinkedCel?.Width ?? 0;
        public override ushort Height => LinkedCel?.Height ?? 0;
        public override PixelBase[] RawPixelData => LinkedCel?.RawPixelData ?? Array.Empty<PixelBase>();

        public LinkedCelChunk(uint length, ushort layerIndex, short x, short y, byte opacity, Frame frame, BinaryReader reader) : base(length, layerIndex, x, y, opacity, CelType.Linked) {
            _file = frame.File;

            FramePosition = reader.ReadUInt16();
        }
    }
}
