# Cloudflare Dynamic DNS Updater

This application updates a specified "A" record for your domain (zone) on Cloudflare when your IP address changes (which it checks for once on start-up and then every sixty minutes). A free and easy alternative to other dynamic DNS solutions.

# Requirements

Requires:
- Windows
- That you have a domain.
- That your domain uses Cloudflare's nameservers (which is free).

Can run on other systems if you re-create the project to target .NET Core.

# Set-up

Makes use of environment variables instead of embedding API keys and secrets within the code, this also makes it easy to change the API key, targeted zone, etc. on the fly without requiring you to edit the code.
Both System and User environment variables can be used, I'd recommend the following practice:
- In a business environment, run the program as its own dedicated user account and use user-scoped environment variables.
- In a home lab environment, use system-scoped environment variables and use Task Scheduler to automatically start the program at boot.
- You may start the program via batch/PowerShell and specify the environment variables on a program scoped level.

The following environment variables must be set:
- `CF_API_KEY`, this is your Cloudflare API key. I recommend you scope the key's access to DNS zone edit permissions only.
- `CF_API_EMAIL`, this is the email address associated with your Cloudflare account.
- `CF_DNS_RECORD_ID`, this is the record ID of the DNS record you wish to modify. Must be an "A" record. **You can find your DNS record ID by making a test update manually to the record, and then heading to "Manage Account" > "Audit Log", click on the log entry that corresponds to your manual record update (the blue arrow), and what you want is the `Resource ID`.
- `CF_DNS_ZONE_ID`, this is the zone ID of your TLD. You can find this using the same method as above.
- `CF_DNS_NAME`, this is the name of the DNS "A" record to update.

If you have the source code open in Visual Studio, restart Visual Studio after setting these if you set them as system or user-scoped environment variables. This is because Visual Studio caches your system and user environment variables when it starts up.

`https://api.ipify.org/` is used to provide the public-facing IPv4 of the machine running the program.
Change the `checkFrequency` variable to change how often the program checks for IP address changes. The default is once every 30 minutes.
