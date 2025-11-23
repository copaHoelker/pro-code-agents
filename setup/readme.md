# Setup and Management of Student User Accounts for Pro Code Agents Training

This directory contains scripts to set up and manage student user accounts for the Pro Code Agents training course.

## Prerequisites

- Azure CLI installed and logged in (`az login`)
- Permissions to create users, groups, and assign roles in Azure Entra (formerly Azure AD)
- Permissions to assign roles on the Azure AI project

## Creating Student User Accounts

Run the unified script to create 8 student users, add them to the "pro-code-agents" group, and assign the Azure AI Developer role to the Azure AI project.

**PowerShell:**

```powershell
.\setup\create-students.ps1
```

**Bash:**

```bash
bash ./setup/create-students.sh
```

This creates users: student01@yourdomain through student08@yourdomain with password "TiTp4student@", adds them to the group, and grants access to the Azure AI project.

## Student Login Information

- **Username**: studentXX@yourdomain (where XX is 01-08)
- **Password**: TiTp4student@
- **Azure AI Portal**: https://ai.azure.com/

Students should change their password on first login.

## Cleanup

To remove student accounts after the training:

1. **Remove Permissions and Delete Users**: Run the cleanup script.

   **PowerShell:**

   ```powershell
   .\setup\clean-users.ps1
   ```

   **Bash:**

   ```bash
   bash ./setup/clean-users.sh
   ```

   This removes the Azure AI Developer role assignments and deletes the user accounts. The "pro-code-agents" group is left intact.

## Notes

- Ensure you have the necessary Azure permissions before running these scripts.
- The scripts automatically detect your tenant domain from your current Azure login.
- If a user creation fails (e.g., user already exists), the script continues with the rest.
- Role assignments are scoped to the specific Azure AI project.
