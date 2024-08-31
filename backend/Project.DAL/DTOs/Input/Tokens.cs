using Microsoft.Extensions.Diagnostics.HealthChecks;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Project.DAL.DTOs.Input
{
    public class Tokens
    {
        public required string AccessToken { get; set; }
        public required string RefreshToken { get; set; }
    }
}
