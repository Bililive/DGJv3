7z a 'DGJv3.7z' "$env:PAPPVEYOR_BUILD_FOLDER\DGJv3\bin\Release\x64\DGJ.dll"
Move-Item -Path 'DGJv3.7z' -Destination "$env:DEPLOY_SITE_GIT\resource\DGJv3\DGJv3.7z"

git --git-dir="$env:DEPLOY_SITE_GIT\.git\" --work-tree="$env:DEPLOY_SITE_GIT" add -A
git --git-dir="$env:DEPLOY_SITE_GIT\.git\" --work-tree="$env:DEPLOY_SITE_GIT" commit --quiet -m "DGJv3 $env:APPVEYOR_BUILD_VERSION"
git --git-dir="$env:DEPLOY_SITE_GIT\.git\" --work-tree="$env:DEPLOY_SITE_GIT" push --quiet --set-upstream origin $env:DEPLOY_SITE_BRANCH 2>&1 | ForEach-Object { $_.ToString() } # WHYYYYYYYYYY

$headers = @{
    'Accept'        = 'application/vnd.github.v3+json'
    'User-Agent'    = 'genteure@github appveyor@genteure.com'
    'Authorization' = "token $env:github_access_token"
}
$body = @{
    'title'                 = "[CI] DGJv3 $env:APPVEYOR_BUILD_VERSION"
    'head'                  = "$env:DEPLOY_SITE_BRANCH"
    'body'                  = "Update file for DGJv3 $env:APPVEYOR_BUILD_VERSION\n别忘了修改 ``plugin_version`` 和 ``plugin_update_datetime``"
    'base'                  = 'master'
    'maintainer_can_modify' = $true
} | ConvertTo-Json

Invoke-RestMethod -Method Post -Headers $headers -Body $body -Uri "https://api.github.com/repos/Bililive/www.danmuji.org/pulls" -ErrorAction:SilentlyContinue | Out-Null
