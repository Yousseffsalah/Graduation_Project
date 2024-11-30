using Smart.Speaker.Core.Entities.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Smart.Speaker.Core.Service
{
    public interface ITokenService
    {
        Task<string> CreateTokenAsync(AppUser user);
    }
}
