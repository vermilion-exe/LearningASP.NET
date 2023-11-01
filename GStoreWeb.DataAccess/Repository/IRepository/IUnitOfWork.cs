using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GStoreWeb.DataAccess.Repository.IRepository
{
    public interface IUnitOfWork
    {
        public ICategoryRepository CategoryUnit { get; }
        public IProductRepository ProductUnit { get; }
        public ICompanyRepository CompanyUnit { get; }
        public IShoppingCartRepository ShoppingCartUnit { get; }
        public IApplicationUserRepository ApplicationUserUnit { get; }
        public IOrderHeaderRepository OrderHeaderUnit { get; }
        public IOrderDetailRepository OrderDetailUnit { get; }
        void Save();
    }
}
