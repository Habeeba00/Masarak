# Project Management and CLI commands

## commands 
```
cd "D:\ITI\GradProj\Masarak"
dotnet ef migrations add InitialCreate --project "Masarak.API"
dotnet ef database update --project "Masarak.API"
```

## how to run project
```
cd "D:\ITI\GradProj\Masarak\Masarak.API"
dotnet run
```
## github 
git checkout -b phaseN
git add .
git commit -m "add phaseN"
git push -u origin phaseN

## Merge 
```
git checkout main
git pull origin main
git merge phaseN
git push origin main

```