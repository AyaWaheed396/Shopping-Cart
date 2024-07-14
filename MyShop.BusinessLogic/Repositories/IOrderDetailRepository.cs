using MyShop.BusinessLogic.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShop.BusinessLogic.Repositories
{
    public interface IOrderDetailRepository : IGenericRepository<OrderDetail>
    { 
        void Update(OrderDetail orderDetail); 
      
    }
}
