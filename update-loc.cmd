@echo off
dotnet build /v:q
pushd .\src\app\GitExtensions
dotnet msbuild /t:_UpdateEnglishTranslations /p:RunTranslationApp=true 
popd