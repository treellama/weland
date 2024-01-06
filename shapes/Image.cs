namespace Weland
{
    public class Image
    {
        public Image(int width, int height)
        {
            Width = width;
            Height = height;

            Data = new byte[Width * Height * 3];
        }

        public int Width;
        public int Height;
        public byte[] Data { get; set; } // r g b

        public Image RotateCW()
        {
            var cw = new Image(Height, Width);
            int dst = 0;

            for (var x = 0; x < Width; ++x)
            {
                for (var y = Height - 1; y >= 0; --y)
                {
                    int src = (x + y * Width) * 3;
                    cw.Data[dst++] = Data[src];
                    cw.Data[dst++] = Data[src + 1];
                    cw.Data[dst++] = Data[src + 2];
                }
            }

            return cw;
        }

        public byte[] ToBGRA()
        {
            byte[] bgra = new byte[Width * Height * 4];
            int src = 0;
            int dst = 0;

            while (src < Data.Length)
            {
                var r = Data[src++];
                var g = Data[src++];
                var b = Data[src++];

                bgra[dst++] = b;
                bgra[dst++] = g;
                bgra[dst++] = r;
                bgra[dst++] = 255;
            }

            return bgra;
        }

        public void FromBGRA(byte[] bgra)
        {
            int src = 0;
            int dst = 0;
            while (src < bgra.Length)
            {
                var b = bgra[src++];
                var g = bgra[src++];
                var r = bgra[src++];
                src++;

                Data[dst++] = r;
                Data[dst++] = g;
                Data[dst++] = b;
            }
        }
    }
}
