using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using EditorChangesHistory;
using Parsers;
using UnityEngine;
using UnityEngine.UI;
using Visualization.Animation;
using Visualization.ClassDiagram;
using Visualization.UI;

namespace Visualization.ABTesting
{
    public class ABTestManager : MonoBehaviour
    {
        private const string LoginOverlayName = "ABTestLoginOverlay";
        private const string CompletionOverlayName = "ABTestCompletionOverlay";
        private const string MidpointOverlayName = "ABTestMidpointOverlay";
        private const string CompleteTaskConfirmOverlayName = "ABTestCompleteTaskConfirmOverlay";
        private const string TaskNumberIndicatorName = "ABTestTaskNumberIndicator";
        private const string LoginTitle = "A/B Test Login";
        private const string LoginDescription = "Enter your assigned pseudonym.";
        private const string UnknownPseudonymError = "Pseudonym not found. Please check your sheet.";
        private const int MidpointTasksCompleted = 2;
        private const int MidpointTotalTasks = 4;

        private static ABTestManager _instance;
        private static bool _isBootstrapped;

        private readonly Dictionary<string, string> _pseudonymToGroup = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { "a", "group_a" },
            { "b", "group_b" }
        };

        private readonly Dictionary<string, GroupConfig> _groupConfigs = new Dictionary<string, GroupConfig>();
        private string _configInitializationError;

        private IClassDiagramBuilder _classDiagramBuilder;
        private SessionState _session;

        private GameObject _loginOverlay;
        private InputField _pseudonymInput;
        private Text _errorText;

        private GameObject _completionOverlay;
        private Text _completionText;
        private Button _completionExitButton;
        private GameObject _midpointOverlay;
        private GameObject _completeTaskConfirmOverlay;
        private GameObject _taskNumberIndicator;
        private Text _taskNumberText;
        private bool _isWaitingForMidpointConfirmation;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        private static void Bootstrap()
        {
            if (_isBootstrapped)
            {
                return;
            }

            _isBootstrapped = true;

            if (FindObjectOfType<ABTestManager>() != null)
            {
                return;
            }

            GameObject bootstrapObject = new GameObject(nameof(ABTestManager));
            DontDestroyOnLoad(bootstrapObject);
            bootstrapObject.AddComponent<ABTestManager>();
        }

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);

            InitializeConfig();
        }

        private void Start()
        {
            _classDiagramBuilder = ClassDiagramBuilderFactory.Create();
            StartCoroutine(ShowLoginModalWhenCanvasReady());
        }

        public static bool TryCompleteCurrentTask(out string message)
        {
            if (_instance == null)
            {
                message = "A/B test manager is not initialized.";
                return false;
            }

            return _instance.TryCompleteCurrentTaskInternal(out message);
        }

        public static bool TryOpenCompleteTaskConfirmation(out string message)
        {
            if (_instance == null)
            {
                message = "A/B test manager is not initialized.";
                return false;
            }

            return _instance.TryOpenCompleteTaskConfirmationInternal(out message);
        }

        private IEnumerator ShowLoginModalWhenCanvasReady()
        {
            while (FindMainCanvas() == null)
            {
                yield return null;
            }

            ShowLoginModal();
        }

        private void InitializeConfig()
        {
            _groupConfigs.Clear();
            _configInitializationError = null;

            try
            {
                Dictionary<string, string> templates = LoadTaskTemplatesFromResources();

                TaskConfig taskE0 = new TaskConfig("E0", templates["E0"], suggestionsEnabled: true);
                TaskConfig taskE1 = new TaskConfig("E1", templates["E1"], suggestionsEnabled: false);
                TaskConfig taskH0 = new TaskConfig("H0", templates["H0"], suggestionsEnabled: true);
                TaskConfig taskH1 = new TaskConfig("H1", templates["H1"], suggestionsEnabled: false);

                _groupConfigs["group_a"] = new GroupConfig(
                    "Group A",
                    new List<TaskConfig> { taskE0, taskH0, taskE1, taskH1 }
                );

                _groupConfigs["group_b"] = new GroupConfig(
                    "Group B",
                    new List<TaskConfig> { taskE1, taskH1, taskE0, taskH0 }
                );
            }
            catch (Exception ex)
            {
                _configInitializationError = $"A/B tasks config initialization failed: {ex.Message}";
                Debug.LogError(_configInitializationError);
            }
        }

        private static Dictionary<string, string> LoadTaskTemplatesFromResources()
        {
            return new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
            {
                ["E0"] = LoadTaskTemplateText("AbTest/E0"),
                ["E1"] = LoadTaskTemplateText("AbTest/E1"),
                ["H0"] = LoadTaskTemplateText("AbTest/H0"),
                ["H1"] = LoadTaskTemplateText("AbTest/H1")
            };
        }

        private static string LoadTaskTemplateText(string resourcePath)
        {
            TextAsset templateAsset = Resources.Load<TextAsset>(resourcePath);
            if (templateAsset == null)
            {
                throw new InvalidOperationException($"Task template is missing in Resources: '{resourcePath}'.");
            }

            if (string.IsNullOrWhiteSpace(templateAsset.text))
            {
                throw new InvalidOperationException($"Task template is empty: '{resourcePath}'.");
            }

            return templateAsset.text;
        }

        private bool TryCompleteCurrentTaskInternal(out string message)
        {
            if (_session == null)
            {
                message = "Session is not started. Enter pseudonym first.";
                ShowLoginModal();
                HideTaskNumberIndicator();
                return false;
            }

            if (_session.CurrentTaskIndex >= _session.Group.Tasks.Count)
            {
                message = "All tasks are already completed.";
                HideTaskNumberIndicator();
                ShowCompletionOverlay(BuildFinalCompletionMessage(), showExitButton: true);
                return false;
            }

            TaskConfig completedTask = _session.Group.Tasks[_session.CurrentTaskIndex];
            LogCompleteTaskEvent(completedTask, _session.CurrentTaskIndex);
            SaveCompletedTaskSnapshot(completedTask);
            ResetStateForTaskTransition();

            _session.CurrentTaskIndex++;

            if (_session.CurrentTaskIndex >= _session.Group.Tasks.Count)
            {
                message = "All tasks are completed.";
                HideTaskNumberIndicator();
                ShowCompletionOverlay(BuildFinalCompletionMessage(), showExitButton: true);
                return true;
            }

            if (ShouldPauseForMidpoint())
            {
                ShowMidpointOverlay();
                message = "Waiting for new tasks from curators.";
                return true;
            }

            if (!TryLoadCurrentTask(out string loadError))
            {
                message = $"Failed to load next task: {loadError}";
                ShowCompletionOverlay(message, showExitButton: false);
                return false;
            }

            message = $"Task {_session.CurrentTaskIndex + 1} loaded.";
            return true;
        }

        private bool TryOpenCompleteTaskConfirmationInternal(out string message)
        {
            if (_session == null)
            {
                message = "Session is not started. Enter pseudonym first.";
                ShowLoginModal();
                HideTaskNumberIndicator();
                return false;
            }

            if (_session.CurrentTaskIndex >= _session.Group.Tasks.Count)
            {
                message = "All tasks are already completed.";
                HideTaskNumberIndicator();
                ShowCompletionOverlay(BuildFinalCompletionMessage(), showExitButton: true);
                return false;
            }

            if (!ShowCompleteTaskConfirmOverlay())
            {
                message = "Cannot show complete task confirmation modal.";
                return false;
            }

            message = "Complete task confirmation is shown.";
            return true;
        }

        private bool ShouldPauseForMidpoint()
        {
            return _session != null &&
                   _session.Group != null &&
                   _session.Group.Tasks != null &&
                   _session.Group.Tasks.Count == MidpointTotalTasks &&
                   _session.CurrentTaskIndex == MidpointTasksCompleted;
        }

        private string BuildFinalCompletionMessage()
        {
            string pseudonym = _session != null && !string.IsNullOrWhiteSpace(_session.Pseudonym)
                ? _session.Pseudonym
                : "unknown";
            string logsDirectory = GetSessionOutputDirectory();

            return
                $"All tasks are completed.\n" +
                $"Please send logs and task files to the curator.\n\n" +
                $"Reminder: sign your survey sheet with your pseudonym.\n" +
                $"Your pseudonym: {pseudonym}\n" +
                $"Logs directory: {logsDirectory}";
        }

        private bool TryLoadCurrentTask(out string error)
        {
            error = string.Empty;

            if (_session == null)
            {
                error = "Session is missing.";
                return false;
            }

            if (_session.CurrentTaskIndex < 0 || _session.CurrentTaskIndex >= _session.Group.Tasks.Count)
            {
                error = "Task index is out of range.";
                return false;
            }

            TaskConfig task = _session.Group.Tasks[_session.CurrentTaskIndex];

            try
            {
                PrepareUiForTaskReload();
                string taskPath = WriteTaskJsonToRuntimePath(task);

                if (Animation.Animation.Instance != null && Animation.Animation.Instance.CurrentProgramInstance != null)
                {
                    Animation.Animation.Instance.CurrentProgramInstance.Reset();
                }

                AnimationData.Instance.SetDiagramPath(taskPath);
                if (MenuManager.Instance != null)
                {
                    MenuManager.Instance.SetDiagramPath(taskPath);
                    MenuManager.Instance.UnshowAnimation();
                }

                _classDiagramBuilder ??= ClassDiagramBuilderFactory.Create();
                _classDiagramBuilder.LoadDiagram();

                OpenCreationMode();
                ApplySuggestionMode(task.SuggestionsEnabled);
                UpdateTaskNumberIndicator();
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to load task '{task.TaskId}': {ex}");
                error = ex.Message;
                return false;
            }

            return true;
        }

        private void ConfigureSessionLogFile()
        {
            if (_session == null)
            {
                return;
            }

            string timestamp = DateTime.Now.ToString("yyyyMMdd_HHmmss");
            string safePseudonym = SanitizeFileName(_session.Pseudonym);
            string safeGroup = SanitizeFileName(_session.GroupKey);
            string fileName = $"ux_events_{timestamp}_{safePseudonym}_{safeGroup}.jsonl";
            string fullPath = Path.Combine(GetSessionOutputDirectory(), fileName);

            UXEventLogger.SetLogFilePath(fullPath);
        }

        private void LogLoginEvent()
        {
            if (_session == null)
            {
                return;
            }

            UXEventLogger.DebugLog("ab_test_login", new
            {
                pseudonym = _session.Pseudonym,
                groupKey = _session.GroupKey,
                groupName = _session.Group != null ? _session.Group.GroupName : null,
                firstTaskSuggestionMode = GetFirstTaskSuggestionMode(),
                logFilePath = UXEventLogger.LogFilePath
            });
        }

        private void LogCompleteTaskEvent(TaskConfig completedTask, int completedTaskIndex)
        {
            if (_session == null)
            {
                return;
            }

            UXEventLogger.DebugLog("ab_test_complete_task", new
            {
                pseudonym = _session.Pseudonym,
                groupKey = _session.GroupKey,
                groupName = _session.Group != null ? _session.Group.GroupName : null,
                firstTaskSuggestionMode = GetFirstTaskSuggestionMode(),
                completedTaskId = completedTask != null ? completedTask.TaskId : null,
                completedTaskIndex,
                completedTaskSuggestionMode = completedTask != null ? completedTask.SuggestionsEnabled : false,
                logFilePath = UXEventLogger.LogFilePath
            });
        }

        private bool GetFirstTaskSuggestionMode()
        {
            return _session != null &&
                   _session.Group != null &&
                   _session.Group.Tasks != null &&
                   _session.Group.Tasks.Count > 0 &&
                   _session.Group.Tasks[0].SuggestionsEnabled;
        }

        private static void ApplySuggestionMode(bool shouldBeEnabled)
        {
            if (SuggestedDiagram.SuggestionsEnabled != shouldBeEnabled)
            {
                SuggestedDiagram.ToggleSuggestions();
            }

            SetSuggestionBulkButtonsInteractable(shouldBeEnabled);
        }

        private static void ResetStateForTaskTransition()
        {
            DiagramChangeTracker.Instance.ClearChanges();
            SuggestedDiagram.ClearSuggestions();

            if (SuggestedDiagram.SuggestionsEnabled)
            {
                SuggestedDiagram.ToggleSuggestions();
            }

            SuggestedDiagram.SuppressTracking = true;
            SetSuggestionBulkButtonsInteractable(false);
        }

        private static void SetSuggestionBulkButtonsInteractable(bool interactable)
        {
            Button[] buttons = FindObjectsOfType<Button>(includeInactive: true);
            if (buttons == null || buttons.Length == 0)
            {
                return;
            }

            foreach (Button button in buttons)
            {
                if (button == null || button.gameObject == null)
                {
                    continue;
                }

                if (!IsSuggestionBulkButton(button.gameObject.name))
                {
                    continue;
                }

                button.interactable = interactable;
            }
        }

        private static bool IsSuggestionBulkButton(string buttonName)
        {
            return buttonName == "SuggestionsAcceptAllButton" ||
                   buttonName == "AcceptAllSuggestionsButton" ||
                   buttonName == "SuggestionsRejectAllButton" ||
                   buttonName == "RejectAllSuggestionsButton";
        }

        private static void OpenCreationMode()
        {
            if (UIEditorManager.Instance != null)
            {
                UIEditorManager.Instance.StartEditing();
            }

            MediatorMainPanel mediatorMainPanel = FindObjectOfType<MediatorMainPanel>();
            if (mediatorMainPanel != null)
            {
                mediatorMainPanel.SetActiveMainPanel(false);
                mediatorMainPanel.SetActiveCreationPanel(true);
            }
        }

        private static void PrepareUiForTaskReload()
        {
            MediatorMainPanel mediatorMainPanel = FindObjectOfType<MediatorMainPanel>();
            if (mediatorMainPanel == null)
            {
                return;
            }

            mediatorMainPanel.SetActiveCreationPanel(false);
            mediatorMainPanel.SetActiveMainPanel(true);
        }

        private string WriteTaskJsonToRuntimePath(TaskConfig task)
        {
            string fileName = $"{task.TaskId}.json";
            string fullPath = Path.Combine(GetSessionOutputDirectory(), fileName);

            File.WriteAllText(fullPath, task.DiagramJson, Encoding.UTF8);
            return fullPath;
        }

        private void SaveCompletedTaskSnapshot(TaskConfig task)
        {
            string outputDirectory = GetSessionOutputDirectory();
            string timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss");
            string safePseudonym = SanitizeFileName(_session.Pseudonym);

            try
            {
                string jsonOutputPath = Path.Combine(outputDirectory, $"ab_completed_{safePseudonym}_{task.TaskId}_{timestamp}.json");
                Parser parser = Parser.GetParser(".json");
                string diagramJson = parser.SaveDiagram();
                File.WriteAllText(jsonOutputPath, diagramJson, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to save completed task JSON snapshot: {ex}");
            }

            try
            {
                string plantUmlOutputPath = Path.Combine(outputDirectory, $"ab_completed_{safePseudonym}_{task.TaskId}_{timestamp}.puml");
                PlantUMLBuilder plantUmlBuilder = new PlantUMLBuilder();
                string plantUmlDiagram = plantUmlBuilder.GetDiagram();

                if (string.IsNullOrWhiteSpace(plantUmlDiagram))
                {
                    throw new InvalidOperationException("PlantUML snapshot is empty.");
                }

                File.WriteAllText(plantUmlOutputPath, plantUmlDiagram, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                Debug.LogError($"Failed to save completed task PlantUML snapshot: {ex}");
            }
        }

        private static string GetExecutableDirectory()
        {
            string currentDirectory = Directory.GetCurrentDirectory();
            if (!string.IsNullOrWhiteSpace(currentDirectory))
            {
                return currentDirectory;
            }

            string executableDirectory = Path.GetDirectoryName(Application.dataPath);
            if (string.IsNullOrWhiteSpace(executableDirectory))
            {
                executableDirectory = Application.persistentDataPath;
            }

            return executableDirectory;
        }

        private string GetSessionOutputDirectory()
        {
            if (_session == null)
            {
                return GetExecutableDirectory();
            }

            if (string.IsNullOrWhiteSpace(_session.OutputDirectory))
            {
                _session.OutputDirectory = BuildSessionOutputDirectory(_session.Pseudonym);
            }

            Directory.CreateDirectory(_session.OutputDirectory);
            return _session.OutputDirectory;
        }

        private static string BuildSessionOutputDirectory(string pseudonym)
        {
            string safePseudonym = SanitizeFileName(pseudonym);
            string directoryName = $"logs_{safePseudonym}";
            string fullPath = Path.GetFullPath(Path.Combine(GetExecutableDirectory(), directoryName));
            Directory.CreateDirectory(fullPath);
            return fullPath;
        }

        private static string SanitizeFileName(string rawName)
        {
            if (string.IsNullOrWhiteSpace(rawName))
            {
                return "unknown";
            }

            char[] invalidChars = Path.GetInvalidFileNameChars();
            var sanitized = new StringBuilder(rawName.Length);

            foreach (char symbol in rawName)
            {
                sanitized.Append(invalidChars.Contains(symbol) ? '_' : symbol);
            }

            return sanitized.ToString();
        }

        private void ShowLoginModal()
        {
            if (_loginOverlay != null)
            {
                _loginOverlay.transform.SetAsLastSibling();
                _loginOverlay.SetActive(true);
                _pseudonymInput.Select();
                _pseudonymInput.ActivateInputField();
                return;
            }

            Canvas canvas = FindMainCanvas();
            if (canvas == null)
            {
                Debug.LogError("Main Canvas is not found. Cannot show A/B login modal.");
                return;
            }

            Font font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            _loginOverlay = CreatePanelRoot(LoginOverlayName, canvas.transform, new Color(0f, 0f, 0f, 0.7f));
            _loginOverlay.transform.SetAsLastSibling();
            GameObject modal = CreateCenteredPanel("ABLoginModal", _loginOverlay.transform, new Vector2(620f, 320f));

            CreateLabel(modal.transform, "Title", LoginTitle, font, 30, new Color(0.1f, 0.1f, 0.1f), new Vector2(560f, 60f), new Vector2(0f, 105f), TextAnchor.MiddleCenter);
            CreateLabel(modal.transform, "Description", LoginDescription, font, 18, new Color(0.2f, 0.2f, 0.2f), new Vector2(560f, 44f), new Vector2(0f, 55f), TextAnchor.MiddleCenter);

            _pseudonymInput = CreateInputField(modal.transform, font, new Vector2(460f, 44f), new Vector2(0f, 5f));
            _pseudonymInput.onEndEdit.AddListener(OnPseudonymEndEdit);

            Button submitButton = CreateButton(modal.transform, font, "Start", new Vector2(240f, 44f), new Vector2(0f, -62f));
            submitButton.onClick.AddListener(TryStartSession);

            _errorText = CreateLabel(
                modal.transform,
                "ErrorText",
                string.Empty,
                font,
                16,
                new Color(0.72f, 0.06f, 0.06f),
                new Vector2(560f, 40f),
                new Vector2(0f, -112f),
                TextAnchor.MiddleCenter
            );
            _errorText.gameObject.SetActive(false);

            _pseudonymInput.Select();
            _pseudonymInput.ActivateInputField();
        }

        private void HideLoginModal()
        {
            if (_loginOverlay != null)
            {
                _loginOverlay.SetActive(false);
            }
        }

        private void OnPseudonymEndEdit(string _)
        {
            if (Input.GetKeyDown(KeyCode.Return) || Input.GetKeyDown(KeyCode.KeypadEnter))
            {
                TryStartSession();
            }
        }

        private void TryStartSession()
        {
            if (_pseudonymInput == null)
            {
                return;
            }

            if (!string.IsNullOrWhiteSpace(_configInitializationError))
            {
                SetLoginError(_configInitializationError);
                return;
            }

            string pseudonym = (_pseudonymInput.text ?? string.Empty).Trim();

            if (string.IsNullOrWhiteSpace(pseudonym))
            {
                SetLoginError("Pseudonym is required.");
                return;
            }

            if (!_pseudonymToGroup.TryGetValue(pseudonym, out string groupKey))
            {
                SetLoginError(UnknownPseudonymError);
                return;
            }

            if (!_groupConfigs.TryGetValue(groupKey, out GroupConfig groupConfig))
            {
                SetLoginError($"Group config is missing for '{groupKey}'.");
                return;
            }

            SetLoginError(string.Empty);

            _session = new SessionState
            {
                Pseudonym = pseudonym,
                GroupKey = groupKey,
                Group = groupConfig,
                CurrentTaskIndex = 0,
                OutputDirectory = BuildSessionOutputDirectory(pseudonym)
            };

            ConfigureSessionLogFile();
            LogLoginEvent();

            HideLoginModal();

            if (!TryLoadCurrentTask(out string loadError))
            {
                ShowCompletionOverlay($"Failed to start the first task: {loadError}", showExitButton: false);
            }
        }

        private void SetLoginError(string error)
        {
            if (_errorText == null)
            {
                return;
            }

            bool showError = !string.IsNullOrWhiteSpace(error);
            _errorText.gameObject.SetActive(showError);
            _errorText.text = showError ? error : string.Empty;
        }

        private void ShowCompletionOverlay(string message, bool showExitButton)
        {
            HideTaskNumberIndicator();

            if (_completionOverlay == null)
            {
                Canvas canvas = FindMainCanvas();
                if (canvas == null)
                {
                    Debug.LogError("Main Canvas is not found. Cannot show completion modal.");
                    return;
                }

                Font font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                _completionOverlay = CreatePanelRoot(CompletionOverlayName, canvas.transform, new Color(0f, 0f, 0f, 0.65f));
                GameObject panel = CreateCenteredPanel("ABCompletionModal", _completionOverlay.transform, new Vector2(760f, 360f));
                _completionText = CreateLabel(
                    panel.transform,
                    "CompletionText",
                    string.Empty,
                    font,
                    24,
                    new Color(0.1f, 0.1f, 0.1f),
                    new Vector2(700f, 240f),
                    new Vector2(0f, 26f),
                    TextAnchor.MiddleCenter
                );

                _completionExitButton = CreateButton(panel.transform, font, "Exit", new Vector2(220f, 44f), new Vector2(0f, -128f));
                _completionExitButton.onClick.AddListener(OnExitApplicationRequested);
            }

            _completionOverlay.SetActive(true);
            _completionOverlay.transform.SetAsLastSibling();
            if (_completionText != null)
            {
                _completionText.text = message;
            }

            if (_completionExitButton != null)
            {
                _completionExitButton.gameObject.SetActive(showExitButton);
            }
        }

        private static void OnExitApplicationRequested()
        {
            Application.Quit();
        }

        private bool EnsureTaskNumberIndicator()
        {
            if (_taskNumberIndicator != null && _taskNumberText != null)
            {
                return true;
            }

            Canvas canvas = FindMainCanvas();
            if (canvas == null)
            {
                return false;
            }

            Font font = Resources.GetBuiltinResource<Font>("Arial.ttf");

            _taskNumberIndicator = new GameObject(TaskNumberIndicatorName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            _taskNumberIndicator.transform.SetParent(canvas.transform, false);

            RectTransform indicatorRect = _taskNumberIndicator.GetComponent<RectTransform>();
            indicatorRect.anchorMin = new Vector2(0f, 1f);
            indicatorRect.anchorMax = new Vector2(0f, 1f);
            indicatorRect.pivot = new Vector2(0f, 1f);
            indicatorRect.anchoredPosition = new Vector2(18f, -18f);
            indicatorRect.sizeDelta = new Vector2(120f, 120f);

            Image indicatorBackground = _taskNumberIndicator.GetComponent<Image>();
            indicatorBackground.color = new Color(0f, 0f, 0f, 0.32f);
            indicatorBackground.raycastTarget = false;

            GameObject textObject = new GameObject("TaskNumberText", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text), typeof(Outline));
            textObject.transform.SetParent(_taskNumberIndicator.transform, false);

            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            _taskNumberText = textObject.GetComponent<Text>();
            _taskNumberText.font = font;
            _taskNumberText.fontSize = 86;
            _taskNumberText.alignment = TextAnchor.MiddleCenter;
            _taskNumberText.color = Color.white;
            _taskNumberText.horizontalOverflow = HorizontalWrapMode.Overflow;
            _taskNumberText.verticalOverflow = VerticalWrapMode.Overflow;
            _taskNumberText.raycastTarget = false;

            Outline outline = textObject.GetComponent<Outline>();
            outline.effectColor = new Color(0f, 0f, 0f, 0.9f);
            outline.effectDistance = new Vector2(2f, -2f);
            outline.useGraphicAlpha = true;

            return true;
        }

        private void UpdateTaskNumberIndicator()
        {
            if (_session == null ||
                _session.Group == null ||
                _session.CurrentTaskIndex < 0 ||
                _session.CurrentTaskIndex >= _session.Group.Tasks.Count)
            {
                HideTaskNumberIndicator();
                return;
            }

            if (!EnsureTaskNumberIndicator())
            {
                return;
            }

            _taskNumberText.text = (_session.CurrentTaskIndex + 1).ToString();
            _taskNumberIndicator.SetActive(true);
            _taskNumberIndicator.transform.SetAsLastSibling();
        }

        private void HideTaskNumberIndicator()
        {
            if (_taskNumberIndicator != null)
            {
                _taskNumberIndicator.SetActive(false);
            }
        }

        private void ShowMidpointOverlay()
        {
            _isWaitingForMidpointConfirmation = true;

            if (_midpointOverlay == null)
            {
                Canvas canvas = FindMainCanvas();
                if (canvas == null)
                {
                    Debug.LogError("Main Canvas is not found. Cannot show midpoint modal.");
                    return;
                }

                Font font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                _midpointOverlay = CreatePanelRoot(MidpointOverlayName, canvas.transform, new Color(0f, 0f, 0f, 0.65f));
                GameObject panel = CreateCenteredPanel("ABMidpointModal", _midpointOverlay.transform, new Vector2(760f, 280f));
                CreateLabel(
                    panel.transform,
                    "MidpointText",
                    "Wait for new tasks from curators.\nWhen you receive them, press OK.",
                    font,
                    24,
                    new Color(0.1f, 0.1f, 0.1f),
                    new Vector2(680f, 160f),
                    new Vector2(0f, 30f),
                    TextAnchor.MiddleCenter
                );

                Button okButton = CreateButton(panel.transform, font, "OK", new Vector2(220f, 44f), new Vector2(0f, -74f));
                okButton.onClick.AddListener(OnMidpointConfirmed);
            }

            _midpointOverlay.SetActive(true);
            _midpointOverlay.transform.SetAsLastSibling();
        }

        private bool ShowCompleteTaskConfirmOverlay()
        {
            if (_completeTaskConfirmOverlay == null)
            {
                Canvas canvas = FindMainCanvas();
                if (canvas == null)
                {
                    Debug.LogError("Main Canvas is not found. Cannot show complete task confirmation modal.");
                    return false;
                }

                Font font = Resources.GetBuiltinResource<Font>("Arial.ttf");
                _completeTaskConfirmOverlay = CreatePanelRoot(CompleteTaskConfirmOverlayName, canvas.transform, new Color(0f, 0f, 0f, 0.65f));
                GameObject panel = CreateCenteredPanel("ABCompleteTaskConfirmModal", _completeTaskConfirmOverlay.transform, new Vector2(760f, 320f));

                CreateLabel(
                    panel.transform,
                    "ConfirmTitle",
                    "Complete Current Task?",
                    font,
                    30,
                    new Color(0.1f, 0.1f, 0.1f),
                    new Vector2(680f, 60f),
                    new Vector2(0f, 92f),
                    TextAnchor.MiddleCenter
                );

                CreateLabel(
                    panel.transform,
                    "ConfirmDescription",
                    "The task will be saved and the next task will open.\nDo you want to continue?",
                    font,
                    22,
                    new Color(0.2f, 0.2f, 0.2f),
                    new Vector2(680f, 120f),
                    new Vector2(0f, 16f),
                    TextAnchor.MiddleCenter
                );

                Button noButton = CreateButton(panel.transform, font, "No", new Vector2(220f, 48f), new Vector2(-130f, -96f));
                noButton.onClick.AddListener(OnCompleteTaskConfirmNo);

                Button yesButton = CreateButton(panel.transform, font, "Yes", new Vector2(220f, 48f), new Vector2(130f, -96f));
                yesButton.onClick.AddListener(OnCompleteTaskConfirmYes);
            }

            _completeTaskConfirmOverlay.SetActive(true);
            _completeTaskConfirmOverlay.transform.SetAsLastSibling();
            return true;
        }

        private void HideCompleteTaskConfirmOverlay()
        {
            if (_completeTaskConfirmOverlay != null)
            {
                _completeTaskConfirmOverlay.SetActive(false);
            }
        }

        private void OnCompleteTaskConfirmNo()
        {
            HideCompleteTaskConfirmOverlay();
        }

        private void OnCompleteTaskConfirmYes()
        {
            HideCompleteTaskConfirmOverlay();

            if (!TryCompleteCurrentTaskInternal(out string message))
            {
                Debug.LogWarning(message);
                return;
            }

            Debug.Log(message);
        }

        private void OnMidpointConfirmed()
        {
            if (!_isWaitingForMidpointConfirmation)
            {
                return;
            }

            _isWaitingForMidpointConfirmation = false;
            if (_midpointOverlay != null)
            {
                _midpointOverlay.SetActive(false);
            }

            if (!TryLoadCurrentTask(out string loadError))
            {
                ShowCompletionOverlay($"Failed to load next task: {loadError}", showExitButton: false);
            }
        }

        private static Canvas FindMainCanvas()
        {
            Canvas[] canvases = FindObjectsOfType<Canvas>(true);
            if (canvases == null || canvases.Length == 0)
            {
                return null;
            }

            Canvas rootCanvas = canvases.FirstOrDefault(canvas => canvas.isRootCanvas && canvas.name == "Canvas");
            return rootCanvas ?? canvases.FirstOrDefault(canvas => canvas.isRootCanvas) ?? canvases[0];
        }

        private static GameObject CreatePanelRoot(string objectName, Transform parent, Color backgroundColor)
        {
            GameObject root = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            root.transform.SetParent(parent, false);

            RectTransform rectTransform = root.GetComponent<RectTransform>();
            rectTransform.anchorMin = Vector2.zero;
            rectTransform.anchorMax = Vector2.one;
            rectTransform.offsetMin = Vector2.zero;
            rectTransform.offsetMax = Vector2.zero;

            Image image = root.GetComponent<Image>();
            image.color = backgroundColor;
            image.raycastTarget = true;

            return root;
        }

        private static GameObject CreateCenteredPanel(string objectName, Transform parent, Vector2 size)
        {
            GameObject panel = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Image));
            panel.transform.SetParent(parent, false);

            RectTransform rectTransform = panel.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = Vector2.zero;
            rectTransform.sizeDelta = size;

            Image image = panel.GetComponent<Image>();
            image.color = new Color(0.96f, 0.96f, 0.96f, 1f);
            image.raycastTarget = true;

            return panel;
        }

        private static Text CreateLabel(
            Transform parent,
            string objectName,
            string textValue,
            Font font,
            int fontSize,
            Color color,
            Vector2 size,
            Vector2 anchoredPosition,
            TextAnchor anchor
        )
        {
            GameObject labelObject = new GameObject(objectName, typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            labelObject.transform.SetParent(parent, false);

            RectTransform rectTransform = labelObject.GetComponent<RectTransform>();
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);
            rectTransform.anchoredPosition = anchoredPosition;
            rectTransform.sizeDelta = size;

            Text label = labelObject.GetComponent<Text>();
            label.font = font;
            label.fontSize = fontSize;
            label.color = color;
            label.alignment = anchor;
            label.horizontalOverflow = HorizontalWrapMode.Wrap;
            label.verticalOverflow = VerticalWrapMode.Truncate;
            label.text = textValue;

            return label;
        }

        private static InputField CreateInputField(Transform parent, Font font, Vector2 size, Vector2 anchoredPosition)
        {
            GameObject inputObject = new GameObject("PseudonymInput", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(InputField));
            inputObject.transform.SetParent(parent, false);

            RectTransform inputRect = inputObject.GetComponent<RectTransform>();
            inputRect.anchorMin = new Vector2(0.5f, 0.5f);
            inputRect.anchorMax = new Vector2(0.5f, 0.5f);
            inputRect.pivot = new Vector2(0.5f, 0.5f);
            inputRect.anchoredPosition = anchoredPosition;
            inputRect.sizeDelta = size;

            Image inputBackground = inputObject.GetComponent<Image>();
            inputBackground.color = Color.white;
            inputBackground.raycastTarget = true;

            InputField inputField = inputObject.GetComponent<InputField>();
            inputField.lineType = InputField.LineType.SingleLine;
            inputField.characterLimit = 64;

            GameObject textArea = new GameObject("Text Area", typeof(RectTransform), typeof(RectMask2D));
            textArea.transform.SetParent(inputObject.transform, false);
            RectTransform textAreaRect = textArea.GetComponent<RectTransform>();
            textAreaRect.anchorMin = Vector2.zero;
            textAreaRect.anchorMax = Vector2.one;
            textAreaRect.offsetMin = new Vector2(12f, 8f);
            textAreaRect.offsetMax = new Vector2(-12f, -8f);

            GameObject placeholderObject = new GameObject("Placeholder", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            placeholderObject.transform.SetParent(textArea.transform, false);
            RectTransform placeholderRect = placeholderObject.GetComponent<RectTransform>();
            placeholderRect.anchorMin = Vector2.zero;
            placeholderRect.anchorMax = Vector2.one;
            placeholderRect.offsetMin = Vector2.zero;
            placeholderRect.offsetMax = Vector2.zero;

            Text placeholder = placeholderObject.GetComponent<Text>();
            placeholder.font = font;
            placeholder.fontSize = 20;
            placeholder.alignment = TextAnchor.MiddleLeft;
            placeholder.color = new Color(0.45f, 0.45f, 0.45f, 0.8f);
            placeholder.text = "Pseudonym";

            GameObject textObject = new GameObject("Text", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            textObject.transform.SetParent(textArea.transform, false);
            RectTransform textRect = textObject.GetComponent<RectTransform>();
            textRect.anchorMin = Vector2.zero;
            textRect.anchorMax = Vector2.one;
            textRect.offsetMin = Vector2.zero;
            textRect.offsetMax = Vector2.zero;

            Text text = textObject.GetComponent<Text>();
            text.font = font;
            text.fontSize = 20;
            text.alignment = TextAnchor.MiddleLeft;
            text.color = new Color(0.1f, 0.1f, 0.1f, 1f);
            text.supportRichText = false;

            inputField.textComponent = text;
            inputField.placeholder = placeholder;

            return inputField;
        }

        private static Button CreateButton(Transform parent, Font font, string label, Vector2 size, Vector2 anchoredPosition)
        {
            GameObject buttonObject = new GameObject("SubmitButton", typeof(RectTransform), typeof(CanvasRenderer), typeof(Image), typeof(Button));
            buttonObject.transform.SetParent(parent, false);

            RectTransform buttonRect = buttonObject.GetComponent<RectTransform>();
            buttonRect.anchorMin = new Vector2(0.5f, 0.5f);
            buttonRect.anchorMax = new Vector2(0.5f, 0.5f);
            buttonRect.pivot = new Vector2(0.5f, 0.5f);
            buttonRect.anchoredPosition = anchoredPosition;
            buttonRect.sizeDelta = size;

            Image buttonImage = buttonObject.GetComponent<Image>();
            buttonImage.color = new Color(0.18f, 0.55f, 0.92f, 1f);
            buttonImage.raycastTarget = true;

            Button button = buttonObject.GetComponent<Button>();
            ColorBlock colors = button.colors;
            colors.normalColor = new Color(0.18f, 0.55f, 0.92f, 1f);
            colors.highlightedColor = new Color(0.24f, 0.61f, 0.97f, 1f);
            colors.pressedColor = new Color(0.12f, 0.44f, 0.78f, 1f);
            colors.selectedColor = colors.highlightedColor;
            colors.disabledColor = new Color(0.5f, 0.5f, 0.5f, 0.9f);
            button.colors = colors;

            GameObject labelObject = new GameObject("Label", typeof(RectTransform), typeof(CanvasRenderer), typeof(Text));
            labelObject.transform.SetParent(buttonObject.transform, false);
            RectTransform labelRect = labelObject.GetComponent<RectTransform>();
            labelRect.anchorMin = Vector2.zero;
            labelRect.anchorMax = Vector2.one;
            labelRect.offsetMin = Vector2.zero;
            labelRect.offsetMax = Vector2.zero;

            Text labelText = labelObject.GetComponent<Text>();
            labelText.font = font;
            labelText.fontSize = 20;
            labelText.alignment = TextAnchor.MiddleCenter;
            labelText.color = Color.white;
            labelText.text = label;

            return button;
        }

        private sealed class GroupConfig
        {
            public string GroupName { get; }
            public List<TaskConfig> Tasks { get; }

            public GroupConfig(string groupName, List<TaskConfig> tasks)
            {
                GroupName = groupName;
                Tasks = tasks ?? new List<TaskConfig>();
            }
        }

        private sealed class TaskConfig
        {
            public string TaskId { get; }
            public string DiagramJson { get; }
            public bool SuggestionsEnabled { get; }

            public TaskConfig(string taskId, string diagramJson, bool suggestionsEnabled)
            {
                TaskId = taskId;
                DiagramJson = diagramJson;
                SuggestionsEnabled = suggestionsEnabled;
            }
        }

        private sealed class SessionState
        {
            public string Pseudonym;
            public string GroupKey;
            public GroupConfig Group;
            public int CurrentTaskIndex;
            public string OutputDirectory;
        }
    }
}
