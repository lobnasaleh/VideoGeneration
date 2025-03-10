﻿using CoursesManagementSystem.Constants;
using CoursesManagementSystem.Data;
using CoursesManagementSystem.Interfaces;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace CoursesManagementSystem.Repository
{
    public class BaseRepository<T> : IBaseRepository<T> where T :class
    {
        private readonly ApplicationDbContext context;

        public BaseRepository(ApplicationDbContext context)
        {
            this.context = context;
        }
        public async Task<T> AddAsync(T entity)
        {

           await context.Set<T>().AddAsync(entity);
            
            return entity;

        }

        public void Delete(T entity)
        {
            context.Set<T>().Remove(entity);
        }

        public IQueryable<T> GetAllQuery(Expression<Func<T, bool>> condition = null,
           string[] include = null,
           Expression<Func<T, object>> orderby = null, string orderByDirection = OrderByDirection.Ascending,
           int pagenumber = 1, int pagesize = 0)
        {
            IQueryable<T> res = context.Set<T>();

            if (condition != null)
            {
                res = res.Where(condition);
            }
            if (include != null)
            {

                foreach (var incl in include)
                {
                    res = res.Include(incl);
                }
            }
            if (orderby != null)
            {

                if (orderByDirection == OrderByDirection.Ascending)
                {
                    res = res.OrderBy(orderby);
                }
                else if (orderByDirection == OrderByDirection.Descending)
                {
                    res = res.OrderByDescending(orderby);
                }
            }
            if (pagesize > 0)
            {
                if (pagesize > 100)
                {
                    pagesize = 100;
                }
                res = res.Skip(pagesize * (pagenumber - 1)).Take(pagesize);
            }

            return res;
        }
        public IQueryable<T> GetQuery(Expression<Func<T, bool>> condition, string[] include = null, bool Tracking = true)
        {
            IQueryable<T> res = context.Set<T>();

            if (condition != null)
            {
                res = res.Where(condition);
            }
            if (include != null)
            {

                foreach (var incl in include)
                {
                    res = res.Include(incl);
                }
            }
            if (!Tracking)
            {
                res = res.AsNoTracking();
            }

            return res;
        }
        public async Task<IEnumerable<T>> GetAllAsync(Expression<Func<T, bool>> condition = null,
            string[] include = null,
            Expression<Func<T, object>> orderby = null, string orderByDirection = OrderByDirection.Ascending,
            int pagenumber = 1, int pagesize = 0)
        {
            IQueryable<T> res = context.Set<T>();

            if (condition != null) { 
             res= res.Where(condition);
            }
            if (include != null) {

                foreach (var incl in include)
                {
                   res= res.Include(incl);
                }
            }
            if (orderby != null) { 
             
                if (orderByDirection == OrderByDirection.Ascending)
                {
                    res= res.OrderBy(orderby);
                }
                else if (orderByDirection == OrderByDirection.Descending)
                {
                    res=res.OrderByDescending(orderby);
                }
            }
            if (pagesize > 0)
            {
                if (pagesize > 100)
                {
                    pagesize = 100;
                }
               res=res.Skip(pagesize * (pagenumber - 1)).Take(pagesize);
            }

            return await res.ToListAsync();
        }
        public async Task<T> GetAsync(Expression<Func<T, bool>> condition, string[] include = null, bool Tracking = true)
        {
            IQueryable<T> res = context.Set<T>();

            if (condition != null)
            {
                res = res.Where(condition);
            }
            if (include != null)
            {

                foreach (var incl in include)
                {
                    res = res.Include(incl);
                }
            }
            if (!Tracking)
            {
                res = res.AsNoTracking();
            }

            return await res.FirstOrDefaultAsync();
        }

        public async Task<T> GetByIdAsync(int id)
        {
            var res = context.Set<T>();

            return await res.FindAsync(id);
           
        }

        public void Update(T newentity)
        {
            context.Set<T>().Update(newentity);
            
        }

 
    }
}
