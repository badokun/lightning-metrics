$ver = [regex]::Match((Get-Content .\src\Lightning.Metrics.App\Lightning.Metrics.App.csproj), '<Version>([^<]+)<').Groups[1].Value
git tag -a "v$ver" -m "$ver"
git push --tags
# git tag -d "stable"
# git tag -a "stable" -m "stable"
# git push --tags --force
