using GStoreWeb.DataAccess.Data;
using GStoreWeb.DataAccess.Repository.IRepository;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GStoreWeb.DataAccess.Repository
{
    public class UnitOfWork : IUnitOfWork
    {
        private ApplicationDbContext _db;
        public ICategoryRepository CategoryUnit { get; private set; }
        public IProductRepository ProductUnit { get; private set; }
        public ICompanyRepository CompanyUnit { get; private set; }
        public IShoppingCartRepository ShoppingCartUnit { get; private set; }
        public IApplicationUserRepository ApplicationUserUnit { get; private set; }
        public IOrderHeaderRepository OrderHeaderUnit { get; private set; }
        public IOrderDetailRepository OrderDetailUnit { get; private set; }
        public UnitOfWork(ApplicationDbContext db)
        {
            _db=db;
            CategoryUnit = new CategoryRepository(_db);
            ProductUnit = new ProductRepository(_db);
            CompanyUnit = new CompanyRepository(_db);
            ShoppingCartUnit = new ShoppingCartRepository(_db);
            ApplicationUserUnit = new ApplicationUserRepository(_db);
            OrderHeaderUnit = new OrderHeaderRepository(_db);
            OrderDetailUnit = new OrderDetailRepository(_db);
        }
        public void Save()
        {
            _db.SaveChanges();
        }
    }
}
