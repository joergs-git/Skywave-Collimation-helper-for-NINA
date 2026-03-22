using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace NINA.AstroCircular.SkyWaver.Imaging {

    /// <summary>
    /// Reads 16-bit FITS files written by RawFitsWriter.
    /// Minimal parser — handles SIMPLE/BITPIX/NAXIS/BZERO format only.
    /// </summary>
    public static class RawFitsReader {

        public class FitsImage {
            public ushort[] Pixels { get; set; }
            public int Width { get; set; }
            public int Height { get; set; }
        }

        /// <summary>
        /// Read a 16-bit FITS file and return pixel data as ushort array.
        /// </summary>
        public static FitsImage Read(string filePath) {
            using (var stream = new FileStream(filePath, FileMode.Open, FileAccess.Read))
            using (var reader = new BinaryReader(stream)) {
                int width = 0, height = 0;
                int bitpix = 16;
                double bzero = 0;

                // Read header (2880-byte blocks of 80-char cards)
                bool endFound = false;
                while (!endFound) {
                    byte[] block = reader.ReadBytes(2880);
                    if (block.Length < 2880) break;

                    string headerBlock = Encoding.ASCII.GetString(block);
                    for (int c = 0; c < 36; c++) {
                        string card = headerBlock.Substring(c * 80, 80);
                        string keyword = card.Substring(0, 8).Trim();

                        if (keyword == "END") { endFound = true; break; }
                        if (keyword == "NAXIS1") width = ParseIntValue(card);
                        if (keyword == "NAXIS2") height = ParseIntValue(card);
                        if (keyword == "BITPIX") bitpix = ParseIntValue(card);
                        if (keyword == "BZERO") bzero = ParseDoubleValue(card);
                    }
                }

                if (width == 0 || height == 0) {
                    throw new InvalidDataException($"Invalid FITS header: NAXIS1={width}, NAXIS2={height}");
                }

                // Read pixel data (big-endian 16-bit)
                int pixelCount = width * height;
                ushort[] pixels = new ushort[pixelCount];

                for (int p = 0; p < pixelCount; p++) {
                    byte hi = reader.ReadByte();
                    byte lo = reader.ReadByte();
                    short stored = (short)((hi << 8) | lo);
                    // Apply BZERO: physical = stored + bzero
                    pixels[p] = (ushort)(stored + (int)bzero);
                }

                return new FitsImage { Pixels = pixels, Width = width, Height = height };
            }
        }

        private static int ParseIntValue(string card) {
            string valPart = card.Substring(10, 20).Trim();
            int slashIdx = valPart.IndexOf('/');
            if (slashIdx >= 0) valPart = valPart.Substring(0, slashIdx).Trim();
            if (int.TryParse(valPart, out int result)) return result;
            return 0;
        }

        private static double ParseDoubleValue(string card) {
            string valPart = card.Substring(10, 20).Trim();
            int slashIdx = valPart.IndexOf('/');
            if (slashIdx >= 0) valPart = valPart.Substring(0, slashIdx).Trim();
            if (double.TryParse(valPart, System.Globalization.NumberStyles.Any,
                System.Globalization.CultureInfo.InvariantCulture, out double result)) return result;
            return 0;
        }
    }
}
