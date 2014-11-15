using System;
using System.Linq;
using System.Reflection;

namespace Platform.Reflection
{
	/// <summary>
	/// Provides extension methods and static utility methods for MemberInfo and derived classes.
	/// </summary>
	public static class MemberInfoUtils
	{
		/// <summary>
		/// Gets and returns the first custom attribute of the given member.
		/// </summary>
		/// <param name="memberInfo">The target member</param>
		/// <typeparam name="T">The type of attribute to get an return</typeparam>
		/// <param name="inherit"></param>
		/// <returns>The custom attribute or null if one was not found</returns>
		public static T GetFirstCustomAttribute<T>(this MemberInfo memberInfo, bool inherit)
			where T : Attribute
		{
			object[] values;

			if (inherit)
			{
				if (memberInfo is Type)
				{
					var type = (Type)memberInfo;

					while (type != null)
					{
						values = type.GetCustomAttributes(typeof(T), true);

						if (values.Length > 0)
						{
							return (T)values[0];
						}

						type = type.BaseType;
					}
				}
				else
				{
					var type = memberInfo.DeclaringType;

					while (type != null && memberInfo != null)
					{
						values = memberInfo.GetCustomAttributes(typeof(T), true);

						if (values.Length > 0)
						{
							return (T)values[0];
						}

						type = type.BaseType;

						switch (memberInfo.MemberType)
						{
							case MemberTypes.Property:
								var propertyInfo = (PropertyInfo)memberInfo;
								try
								{
									memberInfo = type.GetProperty(propertyInfo.Name);
								}
								catch(AmbiguousMatchException)
								{
									memberInfo = type.GetProperties().FirstOrDefault(x => x.Name == memberInfo.Name && x.DeclaringType == type);

									if (memberInfo == null)
									{
										memberInfo = type.GetProperties().FirstOrDefault(x => x.Name == memberInfo.Name);
									}
								}
								break;
							case MemberTypes.Constructor:
								var constructorInfo = (ConstructorInfo)memberInfo;
								memberInfo = type.GetConstructor(constructorInfo.GetParameters().Select(x => x.ParameterType).ToArray());
								break;
							case MemberTypes.Event:
								var eventInfo = (EventInfo)memberInfo;
								memberInfo = type.GetEvent(eventInfo.Name);
								break;
							case MemberTypes.Field:
								var fieldInfo = (FieldInfo)memberInfo;
								memberInfo = type.GetField(fieldInfo.Name);
								break;
							case MemberTypes.Method:
								var methodInfo = (MethodInfo)memberInfo;
								memberInfo = type.GetMethod(methodInfo.Name, methodInfo.GetParameters().Select(x => x.ParameterType).ToArray());
								break;
							default:
								var memberInfos = type.GetMember(memberInfo.Name);

								if (memberInfos.Length > 0)
								{
									memberInfo = memberInfos[1];
								}
								else
								{
									return null;
								}
								break;
						}
					}
				}

				return null;
			}
			else
			{
				values = memberInfo.GetCustomAttributes(typeof(T), false);
			}

			return values.Length > 0 ? (T)values[0] : null;
		}

		/// <summary>
		/// Gets the return type of a member.
		/// </summary>
		/// <returns>The FieldType, PropertyType or ReturnType of the member or null if the member is not a field, property or method</returns>
		public static Type GetMemberReturnType(this MemberInfo memberInfo)
		{
			var fieldInfo = memberInfo as FieldInfo;
			
			if (fieldInfo != null)
			{
				return fieldInfo.FieldType;
			}

			var propertyInfo = memberInfo as PropertyInfo;

			if (propertyInfo != null)
			{
				return propertyInfo.PropertyType;
			}

			var methodInfo = memberInfo as MethodInfo;

			if (methodInfo != null)
			{
				return methodInfo.ReturnType;
			}

			return null;
		}
	}
}
