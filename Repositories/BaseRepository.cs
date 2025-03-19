using Microsoft.EntityFrameworkCore;
using WebMarket.OrderService.Models;

namespace WebMarket.OrderService.Repositories
{
    public abstract class BaseRepository<T> where T : class
    {
        protected OrdersDbContext _context;
        protected readonly DbSet<T> _dbSet;

        protected BaseRepository(OrdersDbContext context, Func<OrdersDbContext, DbSet<T>> dbSetFactory)
        {
            _context = context;
            _dbSet = dbSetFactory(context);
        }

        protected static bool IsIdValid(int id) => id > 0;
    }
}
