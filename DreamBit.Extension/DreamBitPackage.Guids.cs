namespace DreamBit.Extension
{
    partial class DreamBitPackage
    {
        public static class Guids
        {
            // Menu     - 0x1000
            // Group    - 0x2000
            // Command  - 0x3000
            // Toolbar  - 0x4000
            // Icons    - 0x5000

            public const string Package = "9acdf843-80e5-4552-9a16-90a610ebb3ce";
            public const string CommandSet = "ab458d30-1a88-4abe-9d8f-f23ddf53a407";

            // Main Menu
            public const int MainMenu = 0x1000;
            public const int MainMenuGroup = 0x2000;
            public const int SceneMenuGroup = 0x2001;
            public const int BuildContentCommand = 0x3000;
            public const int SceneEditorWindowCommand = 0x3001;
            public const int SceneHierarchyWindowCommand = 0x3002;
            public const int SceneInspectWindowCommand = 0x3003;

            // Scene Editor
            public const string SceneEditorWindow = "bcd03dc6-67bf-4064-80a1-fbbbd463d158";
            public const string SceneEditorIcons = "b95623e8-f268-4d05-8801-60c8a4901134";
            public const int SceneEditorToolbar = 0x4001;
            public const int SceneEditorToolbarGroup = 0x2004;
            public const int RedoCommand = 0x3011;
            public const int UndoCommand = 0x3012;

            // Scene Hierarchy
            public const string SceneHierarchyWindow = "59240e5d-89c8-46c7-ac10-bc921c528b50";
            public const string SceneHierarchyIcons = "cbe4cf96-2ec4-40fe-bb5c-f7d46a0fe701";
            public const int SceneHierarchyToolbar = 0x4000;
            public const int SceneHierarchyToolbarGroup = 0x2002;
            public const int AddGameObjectCommand = 0x3004;
            public const int AddCameraObjectCommand = 0x3006;

            // Scene Inspect
            public const string SceneInspectWindow = "55bf2007-a77c-47c4-b165-90ba52e4576c";

            // Project
            public const int ProjectContextAddGroup = 0x2003;
            public const int AddFontCommand = 0x3005;
            public const int AddSceneCommand = 0x3007;
            public const int EditFontCommand = 0x3008;
            public const int EditSceneCommand = 0x3009;
            public const int AddScriptCommand = 0x3010;
        }
    }
}
