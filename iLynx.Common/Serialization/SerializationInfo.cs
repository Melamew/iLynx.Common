using System;
using System.Reflection;

namespace iLynx.Common.Serialization
{
    /// <summary>
    /// Contains the most basic information for serializing a single member of an object.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SerializationInfo<T>
    {
        private readonly MemberInfo member;
        private readonly Type type;

        /// <summary>
        /// The member that is described with this SerializationInfo
        /// </summary>
        public MemberInfo Member
        {
            get { return member; }
        }

        /// <summary>
        /// Gets the Serializer that can be used to serialize this member (Only if the member is NOT untyped (Interface, object, abstract, etc.) and the member is NOT a delegate type.
        /// </summary>
        public ISerializer TypeSerializer { get; private set; }
        
        /// <summary>
        /// Gets a value indicating whether or not the member described in this instance is 'untyped' (Of type object, an interface, or an abstract class definition).
        /// </summary>
        public bool IsUntyped { get; private set; }

        /// <summary>
        /// Gets a value indicating whether or not the member described in this instance is a delegate type.
        /// </summary>
        public bool IsDelegate { get; private set; }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="member"></param>
        /// <param name="type"></param>
        public SerializationInfo(MemberInfo member, Type type)
        {
            member.Guard("member");
            IsUntyped = type.IsInterface || type.IsAbstract || typeof(object) == type;
            IsDelegate = typeof(Delegate).IsAssignableFrom(type);
            this.member = member;
            this.type = type;
            TypeSerializer = IsUntyped || IsDelegate ? null : GetSerializer(type);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="source"></param>
        /// <returns></returns>
        public object GetValue(object source)
        {
            if (null == source) return null;
            var property = Member as PropertyInfo;
            if (null != property)
                return property.GetValue(source);
            var field = Member as FieldInfo;
            return null != field ? field.GetValue(source) : null;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="target"></param>
        /// <param name="value"></param>
        public void SetValue(ref object target, object value)
        {
            var property = Member as PropertyInfo;
            if (null != property)
            {
                property.SetValue(target, value);
                return;
            }
            var field = Member as FieldInfo;
            if (null == field) return;
            field.SetValue(target, value);
        }

        private static ISerializer GetSerializer(Type type)
        {
            return Serializer.GetSerializer(type.IsEnum ? Enum.GetUnderlyingType(type) : type);
        }

        public override string ToString()
        {
            return string.Format("{0} : {1}", member.Name, type.AssemblyQualifiedName);
        }
    }
}