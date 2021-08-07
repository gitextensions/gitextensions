@echo off
dotnet build /v:q
pushd .\GitExtensions
dotnet msbuild /t:_UpdateEnglishTranslations /p:RunTranslationApp=true 
popd