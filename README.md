# Deployment Guide

## Prerequisites

Ensure the following tools are installed before proceeding:

- **Azure CLI**: [Install Guide](https://learn.microsoft.com/en-us/cli/azure/install-azure-cli-windows?pivots=msi)
- **Pulumi**: [Installation Instructions](https://www.pulumi.com/docs/iac/get-started/azure/begin/)
- **Azure Functions Core Tools**: [GitHub Repository](https://github.com/Azure/azure-functions-core-tools)

## Concepts:
[Pulumi stack](https://www.pulumi.com/docs/iac/concepts/stacks/)

## Deployment Commands

Use the following commands to manage the deployment of the stack by name:

- **Deploy the infrastructure:**
  ```sh
  pulumi up -s functionA
  pulumi up -s functionB
  ```

- **Destroy the infrastructure:**
  ```sh
  pulumi destroy -s functionA
  pulumi destroy -s functionB
  ```

## Known Issues

For any known issues related to Azure Functions Core Tools, refer to the following link:
- [Issue #1616](https://github.com/Azure/azure-functions-core-tools/issues/1616)

