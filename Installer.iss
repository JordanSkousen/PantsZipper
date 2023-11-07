#define MyAppName "PantsZipper"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "Jordan Skousen"
#define MyAppIcoName "Icon.ico"
#define MyAppURL "https://jothedev.com"
#define MyAppExeName "PantsZipper.exe"

[Setup]
; NOTE: The value of AppId uniquely identifies this application. Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{209DC0C8-9B82-4A0C-8A8B-B05CAC70ABC8}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
ChangesAssociations=yes
Compression=lzma
DefaultDirName={autopf}\{#MyAppName}
OutputDir=C:\Users\Jordan\source\repos\PantsZipper
OutputBaseFilename=Installer
SetupIconFile={#MyAppIcoName}
DisableProgramGroupPage=yes
SolidCompression=yes
UninstallDisplayIcon="{app}\{#MyAppIcoName}"
UninstallDisplayName={#MyAppName}
WizardStyle=modern
VersionInfoCompany={#MyAppPublisher}
VersionInfoProductName={#MyAppName}
VersionInfoProductTextVersion={#MyAppVersion}
VersionInfoProductVersion={#MyAppVersion}

[Tasks]
Name: zipAssc; Description: "Associate with *.zip files"; GroupDescription: "File extensions:"
Name: tarAssc; Description: "Associate with *.tar files"; GroupDescription: "File extensions:"
Name: gzAssc; Description: "Associate with *.gz files"; GroupDescription: "File extensions:"
Name: bz2Assc; Description: "Associate with *.bz2 files"; GroupDescription: "File extensions:"

[Files]
Source: ".\PantsZipper\bin\Release\PantsZipper.exe"; DestDir: "{app}"; Flags: ignoreversion 
Source: ".\PantsZipper\bin\Release\PantsZipper.exe.config"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\PantsZipper\bin\Release\ICSharpCode.SharpZipLib.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\PantsZipper\bin\Release\System.Buffers.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\PantsZipper\bin\Release\System.Memory.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\PantsZipper\bin\Release\System.Numerics.Vectors.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\PantsZipper\bin\Release\System.Runtime.CompilerServices.Unsafe.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: ".\PantsZipper\bin\Release\System.Threading.Tasks.Extensions.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: {#MyAppIcoName}; DestDir: {app}; Flags: ignoreversion

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"

[Registry]
Root: HKCR; Subkey: ".zip"; ValueData: "{#MyAppName}"; Flags: uninsdeletevalue; ValueType: string; ValueName: ""; Tasks: zipAssc
Root: HKCR; Subkey: ".tar"; ValueData: "{#MyAppName}"; Flags: uninsdeletevalue; ValueType: string; ValueName: ""; Tasks: tarAssc
Root: HKCR; Subkey: ".gz"; ValueData: "{#MyAppName}"; Flags: uninsdeletevalue; ValueType: string; ValueName: ""; Tasks: gzAssc
Root: HKCR; Subkey: ".bz2"; ValueData: "{#MyAppName}"; Flags: uninsdeletevalue; ValueType: string; ValueName: ""; Tasks: bz2Assc
; Root: HKCR; Subkey: "{#MyAppName}"; ValueData: "{#MyAppName}"; Flags: uninsdeletekey; ValueType: string; ValueName: ""
Root: HKCR; Subkey: "{#MyAppName}\DefaultIcon"; ValueData: "{app}\{#MyAppExeName},0"; ValueType: string; ValueName: ""
Root: HKCR; Subkey: "{#MyAppName}\shell\open\command"; ValueData: """{app}\{#MyAppExeName}"" ""%1"""; ValueType: string; ValueName: ""