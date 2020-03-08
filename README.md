# The TrueLayer Api Example

Welcome to the future: Open Banking!

You'll need an `appSettings.Development.json` in the root of the project with the following or similar:

    {
        "Logging": {
            "LogLevel": {
            "Default": "Information",
            "Microsoft": "Warning",
            "Microsoft.Hosting.Lifetime": "Information"
            }
        },
        "TrueLayerLiveCredentials": {
            "ClientId": "<your-client-id>",
            "ClientSecret": "<your-secret-key>",
            "BaseAuthUrl": "https://auth.truelayer.com",
            "BaseDataApiUrl": "https://api.truelayer.com",
            "RedirectUrl": "https://localhost:5001/api/callback"
        },
        "TrueLayerSandboxCredentials": {
            "ClientId": "<your-client-id>",
            "ClientSecret": "<your-secret-key>",
            "BaseAuthUrl": "https://auth.truelayer-sandbox.com",
            "BaseDataApiUrl": "https://api.truelayer-sandbox.com",
            "RedirectUrl": "https://localhost:5001/api/callback"
        }
    }

To change between Live and Sandbox mode change the environment variable in `Properties/launchsettings.json` `USE_TRUELAYER_SANDBOX` to be `"false"` or `"true"` accordingly. (If running from docker it'll be the environment var in the `docker-compose.override.yml` file in the root.)

To run the app e.g. using docker, run the following command from the root of the project

    $ docker-compose -f docker-compose.yml -f docker-compose.override.yml up --build

To run the unit tests with code coverage in vscode (this creates an `lcov.info` file used by the coverage gutters plugin)

    $ dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=lcov /p:CoverletOutput=./lcov.info TlApiExampleTests

## Api Usage

### Important

In order to create a session to identify the user I decided to implement a simple convenience login as a get request with url params. (Obviously this is only to allow the session to be created for the purposes of the exercise and would never normally be done like this.) Thus you _must_ log in before doing anything by going to the following url in the browser:

    https://localhost:5001/api/login?username=john&password=doe

I've used the same credentials as for the TrueLayer sandbox.

After that, go to the TrueLayer auth url e.g.

    https://auth.truelayer-sandbox.com/?response_type=code&client_id=<your-client-id>&scope=info%20accounts%20balance%20cards%20transactions%20direct_debits%20standing_orders%20offline_access&redirect_uri=https://localhost:5001/api/callback&providers=uk-ob-all%20uk-oauth-all%20uk-cs-mock

Then you can visit the following urls in the broswer (in any order)

    https://localhost:5001/api/transactions
    https://localhost:5001/api/aggregate

Results are cached to prevent further TrueLayer api hits, but cache management and expiry is not handled properly here.

## Notes:

I used a DistributedCache because I figured it would be easier, but in hindsight an sqlite in memory db may have been more efficient to save on all the serializing/deserializing of the entire cache.

I would have liked to have written more unit tests. Hopefully the ones I did serve as an illustration.
