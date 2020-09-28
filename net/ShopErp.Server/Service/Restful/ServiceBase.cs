﻿using System;
using System.Collections.Generic;
using ShopErp.Domain;
using System.Linq;

namespace ShopErp.Server.Service.Restful
{
    public abstract class ServiceBase<E, D> where D : Dao.IDao<E>, new() where E : class, new()
    {
        protected D dao = new D();

        private object scs_lock = new object();

        private List<E> scs = new List<E>();

        protected void LoadCachIfEmpty()
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

        protected void ReloadCach()
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
            this.LoadCachIfEmpty();
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
            this.LoadCachIfEmpty();
            return this.scs.FirstOrDefault(obj => match(obj));
        }

        protected List<E> GetAllInCach()
        {
            this.LoadCachIfEmpty();
            return this.scs;
        }
        public List<T> GetColumnValueBySqlQuery<T>(string query)
        {
            return this.dao.GetColumnValueBySqlQuery<T>(query);
        }
    }
}
