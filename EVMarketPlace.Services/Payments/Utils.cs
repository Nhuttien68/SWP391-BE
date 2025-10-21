using Microsoft.AspNetCore.Http;

namespace EVMarketPlace.Services.Implements
{
    public static class Utils
    {
        public static string GetIpAddress(HttpContext context)
        {
            var ipAddress = string.Empty;
            
            try
            {
                var remoteIpAddress = context.Connection.RemoteIpAddress;
                
                if (remoteIpAddress != null)
                {
                    if (remoteIpAddress.AddressFamily == System.Net.Sockets.AddressFamily.InterNetworkV6)
                    {
                        remoteIpAddress = System.Net.Dns.GetHostEntry(remoteIpAddress).AddressList
                            .FirstOrDefault(x => x.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork);
                    }

                    if (remoteIpAddress != null)
                    {
                        ipAddress = remoteIpAddress.ToString();
                    }
                }
            }
            catch (Exception)
            {
                ipAddress = "Invalid IP:" + context.Connection.RemoteIpAddress;
            }

            return ipAddress;
        }
    }
}