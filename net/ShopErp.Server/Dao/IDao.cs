using ShopErp.Domain;
using ShopErp.Domain.RestfulResponse;
using System;
using System.Collections.Generic;

namespace ShopErp.Server.Dao
{
    public interface IDao<E> where E : class
    {
        E GetById(object id);

        E GetByField(string field, object value);

        DataCollectionResponse<E> GetAll();

        DataCollectionResponse<E> GetAllByField(string field, object value, int pageIndex, int pageSize);

        DataCollectionResponse<E> GetAllByFieldLike(string field, object value, int pageIndex, int pageSize);

        DataCollectionResponse<E> GetPage(string query, int pageIndex, int pageSize, params object[] obj);

        List<T> GetColumnValueBySqlQuery<T>(string query);

        void Save(params object[] objs);

        void Update(params object[] objs);

        void Delete(params object[] objs);

        void SaveOrUpdateById(params object[] objs);

        int ExcuteSqlUpdate(string sql);

        long GetAllCount(string tableName);

        string FormatDateTime(DateTime time);

        DateTime GetDBMinDateTime();

        bool IsLessDBMinDate(DateTime dt);
    }
}
