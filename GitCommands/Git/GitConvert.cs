using System.Collections.Generic;

namespace GitCommands
{
    public static class GitConvert
    {
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

            if (IsBinary(buf))
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
            public long cntNul;
            public long cntCr;
            public long cntLf;
            public long cntCrlf;

            public long cntPrintable;
            public long cntNonPrintable;

            public void ResetBufStatistic()
            {
                cntNul = 0;
                cntCr = 0;
                cntLf = 0;
                cntCrlf = 0;
                cntPrintable = 0;
                cntNonPrintable = 0;
            }
        }

        private static BufStatistic GetBufStatistic(byte[] buf)
        {
            BufStatistic bufStatistic = new BufStatistic();
            bufStatistic.ResetBufStatistic();

            if (buf == null)
            {
                return bufStatistic;
            }

            for (long i = 0; i < buf.Length; i++)
            {
                if (buf[i] == 0x0D)
                {
                    bufStatistic.cntCr++;
                    if (i + 1 < buf.Length)
                    {
                        if (buf[i + 1] == 0x0A)
                        {
                            bufStatistic.cntCrlf++;
                        }
                    }

                    continue;
                }

                if (buf[i] == 0x0A)
                {
                    bufStatistic.cntLf++;
                    continue;
                }

                if (buf[i] == 0x7F)
                {
                    bufStatistic.cntNonPrintable++;
                }
                else if (buf[i] < 0x20)
                {
                    switch (buf[i])
                    {
                        case 0x08:
                        case 0x09:
                        case 0x1B:
                        case 0x0C:
                            bufStatistic.cntPrintable++;
                            break;

                        case 0:
                            bufStatistic.cntNul++;
                            bufStatistic.cntNonPrintable++;
                            break;

                        default:
                            bufStatistic.cntNonPrintable++;
                            break;
                    }
                }
                else
                {
                    bufStatistic.cntPrintable++;
                }
            }

            if (buf.Length >= 1)
            {
                if (buf[buf.Length - 1] == 0x1A)
                {
                    bufStatistic.cntNonPrintable--;
                }
            }

            return bufStatistic;
        }

        public static bool IsBinary(byte[] buf)
        {
            BufStatistic bufStatistic = GetBufStatistic(buf);
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
