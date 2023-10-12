using Troublecat.Aseprite.Abstractions;

namespace Troublecat.Aseprite {
    public struct AsepriteColor {
        private float _r;
        private float _g;
        private float _b;
        private float _a;

        public float R { get => _r; set => _r = value; }
        public float r { get => _r; set => _r = value; }
        public float G { get => _g; set => _g = value; }
        public float g { get => _g; set => _g = value; }
        public float B { get => _b; set => _b = value; }
        public float b { get => _b; set => _b = value; }
        public float A { get => _a; set => _a = value; }
        public float a { get => _a; set => _a = value; }

        public AsepriteColor(float r = 0f, float g = 0f, float b = 0f, float a = 0f) {
            _r = r;
            _g = g;
            _b = b;
            _a = a;
        }

        public AsepriteColor(AsepriteColor color) : this(color.r, color.g, color.b, color.a) { }

        // Adds two colors together. Each component is added separately.
        public static AsepriteColor operator +(AsepriteColor a, AsepriteColor b) { return new AsepriteColor(a.r + b.r, a.g + b.g, a.b + b.b, a.a + b.a); }

        // Subtracts color /b/ from color /a/. Each component is subtracted separately.
        public static AsepriteColor operator -(AsepriteColor a, AsepriteColor b) { return new AsepriteColor(a.r - b.r, a.g - b.g, a.b - b.b, a.a - b.a); }

        // Multiplies two colors together. Each component is multiplied separately.
        public static AsepriteColor operator *(AsepriteColor a, AsepriteColor b) { return new AsepriteColor(a.r * b.r, a.g * b.g, a.b * b.b, a.a * b.a); }

        // Multiplies color /a/ by the float /b/. Each color component is scaled separately.
        public static AsepriteColor operator *(AsepriteColor a, float b) { return new AsepriteColor(a.r * b, a.g * b, a.b * b, a.a * b); }

        // Multiplies color /a/ by the float /b/. Each color component is scaled separately.
        public static AsepriteColor operator *(float b, AsepriteColor a) { return new AsepriteColor(a.r * b, a.g * b, a.b * b, a.a * b); }

        // Divides color /a/ by the float /b/. Each color component is scaled separately.
        public static AsepriteColor operator /(AsepriteColor a, float b) { return new AsepriteColor(a.r / b, a.g / b, a.b / b, a.a / b); }

        public static bool operator ==(AsepriteColor lhs, AsepriteColor rhs) {
            // Returns false in the presence of NaN values.
            return lhs.R == rhs.R && lhs.G == rhs.G && lhs.B == rhs.B && lhs.A == rhs.A;
        }

        public static bool operator !=(AsepriteColor lhs, AsepriteColor rhs) {
            // Returns true in the presence of NaN values.
            return lhs.R != rhs.R || lhs.G != rhs.G || lhs.B != rhs.B || lhs.A != rhs.A;
        }

        // used to allow Colors to be used as keys in hash tables
        public override int GetHashCode() {
            return r.GetHashCode() ^ (g.GetHashCode() << 2) ^ (b.GetHashCode() >> 2) ^ (a.GetHashCode() >> 1);
        }

        // also required for being able to use Colors as keys in hash tables
        public override bool Equals(object? other) {
            if (other == null) return false;
            if (!(other is AsepriteColor)) return false;

            return Equals((AsepriteColor)other);
        }

        public bool Equals(AsepriteColor other) {
            return r.Equals(other.r) && g.Equals(other.g) && b.Equals(other.b) && a.Equals(other.a);
        }
    }
}
