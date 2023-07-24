﻿using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBookWeb.DataAccess;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository
{
	public class UnitOfWork : IUnitOfWork
	{
		private readonly ApplicationDbContext _db;

		public UnitOfWork(ApplicationDbContext db)
		{
			_db = db;
			Category = new CategoryRepository(_db);
			CoverType = new CoverTypRepository(_db);
			Product = new ProductRepository(_db);
			Company = new CompanyRepository(_db);
			ApplicationUser= new ApplicationUserRepository(_db);
			ShoppingCart= new ShoppingCartRepository(_db);
			OrderDetail = new OrderDetailRepository(_db);
			OrderHeader = new OrderHeaderRepository(_db);

		}

		public ICategoryRepository Category { get; private set; }
		public ICoverTypeRepository CoverType { get; private set; }

		public IProductRepository Product { get; private set; }

		public ICompanyRepository Company { get; private set; }

		public IShoppingCartRepository ShoppingCart { get; private set; }
		public IApplicationUserRepository ApplicationUser { get; private set; }

		public IOrderHeaderRepository OrderHeader { get; set; }
		public IOrderDetailRepository OrderDetail { get; set; }

		public void Save()
		{
			_db.SaveChanges();
		}
	}
}
