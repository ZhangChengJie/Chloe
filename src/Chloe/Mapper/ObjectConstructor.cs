﻿using Chloe.Core;
using Chloe.Core.Emit;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Chloe.Mapper
{
    public class ObjectConstructor
    {
        ObjectConstructor(ConstructorInfo constructorInfo)
        {
            if (constructorInfo.DeclaringType.IsAbstract)
                throw new ArgumentException("The type can not be abstract class.");

            this.ConstructorInfo = constructorInfo;
            this.Init();
        }

        void Init()
        {
            ConstructorInfo constructor = this.ConstructorInfo;
            InstanceCreator instanceCreator = DelegateGenerator.CreateObjectGenerator(constructor);
            this.InstanceCreator = instanceCreator;
        }

        public ConstructorInfo ConstructorInfo { get; private set; }
        public InstanceCreator InstanceCreator { get; private set; }

        static readonly System.Collections.Concurrent.ConcurrentDictionary<ConstructorInfo, ObjectConstructor> InstanceCache = new System.Collections.Concurrent.ConcurrentDictionary<ConstructorInfo, ObjectConstructor>();

        public static ObjectConstructor GetInstance(ConstructorInfo constructorInfo)
        {
            ObjectConstructor instance;
            if (!InstanceCache.TryGetValue(constructorInfo, out instance))
            {
                lock (constructorInfo)
                {
                    if (!InstanceCache.TryGetValue(constructorInfo, out instance))
                    {
                        instance = new ObjectConstructor(constructorInfo);
                        InstanceCache.GetOrAdd(constructorInfo, instance);
                    }
                }
            }

            return instance;
        }
    }

    public class ReaderOrdinalEnumerator
    {
        public static readonly MethodInfo MethodOfNext;
        static ReaderOrdinalEnumerator()
        {
            MethodInfo method = typeof(ReaderOrdinalEnumerator).GetMethod("Next");
            MethodOfNext = method;
        }

        List<int> _readerOrdinals;
        int _next;
        int _currentOrdinal;
        public int CurrentOrdinal { get { return this._currentOrdinal; } }
        public ReaderOrdinalEnumerator(List<int> readerOrdinals)
        {
            this._readerOrdinals = readerOrdinals;
            this._next = 0;
            this._currentOrdinal = -1;
        }
        public int Next()
        {
            this._currentOrdinal = this._readerOrdinals[this._next];
            this._next++;
            return this._currentOrdinal;
        }

        public void Reset()
        {
            this._next = 0;
            this._currentOrdinal = -1;
        }
    }
    public class ObjectActivatorEnumerator
    {
        List<IObjectActivator> _objectActivators;
        int _next;

        public static readonly MethodInfo MethodOfNext;
        static ObjectActivatorEnumerator()
        {
            MethodInfo method = typeof(ObjectActivatorEnumerator).GetMethod("Next");
            MethodOfNext = method;
        }

        public ObjectActivatorEnumerator(List<IObjectActivator> objectActivators)
        {
            this._objectActivators = objectActivators;
            this._next = 0;
        }
        public IObjectActivator Next()
        {
            IObjectActivator ret = this._objectActivators[this._next];
            this._next++;
            return ret;
        }
        public void Reset()
        {
            this._next = 0;
        }
    }

}
