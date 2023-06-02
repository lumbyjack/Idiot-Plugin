using System;
using System.Reflection;


namespace IdiotPlugin.Util
{

	public static class ReflectionUtils
	{
		private const BindingFlags InstanceFlags = BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic;

		private const BindingFlags StaticFlags = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;

		public static MethodInfo Method(Type type, string name, BindingFlags flags)
		{
			return type.GetMethod(name, flags) ?? throw new Exception($"Couldn't find method {name} on {type}");
		}

		public static MethodInfo InstanceMethod(Type t, string name)
		{
			return Method(t, name, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic);
		}

		public static MethodInfo StaticMethod(Type t, string name)
		{
			return Method(t, name, BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic);
		}
	}

}
