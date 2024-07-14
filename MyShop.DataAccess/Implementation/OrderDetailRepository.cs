using Microsoft.Extensions.Logging;
using MyShop.BusinessLogic.Models;
using MyShop.BusinessLogic.Repositories;
using MyShop.DataAccess.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MyShop.DataAccess.Implementation
{
    public class OrderDetailRepository : GenericRepository<OrderDetail> , IOrderDetailRepository
	{
        private readonly ApplicationDbContext _context;
        public OrderDetailRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        

		public void Update(OrderDetail orderDetail)
		{
			_context.OrderDetails.Update(orderDetail);
		}

		
	}
}
