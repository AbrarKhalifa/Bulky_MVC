using Bulky.DataAccess.Data;
using Bulky.DataAccess.Repository.IRepository;
using Bulky.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bulky.DataAccess.Repository
{
    public class BannerImageRepository : Repository<BannerImage>, IBannerImageRepository
    {

        private readonly ApplicationDbContext _db;

        public BannerImageRepository(ApplicationDbContext db) : base(db) 
        {

            _db = db;
        }

        public void Update(BannerImage obj)
        {
            _db.BannerImages.Update(obj);
        }
    }
}
