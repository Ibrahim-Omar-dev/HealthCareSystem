using HealthCare.Domain.IRepository;
using HealthCare.Infreastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace HealthCare.Infreastructure.Repository
{
    internal class Repository<TEntity> : IRepository<TEntity> where TEntity : class
    {
        private readonly AppDbContext context;

        public Repository(AppDbContext context)
        {
            this.context = context;
        }
        public async Task<TEntity> AddAsync(TEntity entity)
        {
            await context.AddAsync(entity);
            await context.SaveChangesAsync();
            return entity;
        }

        public async Task<bool> DeleteById(Guid id)
        {
            try
            {
                var entity = await context.Set<TEntity>().FindAsync(id);

                if (entity == null)
                {
                    return false;
                }

                context.Set<TEntity>().Remove(entity);
                await context.SaveChangesAsync();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> DeleteByName(string name)
        {
            try
            {
                var entity = await context.Set<TEntity>()
                    .FirstOrDefaultAsync(e => EF.Property<string>(e, "Name") == name);

                if (entity == null)
                {
                    return false;
                }

                context.Set<TEntity>().Remove(entity);
                await context.SaveChangesAsync();

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<IEnumerable<TEntity>> GetAll()
        {
            return await context.Set<TEntity>().AsNoTracking().ToListAsync();
        }

        public async Task<TEntity?> GetById(Guid id)
        {

            return await context.Set<TEntity>().FindAsync(id);
        }

        public async Task<TEntity?> GetByName(string name)
        {
            return await context.Set<TEntity>().FirstOrDefaultAsync(e => EF.Property<string>(e, "Name").ToLower().Contains(name.ToLower()));

        }

        public async Task<TEntity> GetByIdNoTracking(object id)
        {
            return await context.Set<TEntity>()
                .AsNoTracking()
                .FirstOrDefaultAsync(e => EF.Property<object>(e, "Id").Equals(id));
        }

        public async Task<bool> Update(TEntity entity)
        {
            context.Set<TEntity>().Update(entity);
            var rowsAffected = await context.SaveChangesAsync();
            return rowsAffected > 0;
        }

    }
}
