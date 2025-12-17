# Instalador de Unilocker Client

Este directorio contiene el instalador completo del cliente Unilocker para Windows.

## Archivos

- **UnilockerClientSetup_v1.0.0.exe** (49 MB): Instalador autónomo con runtime de .NET 8 incluido
- **UnilockerInstaller.iss**: Script de Inno Setup para generar el instalador
- **README.md**: Este archivo

## Características del Instalador

### Instalación
- **Completamente autónomo**: No requiere .NET instalado previamente
- **Tamaño**: ~49 MB (incluye .NET 8 Runtime completo)
- **Configuración personalizada**: Durante la instalación se solicita la URL de la API
- **Validación**: Verifica que la URL de la API sea válida (http:// o https://)
- **Ubicación**: Se instala en `C:\Program Files\Unilocker Client`
- **Acceso directo**: Crea icono en el escritorio y en el menú inicio

### Desinstalación
- **Pregunta inteligente**: Al desinstalar, pregunta si desea eliminar la configuración
- **Limpieza completa**: Si se confirma, elimina todos los archivos incluyendo appsettings.json
- **Desinstalador**: Disponible desde Panel de Control > Programas

## Requisitos del Sistema

- Windows 10 (64-bit) o superior
- Windows Server 2016 o superior
- 100 MB de espacio en disco
- Conexión a internet (para comunicarse con la API)

## Uso

### Para el Usuario Final

1. Ejecutar `UnilockerClientSetup_v1.0.0.exe`
2. Seguir el asistente de instalación
3. Ingresar la URL de la API cuando se solicite (ej: http://servidor:5000)
4. Completar la instalación
5. El cliente se iniciará automáticamente

### Para el Desarrollador

Si necesitas regenerar el instalador:

```powershell
# Desde la raíz del proyecto
.\Build-Installer.ps1
```

Este script:
1. Limpia compilaciones anteriores
2. Compila el cliente con `--self-contained true`
3. Copia los archivos al directorio installer
4. Genera el instalador con Inno Setup
5. Limpia archivos temporales

## Configuración de la API

Durante la instalación se crea automáticamente el archivo `appsettings.json` con:

```json
{
  "ApiSettings": {
    "BaseUrl": "http://url-proporcionada-por-usuario"
  }
}
```

Este archivo puede modificarse posteriormente desde:
`C:\Program Files\Unilocker Client\appsettings.json`

## Distribución

El instalador está listo para distribuirse. Puede:
- Compartirse por correo electrónico
- Descargarse desde un servidor web
- Instalarse desde memoria USB
- Distribuirse por sistemas de gestión de software corporativo

No requiere ningún archivo adicional ni dependencias externas.

## Problemas Comunes

### "Windows protegió su PC"
- Hacer clic en "Más información" y luego en "Ejecutar de todas formas"
- Esto es normal para aplicaciones sin firma digital

### Error de instalación
- Verificar que no haya otra instalación de Unilocker Client
- Ejecutar como Administrador
- Verificar espacio en disco disponible

### El cliente no se conecta a la API
- Verificar la URL de la API en appsettings.json
- Confirmar que la API esté ejecutándose
- Verificar conectividad de red

## Soporte Técnico

Para soporte y más información, consultar la documentación principal del proyecto.

## Versión

**v1.0.0** - Versión inicial con runtime de .NET 8 incluido
