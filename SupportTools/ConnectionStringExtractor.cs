using System.Text;

namespace WebMarket.OrderService.SupportTools
{
    public static class ConnectionStringExtractor
    {
        //exmp "server=%DB_SERVER%;port=5432;database=%DB_NAME%;uid=%DB_USER%;password=%DB_PASSWORD%"
        private static string GetEnvironmentVariable(string name)
        {
            var res = Environment.GetEnvironmentVariable(name);
            return res == null ? string.Empty : res;
        }

        private static string ReplaceEnvVar(string where, string name)
        {
            return where.Replace($"%{name}%", GetEnvironmentVariable(name));
        }
        //TODO: Remove vars!
        public static string GetReplacedEnvVariables(string raw, string[] vars)
        {
            if (raw == null)
                return string.Empty;
            if (vars.Length == 0) return raw;
            var res = raw;
            foreach (var var in vars)
            {
                res = ReplaceEnvVar(res, var);
            }
            return res;
        }
        public static string GetReplacedEnvVariables(string raw, char delim = '%')
        {
            if (raw == null)
                return string.Empty;

            var result = new StringBuilder();
            for (int i = 0; i < raw.Length; i++)
            {
                if (raw[i] == delim)
                {
                    int envEndInd = raw.IndexOf(delim, i + 1);
                    if (envEndInd > i)
                    {
                        string envVar = raw.Substring(i + 1, envEndInd - i - 1);
                        result.Append(GetEnvironmentVariable(envVar));
                        i = envEndInd;
                    }
                    else
                    {
                        result.Append(raw[i]);
                    }
                }
                else
                {
                    result.Append(raw[i]);
                }
            }
            return result.ToString();
        }
    }
}
