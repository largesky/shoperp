using System;
using System.Collections.Generic;
using System.Linq;
using NHibernate;
using ShopErp.Domain;
using ShopErp.Domain.RestfulResponse;

namespace ShopErp.Server.Dao.NHibernateDao
{
    public class NHibernateDaoBase<E> : IDao<E>
        where E : class
    {
        private DateTime dbMinDateTime = new DateTime(1970, 01, 01);

        public NHibernate.ISession OpenSession()
        {
            return NHibernateHelper.OpenSession();
        }

        public string GetEntiyName()
        {
            return typeof(E).Name;
        }

        public E GetById(object id)
        {
            ISession session = this.OpenSession();
            try
            {
                return session.Get<E>(id);
            }
            finally
            {
                if (session != null)
                {
                    session.Close();
                }
            }
        }

        public E GetByField(string field, object value)
        {
            ISession session = this.OpenSession();
            try
            {
                var query = session.CreateQuery("from " + GetEntiyName() + " where " + field + " =?").SetParameter(0, value);
                return (E)query.UniqueResult();
            }
            finally
            {
                if (session != null)
                {
                    session.Close();
                }
            }
        }

        public DataCollectionResponse<E> GetAll()
        {
            ISession session = this.OpenSession();
            try
            {
                var query = session.CreateQuery("from " + GetEntiyName());
                var datas = query.List<E>();
                return new DataCollectionResponse<E>(datas);
            }
            finally
            {
                if (session != null)
                {
                    session.Close();
                }
            }
        }

        public DataCollectionResponse<E> GetAllByField(string field, object value, int pageIndex, int pageSize)
        {
            return this.GetPage("from " + GetEntiyName() + " where " + field + " = ?", pageIndex, pageSize, value);
        }

        public DataCollectionResponse<E> GetAllByFieldLike(string field, object value, int pageIndex, int pageSize)
        {
            return this.GetPage("from " + GetEntiyName() + " where " + field + " like ?", pageIndex, pageSize, '%' + value.ToString() + '%');
        }

        public DataCollectionResponse<E> GetPage(string query, int pageIndex, int pageSize, params object[] objs)
        {
            return this.GetPageEx(this.TrimHSql(query) + " order by Id desc", "select count(Id) " + query, pageIndex, pageSize, objs);
        }

        public DataCollectionResponse<E> GetPageEx(string dataQuery, string countQuery, int pageIndex, int pageSize, params object[] objs)
        {
            ISession session = OpenSession();
            DataCollectionResponse<E> ret = null;
            try
            {
                // 查询数据
                string nDataQuery = this.TrimHSql(dataQuery);
                string nCountQuery = this.TrimHSql(countQuery);
                var hDataQuery = session.CreateQuery(nDataQuery);
                if (pageSize > 0)
                {//需要分页
                    hDataQuery.SetFirstResult(pageSize * pageIndex);
                    hDataQuery.SetMaxResults(pageSize);
                }
                for (int i = 0; i < objs.Length; i++)
                {
                    hDataQuery.SetParameter(i, objs[i]);
                }
                IList<E> data = hDataQuery.List<E>();
                ret = new DataCollectionResponse<E>(data);

                // 查询总数
                if (pageSize > 0 && string.IsNullOrWhiteSpace(nCountQuery) == false)
                {
                    var hCountQuery = session.CreateQuery(nCountQuery);
                    for (int i = 0; i < objs.Length; i++)
                    {
                        hCountQuery.SetParameter(i, objs[i]);
                    }
                    int total = int.Parse(hCountQuery.UniqueResult().ToString());
                    ret.Total = total;
                }
                else
                {
                    ret.Total = data.Count;
                }
                return ret;

            }
            finally
            {
                if (session != null)
                {
                    session.Close();
                }
            }
        }

        public List<T> GetColumnValueBySqlQuery<T>(string query)
        {
            ISession session = OpenSession();
            try
            {
                var sqlQ = session.CreateSQLQuery(query);
                List<T> list = new List<T>();
                sqlQ.List(list);
                return list;
            }
            finally
            {
                if (session != null)
                {
                    session.Close();
                }
            }
        }

        public void Save(params object[] objs)
        {
            ISession s = OpenSession();
            ITransaction transaction = null;
            try
            {
                transaction = s.BeginTransaction();
                foreach (Object obj in objs)
                {
                    if (obj == null)
                    {
                        continue;
                    }
                    if (obj.GetType().IsArray == false || obj.GetType() == typeof(string))
                    {
                        s.Save(obj);
                    }
                    else
                    {
                        for (int i = 0; i < ((Array)obj).Length; i++)
                        {
                            s.Save(((Array)obj).GetValue(i));
                        }
                    }
                }
                transaction.Commit();
            }
            catch
            {
                if (transaction != null)
                    transaction.Rollback();
                throw;
            }
            finally
            {
                if (s != null)
                {
                    s.Close();
                }
            }
        }

        public void Update(params object[] objs)
        {
            ISession s = OpenSession();
            ITransaction transaction = null;
            try
            {
                transaction = s.BeginTransaction();
                foreach (Object obj in objs)
                {
                    if (obj == null)
                    {
                        continue;
                    }
                    if (obj.GetType().IsArray == false || obj.GetType() == typeof(string))
                    {
                        s.Update(obj);
                    }
                    else
                    {
                        for (int i = 0; i < ((Array)obj).Length; i++)
                        {
                            s.Update(((Array)obj).GetValue(i));
                        }
                    }
                }
                transaction.Commit();
            }
            catch
            {
                if (transaction != null)
                    transaction.Rollback();
                throw;
            }
            finally
            {
                s.Close();
            }
        }

        public void Delete(params object[] objs)
        {
            ISession s = OpenSession();
            ITransaction transaction = null;
            try
            {
                transaction = s.BeginTransaction();
                foreach (Object obj in objs)
                {
                    if (obj == null)
                    {
                        continue;
                    }
                    if (obj.GetType().IsArray == false || obj.GetType() == typeof(string))
                    {
                        s.Delete(obj);
                    }
                    else
                    {
                        for (int i = 0; i < ((Array)obj).Length; i++)
                        {
                            s.Delete(((Array)obj).GetValue(i));
                        }
                    }
                }
                transaction.Commit();
            }
            catch
            {
                if (transaction != null)
                    transaction.Rollback();
                throw;
            }
            finally
            {
                s.Close();
            }
        }

        public void SaveOrUpdateById(params object[] objs)
        {
            ISession s = OpenSession();
            ITransaction transaction = null;
            try
            {
                transaction = s.BeginTransaction();
                foreach (Object obj in objs)
                {
                    if (obj == null)
                    {
                        continue;
                    }
                    if (obj.GetType().IsArray == false || obj.GetType() == typeof(string))
                    {
                        if (this.GetObjectId(obj) > 0)
                        {
                            s.Update(obj);
                        }
                        else
                        {
                            s.Save(obj);
                        }

                    }
                    else
                    {
                        for (int i = 0; i < ((Array)obj).Length; i++)
                        {
                            if (this.GetObjectId(obj) > 0)
                            {
                                s.Update((((Array)obj).GetValue(i)));
                            }
                            else
                            {
                                s.Save((((Array)obj).GetValue(i)));
                            }
                        }
                    }
                }
                transaction.Commit();
            }
            catch 
            {
                if (transaction != null)
                    transaction.Rollback();
                throw;
            }
            finally
            {
                s.Close();
            }
        }

        public long GetObjectId(object value)
        {
            var properties = value.GetType().GetProperties();
            var property = properties.FirstOrDefault(obj => obj.Name.Equals("Id", StringComparison.OrdinalIgnoreCase));
            if (property == null)
            {
                throw new Exception("没有在类型" + value.GetType().FullName + "找到属性：Id");
            }
            return long.Parse(property.GetValue(value, new object[0]).ToString());
        }

        public string TrimHSql(string hsql)
        {
            hsql = hsql.Trim();

            if (hsql.EndsWith("and"))
            {
                hsql = hsql.Substring(0, hsql.Length - 3);
            }

            if (hsql.EndsWith("where"))
            {
                hsql = hsql.Substring(0, hsql.Length - "where".Length);
            }

            return hsql;
        }

        public string MakeQuery(string name, string value, IList<Object> objs)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }
            objs.Add(value);
            return name + "=? and ";
        }

        public string MakeQueryLike(string name, string value, IList<object> objs)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return string.Empty;
            }
            objs.Add('%' + value + '%');
            return name + " like ? and ";
        }

        public string MakeQuery(string name, int value, int emptyValue = 0)
        {
            if (value == emptyValue)
            {
                return string.Empty;
            }

            return name + "=" + value + " and ";
        }

        public string MakeQuery(string name, long value, long emptyValue = 0)
        {
            if (value == emptyValue)
            {
                return string.Empty;
            }
            return name + "=" + value + " and ";
        }

        public string MakeQuery(string name, DateTime value, bool isOver)
        {
            if (this.IsLessDBMinDate(value))
            {
                return string.Empty;
            }

            if (isOver)
            {
                return name + ">='" + this.FormatDateTime(value) + "' and ";
            }

            return name + "<='" + this.FormatDateTime(value) + "' and ";
        }

        public string MakeQuery(string name, double value, double defalutValue = 0)
        {
            if (value == defalutValue)
            {
                return string.Empty;
            }

            return name + "=" + value.ToString("F4") + " and ";
        }

        public string MakeQuery(string name, bool? value)
        {
            if (value == null)
            {
                return string.Empty;
            }

            return name + "=" + (value == true ? "1" : "0") + " and ";
        }

        public string FormatDateTime(DateTime time)
        {
            return time.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public DateTime GetDBMinDateTime()
        {
            return this.dbMinDateTime;
        }

        public bool IsLessDBMinDate(DateTime dt)
        {
            return this.dbMinDateTime.AddYears(10) >= dt;
        }

        public int ExcuteSqlUpdate(string sql)
        {
            ISession session = this.OpenSession();
            try
            {
                var query = session.CreateSQLQuery(sql);
                return query.ExecuteUpdate();
            }
            finally
            {
                if (session != null)
                {
                    session.Close();
                }
            }
        }

        public long GetAllCount(string tableName)
        {
            ISession session = this.OpenSession();
            try
            {
                var query = session.CreateSQLQuery("select count(id) from [" + tableName + "]");
                return long.Parse(query.UniqueResult().ToString());
            }
            finally
            {
                if (session != null)
                {
                    session.Close();
                }
            }
        }
    }
}
