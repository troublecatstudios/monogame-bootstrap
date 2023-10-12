using System.IO;

namespace Troublecat.Aseprite.Chunks {
    /// <summary>
    /// Represents a "slice key" within the Aseprite file spec
    /// </summary>
    /// <remarks>
    /// + For each slice key<br />
    ///     DWORD Frame number(this slice is valid from this frame to the end of the animation)<br />
    ///     LONG Slice X origin coordinate in the sprite<br />
    ///     LONG Slice Y origin coordinate in the sprite<br />
    ///     DWORD Slice width(can be 0 if this slice hidden in the animation from the given frame)<br />
    ///     DWORD Slice height<br />
    ///     <br />
    ///     + If flags have bit 1<br />
    ///         LONG Center X position(relative to slice bounds)<br />
    ///         LONG Center Y position<br />
    ///         DWORD Center width<br />
    ///         DWORD Center height<br />
    ///     + If flags have bit 2<br />
    ///         LONG Pivot X position(relative to the slice origin)<br />
    ///         LONG Pivot Y position(relative to the slice origin)<br />
    /// </remarks>
    public class SliceEntry {
        public SliceEntry(BinaryReader reader, SliceFlags flags) {
            FrameIndex = reader.ReadAsepriteDword();
            X = reader.ReadAsepriteLong();
            Y = reader.ReadAsepriteLong();
            Width = reader.ReadAsepriteDword();
            Height = reader.ReadAsepriteDword();

            if (flags.HasFlag(SliceFlags.IsNinePatch)) {
                CenterX = reader.ReadAsepriteLong(); // center x position
                CenterY = reader.ReadAsepriteLong(); // center y position
                CenterWidth = reader.ReadAsepriteDword(); // center width
                CenterHeight = reader.ReadAsepriteDword(); // center height
            }

            if (flags.HasFlag(SliceFlags.HasPivot)) {
                // process pivot information
                PivotX = reader.ReadAsepriteLong();
                PivotY = reader.ReadAsepriteLong();
            }
        }

        /// <summary>
        /// X origin coordinate in the sprite
        /// </summary>
        public int X { get; private set; }

        /// <summary>
        /// Y origin coordinate in the sprite
        /// </summary>
        public int Y { get; private set; }

        /// <summary>
        /// Slice width(can be 0 if this slice hidden in the animation from the given frame)
        /// </summary>
        /// <value>
        /// The width.
        /// </value>
        public uint Width { get; private set; }

        /// <summary>
        /// Slice height
        /// </summary>
        /// <value>
        /// The height.
        /// </value>
        public uint Height { get; private set; }

        /// <summary>
        /// Pivot X position(relative to the slice origin)
        /// </summary>
        /// <value>
        /// The pivot x.
        /// </value>
        public int PivotX { get; private set; }

        /// <summary>
        /// Pivot Y position(relative to the slice origin)
        /// </summary>
        /// <value>
        /// The pivot y.
        /// </value>
        public int PivotY { get; private set; }

        /// <summary>
        /// Center X position(relative to slice bounds)
        /// </summary>
        /// <value>
        /// The center x.
        /// </value>
        public int CenterX { get; private set; }

        /// <summary>
        /// Center Y position
        /// </summary>
        /// <value>
        /// The center y.
        /// </value>
        public int CenterY { get; private set; }

        /// <summary>
        /// Center width
        /// </summary>
        /// <value>
        /// The width of the center.
        /// </value>
        public uint CenterWidth { get; private set; }

        /// <summary>
        /// Center height
        /// </summary>
        /// <value>
        /// The height of the center.
        /// </value>
        public uint CenterHeight { get; private set; }

        /// <summary>
        /// Frame number(this slice is valid from this frame to the end of the animation)
        /// </summary>
        /// <value>
        /// The index of the frame.
        /// </value>
        public uint FrameIndex { get; private set; }
    }
}
