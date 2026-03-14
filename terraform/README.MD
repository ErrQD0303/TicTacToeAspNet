# Steps to run the terraform code

1. Create a `.env` file in the `terraform` directory and add the necessary environment variables. You can use the provided `template.env` file as a template and fill in the values accordingly.

2. Create a service principal with no permissions (or with the least permissions possible) using the Azure CLI. You can do this by running the following command:

   ```bash
   source scripts/create-service-principal.sh
   ```

   This command will output the necessary credentials (client ID, client secret, tenant ID, and subscription ID) that you will need to set as environment variables.

3. Add the required permissions to the service principal for the resource group where you want to deploy your infrastructure. You can do this by running the following command:

   ```bash
   source scripts/assign-roles.sh
   ```

   This command will assign the "Contributor" role to the service principal for the specified resource group.

4. Initialize the backend and create the necessary resources for storing the Terraform state by running the following command:

   ```bash
   source scripts/initialize-resources.sh
   ```

5. Finally, you can run the Terraform commands to plan and apply your infrastructure changes:

   ```bash
    terraform init \
   -backend-config="resource_group_name=$TF_VAR_RESOURCE_GROUP_NAME" \
   -backend-config="storage_account_name=$TF_VAR_STORAGE_ACCOUNT_NAME" \
   -backend-config="container_name=$TF_VAR_CONTAINER_NAME" \
   -backend-config="key=$TF_VAR_ENVIRONMENT.tfstate"
   ```

   ```bash
    terraform plan
   ```

   ```bash
    terraform apply --auto-approve
   ```

6. Upload the frontend files from 'wwwroot' folder to the storage account's $web container. You can do this using Azure Storage Explorer or Azure CLI.

   ```bash
   source scripts/upload-frontend.sh
   ```

7. Destroy the infrastructure when you are done:

   ```bash
    terraform destroy --auto-approve
   ```
