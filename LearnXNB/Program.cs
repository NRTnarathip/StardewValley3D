using LearnXNB.Microsoft.Xna.Framework.Content;
using SkiaSharp;
using System.Drawing;
using static System.Net.Mime.MediaTypeNames;

internal class Program
{
    private static void Main(string[] args)
    {
        var gameContentDir = @"C:\Program Files (x86)\Steam\steamapps\common\Stardew Valley\Content";
        var files = Directory.GetFiles(gameContentDir, "*.xnb", SearchOption.AllDirectories);
        foreach (var filePath in files)
        {
            var fileInfo = new FileInfo(filePath);
            try
            {
                Console.WriteLine("try decode file: " + fileInfo.Name);
                DecodeXNB(filePath);
            }
            catch (Exception ex)
            {
                Console.WriteLine("failed decode path: " + filePath);
                Console.WriteLine(ex);
            }
        }

        Console.ReadKey();
    }
    private static void DecodeXNB(string imgPath)
    {
        using var stream = File.OpenRead(imgPath);
        using var reader = new BinaryReader(stream);

        // อ่าน header: XNB
        byte magic1 = reader.ReadByte(); // 'X'
        byte magic2 = reader.ReadByte(); // 'N'
        byte magic3 = reader.ReadByte(); // 'B'
        byte platform = reader.ReadByte(); // เช่น 'w' = Windows

        if (magic1 != 'X' || magic2 != 'N' || magic3 != 'B')
        {
            throw new Exception("ไม่ใช่ไฟล์ XNB ที่ถูกต้อง หรือไม่ได้แปลงสำหรับแพลตฟอร์มที่รองรับ");
        }

        byte version = reader.ReadByte();         // ควรเป็น 5 (XNA 4.0) หรือ 4 (XNA 3.1)
        byte flags = reader.ReadByte();           // บิต 0x80 = LZX, 0x40 = LZ4

        bool isLzxCompressed = (flags & 0x80) != 0;
        bool isLz4Compressed = (flags & 0x40) != 0;

        int xnbFileSize = reader.ReadInt32();

        if (isLzxCompressed || isLz4Compressed)
        {
            int decompressedSize = reader.ReadInt32(); // ขนาดหลัง decompress

            if (isLzxCompressed)
            {
                int compressedDataSize = xnbFileSize - 14; // 14 = header size (10 bytes + 4 decompressSize)
                Console.WriteLine("decode with LZX");
                var decoder = new LzxDecoder(16);
                var resultStream = MyDecompress(decoder, stream, decompressedSize, compressedDataSize);
                var pixels = resultStream.ToArray();

                resultStream.Dispose();
            }
            else // isLz4Compressed
            {
                Console.WriteLine("decode with LZ4");
            }
        }

    }

    static MemoryStream MyDecompress(LzxDecoder dec, Stream stream, int decompressedSize, int compressedSize)
    {
        var decompressedStream = new MemoryStream(decompressedSize);
        long startPos = stream.Position;
        long pos = startPos;
        while (pos - startPos < compressedSize)
        {
            int num = stream.ReadByte();
            int lo = stream.ReadByte();
            int block_size = (num << 8) | lo;
            int frame_size = 32768;
            if (num == 255)
            {
                int num2 = lo;
                lo = (byte)stream.ReadByte();
                frame_size = (num2 << 8) | lo;
                byte num3 = (byte)stream.ReadByte();
                lo = (byte)stream.ReadByte();
                block_size = (num3 << 8) | lo;
                pos += 5;
            }
            else
            {
                pos += 2;
            }
            if (block_size == 0 || frame_size == 0)
            {
                break;
            }
            dec.Decompress(stream, block_size, decompressedStream, frame_size);
            pos += block_size;
            stream.Seek(pos, SeekOrigin.Begin);
        }
        if (decompressedStream.Position != decompressedSize)
        {
            throw new Exception("Decompression failed.");
        }

        decompressedStream.Seek(0L, SeekOrigin.Begin);
        return decompressedStream;
    }

    static void SaveRawPng(byte[] rawPixels, int width, int height, string outputPath)
    {
        // สร้าง Bitmap แบบ 32-bit ARGB
        // สร้าง SKBitmap จาก raw RGBA
        using var bitmap = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);

        // คัดลอก raw → memory ของ bitmap
        unsafe
        {
            fixed (byte* ptr = rawPixels)
            {
                bitmap.InstallPixels(bitmap.Info, (IntPtr)ptr, bitmap.Info.RowBytes);
            }
        }

        // เขียนเป็น PNG ด้วย SKImage
        using var image = SKImage.FromBitmap(bitmap);
        using var data = image.Encode(SKEncodedImageFormat.Png, quality: 100);
        using var fs = File.OpenWrite(outputPath);
        data.SaveTo(fs);
    }
}
