if ($env:APPVEYOR_REPO_BRANCH -eq 'master' -and -not (Test-Path env:APPVEYOR_PULL_REQUEST_NUMBER)) {
  Get-ChildItem -Recurse *.nupkg | Foreach-Object {
    dotnet nuget push --source https://www.nuget.org/api/v2/package --symbol-api-key $env:NUGET_API_KEY $_.FullName
  }
}
