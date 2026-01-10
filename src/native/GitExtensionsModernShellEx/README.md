# Modern GitExtensions shell extension

## Overview
This shell extension implements IExplorerCommand to provide GitExtensions actions in the modern (default) context menu of Windows Explorer. It consists of a DLL implementing the COM server and a sparse MSIX package which is necessary to be able to register the shell extension within Windows.

## Signing (development)
Use `tools/SignModernShellEx.ps1` to sign the MSIX and DLL when they are not already validly signed.

```powershell
powershell -NoLogo -NoProfile -ExecutionPolicy Bypass -File tools\SignModernShellEx.ps1 -PackagePath <path-to>\GitExtensionsModernShellEx.msix -CertificatePath <path-to>\GitExtensionsModernShellEx_TemporaryKey.pfx
```

Notes:
- If the MSIX and DLL are already signed, the script exits without changes unless `-Force` is supplied.
- When signatures are missing, the script prompts to create or reuse a self-signed certificate suitable for development.
- `-DllPath` can override the default DLL path (which is the MSIX directory).

## Registering the sparse package
If a self-signed certificate was used to sign the files, the same certificate must manually be trusted. For that, right-click the certificate and choose *Install PFX*. If you didn't provide your own certificate and the script generated it, the password will be "gitextensions". Install the certificate into the "Trusted People" store for the local computer. Installing it only into the current user store is often not sufficient and can cause installation or registration to fail.

**Troubleshooting certificate / registration issues**
- If `RegisterModernShellEx.ps1` (or `Add-AppxPackage`) fails with certificate or signing errors, verify that the signing certificate is present in the *Local Machine \ Trusted People* store.
- On systems where Windows Developer Mode is disabled, unsigned or untrusted MSIX packages typically cannot be installed. Ensure the MSIX is signed and that the signing certificate is trusted as described above.
- If you previously installed the certificate only in the current user store, move or re-import it into the *Local Machine \ Trusted People* store and retry the registration.

Use `tools/RegisterModernShellEx.ps1` to register or unregister the sparse MSIX. The script uses its own location as the default external content root.

Register:
```powershell
powershell -NoLogo -NoProfile -ExecutionPolicy Bypass -File tools\RegisterModernShellEx.ps1 -PackagePath <path-to>\GitExtensionsModernShellEx.msix
```

Unregister:
```powershell
powershell -NoLogo -NoProfile -ExecutionPolicy Bypass -File tools\RegisterModernShellEx.ps1 -Unregister
```
