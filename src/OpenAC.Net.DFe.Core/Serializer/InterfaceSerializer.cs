using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Xml.Linq;
using OpenAC.Net.Core;
using OpenAC.Net.Core.Extensions;
using OpenAC.Net.DFe.Core.Attributes;

namespace OpenAC.Net.DFe.Core.Serializer;

internal static class InterfaceSerializer
{
    public static XObject[] Serialize(PropertyInfo prop, object parentObject, SerializerOptions options)
    {
        var value = prop.GetValue(parentObject, null);
        var objectType = ObjectType.From(value);
        var attributes = prop.GetAttributes<DFeItemAttribute>();
        var itemAttribute = attributes.SingleOrDefault(x => x.Tipo == value.GetType());

        Guard.Against<OpenDFeException>(itemAttribute == null, $"Nenhum atributo [{nameof(DFeItemAttribute)}] encontrado " +
                                                               $"para o objeto: {nameof(value.GetType)}");

        if (objectType.IsIn(ObjectType.ListType, ObjectType.ArrayType, ObjectType.EnumerableType))
        {
            var list = (ICollection)prop.GetValue(parentObject, null);
            return CollectionSerializer.SerializeObjects(list, itemAttribute, options);
        }

        return objectType == ObjectType.ValueElementType ? ValueElementSerializer.Serialize(prop, parentObject, options) :
            new XObject[] { ObjectSerializer.Serialize(value, value.GetType(), itemAttribute.Name, itemAttribute.Namespace, options) };
    }

    public static object Deserialize(PropertyInfo prop, XElement parentElement, object item, SerializerOptions options)
    {
        var tags = prop.GetAttributes<DFeItemAttribute>();
        foreach (var att in tags)
        {
            var node = parentElement.ElementsAnyNs(att.Name).FirstOrDefault();
            if (node == null) continue;

            var objectType = ObjectType.From(att.Tipo);
            if (objectType.IsIn(ObjectType.ArrayType, ObjectType.EnumerableType))
            {
                var listElement = parentElement.ElementsAnyNs(att.Name);
                var list = (List<object>)CollectionSerializer.Deserialize(typeof(List<object>), listElement, options);
                var type = CollectionSerializer.GetItemType(att.Tipo);
                return objectType == ObjectType.ArrayType ? list.Cast(type).ToArray(type) : list.Cast(type);
            }

            if (objectType == ObjectType.ListType)
            {
                var listElement = parentElement.ElementsAnyNs(att.Name);
                return CollectionSerializer.Deserialize(att.Tipo, listElement, options);
            }

            return objectType == ObjectType.ValueElementType ? ValueElementSerializer.Deserialize(att.Tipo, parentElement, options) :
                ObjectSerializer.Deserialize(att.Tipo, node, options);
        }

        return null;
    }
}