#define AppVersion "1.0.1"
#define AppExe "HareketliDuvarKagidi.exe"

[Setup]
AppId={{C2E1BA5B-A88F-4CD5-BE13-D08C4F521590}
AppName=Hareketli Duvar Kağıdı
AppVersion={#AppVersion}
VersionInfoVersion=1.0.1.0
DefaultDirName={localappdata}\Programs\HareketliDuvarKagidi
DefaultGroupName=Hareketli Duvar Kağıdı
PrivilegesRequired=lowest
ArchitecturesAllowed=x64compatible
ArchitecturesInstallIn64BitMode=x64compatible
MinVersion=10.0.22000
OutputDir=..\dist\{#AppVersion}
OutputBaseFilename=HareketliDuvarKagidi-{#AppVersion}-win-x64-Setup
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
UninstallDisplayIcon={app}\{#AppExe}
CloseApplications=no
RestartApplications=no

[Languages]
Name: "turkish"; MessagesFile: "compiler:Languages\Turkish.isl"

[Tasks]
Name: desktopicon; Description: "Masaüstü kısayolu oluştur"; Flags: unchecked

[Files]
Source: "..\dist\{#AppVersion}\HareketliDuvarKagidi-{#AppVersion}-sources-partial.zip"; DestDir: "{app}\sources"; Flags: ignoreversion nocompression
Source: "..\dist\{#AppVersion}\HareketliDuvarKagidi-{#AppVersion}-sources-partial.zip.sha256"; DestDir: "{app}\sources"; Flags: ignoreversion
Source: "..\dist\{#AppVersion}\win-x64\*"; DestDir: "{app}"; Flags: ignoreversion recursesubdirs createallsubdirs

[Icons]
Name: "{userprograms}\Hareketli Duvar Kağıdı"; Filename: "{app}\{#AppExe}"
Name: "{userdesktop}\Hareketli Duvar Kağıdı"; Filename: "{app}\{#AppExe}"; Tasks: desktopicon

[Registry]
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "HareketliDuvarKagidi"; ValueData: """{app}\{#AppExe}"" --background"; Flags: uninsdeletevalue; Check: AutoStartEnabled
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: none; ValueName: "HareketliDuvarKagidi"; Flags: deletevalue uninsdeletevalue; Check: AutoStartDisabled

[Run]
Filename: "{app}\{#AppExe}"; Description: "Uygulamayı aç"; Flags: nowait postinstall skipifsilent

[Code]
function AutoStartEnabled: Boolean;
var Enabled: Cardinal;
begin
  Enabled := 1;
  if not RegQueryDWordValue(HKCU, 'Software\HareketliDuvarKagidi', 'StartWithWindows', Enabled) then Enabled := 1;
  Result := Enabled <> 0;
end;

function AutoStartDisabled: Boolean;
begin
  Result := not AutoStartEnabled;
end;

function CloseInstalledApplication: Boolean;
var ResultCode: Integer;
    ExePath: String;
begin
  ExePath := ExpandConstant('{app}\{#AppExe}');
  Result := True;
  if FileExists(ExePath) then
    Result := Exec(ExePath, '--shutdown', '', SW_HIDE, ewWaitUntilTerminated, ResultCode) and (ResultCode = 0);
end;

function PrepareToInstall(var NeedsRestart: Boolean): String;
begin
  Result := '';
  if not CloseInstalledApplication then
    Result := 'Uygulama kapatılamadı. Sistem tepsisindeki Çıkış seçeneğini kullanıp tekrar deneyin.';
end;

function InitializeUninstall: Boolean;
begin
  Result := CloseInstalledApplication;
  if not Result then
    MsgBox('Uygulama kapatılamadı. Sistem tepsisinden Çıkış yapıp kaldırmayı tekrar başlatın.', mbError, MB_OK);
end;


