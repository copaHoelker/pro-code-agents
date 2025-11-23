#!/bin/bash

# Script to remove Azure AI Developer role from student users and delete the users
# Leaves the "pro-code-agents" group intact

# Hardcoded project resource ID
projectResourceId="/subscriptions/cd091145-5ea2-4703-ba5d-41063b1d4308/resourceGroups/rg-pro-code-agents/providers/Microsoft.CognitiveServices/accounts/pro-code-agents-resource/projects/pro-code-agents"

# Disable Azure CLI auto upgrade
export AZURE_CLI_DISABLE_AUTO_UPGRADE=1

# Check if logged in to Azure
if [ -z "$(az account show 2>/dev/null)" ]; then
    echo "Not logged in to Azure. Please run 'az login' first."
    exit 1
fi

# Get the current user's domain
current_user=$(az account show --query "user.name" -o tsv)
domain=$(echo "$current_user" | cut -d'@' -f2)

echo "Using domain: $domain"
echo "Using project resource ID: $projectResourceId"

# Role ID for Azure AI Developer
role_id="64702f94-c441-49e6-a78b-ef80e0188fee"

# Remove role and delete each student
for i in {01..08}; do
    user_principal="student$i@$domain"

    echo "Removing role from $user_principal..."
    az role assignment delete --assignee "$user_principal" --role "$role_id" --scope "$projectResourceId"
    if [ $? -ne 0 ]; then
        echo "Failed to remove role from $user_principal (may not have been assigned)"
    else
        echo "Role removed from $user_principal."
    fi

    echo "Deleting user $user_principal..."
    az ad user delete --id "$user_principal"
    if [ $? -ne 0 ]; then
        echo "Failed to delete user $user_principal"
    else
        echo "User $user_principal deleted."
    fi
done

echo "Cleanup completed. The 'pro-code-agents' group has been left intact."
