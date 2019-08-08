# DbModelGenerator

dotnet core msbuild task which generate POCO classes from db up migration files.

This task make the perfect fit to be used in conjunction with Dapper and DapperExtensions.

# How to use it

In your .csproj file simply add the reference to the nuget assembly:

```xml
    <ItemGroup>
        <PackageReference Include="DbModelGenerator" Version="0.0.13"/>
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