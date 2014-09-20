using System;
using System.Linq.Expressions;
using System.Reflection;

namespace iLynx.Common
{
    /// <summary>
    /// Guard...
    /// </summary>
    public static class Guard
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="membExpression"></param>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        /// <exception cref="ArgumentNullException"></exception>
        public static T IsNull<T>(Expression<Func<T>> membExpression)
        {
            var ex = membExpression.Body as MemberExpression;
            if (null == ex) return default(T);
            var value = GetValue(ex);
            if (null == value)
                throw new ArgumentNullException(ex.Member.Name);
            return (T)value;
        }

        private static object GetValue(MemberExpression expression)
        {
            var obj = ((ConstantExpression) expression.Expression).Value;
            var fieldMember = expression.Member as FieldInfo;
            if (null != fieldMember) return fieldMember.GetValue(obj);
            var propertyMember = expression.Member as PropertyInfo;
            return null == propertyMember ? null : propertyMember.GetValue(obj);
        }
    }
}