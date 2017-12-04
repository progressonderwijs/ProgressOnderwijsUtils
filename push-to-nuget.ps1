if ($env:APPVEYOR_REPO_BRANCH -eq 'master') {
  Get-ChildItem -Recurse *.nupkg | Foreach-Object {
    dotnet nuget push --source https://www.nuget.org/api/v2/package --api-key $env:NUGET_API_KEY $_.FullName
  }
}
