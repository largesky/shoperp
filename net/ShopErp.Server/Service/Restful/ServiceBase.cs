using System;
using System.Collections.Generic;
using ShopErp.Domain;
using System.Linq;

namespace ShopErp.Server.Service.Restful
{
    public abstract class ServiceBase<E, D>
        where D : Dao.IDao<E>, new()
        where E : class, new()
    {
        protected D dao = new D();

        private object scs_lock = new object();

        private List<E> scs = new List<E>();

        protected void CheckAndLoadCach()
        {
            if (scs.Count < 1)
            {
                lock (scs_lock)
                {
                    if (scs.Count < 1)
                    {
                        var items = this.dao.GetAll();
                        scs.AddRange(items.Datas);
                    }
                }
            }
        }

        protected void RefreshCach()
        {
            lock (scs_lock)
            {
                scs.Clear();
                var items = this.dao.GetAll();
                scs.AddRange(items.Datas);
            }
        }

        protected void AndOrReplaceInCach(E e, Predicate<E> matchOld)
        {
            this.CheckAndLoadCach();
            lock (scs_lock)
            {
                this.RemoveCach(matchOld);
                this.scs.Add(e);
            }
        }

        protected void RemoveCach(Predicate<E> match)
        {
            lock (scs_lock)
            {
                this.scs.RemoveAll(match);
            }
        }

        protected E GetFirstOrDefaultInCach(Predicate<E> match)
        {
            this.CheckAndLoadCach();
            return this.scs.FirstOrDefault(obj => match(obj));
        }

        protected List<E> GetAllInCach()
        {
            this.CheckAndLoadCach();
            return this.scs;
        }

        public DateTime GetDbMinTime()
        {
            return this.dao.GetDBMinDateTime();
        }

        public bool IsDbMinTime(DateTime time)
        {
            return this.dao.IsLessDBMinDate(time);
        }

        public DateTime FormatToDbTime(DateTime time)
        {
            if (Math.Abs(time.Subtract(DateTime.MinValue).TotalDays) < 100)
            {
                return this.GetDbMinTime();
            }

            if (Math.Abs(time.Subtract(this.GetDbMinTime()).TotalDays) < 100)
            {
                return this.GetDbMinTime();
            }
            return time;
        }

        public string FormatTime(DateTime time)
        {
            return time.ToString("yyyy-MM-dd HH:mm:ss");
        }

        public List<T> GetColumnValueBySqlQuery<T>(string query)
        {
            return this.dao.GetColumnValueBySqlQuery<T>(query);
        }
    }
}
