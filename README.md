# EnvVariableInject for Fody

EnvVariableInject can be used to inject the values of Environment Variables into 
fields at build time, this can be useful for injecting build numbers, secret test config values
or anything that you might not want to store in your code.

Install from nuget:

```ps
Install-Package EnvVariableInject.Fody
```

If the ModuleWeavers.xml file is not updated automatically then edit to add EnvVariableInject

```xml
<Weavers>
  <EnvVariableInject />
</Weavers>
```

## Usage

```csharp
[BuildTimeEnvironmentVariable("InjectTestString")]
public string TestField = "";

[BuildTimeEnvironmentVariable("InjectTestBool")]
public bool TestBool = false;

[BuildTimeEnvironmentVariable("InjectTestInt")]
public int TestInt = 200;

[BuildTimeEnvironmentVariable("InjectTestDouble")]
public double TestDouble = 99.99;
```

The `BuildTimeEnvironmentVariableAttribute` should be added to your project

At build time the weaver will attempt to replace the field with the environment
variable specified in the Attribute constructor

## Known Issues

The following will not work with the Release Build Configuration

```
[BuildTimeEnvironmentVariable("TestString")]
public string TestString;

[BuildTimeEnvironmentVariable("TestInt")]
public int TestInt;

```

This is because the compiler removes the instruction to set the field to null in the constructor.
I intend to rectify this issue in the future by injecting the required instructions when they do not exist.

To workaround this just set any value on the field and it will be replaced.

This package relies on [Fody](https://github.com/Fody/Fody)
