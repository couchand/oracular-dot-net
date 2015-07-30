oracular-dot-net
================

a query builder

  * intro
  * getting started
  * more information

[![Build Status](https://travis-ci.org/couchand/oracular-dot-net.svg?branch=master)](https://travis-ci.org/couchand/oracular-dot-net)

intro
-----

***oracular*** is a little query builder based on the specification
pattern and oriented towards business users. The goal is to refactor
business logic in such a way that it can be easily stored and used to
drive the execution of business applications.

getting started
---------------

Install the package with NuGet.

    NuGet Install-Package Oracular

Load and check your config file.

```csharp
var config = OracularConfig.LoadFromFile(HostingEnvironment.MapPath(@"~/App_Data/oracular.json"));

config.Check();
```

Serialize some spec queries.

```csharp
var customers = config.GetSpec("isCustomerAccount");
var customersSql = spec.ToSql();

var managers = config.GetSpec("isManagerUser");
var managersSql = spec.ToSql();
```

Check a new spec against the config file.

```csharp
var specSource = "isCustomerAccount(Account) AND isManagerUser(Account.Owner)";
config.CheckSpec("Account", specSource);
```

Add the new spec to the live configuration.

```csharp
config.AddSpec("isCustomerOwnedByManager", "Account", specSource);
```

Export the updated configuration file.

```csharp
var updatedJson = config.Export();
```

more information
----------------

See the [oracular][0] project repository for documentation.

[0]: https://github.com/couchand/oracular

##### ╭╮☲☲☲╭╮ #####
