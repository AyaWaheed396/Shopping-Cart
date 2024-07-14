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
    public class OrderHeaderRepository : GenericRepository<OrderHeader> , IOrderHeaderRepository
    {
        private readonly ApplicationDbContext _context;
        public OrderHeaderRepository(ApplicationDbContext context) : base(context)
        {
            _context = context;
        }

        

		public void Update(OrderHeader orderHeader)
		{
			_context.OrderHeaders.Add(orderHeader);
		}

		public void UpdateOrderStatus(int id, string OrderStatus, string PaymentStatus)
		{
			var orderFromDB = _context.OrderHeaders.FirstOrDefault(x => x.Id == id);
			if (orderFromDB != null)
			{
				orderFromDB.OrderStatus = OrderStatus;
				orderFromDB.PaymentDate = DateTime.Now;
				if(PaymentStatus != null)
					orderFromDB.PaymentStatus = PaymentStatus;
			}
		}
	}
}
