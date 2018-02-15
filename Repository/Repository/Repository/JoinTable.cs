using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Data.Entity;
using System.Threading.Tasks;
using System.Data.Entity.Core.Metadata.Edm;
using System.Reflection;

namespace Repository.Repository
{
    public class JoinTable 
    {
        private EntityMetaData entityMetaData = null;
        private DbContext context = null;
        private GenericDataRepository repos = null;
        private Property property = null;

        public JoinTable(DbContext context, GenericDataRepository repos, EntityMetaData entityMetaData)
        {
            this.entityMetaData = entityMetaData;
            this.context = context;
            this.repos = repos;
            property = new Property(entityMetaData);


        }

        private EntityMetaData DetermineJoinTable<ONE, MANY>() where ONE : class where MANY : class
        {
            try
            {
                EntityMetaData joinTableMD = entityMetaData.GetMeta4JoinEntity(
                      entityMetaData.GetMeta4Entity<ONE>(),
                      entityMetaData.GetMeta4Entity<MANY>());
                if (joinTableMD == null) throw new ArgumentException("Entity type unknown in repository.");
                return joinTableMD;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private List<dynamic> DetermineRequiredJoinRecords<ONE, MANY>(EntityMetaData joinTableMD, object[] fkValues1, List<object[]> fkValuesM) where ONE : class where MANY : class
        {
            try
            {
                var entityOneMD = entityMetaData.GetMeta4Entity<ONE>();
                var entityManyMD = entityMetaData.GetMeta4Entity<MANY>();
                List<EdmProperty> joinTableKeyOnePart = property.GetProjectedFKProperties(joinTableMD, entityOneMD);
                List<EdmProperty> joinTableKeyManyPart = property.GetProjectedFKProperties(joinTableMD, entityManyMD);

                Type joinType = Type.GetType(joinTableMD.entity.FullName);
                var joinRecords = new List<dynamic>();
                for (int row = 0; row < fkValuesM.Count(); row++)
                    joinRecords.Add(Activator.CreateInstance(joinType));

                property.SetPropertyValues(joinType, joinRecords, joinTableKeyOnePart, fkValues1);
                property.SetPropertyValues(joinType, joinRecords, joinTableKeyManyPart, fkValuesM);

                return joinRecords;
            }
            catch (Exception e)
            {
                throw e;
            }
        }
        private async Task<List<dynamic>> DetermineActualJoinRecordsAsync<ONE, MANY>(EntityMetaData joinTableMD, object[] fkValues1) where ONE : class where MANY : class
        {
            try
            {
                Type joinType = Type.GetType(joinTableMD.entity.FullName);
                List<dynamic> selection = await GetJoinRecordsAsync<ONE, MANY>(joinTableMD, fkValues1);
                return selection;
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private async Task<List<dynamic>> GetJoinRecordsAsync<ONE, MANY>(EntityMetaData joinTableMD, object[] fkValues1) where ONE : class where MANY : class
        {

            try
            {
                var entityOneMD = entityMetaData.GetMeta4Entity<ONE>();
                List<EdmProperty> PartialKey = property.GetProjectedFKProperties(joinTableMD, entityOneMD);
                return await repos.GetByConstructedKeyAsync(joinTableMD.entity, PartialKey, fkValues1, "", false);
            }
            catch (Exception e)
            {
                throw e;
            }
        }

        private Boolean RecordsAreEqual(List<PropertyInfo> propI, dynamic record1, dynamic record2)
        {
            foreach (PropertyInfo property in propI)
                if (property.GetValue(record1) != property.GetValue(record2)) return false;
            return true;
        }

        private async Task<HttpStatusCode> UpdateJoinEntityRecordsAsync<dynamic>(EntityMetaData joinTableMD, List<dynamic> now, List<dynamic> target) where dynamic : class
        {
            HttpStatusCode result = HttpStatusCode.OK;
            if (now == null && target == null) return result;
            if (target == null) target = new List<dynamic>();
            if (now == null) now = new List<dynamic>();

            List<dynamic> alreadyOk = new List<dynamic>();
            List<dynamic> recordsToDelete = new List<dynamic>();
            List<dynamic> recordsToAdd = new List<dynamic>();

            var propIes = property.GetPropertyInfoPropsOfEntity(joinTableMD);
            Boolean processed = false;
            foreach (var record1 in now)
            {
                processed = false;
                foreach (var record2 in target)
                {
                    if (RecordsAreEqual(propIes, record1, record2))
                    {
                        alreadyOk.Add(record1);
                        target.Remove(record2);
                        processed = true;
                        break;
                    }
                }
                if (!processed) recordsToDelete.Add(record1);
            }

            foreach (var record2 in target)
                recordsToAdd.Add(record2);

            foreach (var record in recordsToDelete)
            {
                result = await repos._DeleteAsync(record);
                if (result != HttpStatusCode.OK) return result;
            }
            foreach (var record in recordsToAdd)
            {
                result = await repos._InsertAsync(record);
                if (result != HttpStatusCode.OK) return result;
            }
            return result;
        }

        public async Task<HttpStatusCode> UpdateJoinEntityAsync<ONE, MANY>(ONE record4One, List<object[]> fkValuesM) where ONE : class where MANY : class
        {
            var originalLazyLoadingSetting = context.Configuration.LazyLoadingEnabled;
            var originalProxyCreationEnabled = context.Configuration.ProxyCreationEnabled;
            try
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;
                if (record4One == null && fkValuesM == null) return HttpStatusCode.BadRequest;

                var joinEntityMD = DetermineJoinTable<ONE, MANY>();
                var EntityOneMD = entityMetaData.GetMeta4Entity<ONE>();
                var fkPropsFromOneToJoin = property.GetFKProperties(joinEntityMD, EntityOneMD);

                var fkPropsFromOneToJoinValues = property.GetPropertyValues<dynamic>(record4One, fkPropsFromOneToJoin);
                if (fkPropsFromOneToJoinValues == null && fkValuesM == null) return HttpStatusCode.BadRequest;

                var required = DetermineRequiredJoinRecords<ONE, MANY>(joinEntityMD, fkPropsFromOneToJoinValues, fkValuesM);
                var actual = await DetermineActualJoinRecordsAsync<ONE, MANY>(joinEntityMD, fkPropsFromOneToJoinValues);
                return (await UpdateJoinEntityRecordsAsync(joinEntityMD, actual, required));
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

        public async Task<List<dynamic[]>> GetPKs4EntityManyAsync<ONE, MANY>(ONE recordFromOne) where ONE : class where MANY : class
        {
            var originalLazyLoadingSetting = context.Configuration.LazyLoadingEnabled;
            var originalProxyCreationEnabled = context.Configuration.ProxyCreationEnabled;
            try
            {
                context.Configuration.LazyLoadingEnabled = false;
                context.Configuration.ProxyCreationEnabled = false;

                var joinTableMD = DetermineJoinTable<ONE, MANY>();
                var EntityOneMD = entityMetaData.GetMeta4Entity<ONE>();
                var EntityMMD = entityMetaData.GetMeta4Entity<MANY>();

                var fkPropsJoinToM = property.GetFKProperties(EntityMMD, joinTableMD);
                var fkPropsOneToJoin = property.GetFKProperties(joinTableMD, EntityOneMD);
                var fkPropsFromOneToJoinValues = property.GetPropertyValues<dynamic>(recordFromOne, fkPropsOneToJoin);

                if (fkPropsFromOneToJoinValues == null) return null;

                List<dynamic> joinSelection = await GetJoinRecordsAsync<ONE, MANY>(joinTableMD, fkPropsFromOneToJoinValues);
                List<dynamic[]> result = new List<dynamic[]>();
                foreach (var row in joinSelection)
                {
                    result.Add(property.GetPropertyValues(row, fkPropsJoinToM));
                }
                return result;
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
    }
}



