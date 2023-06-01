using System.Buffers.Binary;
using System.Collections;
using System.Drawing.Imaging;
using System.Globalization;
using System.Resources;
using System.Text;
using FluentAssertions;
using GitUI.Properties;

namespace GitUITests
{
    [TestFixture]
    public class ResourcesTests
    {
        [Test]
        public void PngResources_ShouldNotContainDPIInformation()
        {
            // arrange
            // Note: do not dispose it, as it's a global resource used by others.
            ResourceSet resourceSet = Images.ResourceManager.GetResourceSet(CultureInfo.InvariantCulture, createIfNotExists: true, tryParents: false);

            // act & assert
            foreach (DictionaryEntry resourceEntry in resourceSet)
            {
                if (resourceEntry.Value is not Bitmap bitmap || !ImageFormat.Png.Equals(bitmap.RawFormat))
                {
                    continue;
                }

                using MemoryStream ms = new();
                bitmap.Save(ms, bitmap.RawFormat);

                IEnumerable<string> pngSectionsList = GetPngSectionTypesList(ms.ToArray());

                pngSectionsList.Should().NotContain("pHYs", $"Images PNG resource '{resourceEntry.Key}' should not contain the 'pHYs' section. Use http://entropymine.com/jason/tweakpng/ tool to remove it.");
            }
        }

        [Test]
        public void ShouldCorrectlyIdentifyPngSections()
        {
            // arrange
            byte[] pngBytes = Convert.FromBase64String("iVBORw0KGgoAAAANSUhEUgAAAAEAAAABCAYAAAAfFcSJAAAAAXNSR0IArs4c6QAAAARnQU1BAACxjwv8YQUAAAAJcEhZcwAADsMAAA7DAcdvqGQAAAANSURBVBhXY2A4/P8/AAVLAsKqu7MNAAAAAElFTkSuQmCC");

            // act
            IEnumerable<string> sections = GetPngSectionTypesList(pngBytes);

            // assert
            sections.Should().Equal("IHDR", "sRGB", "gAMA", "pHYs", "IDAT", "IEND");
        }

        private static IEnumerable<string> GetPngSectionTypesList(byte[] pngBytes)
        {
            Span<byte> cursor = pngBytes.AsSpan();

            // PNG format is very simple - HEADER + N * SECTION
            // Read more info on format here: https://en.wikipedia.org/wiki/Portable_Network_Graphics#File_format

            // Verify header
            ReadOnlySpan<byte> header = stackalloc byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };
            if (!header.SequenceEqual(cursor[0..header.Length]))
            {
                throw new ArgumentException("PNG does not contain a valid header");
            }

            // Skip header
            cursor = cursor[header.Length..];

            List<string> sectionTypes = new();

            // Iterate sections
            while (cursor.Length > 0)
            {
                string type = Encoding.UTF8.GetString(cursor.Slice(4, 4));
                sectionTypes.Add(type);

                int payloadSize = BinaryPrimitives.ReadInt32BigEndian(cursor);
                int sectionSize = (4 * 3) + payloadSize;

                cursor = cursor[sectionSize..];
            }

            return sectionTypes;
        }
    }
}
