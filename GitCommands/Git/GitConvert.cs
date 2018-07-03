using System.Collections.Generic;
using JetBrains.Annotations;

namespace GitCommands
{
    public static class GitConvert
    {
        [CanBeNull]
        public static byte[] ConvertCrLfToWorktree(byte[] buf)
        {
            if (buf == null)
            {
                return buf;
            }

            BufStatistic bufStatistic = GetBufStatistic(buf);

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

            if (IsBinary(bufStatistic))
            {
                return buf;
            }

            List<byte> byteList = new List<byte>();

            if (buf.LongLength >= 1)
            {
                if (buf[0] == 0x0A)
                {
                    byteList.Add(0x0D);
                    byteList.Add(0x0A);
                }
                else
                {
                    byteList.Add(buf[0]);
                }
            }

            for (long index = 1; index < buf.LongLength; index++)
            {
                if (buf[index] == 0x0A)
                {
                    if (buf[index - 1] == 0x0D)
                    {
                        byteList.Add(0x0A);
                    }
                    else
                    {
                        byteList.Add(0x0D);
                        byteList.Add(0x0A);
                    }
                }
                else
                {
                    byteList.Add(buf[index]);
                }
            }

            return byteList.ToArray();
        }

        private struct BufStatistic
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
                    if (buf[i] == 0x0D)
                    {
                        cntCr++;
                        if (i + 1 < buf.Length)
                        {
                            if (buf[i + 1] == 0x0A)
                            {
                                cntCrlf++;
                            }
                        }

                        continue;
                    }

                    if (buf[i] == 0x0A)
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

        private static bool IsBinary(BufStatistic bufStatistic)
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
}
