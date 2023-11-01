﻿using GStoreWeb.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GStoreWeb.DataAccess.Repository.IRepository
{
    public interface IShoppingCartRepository : IRepository<ShoppingCart>
    {
        void Update(ShoppingCart shoppingCart);
    }
}