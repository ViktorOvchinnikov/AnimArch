using System;
using System.Collections.Generic;
using System.Linq;
using Antlr4.Runtime.Misc;
using Antlr4.Runtime.Tree;
using OALProgramControl;
using IToken = Antlr4.Runtime.IToken;
using ParserRuleContext = Antlr4.Runtime.ParserRuleContext;


namespace AnimationControl.UMLDiagram
{
    public class UMLDiagramVisitorConcrete : UMLDiagramBaseVisitor<Object>
    {
        private ClassDiagramManager _classDiagramManager;
        private CDClass _currentClass;
        
        private void HandleError(string errorMessage, ParserRuleContext context)
        {
            throw new Exception(string.Format("Error while processing '{0}'.\n-------------------\n{1}\nChildren-wise: '{2}'", errorMessage, context?.GetText(), string.Join(", ", context?.children?.Select(child => child?.GetText()) ?? new string[] { } )));
        }
        
        public override object VisitDiagram(UMLDiagramParser.DiagramContext context)
        {
            _classDiagramManager = new ClassDiagramManager();
            
            // Skip the @startuml and @enduml tokens and process only the elements
            for (int i = 1; i < context.ChildCount - 2; i++)
            {
                Visit(context.children[i]);
            }

            return _classDiagramManager;
        }

        public override object VisitElement(UMLDiagramParser.ElementContext context)
        {
            return Visit(context.GetChild(0));
        }

        public override object VisitClassDefinition(UMLDiagramParser.ClassDefinitionContext context)
        {
            string className = Visit(context.className()) as string;
            if (string.IsNullOrEmpty(className))
            {
                HandleError("Invalid class name", context);
                return null;
            }

            _currentClass = _classDiagramManager.cdClassPool.SpawnClass(className);
            
            Visit(context.classBody());
            
            _currentClass = null;
            return _currentClass;
        }

        public override object VisitClassBody(UMLDiagramParser.ClassBodyContext context)
        {
            foreach (var member in context.classMember())
            {
                Visit(member);
            }
            return null;
        }

        public override object VisitMethod(UMLDiagramParser.MethodContext context)
        {
            if (_currentClass == null)
            {
                HandleError("Method defined outside of class context", context);
                return null;
            }
            
            string methodName = Visit(context.methodName()) as string;
            string returnType = Visit(context.returnType()) as string;
            
            
            if (string.IsNullOrEmpty(methodName))
            {
                HandleError("Invalid method name", context);
                return null;
            }
            
            CDMethod method = new CDMethod(_currentClass, methodName, returnType);
            
            if (context.methodArguments() != null)
            {
                foreach (var argContext in context.methodArguments().methodArgument())
                {
                    string argType = Visit(argContext.type()) as string;
                    string argName = Visit(argContext.argumentName()) as string;

                    if (string.IsNullOrEmpty(argType) || string.IsNullOrEmpty(argName))
                    {
                        HandleError($"Invalid method argument in method {methodName}", argContext);
                        continue;
                    }
                    
                    method.Parameters.Add(new CDParameter() { Name = argName, Type = argType });
                }
            }
            
            _currentClass.AddMethod(method);
            return method;
        }
        
        public override object VisitAttribute(UMLDiagramParser.AttributeContext context)
        {
            if (_currentClass == null)
            {
                HandleError("Attribute defined outside of class context", context);
                return null;
            }
            
            string attributeName = Visit(context.attributeName()) as string;
            string attributeType = Visit(context.type()) as string;
            
            if (string.IsNullOrEmpty(attributeName))
            {
                HandleError("Invalid attribute name", context);
                return null;
            }
        
            CDAttribute attribute = new CDAttribute(attributeName, attributeType);
            
            _currentClass.AddAttribute(attribute);
            return attribute;
        }

        public override object VisitMethodName(UMLDiagramParser.MethodNameContext context)
        {
            return context.NAME().GetText();
        }
        
        public override object VisitReturnType(UMLDiagramParser.ReturnTypeContext context)
        {
            return context.NAME().GetText();
        }
        
        public override object VisitType(UMLDiagramParser.TypeContext context)
        {
            return context.NAME().GetText();
        }
        
        public override object VisitArgumentName(UMLDiagramParser.ArgumentNameContext context)
        {
            return context.NAME().GetText();
        }
        
        public override object VisitAttributeName(UMLDiagramParser.AttributeNameContext context)
        {
            return context.NAME().GetText();
        }

        public override object VisitRelation(UMLDiagramParser.RelationContext context)
        {
            string fromClassName = Visit(context.className(0)) as string;
            string toClassName = Visit(context.className(1)) as string;

            if (string.IsNullOrEmpty(fromClassName) || string.IsNullOrEmpty(toClassName))
            {
                HandleError("Invalid class name in relation", context);
                return null;
            }
            
            if (!_classDiagramManager.cdClassPool.ClassExists(fromClassName) || 
                !_classDiagramManager.cdClassPool.ClassExists(toClassName))
            {
                HandleError($"Relation references non-existent class: {fromClassName} or {toClassName}", context);
                return null;
            }
            
            string relationType = context.relationType().GetText();
            
            CDRelationship relationship = _classDiagramManager.cdRelationshipPool.SpawnRelationship(fromClassName, toClassName);
            
            if (relationType == "<|--" || relationType == "<|..")
            {
                // Set inheritance relationship - toClass is the superclass of fromClass
                CDClass fromClass = _classDiagramManager.cdClassPool.getClassByName(fromClassName);
                CDClass toClass = _classDiagramManager.cdClassPool.getClassByName(toClassName);
                
                if (fromClass != null && toClass != null)
                {
                    fromClass.SuperClass = toClass;
                }
            }
            
            return relationship;
        }

        public override object VisitClassName(UMLDiagramParser.ClassNameContext context)
        {
            return context.NAME().GetText();
        }
    }
}