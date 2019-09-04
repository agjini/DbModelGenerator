# DbModelGenerator

dotnet core msbuild task which generate POCO classes from db up migration files.

This task make the perfect fit to be used in conjunction with Dapper and DapperExtensions and DbUp.

It allows to make a real database first approach on your project.
A good practice is not to add the generated classes to your SCM (by adding Generated/ folder to `.gitignore` for example), because they will be (re)generated before each build.

# How to use it

In your .csproj file simply add the reference to the nuget assembly:

```xml
    <ItemGroup>
        <PackageReference Include="DbModelGenerator" Version="0.1.6"/>
    </ItemGroup>
```

And then feel free to call the DbGenerateModel task on pre-build step :

```xml
    <Target Name="Generate Db Model" BeforeTargets="BeforeBuild">
        <GenerateDbModel/>
    </Target>
```

This will look for migrations scripts located into the `${ProjectDir}/Scripts` directory.
It will iterate over all sub directories found into the `Scripts` directory.

For each of them :
- It will apply the Dbup migration scripts located in the directory (into an Sqlite in memory database).
- It will generate the POCO classes mapping the schema tables.
- The files will be written to `${ProjectDir}/Generated/${MigrationDirectoryName}` namespace.

# Optional parameters :

- `ScriptsDir` : The absolute directory where to search for DbUp migration scripts
- `IdentityInterface` : An optional identity interface (with an Id property) for the matching class (with an unique id primary key) to implement. It is a generic class with a generic type parameter.

# Example

See provided Example.Service.csproj, except for you have to remove the UsingTask (it is used for the example project to work inside the same solution of the task)
