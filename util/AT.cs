using System;
using System.Reflection;


namespace CustomGameStats
{
    public static class AT
    {
        public static BindingFlags flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Static;

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

        public static UID GetTagUID(string _tagName)
        {
            foreach (Tag _tag in (Tag[])GetValue(typeof(TagSourceManager), TagSourceManager.Instance, "m_tags"))
            {
                if (_tag.TagName.Equals(_tagName))
                {
                    return _tag.UID;
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
