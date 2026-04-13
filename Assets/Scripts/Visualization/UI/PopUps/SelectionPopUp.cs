using TMPro;
using System;
using UnityEngine;

namespace Visualization.UI.PopUps
{
    public class SelectionPopUp : AbstractPopUp
    {
        private const string CreateModeConfirmText = "Choose Nodes";
        private const string EditModeConfirmText = "Accept";

        public TMP_Dropdown dropdown;
        private TMP_Text _confirmButtonText;
        private bool _isRelationEditMode;

        private void Awake()
        {
            Transform confirmButtonLabel = transform.Find("SelectionPConfirm/Text (TMP)");
            if (confirmButtonLabel != null)
            {
                _confirmButtonText = confirmButtonLabel.GetComponent<TMP_Text>();
            }
        }

        public override void ActivateCreation()
        {
            base.ActivateCreation();
            UpdateConfirmButtonLabel();
        }

        public override void Confirmation()
        {
            string selectedOption = dropdown.options[dropdown.value].text;
            if (_isRelationEditMode)
            {
                UIEditorManager.Instance.TryApplyRelationTypeEdit(selectedOption);
                dropdown.SetValueWithoutNotify(0);
                Deactivate();
                return;
            }

            UIEditorManager.Instance.StartSelection(selectedOption);
            dropdown.SetValueWithoutNotify(0);
            Deactivate();
            UIEditorManager.SetDiagramButtonsActive(false);
            UIEditorManager.Instance.ShowSelectionPanel(true);
        }

        public override void Deactivate()
        {
            UIEditorManager.Instance.CancelPendingRelationTypeEdit();
            _isRelationEditMode = false;
            UpdateConfirmButtonLabel();
            base.Deactivate();
        }

        public void SetRelationEditMode(bool isEditMode)
        {
            _isRelationEditMode = isEditMode;
            UpdateConfirmButtonLabel();
        }

        public void SelectRelationOption(string relationType, string relationDirection)
        {
            if (dropdown == null || dropdown.options == null || dropdown.options.Count == 0)
            {
                return;
            }

            string expectedOption = BuildOptionName(relationType, relationDirection);
            int optionIndex = dropdown.options.FindIndex(option =>
                string.Equals(option.text, expectedOption, StringComparison.OrdinalIgnoreCase));

            dropdown.SetValueWithoutNotify(optionIndex >= 0 ? optionIndex : 0);
        }

        private static string BuildOptionName(string relationType, string relationDirection)
        {
            if (string.IsNullOrWhiteSpace(relationType))
            {
                return "Association";
            }

            if (string.Equals(relationType, "Association", StringComparison.OrdinalIgnoreCase) &&
                string.Equals(relationDirection, "none", StringComparison.OrdinalIgnoreCase))
            {
                return "Bi-Directional Association";
            }

            if (string.Equals(relationType, "Association", StringComparison.OrdinalIgnoreCase))
            {
                return "Association";
            }

            return relationType;
        }

        private void UpdateConfirmButtonLabel()
        {
            if (_confirmButtonText == null)
            {
                return;
            }

            _confirmButtonText.text = _isRelationEditMode
                ? EditModeConfirmText
                : CreateModeConfirmText;
        }
    }
}
