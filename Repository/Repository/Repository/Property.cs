using System;
using System.Collections.Generic;
using System.Linq;
using System.Data.Entity.Core.Metadata.Edm;
using System.Reflection;

namespace Repository.Repository
{
    public class Property
    {
        private EntityMetaData entityMetaData = null;
        public Property(EntityMetaData entityMetaData)
        {
            this.entityMetaData = entityMetaData;
        }
 
        internal object[] GetPropertyValues<dynamic>(dynamic entity, List<EdmProperty> pkLst)
        {
            PropertyInfo pkPropertInfo;
            var propILst = entity.GetType().GetProperties();
            List<object> keyValues = new List<object>();
            foreach (var pk in pkLst)
            {
                pkPropertInfo = propILst.Where(x => x.Name == pk.Name).FirstOrDefault();
                var propInstance = Activator.CreateInstance(pkPropertInfo.PropertyType);
                propInstance = pkPropertInfo.GetValue(entity);
                keyValues.Add(propInstance);
            }
            return keyValues.ToArray();
        }

        internal List<PropertyInfo> GetPropertyInfo(Type recordType, List<EdmProperty> props)
        {
            var propILst = recordType.GetProperties();
            var propsInfo = new List<PropertyInfo>();
            foreach (var prop in props)
                propsInfo.Add(propILst.Where(x => x.Name == prop.Name).FirstOrDefault());
            return propsInfo;
        }

        internal List<EdmProperty> GetProjectedFKProperties(EntityMetaData principalEntityMD, EntityMetaData dependentEntityMD)
        {
            foreach (var outRef in dependentEntityMD.outRefs)
                if (outRef.referredEntity == principalEntityMD)
                    return outRef.foreignKeyProjectionProperties;

            return null;
        }

        internal List<EdmProperty> GetFKProperties(EntityMetaData principalEntityMD, EntityMetaData dependentEntityMD)
        {
            foreach (var outRef in dependentEntityMD.outRefs)
                if (outRef.referredEntity == principalEntityMD)
                    return outRef.foreignKeyProperties;

            return null;
        }

        internal List<PropertyInfo> GetPropertyInfoPropsOfEntity(EntityMetaData joinTableMD)
        {
            Type joinType = Type.GetType(joinTableMD.entity.FullName);
            return joinType.GetProperties().ToList();
        }

        internal void SetPropertyValues<dynamic>(Type recordType, List<dynamic> records, List<EdmProperty> Props, List<object[]> values)
        {

            var propInfo = GetPropertyInfo(recordType, Props);
            for (int row = 0; row < records.Count(); row++)
                SetPropertyValues(records[row], propInfo, values.ElementAt(row));
        }
        internal void SetPropertyValues<dynamic>(Type recordType, List<dynamic> records, List<EdmProperty> Props, object[] values)
        {
            var propInfo = GetPropertyInfo(recordType, Props);
            for (int row = 0; row < records.Count(); row++)
                SetPropertyValues(records[row], propInfo, values);
        }
        internal void SetPropertyValues(dynamic record, List<PropertyInfo> propsInfo, object[] values)
        {
            int propIndex = 0;
            foreach (var propInfo in propsInfo)
            {
                propInfo.SetValue(record, values[propIndex]);
                propIndex++;
            }

        }







    }
}



