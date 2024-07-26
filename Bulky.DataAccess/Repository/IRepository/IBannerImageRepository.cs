﻿using Bulky.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Repository.IRepository
{
    public interface IBannerImageRepository : IRepository<BannerImage>
    {

        void Update(BannerImage obj);

    }
}
