﻿using BulkyBook.DataAccess.Repository.IRepository;
using BulkyBookWeb.DataAccess;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace BulkyBook.DataAccess.Repository
{
	public class Repository<T> : IRepository<T> where T : class
	{
		private readonly ApplicationDbContext _db;
		internal DbSet<T> dbset;

		public Repository(ApplicationDbContext db)
		{
			_db = db;
			//_db.ShoppingCarts.AsNoTracking();
			//_db.Products.Include(p => p.Category).Include(p => p.CoverType)
			this.dbset = _db.Set<T>();
		}

		public void Add(T entity)
		{
			this.dbset.Add(entity);
		}


		//includeProperties - "Categories, CoverType"
		public IEnumerable<T> GetAll(Expression<Func<T, bool>>? filter = null, string? includeProperties = null)
		{
			IQueryable<T> query = this.dbset;

			if(filter != null)
				query = query.Where(filter);

			if (includeProperties != null)
			{
				foreach (var includeProperty in includeProperties.Split(new char[] {','}, StringSplitOptions.RemoveEmptyEntries))
				{
					query = query.Include(includeProperty);
				}
			}
			return query.ToList();
		}

		public T GetFirstOrDefault(Expression<Func<T, bool>> filter, string? includeProperties = null, bool tracked = true)
		{
			IQueryable<T> query;
			if (tracked)
			{
				query = this.dbset;
			}
			else
			{
				query = this.dbset.AsNoTracking();
			}

			if (includeProperties != null)
			{
				foreach (var includeProperty in includeProperties.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
				{
					query = query.Include(includeProperty);
				}
			}
			query = query.Where(filter);
			return query.FirstOrDefault();
		}

		public void Remove(T entity)
		{
			this.dbset.Remove(entity);
		}

		public void RemoveRange(IEnumerable<T> entity)
		{
			this.dbset.RemoveRange(entity);
		}
	}
}
