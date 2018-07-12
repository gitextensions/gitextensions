using System.Collections.Generic;
using JetBrains.Annotations;

namespace GitCommands
{
    public static class GitConvert
    {
        private const byte lf = 0x0A;
        private const byte cr = 0x0D;

        [CanBeNull]
        public static byte[] ConvertCrLfToWorktree([CanBeNull] byte[] buf)
        {
            if (buf == null)
            {
                return buf;
            }

            var bufStatistic = new BufStatistic(buf);

            if (bufStatistic.cntLf == 0)
            {
                return buf;
            }

            if (bufStatistic.cntLf == bufStatistic.cntCrlf)
            {
                return buf;
            }

            if (bufStatistic.cntCr != bufStatistic.cntCrlf)
            {
                return buf;
            }

            if (IsBinary())
            {
                return buf;
            }

            var bytes = new List<byte>((int)(buf.Length * 1.01));

            if (buf.Length >= 1)
            {
                if (buf[0] == lf)
                {
                    bytes.Add(cr);
                    bytes.Add(lf);
                }
                else
                {
                    bytes.Add(buf[0]);
                }
            }

            for (var index = 1; index < buf.Length; index++)
            {
                if (buf[index] == lf)
                {
                    if (buf[index - 1] == cr)
                    {
                        bytes.Add(lf);
                    }
                    else
                    {
                        bytes.Add(cr);
                        bytes.Add(lf);
                    }
                }
                else
                {
                    bytes.Add(buf[index]);
                }
            }

            return bytes.ToArray();

            bool IsBinary()
            {
                if (bufStatistic.cntNul > 0)
                {
                    return true;
                }

                if ((bufStatistic.cntPrintable / 128) < bufStatistic.cntNonPrintable)
                {
                    return true;
                }

                return false;
            }
        }

        private readonly struct BufStatistic
        {
            public readonly long cntNul;
            public readonly long cntCr;
            public readonly long cntLf;
            public readonly long cntCrlf;
            public readonly long cntPrintable;
            public readonly long cntNonPrintable;

            public BufStatistic(byte[] buf)
            {
                cntNul = 0;
                cntCr = 0;
                cntLf = 0;
                cntCrlf = 0;
                cntPrintable = 0;
                cntNonPrintable = 0;

                for (long i = 0; i < buf.Length; i++)
                {
                    if (buf[i] == cr)
                    {
                        cntCr++;
                        if (i + 1 < buf.Length)
                        {
                            if (buf[i + 1] == lf)
                            {
                                cntCrlf++;
                            }
                        }

                        continue;
                    }

                    if (buf[i] == lf)
                    {
                        cntLf++;
                        continue;
                    }

                    if (buf[i] == 0x7F)
                    {
                        cntNonPrintable++;
                    }
                    else if (buf[i] < 0x20)
                    {
                        switch (buf[i])
                        {
                            case 0x08:
                            case 0x09:
                            case 0x1B:
                            case 0x0C:
                                cntPrintable++;
                                break;

                            case 0:
                                cntNul++;
                                cntNonPrintable++;
                                break;

                            default:
                                cntNonPrintable++;
                                break;
                        }
                    }
                    else
                    {
                        cntPrintable++;
                    }
                }

                if (buf.Length >= 1)
                {
                    if (buf[buf.Length - 1] == 0x1A)
                    {
                        cntNonPrintable--;
                    }
                }
            }
        }
    }
}
