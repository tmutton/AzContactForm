# AzContactForm

## What is AzContactForm?

AzContactForm is a cloud based service offering the ability to handle contact form communications made usually from websites.

This service is particularly aimed at websites without access to server-side technologies.

## Setup
To deploy this service you will need a Microsoft Azure account with an active subscription.

There are two ways to deploy this service:

- GitHub
- Visual Studio

## GitHub
On the repository page click on the folder "AzContactForm.ResourceGroup".

Click the button "Deploy to Azure".

[Deploy to azure button]

This will send you to deploy.azure.com where you'll be presented with 

## Visual Studio
Clone this repo and open up the solution in Visual Studio (version >= 15.3 and Visual Studio 2017 Tools for Azure Functions installed).

Right click the project "AzContactForm.ResourceGroup" and click "Deploy".

Select your subscription and resource group and click the "Deploy" button.

The deployment process should take a few minutes. When it completes head over to your Azure Portal to view the deployed resources.

You will find three resources of types:

- App Service Plan
- App Service
- Storage account

## Usage

Once AzContactForm has deployed head over to the resource group and navigate to the Function. You'll need to find the post function's url so open that up and click on "Get function URL".

Open the Demo in the repository and use the url above to replace the url in the demo code as seen below.
