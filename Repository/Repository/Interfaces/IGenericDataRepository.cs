using System;
using System.Net;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Repository.Interface
{
    public interface IGenericDataRepository 
    {
        Task<List<ENTITY>> GetAsync<ENTITY>(params object[] values) where ENTITY: class;
        Task<List<ENTITY>> GetAsync<ENTITY>(Expression<Func<ENTITY, bool>> selectionCondition) where ENTITY : class;
        Task<HttpStatusCode> UpdateAsync<ENTITY>(ENTITY data) where ENTITY : class;
        Task<HttpStatusCode> InsertAsync<ENTITY>(ENTITY data) where ENTITY : class;
        Task<HttpStatusCode> DeleteAsync<ENTITY>(params object[] values) where ENTITY : class;

        Task<HttpStatusCode> UpdateJoinEntityAsync<ONE, MANY>(ONE data4One, List<dynamic[]> refs2Many) where ONE: class where MANY: class;
        Task<List<dynamic[]>> GetPKs4EntityManyAsync<ONE, MANY>(ONE dataFromOne) where ONE : class where MANY : class;
    }
}












