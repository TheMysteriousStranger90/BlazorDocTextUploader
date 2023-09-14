# BlazorDocTextUploader
BlazorDocTextUploader


•	Register an account on Azure portal (https://portal.azure.com/).

•	Create Azure Blob storage account.

•	Create Azure Web App for .Net

•	Create ASP.NET WEB application and deploy it to Azure Web App.

•	UI for web app can be one of: Blazor, Angular or React.

•	On the web app on start must be only one page with Form where user can upload a file (must be added validation for only .docx files) and add the user email (validation for email).

•	Web Application is putting that file to the BLOB storage.

•	Create Azure Function with BLOB storage trigger from already created BLOB and when file is added to blob the email is sent to the user with notification the file is successfully uploaded. The URL to the file must be secured with SAS token on the BLOD storage. SAS token must be valid only for 1 hour.

•	Unit tests for backend logic of web app and azure function must be added.

•	As a result, you must provide a link to Azure Web Application with the Form where user can upload the file and receive notification.

•	Please provide URL to the web app and the github link to the source code of web application and azure function.
