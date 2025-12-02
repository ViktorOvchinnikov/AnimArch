using System;
using System.Collections.Generic;
using UnityEngine;
using Visualization.ClassDiagram.ClassComponents;

namespace EditorChangesHistory
{
	[Serializable]
    public class MethodObject
    {
        public string methodName;
        public string returnValue;
        public List<string> arguments;
    }

    [Serializable]
    public class AddMethodData
    {
        public string targetClass;
        public string methodObject;
    }

    [Serializable]
    public class UpdateMethodData
    {
        public string targetClass;
        public string oldMethod;
        public string newMethod;
    }

    [Serializable]
    public class RemoveMethodData
    {
        public string targetClass;
        public string methodName;
    }

	[Serializable]
    public class AddClassData
    {
        public string newClass;
    }

    [Serializable]
    public class UpdateClassData
    {
        public string oldName;
        public string newName;
    }

	[Serializable]
    public class RemoveClassData
    {
        public string oldClassName;
    }

	[Serializable]
    public class AddRelationData
    {
        public string sourceClass;
        public string targetClass;
    }

	[Serializable]
    public class RemoveRelationData
    {
        public string sourceClass;
        public string targetClass;
    }

    public static class DiagramChangeSerializer
    {

        private static string _SerializeMethodObject(Method method) 
        {
            var data = new MethodObject
            {
                methodName = method.Name,
                returnValue = method.ReturnValue,
                arguments = method.arguments ?? new List<string>()
            };

            return JsonUtility.ToJson(data);
        }	

        public static string SerializeAddMethod(string targetClass, Method method)
        {
            string methodObjectJson = _SerializeMethodObject(method);
            var data = new AddMethodData
            {
                targetClass = targetClass,
                methodObject = methodObjectJson
            };
            
            return JsonUtility.ToJson(data);
        }

        // TODO: what if user changes ony arguments or only return value?
        public static string SerializeUpdateMethod(string targetClass, string oldMethod, Method newMethod)
        {
            string newMethodObjectJson = _SerializeMethodObject(newMethod);

            var data = new UpdateMethodData
            {
                targetClass = targetClass,
                oldMethod = oldMethod,
                newMethod = newMethodObjectJson
            };
            
            return JsonUtility.ToJson(data);
        }

        public static string SerializeRemoveMethod(string targetClass, string methodName)
        {
            var data = new RemoveMethodData
            {
                targetClass = targetClass,
                methodName = methodName
            };
            
            return JsonUtility.ToJson(data);
        }

        public static string SerializeAddClass(string newClass)
        {
            var data = new AddClassData
            {
                newClass = newClass
            };
            
            return JsonUtility.ToJson(data);
        }

        public static string SerializeUpdateClass(string oldName, string newName)
        {
            var data = new UpdateClassData
            {
                oldName = oldName,
                newName = newName
            };
            
            return JsonUtility.ToJson(data);
        }

        public static string SerializeRemoveClass(string oldClassName)
        {
            var data = new RemoveClassData
            {
                oldClassName = oldClassName
            };
            
            return JsonUtility.ToJson(data);
        }

        public static string SerializeAddRelation(string sourceClass, string targetClass)
        {
            var data = new AddRelationData
            {
                sourceClass = sourceClass,
                targetClass = targetClass
            };
            
            return JsonUtility.ToJson(data);
        }

        public static string SerializeRemoveRelation(string sourceClass, string targetClass)
        {
            var data = new RemoveRelationData
            {
                sourceClass = sourceClass,
                targetClass = targetClass
            };
            
            return JsonUtility.ToJson(data);
        }
    }
}
