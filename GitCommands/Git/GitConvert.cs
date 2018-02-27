using System.Collections.Generic;

namespace GitCommands
{
    public class GitConvert
    {
        public static byte[] ConvertCrLfToWorktree(byte[] buf)
        {
            if (buf == null)
            {
                return buf;
            }

            BufStatistic bufStatistic = GetBufStatistic(buf);

            if (bufStatistic.CntLf == 0)
            {
                return buf;
            }

            if (bufStatistic.CntLf == bufStatistic.CntCrlf)
            {
                return buf;
            }

            if (bufStatistic.CntCr != bufStatistic.CntCrlf)
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

            for (long index = 1;  index < buf.LongLength; index++)
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
            public long CntNul;
            public long CntCr;
            public long CntLf;
            public long CntCrlf;

            public long CntPrintable;
            public long CntNonPrintable;

            public void ResetBufStatistic()
            {
                CntNul = 0;
                CntCr = 0;
                CntLf = 0;
                CntCrlf = 0;
                CntPrintable = 0;
                CntNonPrintable = 0;
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
                    bufStatistic.CntCr++;
                    if (i + 1 < buf.Length)
                    {
                        if (buf[i + 1] == 0x0A)
                        {
                            bufStatistic.CntCrlf++;
                        }
                    }

                    continue;
                }

                if (buf[i] == 0x0A)
                {
                    bufStatistic.CntLf++;
                    continue;
                }

                if (buf[i] == 0x7F)
                {
                    bufStatistic.CntNonPrintable++;
                }
                else if (buf[i] < 0x20)
                {
                    switch (buf[i])
                    {
                        case 0x08:
                        case 0x09:
                        case 0x1B:
                        case 0x0C:
                            bufStatistic.CntPrintable++;
                            break;

                        case 0:
                            bufStatistic.CntNul++;
                            bufStatistic.CntNonPrintable++;
                            break;

                        default:
                            bufStatistic.CntNonPrintable++;
                            break;
                    }
                }
                else
                {
                    bufStatistic.CntPrintable++;
                }
            }

            if (buf.Length >= 1)
            {
                if (buf[buf.Length - 1] == 0x1A)
                {
                    bufStatistic.CntNonPrintable--;
                }
            }

            return bufStatistic;
        }

        public static bool IsBinary(byte[] buf)
        {
            BufStatistic bufStatistic = GetBufStatistic(buf);
            if (bufStatistic.CntNul > 0)
            {
                return true;
            }

            if ((bufStatistic.CntPrintable / 128) < bufStatistic.CntNonPrintable)
            {
                return true;
            }

            return false;
        }
    }
}
