using Troublecat.Aseprite.Abstractions;

// See: http://wwwimages.adobe.com/www.adobe.com/content/dam/Adobe/en/devnet/pdf/pdfs/PDF32000_2008.pdf
// Page 333
namespace Troublecat.Aseprite.Utils {
    public class Texture2DBlender {
        public static PixelBucket Normal(PixelBucket baseLayer, PixelBucket layer) {
            var newLayer = new PixelBucket(baseLayer.Width, baseLayer.Height);

            for (int x = 0; x < baseLayer.Width; x++) {
                for (int y = 0; y < baseLayer.Height; y++) {
                    var a = baseLayer.GetPixel(x, y);
                    var b = layer.GetPixel(x, y);

                    var c = ((1f - b.a) * a) + (b.a * b);
                    c.a = a.a + b.a * (1f - a.a);

                    newLayer.SetPixel(x, y, c);
                }
            }

            return newLayer;
        }

        public static PixelBucket Multiply(PixelBucket baseLayer, PixelBucket layer, float opacity) {
            var newLayer = new PixelBucket(baseLayer.Width, baseLayer.Height);

            for (int x = 0; x < baseLayer.Width; x++) {
                for (int y = 0; y < baseLayer.Height; y++) {
                    var a = baseLayer.GetPixel(x, y);
                    var b = layer.GetPixel(x, y);

                    var c = new AsepriteColor {
                        r = (a.r) * (opacity * (1f - b.a * (1f - b.r))),
                        g = (a.g) * (opacity * (1f - b.a * (1f - b.g))),
                        b = (a.b) * (opacity * (1f - b.a * (1f - b.b))),
                        a = a.a + b.a * (1f - a.a)
                    };

                    newLayer.SetPixel(x, y, c);
                }
            }

            return newLayer;
        }


        public static PixelBucket Screen(PixelBucket baseLayer, PixelBucket layer) {
            var newLayer = new PixelBucket(baseLayer.Width, baseLayer.Height);

            for (int x = 0; x < baseLayer.Width; x++) {
                for (int y = 0; y < baseLayer.Height; y++) {
                    var a = baseLayer.GetPixel(x, y);
                    var b = layer.GetPixel(x, y);

                    var c = a + b - (a * b);

                    newLayer.SetPixel(x, y, c);
                }
            }

            return newLayer;
        }

        public static PixelBucket Overlay(PixelBucket baseLayer, PixelBucket layer) {
            var newLayer = new PixelBucket(baseLayer.Width, baseLayer.Height);

            for (int x = 0; x < baseLayer.Width; x++) {
                for (int y = 0; y < baseLayer.Height; y++) {
                    var a = baseLayer.GetPixel(x, y);
                    var b = layer.GetPixel(x, y);

                    var c = new AsepriteColor();


                    if (a.r < 0.5)
                        c.r = 2f * a.r * b.r;
                    else
                        c.r = 1f - 2f * (1f - b.r) * (1f - a.r);

                    if (a.g < 0.5)
                        c.g = 2f * a.g * b.g;
                    else
                        c.g = 1f - 2f * (1f - b.g) * (1f - a.g);

                    if (a.b < 0.5)
                        c.b = 2f * a.b * b.b;
                    else
                        c.b = 1f - 2f * (1f - b.b) * (1f - a.b);

                    c = ((1f - b.a) * a) + (b.a * c);

                    c.a = a.a + b.a * (1f - a.a);



                    newLayer.SetPixel(x, y, c);

                }
            }

            return newLayer;
        }

        public static PixelBucket Darken(PixelBucket baseLayer, PixelBucket layer) {
            var newLayer = new PixelBucket(baseLayer.Width, baseLayer.Height);

            for (int x = 0; x < baseLayer.Width; x++) {
                for (int y = 0; y < baseLayer.Height; y++) {
                    var a = baseLayer.GetPixel(x, y);
                    var b = layer.GetPixel(x, y);

                    var c = new AsepriteColor {
                        r = InternalMath.Min(a.r, b.r),
                        g = InternalMath.Min(a.g, b.g),
                        b = InternalMath.Min(a.b, b.b)
                    };

                    c = ((1f - b.a) * a) + (b.a * c);
                    c.a = a.a + b.a * (1f - a.a);

                    newLayer.SetPixel(x, y, c);
                }
            }

            return newLayer;
        }

        public static PixelBucket Lighten(PixelBucket baseLayer, PixelBucket layer) {
            var newLayer = new PixelBucket(baseLayer.Width, baseLayer.Height);

            for (int x = 0; x < baseLayer.Width; x++) {
                for (int y = 0; y < baseLayer.Height; y++) {
                    var a = baseLayer.GetPixel(x, y);
                    var b = layer.GetPixel(x, y);

                    var c = new AsepriteColor {
                        r = ColorBlends.Lighten(a.r, b.r),
                        g = ColorBlends.Lighten(a.g, b.g),
                        b = ColorBlends.Lighten(a.b, b.b)
                    };

                    c = ((1f - b.a) * a) + (b.a * c);
                    c.a = a.a + b.a * (1f - a.a);

                    newLayer.SetPixel(x, y, c);
                }
            }

            return newLayer;
        }

        public static PixelBucket ColorDodge(PixelBucket baseLayer, PixelBucket layer) {
            var newLayer = new PixelBucket(baseLayer.Width, baseLayer.Height);

            for (int x = 0; x < baseLayer.Width; x++) {
                for (int y = 0; y < baseLayer.Height; y++) {
                    var a = baseLayer.GetPixel(x, y);
                    var b = layer.GetPixel(x, y);

                    var c = new AsepriteColor {
                        r = ColorBlends.ColorDodge(a.r, b.r),
                        g = ColorBlends.ColorDodge(a.g, b.g),
                        b = ColorBlends.ColorDodge(a.b, b.b)
                    };

                    c = ((1f - b.a) * a) + (b.a * c);
                    c.a = a.a + b.a * (1f - a.a);

                    newLayer.SetPixel(x, y, c);
                }
            }

            return newLayer;
        }

        public static PixelBucket ColorBurn(PixelBucket baseLayer, PixelBucket layer) {
            var newLayer = new PixelBucket(baseLayer.Width, baseLayer.Height);

            for (int x = 0; x < baseLayer.Width; x++) {
                for (int y = 0; y < baseLayer.Height; y++) {
                    var a = baseLayer.GetPixel(x, y);
                    var b = layer.GetPixel(x, y);

                    var c = new AsepriteColor {
                        r = ColorBlends.ColorBurn(a.r, b.r),
                        g = ColorBlends.ColorBurn(a.g, b.g),
                        b = ColorBlends.ColorBurn(a.b, b.b)
                    };

                    c = ((1f - b.a) * a) + (b.a * c);
                    c.a = a.a + b.a * (1f - a.a);

                    newLayer.SetPixel(x, y, c);
                }
            }

            return newLayer;
        }

        public static PixelBucket HardLight(PixelBucket baseLayer, PixelBucket layer) {
            var newLayer = new PixelBucket(baseLayer.Width, baseLayer.Height);

            for (int x = 0; x < baseLayer.Width; x++) {
                for (int y = 0; y < baseLayer.Height; y++) {
                    var a = baseLayer.GetPixel(x, y);
                    var b = layer.GetPixel(x, y);

                    var c = new AsepriteColor {
                        r = ColorBlends.HardLight(a.r, b.r),
                        g = ColorBlends.HardLight(a.g, b.g),
                        b = ColorBlends.HardLight(a.b, b.b)
                    };

                    c = ((1f - b.a) * a) + (b.a * c);
                    c.a = a.a + b.a * (1f - a.a);

                    newLayer.SetPixel(x, y, c);
                }
            }

            return newLayer;
        }

        public static PixelBucket SoftLight(PixelBucket baseLayer, PixelBucket layer) {
            var newLayer = new PixelBucket(baseLayer.Width, baseLayer.Height);

            for (int x = 0; x < baseLayer.Width; x++) {
                for (int y = 0; y < baseLayer.Height; y++) {
                    var a = baseLayer.GetPixel(x, y);
                    var b = layer.GetPixel(x, y);

                    var c = new AsepriteColor {
                        r = ColorBlends.SoftLight(a.r, b.r),
                        g = ColorBlends.SoftLight(a.g, b.g),
                        b = ColorBlends.SoftLight(a.b, b.b)
                    };

                    c = ((1f - b.a) * a) + (b.a * c);
                    c.a = a.a + b.a * (1f - a.a);

                    newLayer.SetPixel(x, y, c);
                }
            }

            return newLayer;
        }

        public static PixelBucket Difference(PixelBucket baseLayer, PixelBucket layer) {
            var newLayer = new PixelBucket(baseLayer.Width, baseLayer.Height);

            for (int x = 0; x < baseLayer.Width; x++) {
                for (int y = 0; y < baseLayer.Height; y++) {
                    var a = baseLayer.GetPixel(x, y);
                    var b = layer.GetPixel(x, y);

                    var c = new AsepriteColor {
                        r = ColorBlends.Difference(a.r, b.r),
                        g = ColorBlends.Difference(a.g, b.g),
                        b = ColorBlends.Difference(a.b, b.b)
                    };

                    c = ((1f - b.a) * a) + (b.a * c);
                    c.a = a.a + b.a * (1f - a.a);

                    newLayer.SetPixel(x, y, c);
                }
            }

            return newLayer;
        }

        public static PixelBucket Exclusion(PixelBucket baseLayer, PixelBucket layer) {
            var newLayer = new PixelBucket(baseLayer.Width, baseLayer.Height);

            for (int x = 0; x < baseLayer.Width; x++) {
                for (int y = 0; y < baseLayer.Height; y++) {
                    var a = baseLayer.GetPixel(x, y);
                    var b = layer.GetPixel(x, y);

                    var c = new AsepriteColor {
                        r = ColorBlends.Exclusion(a.r, b.r),
                        g = ColorBlends.Exclusion(a.g, b.g),
                        b = ColorBlends.Exclusion(a.b, b.b)
                    };

                    c = ((1f - b.a) * a) + (b.a * c);
                    c.a = a.a + b.a * (1f - a.a);

                    newLayer.SetPixel(x, y, c);
                }
            }

            return newLayer;
        }



        public static PixelBucket Hue(PixelBucket baseLayer, PixelBucket layer) {
            var newLayer = new PixelBucket(baseLayer.Width, baseLayer.Height);

            for (int x = 0; x < baseLayer.Width; x++) {
                for (int y = 0; y < baseLayer.Height; y++) {
                    var a = baseLayer.GetPixel(x, y);
                    var b = layer.GetPixel(x, y);

                    var s = ColorBlends.Sat(a);
                    var l = ColorBlends.Lum(a);

                    var c = ColorBlends.SetLum(ColorBlends.SetSat(b, s), l);

                    c = ((1f - b.a) * a) + (b.a * c);
                    c.a = a.a + b.a * (1f - a.a);

                    newLayer.SetPixel(x, y, c);
                }
            }

            return newLayer;
        }

        public static PixelBucket Saturation(PixelBucket baseLayer, PixelBucket layer) {
            var newLayer = new PixelBucket(baseLayer.Width, baseLayer.Height);

            for (int x = 0; x < baseLayer.Width; x++) {
                for (int y = 0; y < baseLayer.Height; y++) {
                    var a = baseLayer.GetPixel(x, y);
                    var b = layer.GetPixel(x, y);

                    var s = ColorBlends.Sat(b);
                    var l = ColorBlends.Lum(a);

                    var c = ColorBlends.SetLum(ColorBlends.SetSat(a, s), l);

                    c = ((1f - b.a) * a) + (b.a * c);
                    c.a = a.a + b.a * (1f - a.a);

                    newLayer.SetPixel(x, y, c);
                }
            }

            return newLayer;
        }

        public static PixelBucket Color(PixelBucket baseLayer, PixelBucket layer) {
            var newLayer = new PixelBucket(baseLayer.Width, baseLayer.Height);

            for (int x = 0; x < baseLayer.Width; x++) {
                for (int y = 0; y < baseLayer.Height; y++) {
                    var a = baseLayer.GetPixel(x, y);
                    var b = layer.GetPixel(x, y);

                    var c = ColorBlends.SetLum(b, ColorBlends.Lum(a));

                    c = ((1f - b.a) * a) + (b.a * c);
                    c.a = a.a + b.a * (1f - a.a);

                    newLayer.SetPixel(x, y, c);
                }
            }

            return newLayer;
        }

        public static PixelBucket Luminosity(PixelBucket baseLayer, PixelBucket layer) {
            var newLayer = new PixelBucket(baseLayer.Width, baseLayer.Height);

            for (int x = 0; x < baseLayer.Width; x++) {
                for (int y = 0; y < baseLayer.Height; y++) {
                    var a = baseLayer.GetPixel(x, y);
                    var b = layer.GetPixel(x, y);



                    var c = ColorBlends.SetLum(a, ColorBlends.Lum(b));

                    c = ((1f - b.a) * a) + (b.a * c); ;
                    c.a = a.a + b.a * (1f - a.a);

                    newLayer.SetPixel(x, y, c);
                }
            }

            return newLayer;
        }


        public static PixelBucket Addition(PixelBucket baseLayer, PixelBucket layer) {
            var newLayer = new PixelBucket(baseLayer.Width, baseLayer.Height);

            for (int x = 0; x < baseLayer.Width; x++) {
                for (int y = 0; y < baseLayer.Height; y++) {
                    var a = baseLayer.GetPixel(x, y);
                    var b = layer.GetPixel(x, y);

                    var c = a + b;

                    c = ((1f - b.a) * a) + (b.a * c); ;
                    c.a = a.a + b.a * (1f - a.a);

                    newLayer.SetPixel(x, y, c);
                }
            }

            return newLayer;
        }


        public static PixelBucket Subtract(PixelBucket baseLayer, PixelBucket layer) {
            var newLayer = new PixelBucket(baseLayer.Width, baseLayer.Height);

            for (int x = 0; x < baseLayer.Width; x++) {
                for (int y = 0; y < baseLayer.Height; y++) {
                    var a = baseLayer.GetPixel(x, y);
                    var b = layer.GetPixel(x, y);

                    var c = a - b;

                    c = ((1f - b.a) * a) + (b.a * c); ;
                    c.a = a.a + b.a * (1f - a.a);

                    newLayer.SetPixel(x, y, c);
                }
            }

            return newLayer;
        }


        public static PixelBucket Divide(PixelBucket baseLayer, PixelBucket layer) {
            var newLayer = new PixelBucket(baseLayer.Width, baseLayer.Height);

            for (int x = 0; x < baseLayer.Width; x++) {
                for (int y = 0; y < baseLayer.Height; y++) {
                    var a = baseLayer.GetPixel(x, y);
                    var b = layer.GetPixel(x, y);

                    var c = new AsepriteColor(
                        ColorBlends.BlendDivide(a.r, b.r),
                        ColorBlends.BlendDivide(a.g, b.g),
                        ColorBlends.BlendDivide(a.b, b.b)
                        );

                    c = ((1f - b.a) * a) + (b.a * c); ;
                    c.a = a.a + b.a * (1f - a.a);

                    newLayer.SetPixel(x, y, c);
                }
            }

            return newLayer;
        }
    }
}

