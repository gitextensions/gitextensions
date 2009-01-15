@setlocal
@for /F "delims=" %%I in ("%~dp0..") do @set git_install_root=%%~fI
@set path=%git_install_root%\bin;%git_install_root%\mingw\bin;%PATH%
@if "%HOME%"=="" @set HOME=%USERPROFILE%

@set DISPLAY=:0
@set SSH_ASKPASS="C:\Program Files\Git\libexec\git-core\git-gui--askpass"
@set ASK_PASS="C:\Program Files\Git\libexec\git-core\git-gui--askpass"

"C:\Program Files\Git\bin\ssh" -n -t git@github.com
"C:\Program Files\Git\libexec\git-core\git-push"
