//using ZBLandDAC.Data;
//using System;
//using System.Collections.Generic;
//using System.Data.Entity;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Net;
//using System.Threading.Tasks;
//using ZBLandDAC.MetaData;

//namespace ZBLandDAC.Repository
//{

//    public class BridgeDataRepository<TEntity> : IBridgeDataRepository<TEntity> where TEntity : class
//    {
//        private EntityMetaData entityMetaData = null;
//        private DbContext context = null;
//        public BridgeDataRepository(DbContext context)
//        {
//            entityMetaData = new EntityMetaData(context);
//            this.context = context;
//        }

//        public async Task<HttpStatusCode> GetAsync<DTO>(
//                            List<DTO>                           dtoLst,
//                            Action<DTO, T1ManySingleSelection>  map,
//                            Action<DTO, List<int>>              assignListOfKeysToDTO,
//                            Expression<Func<TBridge, int>>      projectionOnTBridge_FKT1_Column,
//                            Expression<Func<TBridge, bool>>     selectionOnTBridge,
//                            int                                 keyT1 = 0)
//                            where DTO : class, new()
//        {
//            var originalLazyLoadingSetting = context.Configuration.LazyLoadingEnabled;
//            try
//            {
//                context.Configuration.LazyLoadingEnabled = false;

//                List<T1ManySingleSelection> t1Lst = null;
//                if (keyT1 == 0)
//                {
//                    t1Lst = await context.Set<T1ManySingleSelection>().ToListAsync<T1ManySingleSelection>();
//                }
//                else
//                {
//                    var t1 = await context.Set<T1ManySingleSelection>().FindAsync(keyT1);
//                    if (t1 != null)
//                        t1Lst = new List<T1ManySingleSelection>() { t1 };
//                }

//                if (t1Lst != null && t1Lst.Count > 0)
//                {
//                    foreach (var t1 in t1Lst)
//                    {
//                        var dto = new DTO();
//                        map(dto, t1);

//                        var keyList = await context.Set<TBridge>()
//                                           .Where(selectionOnTBridge)
//                                           .Select(projectionOnTBridge_FKT1_Column)
//                                           .ToListAsync();
//                        assignListOfKeysToDTO(dto, keyList);
//                        dtoLst.Add(dto);
//                    }
//                    return HttpStatusCode.OK;
//                }
//                return HttpStatusCode.NoContent;
//            }
//            catch (Exception e)
//            {
//                throw e;
//            }
//            finally
//            {
//                context.Configuration.LazyLoadingEnabled = originalLazyLoadingSetting;
//            }
//        }

//        private void UpdateBridgeRecords(
//                        List<TBridge> BridgeRecordsNowInDB,
//                        List<TBridge> BridgeRecordsNeededinDB)
//        {
//            if (BridgeRecordsNowInDB==null && BridgeRecordsNeededinDB==null)
//            {
//                throw new System.ArgumentException("Both Parameters cannot be null", "BridgeRecordsNowInDB and BridgeRecordsNeededinDB");
//            }
//            if (BridgeRecordsNeededinDB==null) BridgeRecordsNeededinDB = new List<TBridge>();
//            if (BridgeRecordsNowInDB == null) BridgeRecordsNowInDB = new List<TBridge>();


//            var bridgeRecordsAlreadyOk = BridgeRecordsNeededinDB.Intersect(BridgeRecordsNowInDB).ToList();
//            var bridgeRecordsToDelete = BridgeRecordsNowInDB.Except(bridgeRecordsAlreadyOk).Distinct().ToList();
//            var bridgeRecordsToAdd = BridgeRecordsNeededinDB.Except(bridgeRecordsAlreadyOk).Distinct().ToList();
//            bridgeRecordsToDelete.ForEach(x => context.Set<TBridge>().Remove(x));
//            bridgeRecordsToAdd.ForEach(x => context.Set<TBridge>().Add(x));
//        }

//        public async Task<HttpStatusCode> UpdateAsync(
//                        Expression<Func<TBridge, int>>                projectionOnBridge_Only_FKT1_Column,
//                        Expression<Func<TBridge, int>>                projectionOnBridge_Only_FKT2_Column,
//                        Expression<Func<TBridge, bool>>               SelectionBridge,
//                        List<TBridge>                                 BridgeRecordsNeededinDB)
//        {
//            var originalLazyLoadingSetting = context.Configuration.LazyLoadingEnabled;
//            try
//            {
//                context.Configuration.LazyLoadingEnabled = false;

//                var T1_pks = BridgeRecordsNeededinDB.Select(projectionOnBridge_Only_FKT1_Column.Compile()).Distinct().ToList<int>();
//                var T2_pks = BridgeRecordsNeededinDB.Select(projectionOnBridge_Only_FKT2_Column.Compile()).Distinct().ToList<int>();

//                if (T1_pks.Count != 1) return HttpStatusCode.BadRequest;

//                foreach (var T1_pk in T1_pks)
//                {
//                    if (await context.Set<T1ManySingleSelection>().FindAsync(T1_pk) == null) return HttpStatusCode.BadRequest;
//                }

//                foreach (var T2_pk in T2_pks)
//                {
//                    if (await context.Set<T2ManyMultipleSelected>().FindAsync(T2_pk) == null) return HttpStatusCode.BadRequest;
//                }

//                var BridgeRecordsNowInDB = await GetAsync(context, SelectionBridge);

//                UpdateBridgeRecords(context, BridgeRecordsNowInDB, BridgeRecordsNeededinDB);
//                return HttpStatusCode.OK;
//            }
//            catch (Exception e)
//            {
//                throw (e);
//            }

//            finally
//            {
//                context.Configuration.LazyLoadingEnabled = originalLazyLoadingSetting;
//            }
//        }

//        public async Task<HttpStatusCode> DeleteAsync(
//                 Expression<Func<TBridge,bool>> SelectionBridge)
//        {
//            var originalLazyLoadingSetting = context.Configuration.LazyLoadingEnabled;
//            try
//            {
//                context.Configuration.LazyLoadingEnabled = false;

//                var BridgeRecordsNowInDB = await GetAsync(SelectionBridge);
//               if (BridgeRecordsNowInDB!=null && BridgeRecordsNowInDB.Count >0)
//                {
//                    UpdateBridgeRecords(context, BridgeRecordsNowInDB, null);
//                }

//                return HttpStatusCode.OK;
//            }
//            catch (Exception e)
//            {
//                throw (e);
//            }

//            finally
//            {
//                context.Configuration.LazyLoadingEnabled = originalLazyLoadingSetting;
//            }
//        }



//    }
//}

