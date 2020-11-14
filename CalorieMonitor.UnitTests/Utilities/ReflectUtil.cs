using System.Reflection;

namespace CalorieMonitor.UnitTests.Utilities
{
    public class ReflectUtil
    {
        public static object GetPrivateValue(object instance, string name)
        {
            return instance.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).GetValue(instance);
        }
        public static object GetStaticValue(object instance, string name)
        {
            return instance.GetType().GetField(name, BindingFlags.Static | BindingFlags.NonPublic).GetValue(instance);
        }
        public static void SetPrivateValue(object instance, string name, object value)
        {
            instance.GetType().GetField(name, BindingFlags.Instance | BindingFlags.NonPublic).SetValue(instance, value);
        }
    }
}
