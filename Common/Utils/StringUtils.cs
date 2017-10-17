using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BitAuto.CarDataUpdate.Common.Utils
{
    public static class StringUtils
    {
        #region Private Static Fields

        private const string DELIM_START = "@";
        private const char DELIM_STOP = '@';
        private const int DELIM_START_LEN = 1;
        private const int DELIM_STOP_LEN = 1;

        #endregion

        public static string SubstituteVariables(string value, IDictionary<string, string> props)
        {
            StringBuilder buf = new StringBuilder();

            int i = 0;
            int j, k;

            while (true)
            {
                j = value.IndexOf(DELIM_START, i);
                if (j == -1)
                {
                    if (i == 0)
                    {
                        return value;
                    }
                    else
                    {
                        buf.Append(value.Substring(i, value.Length - i));
                        return buf.ToString();
                    }
                }
                else
                {
                    buf.Append(value.Substring(i, j - i));
                    k = value.IndexOf(DELIM_STOP, j + DELIM_START_LEN);
                    if (k == -1)
                    {
                        buf.Append(value.Substring(j, value.Length - j));
                        return buf.ToString();
                    }
                    else
                    {
                        j += DELIM_START_LEN;
                        string key = value.Substring(j, k - j);
                        if (props.ContainsKey(key))
                        {
                            string replacement = props[key] as string;

                            if (replacement != null)
                            {
                                buf.Append(replacement);
                            }
                        }
                        i = k + DELIM_STOP_LEN;
                    }
                }
            }
        }
    }
}
