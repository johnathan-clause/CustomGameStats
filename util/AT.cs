using System;
using System.Reflection;


namespace CustomGameStats
{
    public static class AT
    {
        public static BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static;

        public static object Call(object obj, string method, params object[] args)
        {
            var methodInfo = obj.GetType().GetMethod(method, flags);
            if (methodInfo != null)
            {
                return methodInfo.Invoke(obj, args);
            }
            return null;
        }

        public static void SetValue<T>(T value, Type type, object obj, string field)
        {
            FieldInfo fieldInfo = type.GetField(field, flags);
            if (fieldInfo != null)
            {
                fieldInfo.SetValue(obj, value);
            }
        }

        public static object GetValue(Type type, object obj, string value)
        {
            FieldInfo fieldInfo = type.GetField(value, flags);
            if (fieldInfo != null)
            {
                return fieldInfo.GetValue(obj);
            }
            else
            {
                return null;
            }
        }

        public static void InheritBaseValues(object _derived, object _base)
        {
            foreach (FieldInfo fi in _base.GetType().GetFields(flags))
            {
                try { _derived.GetType().GetField(fi.Name).SetValue(_derived, fi.GetValue(_base)); } catch { }
            }

            return;
        }

        public static UID GetTagUIDForName(string _tagName)
        {
            TagSourceManager instance = TagSourceManager.Instance;
            foreach (Tag tag in (Tag[])GetValue(typeof(TagSourceManager), instance, "m_tags"))
            {
                if (tag.TagName.Equals(_tagName))
                {
                    return tag.UID;
                }
            }
            return null;
        }

        public static float GetCharacterStat(CharacterStats _stats, string _field)
        {
            Stat _stat = (Stat)GetValue(typeof(CharacterStats), _stats, _field);
            return _stat.CurrentValue;
        }
        public static float GetPlayerStat(PlayerCharacterStats _stats, string _field)
        {
            Stat _stat = (Stat)GetValue(typeof(PlayerCharacterStats), _stats, _field);
            return _stat.CurrentValue;
        }
    }
}
