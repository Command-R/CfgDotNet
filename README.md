
CfgDotNet
=========
CfgDotNet allows you to manage your configuration data in json for multiple environments.
Simply create a "cfg.json" file in the root of your project in this format:

```
{
    "activeEnvironment": "dev",
    "environments": {
        "dev": {
            "connectionStrings": {
                "MainConnection": {
                    "connectionString": "server=dev.databaseserver.com;database=MyDB_DEV;uid=User_DEV;pwd=pa55w0rd!_DEV",
                    "providerName": "System.Data.SqlClient"
                }
            },
            "appSettings": {
                "showDebugPanel": true
            },
            "elasticsearch": {
                "user": "elastic-user-dev",
                "password": "0d35e633c2cf40a78fb6537294a084f8_dev",
                "baseUrl": "https://dev.fakeelasticserver.com:9200",
                "defaultIndex": "dev_documents"
            }
        },
        "prod": {
            "connectionStrings": {
                "MainConnection": {
                    "connectionString": "server=prod.databaseserver.com;database=MyDB_PROD;uid=User_PROD;pwd=pa55w0rd!_PROD",
                    "providerName": "System.Data.SqlClient"
                }
            },
            "appSettings": {
                "showDebugPanel": false
            },
            "elasticsearch": {
                "user": "elastic-user-prod",
                "password": "847d80d268e842dd9ff4562c3117417c_prod",
                "baseUrl": "https://prod.fakeelasticserver.com:9200",
                "defaultIndex": "prod_documents"
            }
        }
    }
}
```

You can provide a BaseEnvironment and just override the necessary values in other environments:

```
{
    "activeEnvironment": "prod",
    "baseEnvironment": "dev",
    "environments": {
        "dev": {
            "connectionStrings": {
                "MainConnection": {
                    "connectionString": "server=dev.databaseserver.com;database=MyDB_DEV;uid=User_DEV;pwd=pa55w0rd!_DEV",
                    "providerName": "System.Data.SqlClient"
                }
            },
            "appSettings": {
                "alwaysTheSame": "yep",
                "showDebugPanel": true
            },
            "elasticsearch": {
                "user": "elastic-user",
                "password": "0d35e633c2cf40a78fb6537294a084f8_dev",
                "baseUrl": "https://fakeelasticserver.com:9200",
                "defaultIndex": "documents"
            }
        },
        "prod": {
            "connectionStrings": {
                "MainConnection": {
                    "connectionString": "server=prod.databaseserver.com;database=MyDB_PROD;uid=User_PROD;pwd=pa55w0rd!_PROD"
                }
            },
            "appSettings": {
                "showDebugPanel": false
            },
            "elasticsearch": {
                "password": "847d80d268e842dd9ff4562c3117417c_prod"
            }
        }
    }
}
```

You can also use the built-in SettingsManager which makes it easy to use strongly-typed settings classes in your
code. These classes can be populated from the app.config or web.config AppSettings or ConnectionStrings sections,
also, cfg.json and Databases are supported. You can just add whichever providers you want, load the settings,
optionally validate, then store in an ioc container. The mandatory ISettings interface includes a Validate() method
that can throw exceptions if, for example, a database isn't available allowing your application to fail-fast
on startup. Providers can even depend on settings populated by other providers (eg SqlDatabaseSettingsProvider
can pull its ConnectionString)

```
const string environment = "prod";
const string cfgPath = "cfg.json";
var assemblies = new[] { GetType().Assembly };

new SettingsManager()
    .AddProvider(new ConnectionStringsSettingsProvider())
    .AddProvider(new AppSettingsSettingsProvider())
    .AddProvider(new CfgDotNetSettingsProvider(environment, cfgPath))
    .AddSettings(assemblies)
    .AddProvider<Settings>(x => new SqlDatabaseSettingsProvider(x.ConnectionString))
    .LoadSettingsFromProviders()
    .Validate()
    .ForEach(x => Container.RegisterSingle(x.GetType(), x));
```
