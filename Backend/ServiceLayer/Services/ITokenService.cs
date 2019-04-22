﻿using DataAccessLayer.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceLayer.Services
{
    public interface ITokenService
    {
        string GenerateToken();
        string GenerateSignature(string plaintext, Application app);
    }
}
