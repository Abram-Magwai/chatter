using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace chatter.core.interfaces
{
    public interface IMongoRepository <TDocument> {
        Task<List<TDocument>> GetAsync();
        Task<TDocument?> GetAsync(string id);
        Task CreateAsync(TDocument model);
        Task UpdateAsync(string id, TDocument updatedModel);
        Task RemoveAsync(string id);
        TDocument FindOne(Expression<Func<TDocument, bool>> filterExpression);
        IQueryable<TDocument> AsQueryable();
    }
}