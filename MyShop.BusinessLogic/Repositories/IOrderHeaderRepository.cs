﻿using MyShop.BusinessLogic.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShop.BusinessLogic.Repositories
{
    public interface IOrderHeaderRepository : IGenericRepository<OrderHeader>
    { 
        void Update(OrderHeader orderHeader); 
        void UpdateOrderStatus(int id, string OrderStatus, string PaymentStatus); 
    }
}
