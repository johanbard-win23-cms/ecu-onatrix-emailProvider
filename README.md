# Umbraco CSM
## EmailProvider
Azure Functions (Function EmailSender) tillhörande [ecu-onatrix](https://github.com/johanbard-win23-cms/ecu-onatrix)
- Lyssnar på en ServiceBus, tar emot meddelande och skickar vidare till Communication Service (som använder Email Communication Service) för att skicka e-post-objektet som fanns i meddelandet
