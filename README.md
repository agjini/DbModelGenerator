# DbModelGenerator
dotnet core msbuild task which generate POCO classes from db up migration files

# How to use it

In your .csproj file, add the reference to the nuget assembly:

```xml
    <ItemGroup>
        <PackageReference Include="DbModelGenerator" Version="0.0.9"/>
    </ItemGroup>
```

Then feel free to cal the DbGenerateModel task on pre-build step :

```xml
    <Target Name="Greet" BeforeTargets="Build">
        <GenerateDbModel/>
    </Target>
```

This will look for migrations scripts located into the current `${ProjectDir}/Scripts` directory.
It will iterate for all sub directories inside Scripts directory.

For each of them :
- It will apply the Dbup migration scripts located in the directory (into an Sqlite in memory database).
- It will generate the POCO classes mapping exactly the schema tables.
- The files will be written to `${ProjectDir}/Generated/${MigrationDirectoryName}`