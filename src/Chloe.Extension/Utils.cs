﻿using Chloe.Infrastructure;
using Chloe.Reflection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Chloe.Extension
{
    static class Utils
    {
        public static Task<T> MakeTask<T>(Func<T> func)
        {
#if net40
            T result = func();
            var task = new Task<T>(() => { return result; });
            task.Start();
            return task;
#else
            return Task.FromResult(func());
#endif
        }

        public static DbParam[] BuildParams(IDbContext dbContext, object parameter)
        {
            if (parameter == null)
                return new DbParam[0];

            if (parameter is IEnumerable<DbParam>)
            {
                return ((IEnumerable<DbParam>)parameter).ToArray();
            }

            IDatabaseProvider databaseProvider = ((DbContext)dbContext).DatabaseProvider;

            List<DbParam> parameters = new List<DbParam>();
            Type parameterType = parameter.GetType();
            var props = parameterType.GetProperties();
            foreach (var prop in props)
            {
                if (prop.GetGetMethod() == null)
                {
                    continue;
                }

                object value = ReflectionExtension.GetMemberValue(prop, parameter);

                string paramName = databaseProvider.CreateParameterName(prop.Name);

                DbParam p = new DbParam(paramName, value, prop.PropertyType);
                parameters.Add(p);
            }

            return parameters.ToArray();
        }
    }
}
