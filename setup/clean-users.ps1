# Script to remove Azure AI Developer role from student users and delete the users
# Leaves the "pro-code-agents" group intact

# Hardcoded project resource ID
$ProjectResourceId = "/subscriptions/cd091145-5ea2-4703-ba5d-41063b1d4308/resourceGroups/rg-pro-code-agents/providers/Microsoft.CognitiveServices/accounts/pro-code-agents-resource/projects/pro-code-agents"

# Check if logged in to Azure
try {
    $account = az account show 2>$null
    if (-not $account) {
        Write-Host "Not logged in to Azure. Please run 'az login' first."
        exit 1
    }
} catch {
    Write-Host "Not logged in to Azure. Please run 'az login' first."
    exit 1
}

# Get the current user's domain
$currentUser = az account show --query "user.name" -o tsv
$domain = $currentUser.Split('@')[1]

Write-Host "Using domain: $domain"
Write-Host "Using project resource ID: $ProjectResourceId"

# Role ID for Azure AI Developer
$roleId = "64702f94-c441-49e6-a78b-ef80e0188fee"

# Remove role and delete each student
for ($i = 1; $i -le 8; $i++) {
    $userPrincipal = "student{0:D2}@{1}" -f $i, $domain

    Write-Host "Removing role from $userPrincipal..."
    az role assignment delete --assignee $userPrincipal --role $roleId --scope $ProjectResourceId
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Failed to remove role from $userPrincipal (may not have been assigned)"
    } else {
        Write-Host "Role removed from $userPrincipal."
    }

    Write-Host "Deleting user $userPrincipal..."
    az ad user delete --id $userPrincipal
    if ($LASTEXITCODE -ne 0) {
        Write-Host "Failed to delete user $userPrincipal"
    } else {
        Write-Host "User $userPrincipal deleted."
    }
}

Write-Host "Cleanup completed. The 'pro-code-agents' group has been left intact."