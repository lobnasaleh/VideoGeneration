using CoursesManagementSystem.Constants;
using System.Linq.Expressions;

namespace CoursesManagementSystem.Interfaces
{
    public interface IBaseRepository<T> where T : class
    {
       
       //getall and getone but returning iqueryable 

        public IQueryable<T> GetAllQuery(Expression<Func<T, bool>> condition = null,
           string[] include = null,
           Expression<Func<T, object>> orderby = null, string orderByDirection = OrderByDirection.Ascending,
           int pagenumber = 1, int pagesize = 0);

        public IQueryable<T> GetQuery(Expression<Func<T, bool>>? condition, string[] include = null, bool Tracking = true);

        //getAll-->condition
        public Task<IEnumerable<T>> GetAllAsync(
            Expression<Func<T,bool>>? condition=null,string[]include=null,Expression<Func<T,object>>? orderby=null,
            string orderByDirection=OrderByDirection.Ascending,
            int pagenumber=1,int pagesize=0  );

        //getOne-->condition

        public Task<T> GetAsync(Expression<Func<T, bool>>? condition, string[] include = null,bool Tracking=true);

        //getById 
        public Task<T> GetByIdAsync(int id);
        //add
        public Task<T> AddAsync(T entity);

        //update
        public void Update(T newentity);

        //delete
        public void Delete(T entity);

    }
}
