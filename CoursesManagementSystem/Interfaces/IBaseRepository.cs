using CoursesManagementSystem.Constants;
using System.Linq.Expressions;

namespace CoursesManagementSystem.Interfaces
{
    public interface IBaseRepository<T> where T : class
    {
        //getAll-->condition
        //condition,include,orderby,orderdirection,pagenumber,pagezsize

        public Task<IEnumerable<T>> GetAllAsync(
            Expression<Func<T,bool>>? condition=null,string[]include=null,Expression<Func<T,object>>? orderby=null,
            string orderByDirection=OrderByDirection.Ascending,
            int pagenumber=1,int pagesize=0  );

        //getOne-->condition

        public Task<T> GetAsync(Expression<Func<T, bool>>? condition, string[] include = null,bool Tracking=true);

        //add
        public Task<T> AddAsync(T entity);

        //update
        public void Update(T newentity);

        //delete
        public void Delete(T entity);

    }
}
