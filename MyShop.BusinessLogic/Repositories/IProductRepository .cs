﻿using MyShop.BusinessLogic.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShop.BusinessLogic.Repositories
{
    public interface IProductRepository : IGenericRepository<Product>
    { 
        void Update(Product product); 
    }
}
