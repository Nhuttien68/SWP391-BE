using EVMarketPlace.Services.Interfaces;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EVMarketPlace.Services.Implements
{

        public class OtpService : IOtpService
        {
            private readonly IMemoryCache _cache;
            public OtpService(IMemoryCache cache)
            {
                _cache = cache;
            }

            public string GenerateAndSaveOtp(string email, int expireMinutes = 5)
            {
                var random = new Random();
                var otp = string.Join("", Enumerable.Range(0, 6).Select(_ => random.Next(0, 10)));

                // Lưu vào cache với key là email
                _cache.Set(email, otp, TimeSpan.FromMinutes(expireMinutes));

                return otp;
            }

            public bool VerifyOtp(string email, string otp)
            {
                if (_cache.TryGetValue(email, out string cachedOtp))
                {
                    if (cachedOtp == otp)
                    {
                        // Xóa sau khi dùng (1 lần)
                        _cache.Remove(email);
                        return true;
                    }
                }
                return false;
            }
        }
    }

