using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using Visualization.ClassDiagram;
using Visualization.ClassDiagram.ClassComponents;
using Visualization.UI.ClassComponentsManagers;
using EditorChangesHistory;


namespace Visualization.UI.PopUps
{
    public class AddMethodPopUp : AbstractMethodPopUp
    {
        private const string ErrorMethodNameExists = "Method with the same name already exists";

        private new void Awake()
        {
            base.Awake();
        }

        public override void ActivateCreation()
        {
            base.ActivateCreation();
            UIEditorManager.Instance.ParameterPopUpCallee = "Add";
        }

        public override void Confirmation()
        {
            if (inp.text == "")
            {
                DisplayError(ErrorEmptyName);
                return;
            }

            string _returnType = GetType();
            if (_returnType == null)
            {
                return;
            }

            var newMethod = new Method
            {
                Name = inp.text,
                ReturnValue = _returnType,
                arguments = _parameters.Select(param => param.name).ToList()
            };

            if (UIEditorManager.Instance.isNetworkDisabledOrIsServer())
            {
                if (DiagramPool.Instance.ClassDiagram.FindMethodByName(className.text, newMethod.Name) != null)
                {
                    DisplayError(ErrorMethodNameExists);
                    return;
                }
                newMethod.Id = Guid.NewGuid().ToString();
            }
            else
            {
                var classNetworkId = findClassClient(className.text);
                if (classNetworkId == 0)
                {
                    DisplayError(ErrorEmptyName);
                    return;
                }

                if (findMethodClient(classNetworkId, newMethod.Name) != null)
                {
                    DisplayError(ErrorMethodNameExists);
                    return;
                }
            }

            UIEditorManager.Instance.mainEditor.AddMethod(className.text, newMethod);
            
            string serializedData = DiagramChangeSerializer.SerializeAddMethod(className.text, newMethod);
            DiagramChangeEvent changeEvent = new DiagramChangeEvent(ChangeType.AddMethod, serializedData);
            DiagramChangeTracker.Instance.TrackChange(changeEvent);
            
            Deactivate();
        }

        public override void Deactivate()
        {
            base.Deactivate();
            _parameters = new ();
            parameterContent.DetachChildren();
        }
    }
}
