using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Data.Entity;
using System.Linq.Expressions;
using Repository.Interface;
using System.Threading.Tasks;
using System.Text;
using System.Data.Entity.Core.Metadata.Edm;


namespace Repository.Repository
{
    public class GenericDataRepository : IGenericDataRepository {
        private EntityMetaData entityMetaData = null;
        private DbContext context = null;
        private JoinTable joinTable= null;
        private Property property = null;
        public GenericDataRepository(DbContext context)
        {
            entityMetaData = new EntityMetaData(context);
            this.context = context;
            joinTable = new JoinTable(context, this, entityMetaData) ;
            property = new Property(entityMetaData); 
         }
        enum Action
        {
            CREATE = 0,
            READ = 1,
            UPDATE = 2,
            DELETE = 1,
        }


        private Boolean CheckMetaDataArguments(EntityMetaData.OutboundReferences foreignKeyMD)
        {
            if (foreignKeyMD.foreignKeyProperties == null) return false;
            if (foreignKeyMD.foreignKeyProperties.Count != foreignKeyMD.foreignKeyProjectionProperties.Count) throw new DataMisalignedException("Metadata content error.");
            if (foreignKeyMD.foreignKeyProperties.Count == 0) throw new ArgumentOutOfRangeException("Metadata content error.");
            return true;
        }

        private Boolean NullAllowedAndPresent(EntityMetaData.OutboundReferences foreignKeyMD, object[] foreignKeyValues)
        {

            if (foreignKeyMD.Nullable) 
            {
                foreach (var value in foreignKeyValues)
                {
                    if (value == null) return true;
                }
                return false;
            }
            return false;
        }

        private Boolean NumberOfRecordsMatchesMultiplicity(int RecordCnt, RelationshipMultiplicity multiplicity)
        {
            switch (multiplicity)
            {
                case RelationshipMultiplicity.ZeroOrOne:  // zero is ignored because a foreignKey != null is assumed.
                case RelationshipMultiplicity.One:
                    {
                        if (RecordCnt == 1) return true;
                        break;
                    }
                case RelationshipMultiplicity.Many:
                    {
                        return true;
                    }
                default:
                    {
                        throw new ArgumentException("Switch Error.");
                    }
            }
            return false;
        }
        private Boolean NumberOfRecordsMatchesMultiplicity4Delete(int RecordCnt, RelationshipMultiplicity multiplicity)
        {
            switch (multiplicity)
            {
                case RelationshipMultiplicity.ZeroOrOne:  // zero is ignored because a foreignKey != null is assumed.
                case RelationshipMultiplicity.Many:
                case RelationshipMultiplicity.One:
                    {
                        if (RecordCnt > 1) return true;
                        break;
                    }
                default:
                    {
                        throw new ArgumentException("Switch Error.");
                    }
            }
            return false;
        }


        // For Insert: For the potential new situation: Outbound references: If fkKey is nullable AND contains nullvalues then allow else see function "NumberOfRecordsMatchesMultiplicity"
        //                                              Inbound references: no test needed.
        // For Delete: For the potential new situation: Outbound references:  First check for Cascade deletes, if so Cascade the deletion else always allow
        //                                              Inbound references:  See functon "NumberOfRecordsMatchesMultiplicity4Delete"
        // For Update: For the potential new situation: Outbound references: Same as Insert. 
        //                                              Inbound references: Always allow
 
        private async Task<Boolean> CheckInboundReferences(dynamic record) 
        {
            EntityMetaData recordMD = entityMetaData.GetMeta4Entity(record);
            if (recordMD == null) throw new ArgumentException("Entity type unknown in repository.");

            foreach (var inRefEntity in recordMD.inRefs)
            {
                foreach (var foreignKey in inRefEntity.outRefs)     // foreignKey inRef---->----references---->record (= record to delete)
                {
                    if (foreignKey.referredEntity  != recordMD) break; //only Inrefs to "me"
                    {                                                   // now we have got all the inbound references.
                        if (!CheckMetaDataArguments(foreignKey)) return false;
                        object[] KeyValues = property.GetPropertyValues(record, foreignKey.foreignKeyProjectionProperties);
                        if (NullAllowedAndPresent(foreignKey, KeyValues)) break;

                        int Referrers = await CountOccurencesOfEntityAsync(
                           inRefEntity.entity,
                           foreignKey.foreignKeyProperties,
                           KeyValues);
                        if (Referrers == 0) break;  // Nobody is referring to current record, so no need to check!

                        int recordCnt = await CountOccurencesOfEntityAsync(
                          foreignKey.referredEntity.entity,
                          foreignKey.foreignKeyProjectionProperties,
                          KeyValues);
                        if (!NumberOfRecordsMatchesMultiplicity4Delete(recordCnt, foreignKey.multiplicity))
                           return false;
                        break;
                    }
                }
            }
            return true;
        }
 
    private async Task<Boolean> CheckOutboundReferences(dynamic record, Action action)
        {
            EntityMetaData recordMD = entityMetaData.GetMeta4Entity(record);
            if (recordMD == null) throw new ArgumentException("Entity type unknown in repository.");

            foreach (var foreignKey in recordMD.outRefs)
            {
                if (!CheckMetaDataArguments(foreignKey)) return false;
                object[] KeyValues = property.GetPropertyValues(record, foreignKey.foreignKeyProperties);
                if (NullAllowedAndPresent(foreignKey, KeyValues)) break;
                int recordCnt = (await CountOccurencesOfEntityAsync(
                          foreignKey.referredEntity.entity,
                          foreignKey.foreignKeyProjectionProperties,
                          KeyValues));

                switch (action)
                {
                    case Action.CREATE:
                    case Action.UPDATE:
                        { 
                            if (!NumberOfRecordsMatchesMultiplicity(recordCnt, foreignKey.multiplicity))
                            return false;
                            break;
                        }
                    case  Action.DELETE:
                        {
                            if (recordCnt > 0 && foreignKey.deleteBehavior == OperationAction.Cascade)
                                return await CascadeByDeleteAsync(      // recursion
                                                 foreignKey.referredEntity.entity,
                                                 foreignKey.foreignKeyProjectionProperties,
                                                 KeyValues);
                            break;
                        } 
                    default:
                        {
                            throw new ArgumentException("Switch error.");
                        }

                }
            }
            return true;
        }



   
        private async Task<Boolean> CascadeByDeleteAsync(EdmType targetEntityMDType, List<EdmProperty> targetProps,  object[] targetKeyValues)
        {

            List<dynamic>  records2Delete = await GetByConstructedKeyAsync(targetEntityMDType, targetProps, targetKeyValues, "",tracking:false);
            var EntityName = targetEntityMDType.Name.ToString();
            var metaDataRecord = entityMetaData.GetMeta4Entity(EntityName);
            if (metaDataRecord == null) throw new ArgumentException("Entity type unknown in repository.");

            foreach (var record in records2Delete)
            {
                 object[] KeyValues = property.GetPropertyValues(record, metaDataRecord.primaryKeysProperties);
                try
                {
                    if (await _DeleteAsync(record) != HttpStatusCode.OK) return false;
                 }
                catch (Exception e)
                {
                    throw (e);
                }
            }
            return true;
        }

 

        internal async Task<List<dynamic>> GetByConstructedKeyAsync(EdmType targetEntityMDType, List<EdmProperty> keyProps, object[] keyValues, string specifierTop, Boolean tracking) 
        {

            var conditionStr = new StringBuilder();
            var targetEntityName = targetEntityMDType.Name.ToString();
            var targetEntityType = Type.GetType(targetEntityMDType.ToString());
 
            for (int index = 0; index < keyProps.Count(); index++)
            {
                if (conditionStr.Length > 0) conditionStr.Append(" AND ");
                conditionStr.Append($"{keyProps[index].Name.ToString()}= @p{index}");
            }
            List<dynamic> result = null;
            if (tracking)
                result = await context.Set(targetEntityType).SqlQuery($"SELECT {specifierTop} * FROM dbo.{targetEntityName} WHERE {conditionStr}", keyValues).ToListAsync();
            else
                result = await context.Set(targetEntityType).SqlQuery($"SELECT {specifierTop} * FROM dbo.{targetEntityName} WHERE {conditionStr}", keyValues).AsNoTracking().ToListAsync();
            if (result != null)
                foreach (var row in result)
                    if (row.GetType() != targetEntityType && row.GetType().BaseType != targetEntityType) throw new ArgumentException("Wrong entity type retrieved.");
            return result;
        }


        // Count occurences of occurences of records by using foreignKey.                  
        // returns:
        //     0: No record found.
        //     1: One and only one record found.
        //     2: More than one record found.                
        private async Task<int> CountOccurencesOfEntityAsync( EdmType Entity, List<EdmProperty> targetKeyProps, object[] targetKeyValues)
        {
            List<dynamic> lst = await GetByConstructedKeyAsync(Entity, targetKeyProps,  targetKeyValues, "TOP 2", tracking: false);
            return lst.Count();
        }

 
        private async Task<List<dynamic>> _GetAsync(EntityMetaData EntityMD, object[] keyValues)
        {

            return await GetByConstructedKeyAsync(EntityMD.entity, EntityMD.primaryKeysProperties, keyValues, "", tracking: false);

        }

        internal async Task<HttpStatusCode> _InsertAsync(dynamic data)
        {
            try
            {
                var metaDataRecord = entityMetaData.GetMeta4Entity(data);
                if (metaDataRecord == null) throw new ArgumentException("Entity type unknown in repository.");

                if (await CheckOutboundReferences(data, Action.CREATE) == false) return HttpStatusCode.BadRequest;
                 context.Entry(data).State = EntityState.Added;
                return HttpStatusCode.OK;
            }
            catch (Exception e)
            {
                throw (e);
            }
        }

        internal async Task<HttpStatusCode> _UpdateAsync(dynamic data)
        {
           try
            {
 
                var metaDataRecord = entityMetaData.GetMeta4Entity(data);
                if (metaDataRecord == null) throw new ArgumentException("Entity type unknown in repository");

                object[] pkValues = property.GetPropertyValues(data, metaDataRecord.primaryKeysProperties);
                dynamic[] records = (await _GetAsync(metaDataRecord, pkValues)).ToArray();
                if (records == null || records.Length != 1) return HttpStatusCode.BadRequest;
                var recordsArray = records.ToArray();
   
                if (await CheckOutboundReferences(data ,Action.UPDATE) == false) return HttpStatusCode.BadRequest;
                 context.Entry(data).State = EntityState.Modified;
                return HttpStatusCode.OK;
            }
            catch (Exception e)
            {
                throw (e);
            }
         }

       
       internal async Task<HttpStatusCode> _DeleteAsync(dynamic data) 
        {
            try
            {
                var metaDataRecord = entityMetaData.GetMeta4Entity(data);
                var keyValues = property.GetPropertyValues(data, metaDataRecord.primaryKeysProperties);
                List<dynamic> records = await _GetAsync(metaDataRecord, keyValues);
                if (records == null ) return HttpStatusCode.BadRequest;
                var recordsArray = records.ToArray();
                if (recordsArray.Count() != 1) return HttpStatusCode.BadRequest;
                if (await CheckOutboundReferences(recordsArray[0], Action.DELETE) == false) return HttpStatusCode.BadRequest;
                if (await CheckInboundReferences(recordsArray[0]) == false) return HttpStatusCode.BadRequest;
                context.Entry(recordsArray[0]).State = EntityState.Deleted;
                context.SaveChanges();
                return HttpStatusCode.OK;
            }
            catch (Exception e)
            {
                throw (e);
            }
        }


        public async Task<List<Z>> GetAsync<Z>(params object[] keyValues) where Z : class
        {
            var originalLazyLoadingSetting = context.Configuration.LazyLoadingEnabled;
            var originalProxyCreationEnabled = context.Configuration.ProxyCreationEnabled;
            try
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                if (keyValues.Count() == 1 && keyValues[0].GetType() == typeof(int) && 0 == (int)keyValues[0])
                    return await context.Set<Z>().ToListAsync();
                else
                {
                    var row = (await context.Set<Z>().FindAsync(keyValues));
                    if (row == null) return null;
                    return new List<Z>() { row };
                }
            }

            catch (Exception e)
            {
                throw (e);
            }
            finally
            {
                context.Configuration.LazyLoadingEnabled = originalLazyLoadingSetting;
                context.Configuration.ProxyCreationEnabled = originalProxyCreationEnabled;
            }
        }


        public async Task<List<Z>> GetAsync<Z>(Expression<Func<Z, bool>> selectionCondition) where Z : class
        {
            var originalLazyLoadingSetting = context.Configuration.LazyLoadingEnabled;
            var originalProxyCreationEnabled = context.Configuration.ProxyCreationEnabled;
            try
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                return await context.Set<Z>().Where(selectionCondition).ToListAsync();
            }

            catch (Exception e)
            {
                throw (e);
            }
            finally
            {
                context.Configuration.LazyLoadingEnabled = originalLazyLoadingSetting;
                context.Configuration.ProxyCreationEnabled = originalProxyCreationEnabled;
            }
        }

        public async Task<HttpStatusCode> InsertAsync<ENTITY>(ENTITY data) where ENTITY : class
        {
            var originalLazyLoadingSetting = context.Configuration.LazyLoadingEnabled;
            var originalProxyCreationEnabled = context.Configuration.ProxyCreationEnabled;
            try
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                return await _InsertAsync(data);
            }
            catch (Exception e)
            {
                throw (e);
            }
            finally
            {
                context.Configuration.LazyLoadingEnabled = originalLazyLoadingSetting;
                context.Configuration.ProxyCreationEnabled = originalProxyCreationEnabled;
            }
        }

        public async Task<HttpStatusCode> UpdateAsync<ENTITY>(ENTITY data) where ENTITY : class
        {
            var originalLazyLoadingSetting = context.Configuration.LazyLoadingEnabled;
            var originalProxyCreationEnabled = context.Configuration.ProxyCreationEnabled;
            try
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;

                return await _UpdateAsync(data);
            }
            catch (Exception e)
            {
                throw (e);
            }
            finally
            {
                context.Configuration.LazyLoadingEnabled = originalLazyLoadingSetting;
                context.Configuration.ProxyCreationEnabled = originalProxyCreationEnabled;
            }

        }


        public async Task<HttpStatusCode> DeleteAsync<ENTITY>(params object[] keyValues) where ENTITY : class
        {
            var originalLazyLoadingSetting = context.Configuration.LazyLoadingEnabled;
            var originalProxyCreationEnabled = context.Configuration.ProxyCreationEnabled;
            try
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                List<ENTITY> records = (await GetAsync<ENTITY>(keyValues));
                if (records == null || records.FindLastIndex(x => true) != 0) return HttpStatusCode.BadRequest;
                var record = records.First();
                if (record == null) return HttpStatusCode.BadRequest;
                context.Entry(record).State = EntityState.Detached;
                return await _DeleteAsync(record); 
            }
            catch (Exception e)
            {
                throw (e);
            }
            finally
            {
                context.Configuration.LazyLoadingEnabled = originalLazyLoadingSetting;
                context.Configuration.ProxyCreationEnabled = originalProxyCreationEnabled;
            }
        }

        public async Task<HttpStatusCode> UpdateJoinEntityAsync<ONE, MANY>(ONE record4One, List<object[]> fkValuesM) where ONE : class where MANY : class
        {
            if (joinTable != null)
            {
                return await joinTable.UpdateJoinEntityAsync<ONE, MANY>(record4One, fkValuesM);
            }
            else
            {
                throw new NullReferenceException("Object is null");
            }
        }

        public async Task<List<dynamic[]>> GetPKs4EntityManyAsync<ONE, MANY>(ONE recordFromOne) where ONE : class where MANY : class
        {
            if (joinTable != null)
            {
                return await joinTable.GetPKs4EntityManyAsync<ONE, MANY>(recordFromOne);
            }
            else
            {
                throw new NullReferenceException("Object is null");
            }
        }





    }
}



