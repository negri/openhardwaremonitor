; Script generated by the Inno Script Studio Wizard.
; SEE THE DOCUMENTATION FOR DETAILS ON CREATING INNO SETUP SCRIPT FILES!

#ifndef RootDir
#define RootDir ".."
#define IsDebug "true"
#endif

#define MyAppName "OpenHardwareMonitor"
#define MyAppVersion GetFileVersion(RootDir + "\OpenHardwareMonitor\bin\Release\OpenHardwareMonitor.exe")
#define MyAppPublisher "Leica Geosystems AG"
#define MyAppURL "https://github.com/hexagon-oss/openhardwaremonitor"
#define MyAppExeName "OpenHardwareMonitor.exe"

[Setup]
; NOTE: The value of AppId uniquely identifies this application.
; Do not use the same AppId value in installers for other applications.
; (To generate a new GUID, click Tools | Generate GUID inside the IDE.)
AppId={{1212D497-FA88-4C96-906E-6AA769F2D704}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppVerName={#MyAppName} {#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
AppCopyright=Copyright � 2010-2023 Michael M�ller at al, See license
DefaultDirName={autopf64}\{#MyAppName}
DefaultGroupName={#MyAppName}
AllowNoIcons=yes
SourceDir={#RootDir}
LicenseFile=LICENSE.txt
OutputDir=artifacts
OutputBaseFilename=OpenHardwareMonitorSetup
SetupIconFile=OpenHardwareMonitor\icon.ico
UninstallDisplayIcon={app}\{#MyAppExeName}
Compression=lzma
SolidCompression=yes
DirExistsWarning=no

[Languages]
Name: "english"; MessagesFile: "compiler:Default.isl"
Name: "armenian"; MessagesFile: "compiler:Languages\Armenian.isl"
Name: "brazilianportuguese"; MessagesFile: "compiler:Languages\BrazilianPortuguese.isl"
Name: "catalan"; MessagesFile: "compiler:Languages\Catalan.isl"
Name: "corsican"; MessagesFile: "compiler:Languages\Corsican.isl"
Name: "czech"; MessagesFile: "compiler:Languages\Czech.isl"
Name: "danish"; MessagesFile: "compiler:Languages\Danish.isl"
Name: "dutch"; MessagesFile: "compiler:Languages\Dutch.isl"
Name: "finnish"; MessagesFile: "compiler:Languages\Finnish.isl"
Name: "french"; MessagesFile: "compiler:Languages\French.isl"
Name: "german"; MessagesFile: "compiler:Languages\German.isl"
Name: "hebrew"; MessagesFile: "compiler:Languages\Hebrew.isl"
Name: "icelandic"; MessagesFile: "compiler:Languages\Icelandic.isl"
Name: "italian"; MessagesFile: "compiler:Languages\Italian.isl"
Name: "japanese"; MessagesFile: "compiler:Languages\Japanese.isl"
Name: "norwegian"; MessagesFile: "compiler:Languages\Norwegian.isl"
Name: "polish"; MessagesFile: "compiler:Languages\Polish.isl"
Name: "portuguese"; MessagesFile: "compiler:Languages\Portuguese.isl"
Name: "russian"; MessagesFile: "compiler:Languages\Russian.isl"
Name: "slovak"; MessagesFile: "compiler:Languages\Slovak.isl"
Name: "slovenian"; MessagesFile: "compiler:Languages\Slovenian.isl"
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"
Name: "turkish"; MessagesFile: "compiler:Languages\Turkish.isl"
Name: "ukrainian"; MessagesFile: "compiler:Languages\Ukrainian.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}"; Flags: unchecked
Name: "autostart"; Description: "Register OpenHardwareMonitor for Automatic Startup"

[Files]
Source: "OpenHardwareMonitor\bin\Release\OpenHardwareMonitor.exe"; DestDir: "{app}"; Flags: ignoreversion
Source: "OpenHardwareMonitor\bin\Release\Aga.Controls.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "OpenHardwareMonitor\bin\Release\CommandLine.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "OpenHardwareMonitor\bin\Release\Grapeseed.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "OpenHardwareMonitor\bin\Release\Grapevine.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "OpenHardwareMonitor\bin\Release\Microsoft.Extensions.Configuration.Abstractions.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "OpenHardwareMonitor\bin\Release\Microsoft.Extensions.Configuration.Binder.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "OpenHardwareMonitor\bin\Release\Microsoft.Extensions.Configuration.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "OpenHardwareMonitor\bin\Release\Microsoft.Extensions.Configuration.FileExtensions.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "OpenHardwareMonitor\bin\Release\Microsoft.Extensions.Configuration.Json.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "OpenHardwareMonitor\bin\Release\Microsoft.Extensions.DependencyInjection.Abstractions.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "OpenHardwareMonitor\bin\Release\Microsoft.Extensions.DependencyInjection.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "OpenHardwareMonitor\bin\Release\Microsoft.Extensions.FileProviders.Abstractions.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "OpenHardwareMonitor\bin\Release\Microsoft.Extensions.FileProviders.Physical.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "OpenHardwareMonitor\bin\Release\Microsoft.Extensions.FileSystemGlobbing.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "OpenHardwareMonitor\bin\Release\Microsoft.Extensions.Logging.Abstractions.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "OpenHardwareMonitor\bin\Release\Microsoft.Extensions.Logging.Configuration.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "OpenHardwareMonitor\bin\Release\Microsoft.Extensions.Logging.Console.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "OpenHardwareMonitor\bin\Release\Microsoft.Extensions.Logging.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "OpenHardwareMonitor\bin\Release\Microsoft.Extensions.Options.ConfigurationExtensions.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "OpenHardwareMonitor\bin\Release\Microsoft.Extensions.Options.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "OpenHardwareMonitor\bin\Release\Microsoft.Extensions.Primitives.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "OpenHardwareMonitor\bin\Release\NLog.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "OpenHardwareMonitor\bin\Release\NLog.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "OpenHardwareMonitor\bin\Release\NLog.Extensions.Logging.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "OpenHardwareMonitor\bin\Release\OpenHardwareMonitor.deps.json"; DestDir: "{app}"; Flags: ignoreversion
Source: "OpenHardwareMonitor\bin\Release\OpenHardwareMonitor.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "OpenHardwareMonitor\bin\Release\OpenHardwareMonitor.dll.config"; DestDir: "{app}"; Flags: ignoreversion
Source: "OpenHardwareMonitor\bin\Release\OpenHardwareMonitor.pdb"; DestDir: "{app}"; Flags: ignoreversion
Source: "OpenHardwareMonitor\bin\Release\OpenHardwareMonitor.runtimeconfig.json"; DestDir: "{app}"; Flags: ignoreversion
Source: "OpenHardwareMonitor\bin\Release\OpenHardwareMonitorLib.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "OpenHardwareMonitor\bin\Release\OpenHardwareMonitorLib.pdb"; DestDir: "{app}"; Flags: ignoreversion
Source: "OpenHardwareMonitor\bin\Release\OxyPlot.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "OpenHardwareMonitor\bin\Release\OxyPlot.WindowsForms.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "OpenHardwareMonitor\bin\Release\System.IO.Ports.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "OpenHardwareMonitor\bin\Release\System.Management.dll"; DestDir: "{app}"; Flags: ignoreversion
Source: "OpenHardwareMonitor\bin\Release\ref\*"; DestDir: "{app}\ref"; Flags: ignoreversion recursesubdirs createallsubdirs skipifsourcedoesntexist
Source: "OpenHardwareMonitor\bin\Release\runtimes\*"; DestDir: "{app}\runtimes"; Flags: ignoreversion recursesubdirs createallsubdirs
Source: "OpenHardwareMonitor\bin\Release\web\*"; DestDir: "{app}\web"; Flags: ignoreversion recursesubdirs createallsubdirs
; NOTE: Don't use "Flags: ignoreversion" on any shared system files

[Icons]
Name: "{group}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
Name: "{commondesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
Filename: "{app}\{#MyAppExeName}"; Parameters: "--autostartupmode logon --closeall --startminimized --minimizetotray --run"; Flags: waituntilterminated; Description: "Performing post-install tasks"; Tasks: autostart
Filename: "{app}\{#MyAppExeName}"; Parameters: "--startnormal"; Flags: nowait postinstall skipifsilent runascurrentuser; Description: "{cm:LaunchProgram,{#StringChange(MyAppName, '&', '&&')}}"

[UninstallRun]
Filename: "{app}\{#MyAppExeName}"; Parameters: "--autostartupmode disable --closeall"; Flags: waituntilterminated runascurrentuser; RunOnceId: "RemoveService"

[UninstallDelete]
Type: files; Name: "{app}\OpenHardwareMonitor.config"
Type: files; Name: "{app}\OpenHardwareMonitorLib.sys"

[InstallDelete]
// On install/update reset the configuration. May not be exactly desirable, but avoids a number of possible errors
Type: files; Name: "{app}\OpenHardwareMonitor.config"

[Code]
/////////////////////////////////////////////////////////////////////
function GetUninstallString(): String;
var
  sUnInstPath: String;
  sUnInstallString: String;
begin
  sUnInstPath := ExpandConstant('Software\Microsoft\Windows\CurrentVersion\Uninstall\{#emit SetupSetting("AppId")}_is1');
  sUnInstallString := '';
  if not RegQueryStringValue(HKLM, sUnInstPath, 'UninstallString', sUnInstallString) then
    RegQueryStringValue(HKCU, sUnInstPath, 'UninstallString', sUnInstallString);
  Result := sUnInstallString;
end;


/////////////////////////////////////////////////////////////////////
function IsUpgrade(): Boolean;
begin
    Result := (GetUninstallString() <> '');
end;

/////////////////////////////////////////////////////////////////////
// Uninstalls the previous version of our own installation (regardless of whether that one was never or older than the current one)
function UnInstallOldVersion(): Integer;
var
  sUnInstallString: String;
  iResultCode: Integer;
begin
// Return Values:
// 1 - uninstall string is empty
// 2 - error executing the UnInstallString
// 3 - successfully executed the UnInstallString

  // default return value
  Result := 0;

  // get the uninstall string of the old app
  sUnInstallString := GetUninstallString();
  if sUnInstallString <> '' then begin
    sUnInstallString := RemoveQuotes(sUnInstallString);
    if Exec(sUnInstallString, '/SILENT /NORESTART /SUPPRESSMSGBOXES','', SW_HIDE, ewWaitUntilTerminated, iResultCode) then
      Result := 3
    else
      Result := 2;
  end else
    Result := 1;
end;

procedure TaskKill(FileName: String);
var
  ResultCode: Integer;
begin
    Exec('taskkill.exe', '/f /im ' + '"' + FileName + '"', '', SW_HIDE,
     ewWaitUntilTerminated, ResultCode);
end;

procedure KillExistingInstances();
begin
    TaskKill('OpenHardwareMonitor.exe');
end;

/////////////////////////////////////////////////////////////////////
procedure CurStepChanged(CurStep: TSetupStep);
begin
  if (CurStep=ssInstall) then
  begin
    WizardForm.StatusLabel.Caption := 'Uninstalling old versions...';
    if (IsUpgrade()) then
    begin
      UnInstallOldVersion();
    end;
    // If there are still instances running, kill them now
    KillExistingInstances();
  end;
end;
