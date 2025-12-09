; Script de instalación Unilocker Client
; Generado para Inno Setup 6
; https://jrsoftware.org/isinfo.php

#define MyAppName "Unilocker Client"
#define MyAppVersion "1.0.0"
#define MyAppPublisher "Universidad Privada del Valle"
#define MyAppURL "https://www.univalle.edu"
#define MyAppExeName "Unilocker.Client.exe"
#define MyAppGUID "{{3F8E9A2C-5D1B-4E7F-9C3A-8B6D4E2F1A90}"

[Setup]
; Información de la aplicación
AppId={#MyAppGUID}
AppName={#MyAppName}
AppVersion={#MyAppVersion}
AppPublisher={#MyAppPublisher}
AppPublisherURL={#MyAppURL}
AppSupportURL={#MyAppURL}
AppUpdatesURL={#MyAppURL}
DefaultDirName={autopf}\Unilocker
DisableProgramGroupPage=yes
; LicenseFile=LICENSE.txt
OutputDir=installer
OutputBaseFilename=UnilockerClientSetup_v{#MyAppVersion}
; SetupIconFile=icon.ico
Compression=lzma2
SolidCompression=yes
WizardStyle=modern
PrivilegesRequired=admin
ArchitecturesAllowed=x64
ArchitecturesInstallIn64BitMode=x64
UninstallDisplayIcon={app}\{#MyAppExeName}
VersionInfoVersion={#MyAppVersion}
VersionInfoCompany={#MyAppPublisher}
VersionInfoDescription=Cliente de Control de Laboratorios Unilocker

[Languages]
Name: "spanish"; MessagesFile: "compiler:Languages\Spanish.isl"

[Tasks]
Name: "desktopicon"; Description: "{cm:CreateDesktopIcon}"; GroupDescription: "{cm:AdditionalIcons}";
Name: "startupicon"; Description: "Ejecutar al iniciar Windows (Recomendado para modo laboratorio)"; GroupDescription: "Opciones de inicio:"

[Files]
; Archivo principal
Source: "Unilocker.Client\publish\win-x64\Unilocker.Client.exe"; DestDir: "{app}"; Flags: ignoreversion
; NO copiar appsettings.json - se creará dinámicamente con la URL configurada por el usuario

[Icons]
; Iconos en menú de inicio
Name: "{autoprograms}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"
; Icono en escritorio (opcional)
Name: "{autodesktop}\{#MyAppName}"; Filename: "{app}\{#MyAppExeName}"; Tasks: desktopicon

[Run]
; Ejecutar después de la instalación
Filename: "{app}\{#MyAppExeName}"; Description: "Ejecutar {#MyAppName}"; Flags: nowait postinstall skipifsilent

[UninstallDelete]
; Limpiar archivos de configuración al desinstalar
Type: filesandordirs; Name: "{commonappdata}\Unilocker"
Type: files; Name: "{app}\appsettings.json"

[Registry]
; Agregar al inicio automático si la tarea está seleccionada
Root: HKCU; Subkey: "Software\Microsoft\Windows\CurrentVersion\Run"; ValueType: string; ValueName: "UnilockerClient"; ValueData: """{app}\{#MyAppExeName}"""; Flags: uninsdeletevalue; Tasks: startupicon

[Code]
var
  ApiUrlPage: TInputQueryWizardPage;
  ConfiguredApiUrl: String;

procedure InitializeWizard;
begin
  // Crear página personalizada para configurar la URL de la API
  ApiUrlPage := CreateInputQueryPage(wpSelectTasks,
    'Configuración del Servidor API', 
    'Ingrese la dirección del servidor donde está instalada la API de Unilocker',
    'La URL debe comenzar con http:// o https://. ' +
    'Ejemplo: http://192.168.0.5:5013 o http://localhost:5013');
  
  // Agregar campo de entrada para la URL
  ApiUrlPage.Add('URL de la API:', False);
  
  // Valor por defecto
  ApiUrlPage.Values[0] := 'http://localhost:5013';
end;

function ValidateApiUrl(Url: String): Boolean;
begin
  Result := False;
  
  // Verificar que no esté vacío
  if Length(Url) = 0 then
  begin
    MsgBox('La URL no puede estar vacía.', mbError, MB_OK);
    Exit;
  end;
  
  // Verificar que comience con http:// o https://
  if (Pos('http://', LowerCase(Url)) <> 1) and (Pos('https://', LowerCase(Url)) <> 1) then
  begin
    MsgBox('La URL debe comenzar con http:// o https://', mbError, MB_OK);
    Exit;
  end;
  
  Result := True;
end;

function NextButtonClick(CurPageID: Integer): Boolean;
begin
  Result := True;
  
  // Validar la URL cuando el usuario sale de la página de configuración
  if CurPageID = ApiUrlPage.ID then
  begin
    Result := ValidateApiUrl(ApiUrlPage.Values[0]);
    if Result then
    begin
      // Remover trailing slash si existe
      ConfiguredApiUrl := ApiUrlPage.Values[0];
      if Copy(ConfiguredApiUrl, Length(ConfiguredApiUrl), 1) = '/' then
        ConfiguredApiUrl := Copy(ConfiguredApiUrl, 1, Length(ConfiguredApiUrl) - 1);
    end;
  end;
end;

procedure CreateAppSettingsJson();
var
  AppSettingsFile: String;
  JsonContent: TArrayOfString;
begin
  AppSettingsFile := ExpandConstant('{app}\appsettings.json');
  
  // Crear contenido del JSON
  SetArrayLength(JsonContent, 13);
  JsonContent[0] := '{';
  JsonContent[1] := '  "ApiSettings": {';
  JsonContent[2] := '    "BaseUrl": "' + ConfiguredApiUrl + '"';
  JsonContent[3] := '  },';
  JsonContent[4] := '  "AppSettings": {';
  JsonContent[5] := '    "DataDirectory": "C:\\ProgramData\\Unilocker",';
  JsonContent[6] := '    "MachineIdFile": "machine.id",';
  JsonContent[7] := '    "RegisteredFlagFile": "registered.flag"';
  JsonContent[8] := '  }';
  JsonContent[9] := '}';
  
  // Guardar archivo
  SaveStringsToFile(AppSettingsFile, JsonContent, False);
end;

procedure CurStepChanged(CurStep: TSetupStep);
begin
  if CurStep = ssPostInstall then
  begin
    // Crear directorio de datos de aplicación
    if not DirExists(ExpandConstant('{commonappdata}\Unilocker')) then
      CreateDir(ExpandConstant('{commonappdata}\Unilocker'));
    
    // Crear o actualizar appsettings.json con la URL configurada
    CreateAppSettingsJson();
  end;
end;

function InitializeSetup(): Boolean;
begin
  Result := True;
  
  // Verificar que estamos en Windows 10 o superior
  if not IsWin64 then
  begin
    MsgBox('Este software requiere Windows 10/11 de 64 bits.', mbError, MB_OK);
    Result := False;
  end;
end;

procedure CurUninstallStepChanged(CurUninstallStep: TUninstallStep);
var
  ResultCode: Integer;
begin
  if CurUninstallStep = usPostUninstall then
  begin
    // Preguntar si desea eliminar los datos de configuración
    if MsgBox('¿Desea eliminar también los archivos de configuración y datos del equipo?' + #13#10 + 
              '(Si planea reinstalar, puede mantenerlos)', mbConfirmation, MB_YESNO) = IDYES then
    begin
      // Eliminar directorio de datos
      if DirExists(ExpandConstant('{commonappdata}\Unilocker')) then
        DelTree(ExpandConstant('{commonappdata}\Unilocker'), True, True, True);
    end;
  end;
end;
