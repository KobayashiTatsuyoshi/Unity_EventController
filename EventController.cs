using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public interface IQuery<out T>
{
    Dispose Subscribe(System.Action<T> action);
    IQuery<T> Where(System.Func<T, bool> condition);
    IQuery<S> Select<S>(System.Func<T, S> selector);
}

public interface IExecuter<in T> where T : unmanaged
{
    void Next(T args);
}

public delegate void Dispose();

public class EventController
{
    private Executer<T> GetExecuter<T>() where T : unmanaged
    {
        if (m_executers == null) m_executers = new List<IConvert>();
        var target = m_executers.FirstOrDefault(e => e.Is<T>());
        if (target != null) return target.As<T>();
        var exe = new Executer<T>();
        m_executers.Add(exe);
        return exe;
    }

    private static EventController _instance;
    private static EventController Instance
    {
        get
        {
            if (_instance != null) return _instance;
            _instance = new EventController();
            return _instance;
        }
    }

    public static IExecuter<T> Regist<T>() where T : unmanaged
    {
        return Instance.GetExecuter<T>();
    }

    public static IQuery<T> AddQuery<T>() where T : unmanaged
    {
        return new Entry<T>(Instance.GetExecuter<T>());
    }

    public static Dispose AddQuery<T>(System.Action<T> act) where T : unmanaged
    {
        return AddQuery<T>().Subscribe(act);
    }

    private List<IConvert> m_executers;

    private interface IConvert
    {
        bool Is<Target>() where Target : unmanaged;
        Executer<Target> As<Target>() where Target : unmanaged;
    }

    private interface IExecutable<out T> where T : unmanaged
    {
        void Push(Action<T> act);
        void Remove(Action<T> act);
    }

    private class Executer<T> : IExecuter<T>, IExecutable<T>, IConvert where T : unmanaged
    {
        public event Action<T> m_action;

        public Executer<Target> As<Target>() where Target : unmanaged
        {
            return this as Executer<Target>;
        }

        public bool Is<Target>() where Target : unmanaged
        {
            return this is Executer<Target>;
        }

        public void Next(T args)
        {
            if(m_action != null) m_action(args);
        }

        public void Push(Action<T> act) => m_action += act;

        public void Remove(Action<T> act) => m_action -= act;
    }

    private struct Entry<TSrc> : IQuery<TSrc> where TSrc : unmanaged
    {
        private Query<TSrc> m_entry;
        public Entry(IExecutable<TSrc> executable)
        {
            Func<Action<TSrc>, Dispose> entry = (onNext) =>
            {
                executable.Push(onNext);
                return () => executable.Remove(onNext);
            };
            m_entry = new Query<TSrc>(entry);
        }

        public IQuery<TDst> Select<TDst>(Func<TSrc, TDst> selector) => m_entry.Select(selector);

        public Dispose Subscribe(Action<TSrc> action) => m_entry.Subscribe(action);

        public IQuery<TSrc> Where(Func<TSrc, bool> condition) => m_entry.Where(condition);

        private struct Query<TQueue> : IQuery<TQueue>
        {
            Func<Action<TQueue>, Dispose> m_entry;

            public Query(Func<Action<TQueue>, Dispose> entry)
            {
                m_entry = entry;
            }

            public IQuery<TDst> Select<TDst>(Func<TQueue, TDst> selector)
            {
                var entry = m_entry;
                Func<Action<TDst>, Dispose> nextEntry = (onNext) =>
                {
                    Action<TQueue> select = (arg) =>
                    {
                        onNext(selector(arg));
                    };
                    return entry(select);
                };
                return new Query<TDst>(nextEntry);
            }

            public Dispose Subscribe(Action<TQueue> action)
            {
                return m_entry(action);
            }

            public IQuery<TQueue> Where(Func<TQueue, bool> condition)
            {
                var entry = m_entry;
                Func<Action<TQueue>, Dispose> nextEntry = (onNext) =>
                {
                    Action<TQueue> where = (arg) =>
                    {
                        if (condition(arg)) onNext(arg);
                    };
                    return entry(where);
                };
                return new Query<TQueue>(nextEntry);
            }
        }
    }
}
