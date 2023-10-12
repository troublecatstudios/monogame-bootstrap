using System.IO;
using System.Text;

namespace Troublecat.Aseprite {
    internal static class BInaryReaderExtensions {
        /// <summary>
        /// Reads the aseprite byte.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns>An 8-bit unsigned integer value</returns>
        public static byte ReadAsepriteByte(this BinaryReader reader) { return reader.ReadByte(); }
        /// <summary>
        /// Reads the aseprite word.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns>A 16-bit unsigned integer value</returns>
        public static ushort ReadAsepriteWord(this BinaryReader reader) { return reader.ReadUInt16(); }
        /// <summary>
        /// Reads the aseprite short.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns>A 16-bit signed integer value</returns>
        public static short ReadAsepriteShort(this BinaryReader reader) { return reader.ReadInt16(); }
        /// <summary>
        /// Reads the aseprite dword.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns>A 32-bit unsigned integer value</returns>
        public static uint ReadAsepriteDword(this BinaryReader reader) { return reader.ReadUInt32(); }
        /// <summary>
        /// Reads the aseprite long.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns>A 32-bit signed integer value</returns>
        public static int ReadAsepriteLong(this BinaryReader reader) { return reader.ReadInt32(); }
        /// <summary>
        /// Reads the aseprite string. First calls <see cref="ReadAsepriteWord(BinaryReader)"/> to get the string length. Then calls <see cref="ReadAsepriteBytes(BinaryReader, int)"/> to get the string value.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <returns>characters (in UTF-8) The '\0' character is not included.</returns>
        public static string ReadAsepriteString(this BinaryReader reader) { return Encoding.UTF8.GetString(reader.ReadAsepriteBytes(reader.ReadAsepriteWord())); }
        /// <summary>
        /// Reads the aseprite bytes.
        /// </summary>
        /// <param name="reader">The reader.</param>
        /// <param name="number">The number.</param>
        /// <returns>An array of 8-bit unsigned integer values</returns>
        public static byte[] ReadAsepriteBytes(this BinaryReader reader, int number) { return reader.ReadBytes(number); }
        public static void Seek(this BinaryReader reader, int number) { reader.BaseStream.Position += number; }
    }
}
