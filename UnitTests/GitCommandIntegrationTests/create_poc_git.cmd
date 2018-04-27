set POC_REPO=%~dp0repos\poc
attrib /s -r -h -s %POC_REPO%\*
del /s /q %POC_REPO%
mkdir %POC_REPO%
pushd %POC_REPO%
call :git init
git config --local user.name "John Doe"
git config --local user.email johndoe@example.com
call :git checkout -b master

echo poc>poc.txt
call :git add poc.txt
call :git commit -m "poc"
call :git checkout -b test1
call :git checkout -b test2
call :git checkout -b test3
git checkout -b feature/magic
call :git tag tag1
call :git tag -a tag2 -m "annotation"

echo pocpoc>pocpoc.txt
call :git add pocpoc.txt
call :git commit -m "pocpoc"

call :git tag tag3
call :git tag -a another_annotated_tag -m "annotation"

git checkout master

call :cleanup
popd

goto fin

:cleanup
git reflog expire --all --expire=now
git gc --prune=now --aggressive
del %POC_REPO%\.git\hooks\*.sample
git config --local --unset user.name 
git config --local --unset user.email
goto fin

:git
git %*
timeout /t 2 > nul

:fin
