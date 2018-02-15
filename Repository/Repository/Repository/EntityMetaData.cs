using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using NLog;
using System.Linq;
using System.Data.Entity.Infrastructure;
using System.Data;
using System.Data.Entity;

namespace Repository.Repository
{
    public class EntityMetaData
    {
        protected static List<EntityMetaData> entities = new List<EntityMetaData>();
        private static Boolean initialised = false;

        public class OutboundReferences
        {
            public EntityMetaData referredEntity = null;
            public List<EdmProperty> foreignKeyProperties = null;
            public List<EdmProperty> foreignKeyProjectionProperties = null;
            public OperationAction deleteBehavior = OperationAction.None;
            public Boolean Nullable = false;
            public RelationshipMultiplicity multiplicity;
        }
        public EdmType entity = null;
        public List<EntityMetaData> inRefs = null;
        public List<OutboundReferences> outRefs = null;
        public List<EdmProperty> primaryKeysProperties = null;

        public static Logger logger = LogManager.GetLogger("LoggerInstance");

        private String MakeLogStr4Entry(String methodNameStr)
        {
            return $"{methodNameStr}: Entry into {methodNameStr}.";
        }
        private String MakeLogStr4Exit(String methodNameStr)
        {
            return $"{methodNameStr}: Exit from {methodNameStr}. ReturnCode: ";
        }

        private EntityMetaData LookUp(List<EntityMetaData> list, EdmType searchArg)
        {
            var a = list.Where(x => x.entity == searchArg).FirstOrDefault();
            return a;
        }

        private List<AssociationType> GetAllAssociationsViaCSpaceItems(DbContext context)
        {
            var objectContext = ((IObjectContextAdapter)context).ObjectContext;
            var workspace = objectContext.MetadataWorkspace;
            var items = workspace.GetItems(DataSpace.CSpace);
            var associationsInCSpace = new List<AssociationType>();
            foreach (var item in items)
            {
                var assocType = item as AssociationType;
                if (assocType != null && assocType.IsForeignKey)
                {
                    associationsInCSpace.Add(assocType);
                }
            }
            return associationsInCSpace;
        }

        private List<EntityType> GetAllEntitiesViaCSpaceItems(DbContext context)
        {
            var objectContext = ((IObjectContextAdapter)context).ObjectContext;
            var workspace = objectContext.MetadataWorkspace;
            var items = workspace.GetItems(DataSpace.CSpace);
            var entitiesInCSpace = new List<EntityType>();
            foreach (var item in items)
            {
                EntityType entityType = item as EntityType;
                if (entityType != null)
                {
                    entitiesInCSpace.Add(entityType);
                }
            }
            return entitiesInCSpace;
        }

        private List<EntityMetaData> InitializeAllEntityEntries(List<EntityMetaData> entities, List<EntityType> entitiesInContext)
        {
            foreach (var entityType in entitiesInContext)
            {

                var primaryKeysProps = new List<EdmProperty>();
                primaryKeysProps = (List<EdmProperty>)entityType.KeyProperties.ToList();

                entities.Add(new EntityMetaData()
                {
                    entity = entityType,
                    primaryKeysProperties = primaryKeysProps
                });
            }
            return entities;
        }

        private EntityMetaData.OutboundReferences SetForeignKeyInfo(
                            EntityMetaData currentEntity,
                            EntityMetaData referredEntity,
                            List<EdmProperty> foreignKeyProperties,
                            List<EdmProperty> projectionOfForeignKeyProperties,
                            OperationAction DeleteBehavior,
                            RelationshipMultiplicity RelationshipMultiplicity)
        {
            if (foreignKeyProperties == null || foreignKeyProperties.Count() == 0) return null;
            return new EntityMetaData.OutboundReferences()
            {
                foreignKeyProperties = foreignKeyProperties,
                foreignKeyProjectionProperties = projectionOfForeignKeyProperties,
                referredEntity = referredEntity,
                deleteBehavior = DeleteBehavior,
                multiplicity = RelationshipMultiplicity,
                Nullable = foreignKeyProperties.First().Nullable
            };
        }

       private List<EntityMetaData> SetDependencyInfoViaAssociations(List<EntityMetaData> entities, List<AssociationType> assocInContext)
        {
            EntityMetaData.OutboundReferences dep=null;
            foreach (var association in assocInContext)
            {
                foreach (var reference in association.ReferentialConstraints)
                {
                    var currentEntity = LookUp(entities, reference.FromProperties.First().DeclaringType);
                    var dependentEntity = LookUp(entities, reference.ToProperties.First().DeclaringType);
                    dep = SetForeignKeyInfo(currentEntity,
                                            dependentEntity,
                                            reference.FromProperties.ToList(),
                                            reference.ToProperties.ToList(),
                                            reference.FromRole.DeleteBehavior,
                                            reference.ToRole.RelationshipMultiplicity);
                    if (dep != null)
                    {
                        if (currentEntity.outRefs == null) currentEntity.outRefs = new List<EntityMetaData.OutboundReferences>();
                        currentEntity.outRefs.Add(dep);
                    }

                    dep = SetForeignKeyInfo(dependentEntity,
                                            currentEntity,
                                            reference.ToProperties.ToList(),
                                            reference.FromProperties.ToList(),
                                            reference.ToRole.DeleteBehavior,
                                            reference.FromRole.RelationshipMultiplicity);
                    if (dep != null)
                    {
                        if (dependentEntity.outRefs == null) dependentEntity.outRefs = new List<EntityMetaData.OutboundReferences>();
                        dependentEntity.outRefs.Add(dep);
                    }
                }
            }
            return entities;
        }

        private List<EntityMetaData> FixUpInboundReferences(List<EntityMetaData> entities)
        {
            foreach (var entityType in entities)
            {
                if (entityType.outRefs == null) break;
                var entitiesWithInboundReference = entityType.outRefs.Select(x => x.referredEntity).ToList();
                foreach (var entity in entitiesWithInboundReference)
                {
                    if (entity.inRefs == null) entity.inRefs = new List<EntityMetaData>();
                    entity.inRefs.Add(entityType);
                }
            }
            return entities;
        }
        private EntityMetaData.OutboundReferences FindRef(EntityMetaData referringEntity, EntityMetaData referredEntity)
        {
            if (referringEntity.outRefs == null) return null;
            foreach (var outref in referringEntity.outRefs) if (outref.referredEntity == referredEntity) return outref;
            return null;
        }

        private void LogRef(EdmType source, EntityMetaData.OutboundReferences references, String methodNameStr)
        {
            if (references == null) throw new NoNullAllowedException("Unexpected Null pointer");
            logger.Log(LogLevel.Info, $"{methodNameStr}:  ");
            logger.Log(LogLevel.Info, $"{methodNameStr}: The referred entity is: {references.referredEntity.entity.Name}  ");
            foreach (var prop in references.foreignKeyProperties)
            {
                logger.Log(LogLevel.Info, $"{methodNameStr}:     with foreignkey property: {source.Name} : {prop.Name}  ");
            }
            foreach (var prop in references.foreignKeyProjectionProperties)
            {
                logger.Log(LogLevel.Info, $"{methodNameStr}:     Corresponding with  {references.referredEntity.entity.Name} : {prop.Name}  ");
            }
            logger.Log(LogLevel.Info, $"{methodNameStr}: Multiplicity: {references.multiplicity.ToString()}  ");
            logger.Log(LogLevel.Info, $"{methodNameStr}: Delete behavior: {references.deleteBehavior.ToString()}  ");
            logger.Log(LogLevel.Info, $"{methodNameStr}: Nulls Allowed: {references.Nullable.ToString()}  ");
            logger.Log(LogLevel.Info, $"{methodNameStr}:  ");
        }

        private void LogMetaData(List<EntityMetaData> entities)
        {
            String methodNameStr = "LogMetaData";
            logger.Log(LogLevel.Info, $"{MakeLogStr4Entry(methodNameStr)}.");
            foreach (var entity in entities)
            {
                logger.Log(LogLevel.Info, $"{methodNameStr}  ------------------------------------------------");
                logger.Log(LogLevel.Info, $"{methodNameStr}: Entity={entity.entity.Name}.");


                logger.Log(LogLevel.Info, $"{methodNameStr}: Primary key:");
                foreach (var key in entity.primaryKeysProperties)
                {
                    logger.Log(LogLevel.Info, $"{methodNameStr}:  {key.Name}  ");
                }
                logger.Log(LogLevel.Info, $"{methodNameStr}:  ");

                if (entity.outRefs == null)
                    logger.Log(LogLevel.Info, $"{methodNameStr}: Entity has no outbound references via its foreign key.");
                else foreach (var outref in entity.outRefs) LogRef(entity.entity, outref, methodNameStr + "<=====<");

                if (entity.inRefs == null)
                    logger.Log(LogLevel.Info, $"{methodNameStr}: No entities have a reference via its foreign key to {entity.entity.Name}.");
                else
                {
                    foreach (var entityReachingOutToMe in entity.inRefs)
                    {
                        logger.Log(LogLevel.Info, $"{methodNameStr}: The following entity has a reference via its foreign key: {entityReachingOutToMe.entity.Name}:");
                        var _inref = FindRef(entityReachingOutToMe, entity);
                        LogRef(entityReachingOutToMe.entity, _inref, methodNameStr + ">=====>");
                    }
                    logger.Log(LogLevel.Info, $"{methodNameStr}:  ");
                }
                logger.Log(LogLevel.Info, $"{methodNameStr}  ------------------------------------------------");
            }
            logger.Log(LogLevel.Info, $"{MakeLogStr4Exit(methodNameStr)}.");

        }
        private EntityMetaData()
        {
        }

        public EntityMetaData(DbContext context)
        {
            if (!initialised)
            {
                lock (entities)
                {
                    if (!initialised)  // this is needed for avoiding multiple initialisations when multiple threads are newing up EntityMetaData 
                    {                  // and initialised is still false. For example: One thread waits for lock and one thread is initialising.
                        var entitiesInContext = GetAllEntitiesViaCSpaceItems(context);
                        var associationsInContext = GetAllAssociationsViaCSpaceItems(context);

                        entities = InitializeAllEntityEntries(entities, entitiesInContext);
                        entities = SetDependencyInfoViaAssociations(entities, associationsInContext);
                        entities = FixUpInboundReferences(entities);
                        LogMetaData(entities);
                        initialised = true;
                    }
                }

            }
        }

        public EntityMetaData GetMeta4Entity(String entityName)
        {
            if (!initialised) return null;
            return entities.Where(x => x.entity.Name == entityName).FirstOrDefault();
        }
        public EntityMetaData GetMeta4Entity(dynamic data)
        {
            String entityName = "";
            if (data.GetType().BaseType == typeof(System.Object))
                entityName = data.GetType().Name.ToString();
            else
                entityName = data.GetType().BaseType.Name.ToString();

            return GetMeta4Entity(entityName);
        }
        public EntityMetaData GetMeta4Entity<T>()
        {
            return GetMeta4Entity(typeof(T).Name.ToString());
        }

        public EntityMetaData GetMeta4JoinEntity(EntityMetaData leftRecord, EntityMetaData rightRecord)
        {
            if (leftRecord == null || rightRecord == null) throw new ArgumentException("Entity type unknown in repository.");
            foreach (var leftPrincipal in leftRecord.outRefs)
                foreach (var rightPrincipal in rightRecord.outRefs)
                    if (leftPrincipal.referredEntity == rightPrincipal.referredEntity)
                        return leftPrincipal.referredEntity;
            return null;
        }
    }
}
