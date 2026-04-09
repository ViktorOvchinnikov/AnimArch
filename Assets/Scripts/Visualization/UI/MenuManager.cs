using System;
using System.Collections.Generic;
using System.Linq;
using Assets.Scripts.Util;
using OALProgramControl;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using Visualisation.Animation;
using Visualization.Animation;
using Visualization.ClassDiagram;
using Visualization.ClassDiagram.ClassComponents;
using Visualization.ClassDiagram.ComponentsInDiagram;
using UnityEngine.Localization.Settings;
using AnimArch.Extensions;
using UnityEditor;
using Visualization.ClassDiagram.Editors;
using System.Text.RegularExpressions;

namespace Visualization.UI
{
    public class MenuManager : Singleton<MenuManager>
    {
        FileLoader fileLoader;

        //UI Panels
        [SerializeField] private GameObject animationScreen;
        [SerializeField] private GameObject mainScreen;
        [SerializeField] private Button saveBtn;
        [SerializeField] private TMP_Text diagramPathLabel;
        [SerializeField] private Button diagramRemoveBtn;
        [SerializeField] private TMP_Text animationLabel;
        [SerializeField] private Button animationRemoveBtn;
        [SerializeField] private TMP_InputField scriptCode;
        [SerializeField] private GameObject PanelColors;
        [SerializeField] private GameObject PanelInteractiveIntro;
        [SerializeField] private GameObject PanelInteractive;
        [SerializeField] private GameObject PanelMethod;
        [SerializeField] public Button generatePythonBtn;
        public bool isCreating = false;
        [SerializeField] private List<GameObject> methodButtons;
        [SerializeField] private TMP_Text ClassNameTxt;
        [SerializeField] private GameObject InteractiveText; // Modify to have static text - Click class in the diagram to start editing (InteractiveText.GetComponent<DotsAnimation>().currentText)
        [SerializeField] private GameObject PanelInteractiveShow; // To be deleleted
        [SerializeField] private TMP_Text classFromTxt; // To be deleleted
        [SerializeField] private TMP_Text classToTxt; // To be deleleted
        [SerializeField] private TMP_Text methodFromTxt; // To be deleleted
        [SerializeField] private TMP_Text methodToTxt; // To be deleleted
        [SerializeField] private GameObject PanelInteractiveCompleted;
        [SerializeField] private Slider speedSlider;
        [SerializeField] private TMP_Text speedLabel;
        private string interactiveSource = "source";
        private string sourceClassName = "";
        [SerializeField] public GameObject panelAnimationPlay;
        [SerializeField] public GameObject panelStepMode;
        [SerializeField] public GameObject panelPlayMode;
        [SerializeField] private TMP_InputField sepInput;
        [SerializeField] private TMP_Text classTxt;
        [SerializeField] private TMP_Text methodTxt;
        [SerializeField] private Toggle hideRelToggle;
        [SerializeField] private Toggle fillEdgeToggle;
        [SerializeField] public GameObject PanelChooseAnimationStartMethod;
        [SerializeField] public GameObject PanelSourceCodeAnimation;
        [SerializeField] public GameObject ShowErrorBtn;
        [SerializeField] public GameObject ErrorPanel;
        [SerializeField] public GameObject NotSelectedToAnimatePanel;
        [SerializeField] public GameObject notSelectedClassText;
        [SerializeField] public GameObject notSelectedMethodText;
        [SerializeField] public Button HideNotSelectedPanelBtn;
        [SerializeField] public GameObject MethodParameterPrefab;
        [SerializeField] public GameObject EnterParameterPopUp;

        // [SerializeField] public TMP_Text MaskingFileLabel;
        // [SerializeField] public Button RemoveMaskingBtn;
        public Anim createdAnim;
        public bool isPlaying = false;
        public GameObject playIntroTexts;
        public List<AnimMethod> animMethods;
        public bool isSelectingNode;
        public IStrategy Strategy = new StrategyProduction();

        private float SuperSpeedCoefficient
        {
            get
            {
                return Animation.Animation.Instance.SuperSpeed ? 0.0000001f : 1.0f;
            }
        }

        public void SetLanguage(int language)
        {
            LocalizationSettings.SelectedLocale = LocalizationSettings.AvailableLocales.Locales[language];
            // pokus o zmenu jazyka na slovencinu pri animacii select class...
            // var tableReference = LocalizationSettings.StringDatabase.GetTable("StringTable");
            // var tableEntryReference = tableReference.GetEntry("playIntro1Key");
            // InteractiveText.GetComponent<DotsAnimation>().currentText = tableEntryReference.GetLocalizedString();
            // Debug.LogError("SetLanguage()");
            // Debug.LogError(tableEntryReference.GetLocalizedString());
        }

        // executed on pressing show error button

        private MethodPagination SourceCodeMethodPagination;
        private MethodPagination StartingMethodPagination;

        [SerializeField]
        private ScrollableMethodList scrollableMethodListSourceCodeEdit;

        [SerializeField]
        private ScrollableMethodList scrollableMethodListAnimationPlay;
        public void ShowErrorPanel()
        {
            ShowErrorPanel(null);
        }

        public void ShowErrorPanel(EXEExecutionResult executionResult = null) 
        {
            ShowErrorBtn.GetComponent<Button>().interactable = false;
            ErrorPanel.SetActive(true);
            ErrorPanel.GetComponent<ExecutionErrorPanel>().FillPanel(executionResult);
        }

        // executed on pressing X button
        public void HideErrorPanel() 
        {
            ShowErrorBtn.GetComponent<Button>().interactable = true;
            ErrorPanel.SetActive(false);
        }

        // executed after stopping or rerunning animation
        public void HideErrorPanelOnStopButton() 
        {
            ErrorPanel.SetActive(false);
            ShowErrorBtn.GetComponent<Button>().interactable = false;
        }

        public void ShowNotSelectedPanel(String unselectedType)
        {
            NotSelectedToAnimatePanel.SetActive(true);
            HideNotSelectedPanelBtn.interactable = true;

            if(unselectedType.Equals("class"))
            {
                notSelectedClassText.gameObject.SetActive(true);
                notSelectedMethodText.gameObject.SetActive(false);
            }else if(unselectedType.Equals("method"))
            {
                notSelectedClassText.gameObject.SetActive(false);
                notSelectedMethodText.gameObject.SetActive(true);
            }
        }

        public void HideNotSelectedPanel()
        {
            notSelectedClassText.gameObject.SetActive(true);
            notSelectedMethodText.gameObject.SetActive(true);
            NotSelectedToAnimatePanel.SetActive(false);
            HideNotSelectedPanelBtn.interactable = false;

            //Button exitAnimationBtn = GameObject.Find("ButtonExit").GetComponent<Button>();
            //exitAnimationBtn.onClick.Invoke();
        }
        class InteractiveData
        {
            public Subject<string> ClassClickedInClassDiagram;
            public Subject<string> CurrentMethodOwningClass;
            public Subject<string> CurrentMethod;

            public InteractiveData()
            {
                this.ClassClickedInClassDiagram = new Subject<string>(string.Empty);
                this.CurrentMethodOwningClass = new Subject<string>(string.Empty);
                this.CurrentMethod = new Subject<string>(string.Empty);
            }

            public void Clear()
            {
                this.ClassClickedInClassDiagram.SetValue(string.Empty);
                this.CurrentMethodOwningClass.SetValue(string.Empty);
                this.CurrentMethod.SetValue(string.Empty);
            }
        }


        InteractiveData interactiveData = new InteractiveData();

        private void Awake()
        {
            fileLoader = GameObject.Find("FileManager").GetComponent<FileLoader>();

            this.interactiveData = new InteractiveData();
            this.interactiveData.ClassClickedInClassDiagram.Register((string value) => { ClassNameTxt.text = value; });
            this.interactiveData.CurrentMethodOwningClass.Register((string value) => { classTxt.text = value; });
            this.interactiveData.CurrentMethod.Register((string value) => { methodTxt.text = value; });
        }

        private void Start()
        {
            UpdateSpeed();
        }

        public void SetDiagramPath(string diagramPath)
        {
            diagramPathLabel.text = diagramPath;
            diagramRemoveBtn.interactable = true;
        }

        public void RemoveDiagram()
        {
            UIEditorManager.Instance.mainEditor.ClearDiagram();
        }
        public void SetSelectedAnimation(string name)
        {
            animationLabel.text = name;
            animationRemoveBtn.interactable = true;
        }

        public void InitializeAnim()
        {
            createdAnim = new Anim("");
            createdAnim.Initialize();
            generatePythonBtn.interactable = true;
        }

        public void StartAnimate()
        {
            scriptCode.text = string.Empty;
            isCreating = true;
            PanelInteractiveIntro.SetActive(true);
            PanelMethod.SetActive(false);
            PanelInteractive.SetActive(true);
            PanelInteractiveCompleted.SetActive(false);
            animationScreen.SetActive(true);
            mainScreen.SetActive(false);
        }

        public void EndAnimate()
        {
            isCreating = false;
            PanelInteractiveIntro.SetActive(false);
            PanelInteractive.SetActive(false);
            animationScreen.SetActive(false);
            saveBtn.interactable = false;
            mainScreen.SetActive(true);
            PanelInteractiveCompleted.SetActive(false);
        }

        public void SelectClass(String name)
        {

            // Save animation code
            SaveCurrentAnimation();

            // Unhighlight
            UnselectMethod();
            UnselectClass();

            // Set and highlight currently selected classs
            interactiveData.ClassClickedInClassDiagram.SetValue(name);
            Animation.Animation.Instance.HighlightClass(interactiveData.ClassClickedInClassDiagram.GetValue(), true);

            // Setup method buttons
            Class selectedClass = DiagramPool.Instance.ClassDiagram.FindClassByName(name).ParsedClass;
            PanelInteractiveIntro.SetActive(false);
            
            PanelMethod.SetActive(true);
            scrollableMethodListSourceCodeEdit.CurrentState = EditModeState.Instance;
            scrollableMethodListSourceCodeEdit.FillItems(selectedClass.Methods.Select(method => method.Name).ToList());

            PanelInteractiveIntro.SetActive(false);
            PanelMethod.SetActive(true);
        }

        private void UnselectClass()
        {
            if (!string.IsNullOrEmpty(interactiveData.ClassClickedInClassDiagram.GetValue()))
            {
                Animation.Animation.Instance.HighlightClass(interactiveData.ClassClickedInClassDiagram.GetValue(), false);
            }

            if (!string.IsNullOrEmpty(interactiveData.CurrentMethodOwningClass.GetValue()))
            {
                Animation.Animation.Instance.HighlightClass(interactiveData.CurrentMethodOwningClass.GetValue(), false);
            }
        }

        public void SelectMethod(string methodName)
        {
            interactiveData.CurrentMethodOwningClass.SetValue(interactiveData.ClassClickedInClassDiagram.GetValue());
            interactiveData.ClassClickedInClassDiagram.SetValue(string.Empty);
            interactiveData.CurrentMethod.SetValue(methodName);

            Animation.Animation.Instance.HighlightMethod
            (
                interactiveData.CurrentMethodOwningClass.GetValue(),
                interactiveData.CurrentMethod.GetValue(),
                true
            );
            sepInput.interactable = true;
            sepInput.text
                = createdAnim.GetMethodBody
                (
                    interactiveData.CurrentMethodOwningClass.GetValue(),
                    interactiveData.CurrentMethod.GetValue()
                );

            PanelInteractiveIntro.SetActive(true);
            PanelMethod.SetActive(false);
        }

        private void UnselectMethod()
        {
            if (!string.IsNullOrEmpty(interactiveData.CurrentMethod.GetValue()))
            {
                Animation.Animation.Instance.HighlightMethod(interactiveData.CurrentMethodOwningClass.GetValue(), interactiveData.CurrentMethod.GetValue(), false);
            }
        }

        //Save animation to file and memory
        private void SaveCurrentAnimation()
        {
            if
            (
                !string.IsNullOrEmpty(interactiveData.CurrentMethodOwningClass.GetValue())
                && !string.IsNullOrEmpty(interactiveData.CurrentMethod.GetValue())
            )
            {
                createdAnim.SetMethodCode
                (
                    interactiveData.CurrentMethodOwningClass.GetValue(),
                    interactiveData.CurrentMethod.GetValue(),
                    sepInput.text
                );
            }
        }
        public void SaveAnimation()
        {
            SaveCurrentAnimation();

            scriptCode.gameObject.SetActive(true);

            scriptCode.GetComponent<CodeHighlighter>().RemoveColors();
            scriptCode.gameObject.SetActive(false);
            fileLoader.SaveAnimation(createdAnim);
            EndAnimate();
        }

        public void OpenAnimation()
        {
            StartAnimate();
            createdAnim = AnimationData.Instance.selectedAnim;
        }

        public void ActivatePanelColors(bool show)
        {
            PanelColors.SetActive(show);
        }

        public void UpdateSpeed()
        {
            if (speedSlider && speedLabel)
            {
                AnimationData.Instance.AnimSpeed = speedSlider.value * SuperSpeedCoefficient;
                speedLabel.text = speedSlider.value.ToString() + "s";
            }
        }

        public void PlayAnimation()
        {
            Debug.Assert(!AnimationData.Instance.selectedAnim.AnimationName.Equals(""));

            TooltipManager.Instance.HideTooltip();  
            PanelChooseAnimationStartMethod.SetActive(true);
            PanelSourceCodeAnimation.SetActive(false);

            isPlaying = true;
            panelAnimationPlay.SetActive(true);
            mainScreen.SetActive(false);

            playIntroTexts.SetActive(true);
            if (Animation.Animation.Instance.standardPlayMode)
            {
                panelStepMode.SetActive(false);
                panelPlayMode.SetActive(true);
            }
            else
            {
                panelPlayMode.SetActive(false);
                panelStepMode.SetActive(true);
            }
        }

        public void ResetInteractiveSelection()
        {
            if (!string.IsNullOrEmpty(interactiveData.CurrentMethodOwningClass.GetValue()))
            {
                Animation.Animation.Instance.HighlightClass(interactiveData.CurrentMethodOwningClass.GetValue(), false);
            }

            if (interactiveData.CurrentMethod.GetValue() != null)
            {
                Animation.Animation.Instance.HighlightMethod
                (
                    interactiveData.CurrentMethodOwningClass.GetValue(),
                    interactiveData.CurrentMethod.GetValue(),
                    false
                );
            }

            interactiveData.Clear();
            PanelInteractiveIntro.SetActive(true);
            PanelMethod.SetActive(false);
        }

        public void ChangeMode()
        {
            Animation.Animation.Instance.UnhighlightAll();
            Animation.Animation.Instance.isPaused = false;
            if (Animation.Animation.Instance.standardPlayMode)
            {
                Animation.Animation.Instance.standardPlayMode = false;
                panelPlayMode.SetActive(false);
                panelStepMode.SetActive(true);
            }
            else
            {
                Animation.Animation.Instance.standardPlayMode = true;
                panelStepMode.SetActive(false);
                panelPlayMode.SetActive(true);
            }
        }

        public void SelectPlayClass(string name)
        {
            DiagramPool.Instance.ObjectDiagram.ResetDiagram();
            DiagramPool.Instance.ObjectDiagram.LoadDiagram();

            DiagramPool.Instance.ActivityDiagram.ResetDiagram();
            DiagramPool.Instance.ActivityDiagram.LoadDiagram();

            Animation.Animation.Instance.UnhighlightAll();
            Animation.Animation.Instance.HighlightClass(name, true);

            playIntroTexts.SetActive(false);
            Animation.Animation.Instance.startClassName = name;

            Class selectedClass = DiagramPool.Instance.ClassDiagram.FindClassByName(name).ParsedClass;
            animMethods = AnimationData.Instance.selectedAnim.GetMethodsByClassName(name);
            scrollableMethodListAnimationPlay.CurrentState = PlayModeState.Instance;
            if(animMethods != null)
            {
                scrollableMethodListAnimationPlay.FillItems(animMethods.Select(method => method.Name).ToList());
            }
            else
            {
                scrollableMethodListAnimationPlay.ClearItems();
            }
        }

        // public void SelectPlayMethod(string startMethodName)
        // {
        //     string startClassName = Animation.Animation.Instance.startClassName;

        //     Animation.Animation.Instance.startMethodName = startMethodName;

        //     playIntroTexts.SetActive(true);
        //     Animation.Animation.Instance.HighlightClass(startClassName, false);
        // }
        private void ApplyPlayMethodSelection(string startMethodName)
        {
            Animation.Animation a = Animation.Animation.Instance;
            string startClassName = a.startClassName;
            a.startMethodName = startMethodName;

            playIntroTexts.SetActive(true);
            Debug.Log("Selected class: " + startClassName + " Selected Method: " + a.startMethodName);
            a.HighlightClass(startClassName, false);
        }

        private EXEValueBase ParseParameterValue(string value, string type)
        {
            if (!EXETypes.IsPrimitive(EXETypes.ConvertEATypeName(type)))
            {
                return EXETypes.DefaultValue(type, Animation.Animation.Instance.CurrentProgramInstance.ExecutionSpace);
            }
            try {
                EXEValueBase result = null;
                if ("string".Equals(EXETypes.ConvertEATypeName(type)))
                {
                    result = new EXEValueString("\"" + value + "\"");
                }
                else
                {
                    result = EXETypes.DeterminePrimitiveValue(value);
                    if (result == null || !EXETypes.ConvertEATypeName(type).Equals(result.TypeName))
                    {
                        return null;
                    }
                }
                return result;
            } catch (ArgumentException e) {
                Debug.LogError(e.ToString());
                return null;
            } catch (FormatException e) {
                Debug.LogError(e.ToString());
                return null;
            }
        }

        private EXEValueBase ParseUserInput(string value, string type)
        {
            if (EXETypes.IsValidArrayType(type))
            {
                List<EXEValueBase> exeList = new List<EXEValueBase>();
                string listType = type.Substring(0, type.Length-2);
                if (value.Length > 0)
                {
                    foreach (string listItem in value.Split(','))
                    {
                        EXEValueBase newValue = ParseParameterValue(listItem.Trim(), listType);
                        if (newValue == null)
                        {
                            return null;
                        }
                        exeList.Add(newValue);
                    }
                }
                return new EXEValueArray(type, exeList);
            }

            return ParseParameterValue(value, type);
        }

        private Transform parameterHolder
        { 
            get 
            {
                MediatorEnterParameterPopUp mediator = EnterParameterPopUp.GetComponent<MediatorEnterParameterPopUp>();
                return mediator.Content.transform;
            } 
        }

        public void SaveParametersForInitialMethod()
        {
            MediatorEnterParameterPopUp mediator = EnterParameterPopUp.GetComponent<MediatorEnterParameterPopUp>();
            Animation.Animation a = Animation.Animation.Instance;
            string startMethodName = mediator.GetMethodLabelText();
            bool inputCorrect = true;

            List<EXEVariable> newParameters = new List<EXEVariable>();

            foreach (Transform parameter in parameterHolder)
            {
                string parameterName  = parameter.gameObject.GetComponent<MethodParameterManager>().ParameterName.GetComponent<TMP_Text>().text;
                string parameterValue = parameter.gameObject.GetComponent<MethodParameterManager>().ParameterValueText.GetComponent<TMP_Text>().text;
                if (Regex.Replace(parameterValue, @"[^\w:/ \.,]", string.Empty).Length == 0)
                {
                    parameterValue = parameter.gameObject.GetComponent<MethodParameterManager>().PlaceholderText.GetComponent<TMP_Text>().text;
                }
                string parameterType = parameter.gameObject.GetComponent<MethodParameterManager>().ParameterType.GetComponent<TMP_Text>().text;
                GameObject errorLabel = parameter.gameObject.GetComponent<MethodParameterManager>().ErrorLabel.gameObject;
                errorLabel.SetActive(false);

                EXEValueBase parameterExeValue = ParseUserInput(Regex.Replace(parameterValue, @"[^\w:/ \.,]", string.Empty), parameterType);
                if (parameterExeValue == null)
                {
                    parameter.gameObject.GetComponent<MethodParameterManager>().SetErrorLabelText(parameterType);
                    errorLabel.SetActive(true);
                    inputCorrect = false;
                }
                else
                {
                    newParameters.Add(new EXEVariable(parameterName, parameterExeValue));
                }
            }

            if (inputCorrect)
            {
                a.startMethodParameters[startMethodName] = newParameters;
                mediator.SetActiveEnterParameterPopUp(false);
                ApplyPlayMethodSelection(startMethodName);
            }
        }

        public void SelectPlayMethod(string startMethodName)
        {
            Animation.Animation a = Animation.Animation.Instance;
            string startClassName = a.startClassName;
            List<CDParameter> parameters = a.CurrentProgramInstance.ExecutionSpace.getClassByName(startClassName).GetMethodByName(startMethodName).Parameters;

            if (parameters.Count == 0)
            {
                ApplyPlayMethodSelection(startMethodName);
                return;
            }

            MediatorEnterParameterPopUp mediator = EnterParameterPopUp.GetComponent<MediatorEnterParameterPopUp>();

            foreach (Transform child in mediator.Content.transform)
            {
                Destroy(child.gameObject);
            }

            for (int i = 0; i < parameters.Count; i++)
            {
                CDParameter parameter = parameters[i];
                GameObject parameterGo = Instantiate(MethodParameterPrefab, mediator.Content.transform);
                parameterGo.GetComponent<MethodParameterManager>().ParameterName.GetComponent<TMP_Text>().text = parameter.Name;
                parameterGo.GetComponent<MethodParameterManager>().ParameterType.GetComponent<TMP_Text>().text = parameter.Type;
                string parameterValue;

                if (!EXETypes.IsPrimitive(EXETypes.ConvertEATypeName(parameter.Type.Replace("[]", ""))))
                {
                    parameterGo.GetComponent<MethodParameterManager>().ParameterValue.GetComponent<TMP_InputField>().interactable = false;
                    parameterGo.GetComponent<MethodParameterManager>().WarningLabel.gameObject.SetActive(true);
                    parameterValue = " ";
                }
                else
                {
                    VisitorCommandToString visitor = new VisitorCommandToString();

                    if (a.startMethodParameters.ContainsKey(startMethodName))
                    {
                        a.startMethodParameters[startMethodName][i].Value.Accept(visitor);
                    }
                    else
                    {
                        EXETypes.DefaultValue(parameter.Type, a.CurrentProgramInstance.ExecutionSpace).Accept(visitor);
                    }
                    
                    parameterValue = visitor.GetCommandString();
                }

                parameterGo.GetComponent<MethodParameterManager>().SetPlaceholderText(parameterValue);
            }

            mediator.SetMethodLabelText(startMethodName);
            mediator.SetActiveEnterParameterPopUp(true);
        }

        public void UnshowAnimation()
        {
            Animation.Animation.Instance.UnhighlightAll();
            scrollableMethodListAnimationPlay.ClearItems();
        }

        public void EndPlay()
        {
            isPlaying = false;

            Animation.Animation.Instance.startClassName = "";
            Animation.Animation.Instance.startMethodName = "";
        }

        public void HideGraphRelations()
        {
            if (hideRelToggle.isOn)
            {
                foreach (var interGraphRelation in DiagramPool.Instance.RelationsClassToObject)
                {
                    interGraphRelation.Show();
                }
            }
            else
            {
                foreach (var interGraphRelation in DiagramPool.Instance.RelationsClassToObject)
                {
                    interGraphRelation.Hide();
                }
            }
        }
        public void ChangeEdgeHighlighting()
        {
            HighlightEdgeState state = HighlightImmediateState.GetInstance(); 
            if (fillEdgeToggle.isOn)
            {
               state = HighlightFillState.GetInstance(); 
            }

            Animation.Animation.Instance.SetEdgeHighlighter(state);
        }

        public static void SetAnimationButtonsActive(bool active)
        {
            SetButtonInteractable("AnimationPanel/Buttons/Edit", active);
            SetButtonInteractable("AnimationPanel/Buttons/Play", active);
            SetButtonInteractable("GenerateToPythonButton", active);
            // generatePythonBtn.interactable = true; // TODO co je lepsie? takto by sme museli zmenit funciu zo static
        }

        private static void SetButtonInteractable(string path, bool interactable)
        {
            GameObject buttonObject = GameObject.Find(path);
            if (buttonObject == null)
            {
                return;
            }

            Button button = buttonObject.GetComponentInChildren<Button>(includeInactive: true);
            if (button != null)
            {
                button.interactable = interactable;
            }
        }
        public void RefreshSourceCodePanel(EXEScopeMethod currentMethodScope)
        {
            PanelChooseAnimationStartMethod.SetActive(false);
            PanelSourceCodeAnimation.SetActive(true);

            PanelSourceCodeAnimation
                .GetComponent<PanelSourceCodeAnimation>()
                .SetMethodLabelText(currentMethodScope.MethodDefinition.OwningClass.Name, currentMethodScope.MethodDefinition.Name);

            VisitorCommandToString visitor = new VisitorCommandToString();
            visitor.ActivateHighlighting();
            currentMethodScope.Accept(visitor);
            string sourceCode = visitor.GetCommandString();
            PanelSourceCodeAnimation.GetComponent<PanelSourceCodeAnimation>().SetSourceCodeText(sourceCode);
        }
    }
}
