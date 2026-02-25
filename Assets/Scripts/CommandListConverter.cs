using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace nodedef
{
    /// <summary>
    /// JSON 배열 안의 각 원소가 { "type": "xxx", ... } 형태일 때,
    /// type 값에 맞는 구체 클래스로 디시리얼라이즈하는 컨버터.
    /// (클래스 이름이 type과 비슷하게 매칭되도록 설계)
    /// </summary>
    public sealed class CommandListConverter : JsonConverter
    {
        // (elementBaseType, typeStringNormalized) -> concreteType 캐시
        private static readonly Dictionary<(Type, string), Type> _cache = new();

        public override bool CanWrite => false;

        public override bool CanConvert(Type objectType)
        {
            // List<T>, T[] 정도를 대상으로 함
            return GetElementType(objectType) != null;
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            var elementType = GetElementType(objectType);
            if (elementType == null)
                return serializer.Deserialize(reader, objectType);

            var token = JToken.Load(reader);

            // null 처리
            if (token.Type == JTokenType.Null)
                return null;

            // 배열이 아니면 그냥 기본 역직렬화
            if (token is not JArray arr)
                return token.ToObject(objectType, serializer);

            // List<T>로 만들기
            var listType = typeof(List<>).MakeGenericType(elementType);
            var list = (IList)Activator.CreateInstance(listType);

            foreach (var item in arr)
            {
                if (item is not JObject obj)
                {
                    // 이상한 형태면 그냥 elementType로 시도
                    list.Add(item.ToObject(elementType, serializer));
                    continue;
                }

                var typeStr = obj["type"]?.ToString();
                var concrete = ResolveConcreteType(elementType, typeStr);

                // 매칭 실패하면 elementType로라도 시도 (elementType이 추상/인터페이스면 여기서 또 에러날 수 있음)
                var finalType = concrete ?? elementType;

                var parsed = obj.ToObject(finalType, serializer);
                list.Add(parsed);
            }

            // objectType이 배열이면 배열로 변환
            if (objectType.IsArray)
            {
                var array = Array.CreateInstance(elementType, list.Count);
                list.CopyTo(array, 0);
                return array;
            }

            return list;
        }

        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            throw new NotSupportedException("CommandListConverter does not support writing.");
        }

        private static Type GetElementType(Type objectType)
        {
            if (objectType.IsArray) return objectType.GetElementType();

            if (objectType.IsGenericType)
            {
                var def = objectType.GetGenericTypeDefinition();
                if (def == typeof(List<>) || def == typeof(IList<>) || def == typeof(IEnumerable<>))
                    return objectType.GetGenericArguments()[0];
            }

            return null;
        }

        private static Type ResolveConcreteType(Type elementBaseType, string typeStr)
        {
            if (string.IsNullOrEmpty(typeStr))
                return null;

            var key = (elementBaseType, Normalize(typeStr));
            if (_cache.TryGetValue(key, out var cached))
                return cached;

            // elementBaseType를 상속/구현하는 모든 구체 타입 스캔
            var candidates = AppDomain.CurrentDomain.GetAssemblies()
                .SelectMany(a =>
                {
                    try { return a.GetTypes(); }
                    catch { return Array.Empty<Type>(); }
                })
                .Where(t => t != null
                            && !t.IsAbstract
                            && elementBaseType.IsAssignableFrom(t));

            // type 문자열과 클래스명을 "느슨하게" 매칭
            foreach (var t in candidates)
            {
                var n = Normalize(t.Name);
                // 흔한 접미사 제거 후 다시 비교
                var n2 = Normalize(StripSuffixes(t.Name));

                if (n == key.Item2 || n2 == key.Item2)
                {
                    _cache[key] = t;
                    return t;
                }
            }

            // 못 찾으면 캐시에 null 저장(반복 스캔 방지)
            _cache[key] = null;
            return null;
        }

        private static string StripSuffixes(string name)
        {
            // 너 프로젝트에서 흔히 쓰는 접미사 가정
            var s = name;
            foreach (var suf in new[] { "Def", "Command", "Action", "Cmd" })
            {
                if (s.EndsWith(suf, StringComparison.OrdinalIgnoreCase))
                    s = s.Substring(0, s.Length - suf.Length);
            }
            return s;
        }

        private static string Normalize(string s)
        {
            // set_flag, setFlag, SetFlag, SETFLAG 모두 같은 키로
            var chars = s.Where(char.IsLetterOrDigit).ToArray();
            return new string(chars).ToLowerInvariant();
        }
    }
}