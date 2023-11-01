using GStoreWeb.DataAccess.Data;
using GStoreWeb.DataAccess.Repository.IRepository;
using GStoreWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GStoreWeb.DataAccess.Repository
{
    public class OrderHeaderRepository : Repository<OrderHeader>, IOrderHeaderRepository
    {
        private ApplicationDbContext _db;
        public OrderHeaderRepository(ApplicationDbContext db) : base(db)
        {
            _db = db;
        }
        public void Update(OrderHeader header)
        {
            _db.Update(header);
        }

		public void UpdateStatus(int id, string orderStatus, string? paymentStatus = null)
		{
			var orderHeaderFromDb = _db.OrderHeaders.FirstOrDefault(x=>x.Id == id);
            orderHeaderFromDb.OrderStatus = orderStatus;
            if(!string.IsNullOrEmpty(paymentStatus) )
            {
                orderHeaderFromDb.PaymentStatus = paymentStatus;    
            }
		}

		public void UpdateStripePaymentID(int id, string sessionId, string paymentIntentId)
		{
			var orderHeaderFromDb = _db.OrderHeaders.FirstOrDefault(x => x.Id == id);
            if(!string.IsNullOrEmpty(sessionId) )
            {
                orderHeaderFromDb.SessionId = sessionId;
            }
            if(!string.IsNullOrEmpty(paymentIntentId) )
            {
                orderHeaderFromDb.PaymentIntentId = paymentIntentId;
                orderHeaderFromDb.PaymentDate = DateTime.Now;
            }
		}
	}
}
