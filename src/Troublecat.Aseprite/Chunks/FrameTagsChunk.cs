using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Troublecat.Aseprite.Chunks {
    public enum LoopAnimation : byte {
        Forward = 0,
        Reverse = 1,
        PingPong = 2,
    }


    public class FrameTag {
        /// <summary>
        /// Gets the starting frame index.
        /// </summary>
        public ushort FrameFrom { get; private set; }

        /// <summary>
        /// Gets the ending frame index.
        /// </summary>
        public ushort FrameTo { get; private set; }

        /// <summary>
        /// Gets the animation type.
        /// </summary>
        /// <value>
        /// The animation type.
        /// </value>
        public LoopAnimation Animation { get; private set; }
        private byte[] ForFuture { get; set; } // 8 Bytes

        /// <summary>
        /// Gets or sets the color of the tag.
        /// </summary>
        /// <value>
        /// The color of the tag.
        /// </value>
        public AsepriteColor TagColor { get; set; } // 3 Bytes
                                                    // 1 Extra Byte

        /// <summary>
        /// Gets the name of the tag.
        /// </summary>
        /// <value>
        /// The name of the tag.
        /// </value>
        public string TagName { get; private set; }

        public FrameTag(BinaryReader reader) {
            FrameFrom = reader.ReadUInt16();
            FrameTo = reader.ReadUInt16();
            Animation = (LoopAnimation)reader.ReadByte();
            ForFuture = reader.ReadBytes(8);

            byte[] colorBytes = reader.ReadBytes(3);

            TagColor = new AsepriteColor((colorBytes[0] / 255f), (colorBytes[1] / 255f), (colorBytes[2] / 255f));

            reader.ReadByte(); // Extra byte (zero)

            ushort nameLength = reader.ReadUInt16();
            TagName = Encoding.Default.GetString(reader.ReadBytes(nameLength));
        }
    }

    public class FrameTagsChunk : AsepriteFileChunk {
        /// <summary>
        /// Gets the tag count.
        /// </summary>
        /// <value>
        /// The tag count.
        /// </value>
        public ushort TagCount { get; private set; }
        private byte[] ForFuture { get; set; } // 8 Bytes

        /// <summary>
        /// Gets the tags.
        /// </summary>
        /// <value>
        /// The tags.
        /// </value>
        public List<FrameTag> Tags { get; private set; }

        public FrameTagsChunk(uint length, BinaryReader reader) : base(length, ChunkType.FrameTags) {
            TagCount = reader.ReadUInt16();
            ForFuture = reader.ReadBytes(8);

            Tags = new List<FrameTag>();

            for (int i = 0; i < TagCount; i++) {
                Tags.Add(new FrameTag(reader));
            }
        }
    }
}
