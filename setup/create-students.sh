#!/bin/bash

# Script to create 8 student user accounts in Azure Entra, add them to a group, and assign Azure AI Developer role

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

# Create the group
echo "Creating group 'pro-code-agents'..."
group_id=$(az ad group create --display-name "pro-code-agents" --mail-nickname "pro-code-agents" --query "id" -o tsv)
if [ $? -ne 0 ]; then
    echo "Failed to create group."
    exit 1
fi
echo "Group created with ID: $group_id"

# Role ID for Azure AI Developer
role_id="64702f94-c441-49e6-a78b-ef80e0188fee"

# Password for all users
password="TiTp4student@"

# Create 8 users
for i in {01..08}; do
    user_principal="student$i@$domain"
    display_name="Student $i"

    echo "Creating user: $user_principal"
    user_id=$(az ad user create --display-name "$display_name" --user-principal-name "$user_principal" --password "$password" --force-change-password-next-sign-in false --query "id" -o tsv)
    if [ $? -ne 0 ]; then
        echo "Failed to create user $user_principal"
        continue
    fi

    # Add user to group
    echo "Adding $user_principal to group..."
    az ad group member add --group "$group_id" --member-id "$user_id"
    if [ $? -ne 0 ]; then
        echo "Failed to add $user_principal to group"
    else
        echo "User $user_principal created and added to group."
    fi

    # Assign Azure AI Developer role
    echo "Assigning Azure AI Developer role to $user_principal..."
    az role assignment create --assignee "$user_principal" --role "$role_id" --scope "$projectResourceId"
    if [ $? -ne 0 ]; then
        echo "Failed to assign role to $user_principal"
    else
        echo "Role assigned to $user_principal."
    fi
done

echo "Script completed."
