using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using POC.DataBase;
using System.Data.Objects;

namespace POC.DAL
{
    public class GenericRepository<TEntity> : IGenericRepository<TEntity> where TEntity : class
    {
        /// <summary>
        /// The context object for the database
        /// </summary>
        private POC_TAXEntities context = null;

        /// <summary>
        /// The IObjectSet that represents the current entity.
        /// </summary>
        private ObjectSet<TEntity> objectSet;

        /// <summary>
        /// Initializes a new instance of the DataRepository class
        /// </summary>
        public GenericRepository()
        {
            if (context == null)
                context = new POC_TAXEntities();

            objectSet = context.CreateObjectSet<TEntity>();
        }

        /// <summary>
        /// Gets all records as an IQueryable
        /// </summary>
        /// <returns>An IQueryable object containing the results of the query</returns>
        public IQueryable<TEntity> Fetch()
        {
            return objectSet;
        }

        /// <summary>
        /// Gets all records as an IEnumberable
        /// </summary>
        /// <returns>An IEnumberable object containing the results of the query</returns>
        public IEnumerable<TEntity> GetAll()
        {
            return Fetch().AsEnumerable();
        }

        /// <summary>
        /// Finds a record with the specified criteria
        /// </summary>
        /// <param name="predicate">Criteria to match on</param>
        /// <returns>A collection containing the results of the query</returns>
        public IEnumerable<TEntity> Find(Func<TEntity, bool> predicate)
        {
            return objectSet.Where<TEntity>(predicate);
        }

        /// <summary>
        /// Gets a single record by the specified criteria (usually the unique identifier)
        /// </summary>
        /// <param name="predicate">Criteria to match on</param>
        /// <returns>A single record that matches the specified criteria</returns>
        public TEntity Single(Func<TEntity, bool> predicate)
        {
            return objectSet.SingleOrDefault<TEntity>(predicate);
        }

        /// <summary>
        /// The first record matching the specified criteria
        /// </summary>
        /// <param name="predicate">Criteria to match on</param>
        /// <returns>A single record containing the first record matching the specified criteria</returns>
        public TEntity First(Func<TEntity, bool> predicate)
        {
            return objectSet.FirstOrDefault<TEntity>(predicate);
        }

        /// <summary>
        /// Deletes the specified entitiy
        /// </summary>
        /// <param name="entity">Entity to delete</param>
        /// <exception cref="ArgumentNullException"> if <paramref name="entity"/> is null</exception>
        public void Delete(TEntity entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            objectSet.DeleteObject(entity);

            SaveChanges(SaveOptions.DetectChangesBeforeSave);
        }

        /// <summary>
        /// Deletes records matching the specified criteria
        /// </summary>
        /// <param name="predicate">Criteria to match on</param>
        public void Delete(Func<TEntity, bool> predicate)
        {
            IEnumerable<TEntity> records = from x in objectSet.Where<TEntity>(predicate) select x;

            foreach (TEntity record in records)
            {
                objectSet.DeleteObject(record);
            }

            SaveChanges(SaveOptions.DetectChangesBeforeSave);
        }

        /// <summary>
        /// Adds the specified entity
        /// </summary>
        /// <param name="entity">Entity to add</param>
        /// <param name="saveAction">User may exclude the save of all the child elements.</param>
        /// <exception cref="ArgumentNullException"> if <paramref name="entity"/> is null</exception>
        public void Add(TEntity entity, bool saveAction = true)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            objectSet.AddObject(entity);

            if (saveAction)
                SaveChanges(SaveOptions.DetectChangesBeforeSave);
        }

        /// <summary>
        /// Adds a list of the specified entities
        /// </summary>
        /// <param name="list">Entities to add</param>
        /// <exception cref="ArgumentNullException"> if <paramref name="entity"/> is null</exception>
        public void Add(List<TEntity> list)
        {
            if (list.Count == 0)
            {
                throw new ArgumentNullException("list of entities");
            }

            foreach (var item in list)
            {
                objectSet.AddObject(item);
            }

            SaveChanges(SaveOptions.DetectChangesBeforeSave);
        }

        /// <summary>
        /// Updates the specified entity
        /// </summary>
        /// <param name="entity">Entity to update</param>
        /// <param name="listProperties">List with the properties of the entity that will be updated</param>
        /// <exception cref="ArgumentNullException"> if <paramref name="entity"/> is null</exception>
        public void Update(TEntity entity, List<string> listProperties)
        {
            if (entity == null)
            {
                throw new ArgumentNullException("entity");
            }

            //This step is needed to obtain the list of properties of the Entity on the model,
            //and give the values that the object in memory has, for each property.
            var dictionaryEntityProperties = GetPropertiesAndFillValuesByEntity(entity);

            //This way we will get the Entity from the model, so we have her attached to the model.
            var originalEntity = GetOriginalEntity(entity,
                dictionaryEntityProperties.First().Key,
                dictionaryEntityProperties.First().Value);

            //Needed to iterate the properties of originalEntity, and fill with values of entity
            var originalEntityPropertiesInfo = originalEntity.GetType().GetProperties();

            foreach (var property in listProperties)
                originalEntityPropertiesInfo.Single(c => c.Name == property).SetValue(originalEntity, dictionaryEntityProperties.Single(c => c.Key == property).Value, null);

            SaveChanges(SaveOptions.DetectChangesBeforeSave);
        }

        /// <summary>
        /// Attaches the specified entity
        /// </summary>
        /// <param name="entity">Entity to attach</param>
        public void Attach(TEntity entity)
        {
            objectSet.Attach(entity);
        }

        /// <summary>
        /// Saves all context changes
        /// </summary>
        public void SaveChanges()
        {
            context.SaveChanges();
        }

        /// <summary>
        /// Saves all context changes with the specified SaveOptions
        /// </summary>
        /// <param name="options">Options for saving the context</param>
        public void SaveChanges(SaveOptions options)
        {
            context.SaveChanges(options);
        }

        /// <summary>
        /// Releases all resources used
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Releases all resources used
        /// </summary>
        /// <param name="disposing">A boolean value indicating whether or not to dispose managed resources</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                if (context != null)
                {
                    context.Dispose();
                    context = null;
                }
            }
        }

        /// <summary>
        /// Method that allows to make a dynamic query to get an Entity from the repository
        /// </summary>
        /// <param name="entity"></param>
        /// <param name="propertyName"></param>
        /// <param name="propertyValue"></param>
        /// <returns></returns>
        private TEntity GetOriginalEntity(TEntity entity, string propertyName, object propertyValue)
        {
            IQueryable<TEntity> query = context.CreateObjectSet<TEntity>();
            return query.Where(String.Format("{0} = {1}", propertyName, propertyValue.ToString())).SingleOrDefault();
        }

        /// <summary>
        /// Method that gets all the properties of the Entity from the model, and fill the properties
        /// with the matching values from the entity passed as parameter.
        /// </summary>
        /// <param name="entity"></param>
        /// <returns></returns>
        private Dictionary<string, object> GetPropertiesAndFillValuesByEntity(TEntity entity)
        {
            var dictionaryProperties = new Dictionary<string, object>();

            foreach (var property in typeof(TEntity).GetProperties())
            {
                try
                {
                    dictionaryProperties.Add(property.Name, property.GetValue(entity, null));
                }
                catch (System.Reflection.TargetInvocationException ex)
                {
                    //The type ObjectDisposedException is ignored for now.
                    if (ex.InnerException.GetType() != typeof(ObjectDisposedException))
                    {
                        throw;
                    }
                }
            }

            return dictionaryProperties;
        }
    }
}
